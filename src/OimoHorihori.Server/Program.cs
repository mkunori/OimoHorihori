using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using OimoHorihori.Shared.Accounts;
using OimoHorihori.Shared.Admin;
using OimoHorihori.Shared.Auth;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Shared.Profiles;
using OimoHorihori.Server.Security;
using OimoHorihori.Server.Services;
using OimoHorihori.Shared.Rankings;
using OimoHorihori.Shared.Saves;
using System.Net;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

string[] allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>()
    ?? Array.Empty<string>();

const string ClientCorsPolicy = "ClientCors";

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        ClientCorsPolicy,
        policy =>
        {
            policy
                .SetIsOriginAllowed(origin =>
                {
                    if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri? uri))
                    {
                        return false;
                    }

                    // ローカル開発
                    if (uri.Host == "localhost" || uri.Host == "127.0.0.1")
                    {
                        return true;
                    }

                    // 公開Client
                    return allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
                })
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

string dbPath = builder.Configuration["Database:Path"] ?? Path.Combine(builder.Environment.ContentRootPath, "oimohorihori.db");

builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite($"Data Source={dbPath}"));
builder.Services.AddScoped<IPasswordHasher<UserAccount>, PasswordHasher<UserAccount>>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddSingleton<AdminAuthService>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownProxies.Add(IPAddress.Loopback);
});


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseCors(ClientCorsPolicy);

app.MapGet("/api/health", () => { return "OK"; });

app.MapPost("/api/auth/register",
    async (RegisterRequest request, AppDbContext db, IPasswordHasher<UserAccount> passwordHasher) =>
    {
        string userName = request.UserName?.Trim() ?? string.Empty;
        string pin = request.Pin ?? string.Empty;

        if (userName.Length is < 1 or > 10)
        {
            return Results.BadRequest(new ApiErrorResponse("ユーザー名は1〜10文字で入力してください。"));
        }

        bool validPin = pin.Length == 4 && pin.All(c => c is >= '0' and <= '9');

        if (!validPin)
        {
            return Results.BadRequest(new ApiErrorResponse("PINは数字4桁で入力してください。"));
        }

        string normalizedUserName = userName.ToUpperInvariant();
        bool alreadyExists = await db.UserAccounts.AnyAsync(user => user.NormalizedUserName == normalizedUserName);

        if (alreadyExists)
        {
            return Results.Conflict(new ApiErrorResponse("そのユーザー名はすでに使用されています。"));
        }

        UserAccount account = new()
        {
            UserName = userName,
            NormalizedUserName = normalizedUserName,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        account.PinHash = passwordHasher.HashPassword(account, pin);

        db.UserAccounts.Add(account);

        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Results.Conflict(new ApiErrorResponse("そのユーザー名はすでに使用されています。"));
        }

        return Results.Ok(new RegisterResponse(account.Id, account.UserName));
    });

app.MapPost("/api/auth/login",
    async (LoginRequest request, AppDbContext db, IPasswordHasher<UserAccount> passwordHasher) =>
    {
        string userName = request.UserName?.Trim() ?? string.Empty;
        string pin = request.Pin ?? string.Empty;

        if (userName.Length is < 1 or > 10)
        {
            return Results.BadRequest(new ApiErrorResponse("ユーザー名またはPINが正しくありません。"));
        }

        bool validPin = pin.Length == 4 && pin.All(c => c is >= '0' and <= '9');

        if (!validPin)
        {
            return Results.BadRequest(new ApiErrorResponse("ユーザー名またはPINが正しくありません。"));
        }

        string normalizedUserName = userName.ToUpperInvariant();

        UserAccount? account = await db.UserAccounts.SingleOrDefaultAsync(user => user.NormalizedUserName == normalizedUserName);

        if (account is null || account.IsDeleted || account.IsDisabled)
        {
            return Results.Unauthorized();
        }

        PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(account, account.PinHash, pin);

        if (result == PasswordVerificationResult.Failed)
        {
            return Results.Unauthorized();
        }

        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            account.PinHash = passwordHasher.HashPassword(account, pin);
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        account.LastLoginAtUtc = now;

        string sessionToken = SessionTokenHelper.CreateToken();
        string tokenHash = SessionTokenHelper.HashToken(sessionToken);

        DateTimeOffset expiresAtUtc = now.AddDays(30);

        AuthSession session = new()
        {
            UserId = account.Id,
            TokenHash = tokenHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = expiresAtUtc
        };

        db.AuthSessions.Add(session);

        await db.SaveChangesAsync();

        return Results.Ok(new LoginResponse(account.Id, account.UserName, sessionToken, expiresAtUtc));
    });

app.MapGet("/api/auth/me",
    async (HttpRequest request, SessionService sessionService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(new CurrentUserResponse(user.Id, user.UserName));
    });

app.MapPost("/api/auth/logout",
    async (HttpRequest request, AppDbContext db) =>
    {
        string authorization = request.Headers.Authorization.ToString();

        const string prefix = "Bearer ";

        if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return Results.NoContent();
        }

        string token = authorization[prefix.Length..].Trim();

        if (!string.IsNullOrEmpty(token))
        {
            string tokenHash = SessionTokenHelper.HashToken(token);

            AuthSession? session = await db.AuthSessions.SingleOrDefaultAsync(s => s.TokenHash == tokenHash);

            if (session is not null)
            {
                db.AuthSessions.Remove(session);

                await db.SaveChangesAsync();
            }
        }

        return Results.NoContent();
    });

app.MapGet("/api/save",
    async (HttpRequest request, AppDbContext db, SessionService sessionService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        GameSave? gameSave = await db.GameSaves.SingleOrDefaultAsync(save => save.UserId == user.Id);

        if (gameSave is null)
        {
            return Results.NoContent();
        }

        SaveData? saveData = JsonSerializer.Deserialize<SaveData>(gameSave.SaveJson);

        if (saveData is null)
        {
            return Results.Problem("セーブデータを読み込めませんでした。");
        }

        return Results.Ok(new ServerSaveResponse(gameSave.Revision, saveData, gameSave.UpdatedAtUtc));
    });

app.MapPut("/api/save",
    async (ServerSaveRequest saveRequest, HttpRequest request, AppDbContext db, SessionService sessionService, RankingService rankingService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        if (!IsValidAscentProgress(saveRequest.Save))
        {
            return Results.BadRequest(new ApiErrorResponse("ASCENTデータの整合性を確認できませんでした。"));
        }

        GameSave? currentSave = await db.GameSaves.SingleOrDefaultAsync(save => save.UserId == user.Id);

        //
        // 初回保存
        //
        if (currentSave is null)
        {
            if (saveRequest.Revision != 0)
            {
                return Results.Conflict();
            }

            DateTimeOffset now = DateTimeOffset.UtcNow;

            GameSave newSave =
                new()
                {
                    UserId = user.Id,
                    Revision = 1,
                    SaveJson = JsonSerializer.Serialize(saveRequest.Save),
                    UpdatedAtUtc = now
                };

            db.GameSaves.Add(newSave);

            await rankingService.UpdateFromSaveAsync(user, saveRequest.Save);

            await db.SaveChangesAsync();

            return Results.Ok(
                new ServerSaveResponse(newSave.Revision, saveRequest.Save, now));
        }

        //
        // Revision競合確認
        //
        if (saveRequest.Revision != currentSave.Revision)
        {
            SaveData? latestSave = JsonSerializer.Deserialize<SaveData>(currentSave.SaveJson);

            if (latestSave is null)
            {
                return Results.Problem("セーブデータを読み込めませんでした。");
            }

            return Results.Conflict(new ServerSaveResponse(currentSave.Revision, latestSave, currentSave.UpdatedAtUtc));
        }

        //
        // 最低限の整合性チェック
        //
        SaveData? oldSave = JsonSerializer.Deserialize<SaveData>(currentSave.SaveJson);

        if (oldSave is null)
        {
            return Results.Problem("セーブデータを読み込めませんでした。");
        }

        // 新しいVersionのセーブを
        // 古いClientで上書きさせない
        if (saveRequest.Save.Version < oldSave.Version)
        {
            return Results.Conflict(new ServerSaveResponse(currentSave.Revision, oldSave, currentSave.UpdatedAtUtc));
        }

        if (saveRequest.Save.TotalPotato < oldSave.TotalPotato || saveRequest.Save.BestProductionPerSecond < oldSave.BestProductionPerSecond || saveRequest.Save.ReplantCount < oldSave.ReplantCount)
        {
            return Results.BadRequest(new ApiErrorResponse("セーブデータの整合性を確認できませんでした。"));
        }

        if (oldSave.Version >= 5 && saveRequest.Save.Version >= 5)
        {
            if (saveRequest.Save.AscentCount < oldSave.AscentCount
                || saveRequest.Save.TotalRootEarned < oldSave.TotalRootEarned
                || saveRequest.Save.RootAbundanceLevel < oldSave.RootAbundanceLevel
                || saveRequest.Save.RootFertilityLevel < oldSave.RootFertilityLevel
                || saveRequest.Save.RootRetillLevel < oldSave.RootRetillLevel
                || saveRequest.Save.RootSeedBlessingLevel < oldSave.RootSeedBlessingLevel)
            {
                return Results.BadRequest(new ApiErrorResponse("ASCENT進行の整合性を確認できませんでした。"));
            }

            if (oldSave.AutoBuyUnlocked && !saveRequest.Save.AutoBuyUnlocked)
            {
                return Results.BadRequest(new ApiErrorResponse("AUTO BUYの解放状態を確認できませんでした。"));
            }

            if (oldSave.AutoRetillUnlocked && !saveRequest.Save.AutoRetillUnlocked)
            {
                return Results.BadRequest(new ApiErrorResponse("AUTO RETILLの解放状態を確認できませんでした。"));
            }
        }


        //
        // 保存成功
        //
        currentSave.Revision++;
        currentSave.SaveJson = JsonSerializer.Serialize(saveRequest.Save);
        currentSave.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await rankingService.UpdateFromSaveAsync(user, saveRequest.Save);
        await db.SaveChangesAsync();

        return Results.Ok(new ServerSaveResponse(currentSave.Revision, saveRequest.Save, currentSave.UpdatedAtUtc));
    });

app.MapDelete("/api/save",
    async (HttpRequest request, AppDbContext db, SessionService sessionService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        GameSave? gameSave = await db.GameSaves.SingleOrDefaultAsync(save => save.UserId == user.Id);

        if (gameSave is not null)
        {
            db.GameSaves.Remove(gameSave);
        }

        // 実績由来の称号も初期化
        user.EquippedTitleId = null;

        await db.SaveChangesAsync();

        return Results.NoContent();
    });

app.MapGet("/api/rankings/{category}",
    async (string category, HttpRequest request, SessionService sessionService, RankingService rankingService) =>
    {
        if (!Enum.TryParse(category, ignoreCase: true, out RankingCategory rankingCategory))
        {
            return Results.BadRequest(new ApiErrorResponse("ランキング部門が不正です。"));
        }

        UserAccount? currentUser = await sessionService.GetCurrentUserAsync(request);
        RankingResponse ranking = await rankingService.GetRankingAsync(rankingCategory, currentUser?.Id);

        return Results.Ok(ranking);
    });

app.MapGet("/api/profile/titles",
    async (HttpRequest request, AppDbContext db, SessionService sessionService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        GameSave? gameSave = await db.GameSaves.SingleOrDefaultAsync(save => save.UserId == user.Id);

        SaveData? saveData = null;

        if (gameSave is not null)
        {
            saveData = JsonSerializer.Deserialize<SaveData>(gameSave.SaveJson);
        }

        List<TitleOptionResponse> titles = TitleCatalog.All
                .Select(title =>
                {
                    bool unlocked = saveData?.AchievementUnlockedAtUtc?.ContainsKey(title.AchievementId) ?? false;
                    bool equipped = user.EquippedTitleId == title.Id;

                    return new TitleOptionResponse(title.Id, title.Name, unlocked, equipped);
                })
                .ToList();

        return Results.Ok(new TitleSettingsResponse(user.EquippedTitleId, titles));
    });

app.MapPut("/api/profile/title",
    async (EquipTitleRequest equipRequest, HttpRequest request, AppDbContext db, SessionService sessionService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        //
        // 称号解除
        //
        if (string.IsNullOrWhiteSpace(equipRequest.TitleId))
        {
            user.EquippedTitleId = null;

            await db.SaveChangesAsync();

            return Results.NoContent();
        }

        TitleDefinition? title = TitleCatalog.All.FirstOrDefault(title => title.Id == equipRequest.TitleId);

        if (title is null)
        {
            return Results.BadRequest(new ApiErrorResponse("存在しない称号です。"));
        }

        GameSave? gameSave = await db.GameSaves.SingleOrDefaultAsync(save => save.UserId == user.Id);

        if (gameSave is null)
        {
            return Results.BadRequest(new ApiErrorResponse("サーバーセーブがありません。"));
        }

        SaveData? saveData = JsonSerializer.Deserialize<SaveData>(gameSave.SaveJson);

        if (saveData is null)
        {
            return Results.Problem("セーブデータを読み込めませんでした。");
        }

        bool unlocked = saveData.AchievementUnlockedAtUtc.ContainsKey(title.AchievementId);

        if (!unlocked)
        {
            return Results.BadRequest(new ApiErrorResponse("まだ解放されていない称号です。"));
        }

        user.EquippedTitleId = title.Id;

        await db.SaveChangesAsync();

        return Results.NoContent();
    });

app.MapGet("/api/profiles/{userId:guid}",
    async (Guid userId, AppDbContext db) =>
    {
        UserAccount? user = await db.UserAccounts.AsNoTracking().SingleOrDefaultAsync(user => user.Id == userId && !user.IsDeleted && !user.IsDisabled);

        if (user is null)
        {
            return Results.NotFound();
        }

        GameSave? gameSave = await db.GameSaves.AsNoTracking().SingleOrDefaultAsync(save => save.UserId == user.Id);

        SaveData? saveData = null;

        if (gameSave is not null)
        {
            saveData = JsonSerializer.Deserialize<SaveData>(gameSave.SaveJson);
        }

        string? equippedTitleName = TitleCatalog.All.FirstOrDefault(title => title.Id == user.EquippedTitleId)?.Name;

        int achievementCount = saveData?.AchievementUnlockedAtUtc?.Count ?? 0;
        int ascentCount = saveData?.AscentCount ?? 0;
        int rootPower = 0;

        if (saveData is not null && saveData.Version >= 5)
        {
            rootPower =
                saveData.RootAbundanceLevel
                + saveData.RootFertilityLevel
                + saveData.RootRetillLevel
                + saveData.RootSeedBlessingLevel
                + (saveData.AutoBuyUnlocked ? 1 : 0)
                + (saveData.AutoRetillUnlocked ? 1 : 0);
        }

        int oimoSpeciesCount = saveData?.OimoDiscoveryCounts?.Count(pair => pair.Value > 0) ?? 0;

        DateTimeOffset? gameStartedAtUtc = saveData is not null && saveData.GameStartedAtUtc != default ? saveData.GameStartedAtUtc : null;

        return Results.Ok(
            new PublicProfileResponse(
                user.Id,
                user.UserName,
                equippedTitleName,
                saveData?.TotalPotato ?? 0,
                saveData?.BestProductionPerSecond ?? 0,
                saveData?.ReplantCount ?? 0,
                ascentCount,
                rootPower,
                achievementCount,
                oimoSpeciesCount,
                gameStartedAtUtc)
        );
    });

app.MapDelete("/api/account",
    async (HttpRequest request, AppDbContext db, SessionService sessionService) =>
    {
        UserAccount? user = await sessionService.GetCurrentUserAsync(request);

        if (user is null)
        {
            return Results.Unauthorized();
        }

        GameSave? gameSave = await db.GameSaves.SingleOrDefaultAsync(save => save.UserId == user.Id);

        SaveData? latestSave = null;

        if (gameSave is not null)
        {
            latestSave = JsonSerializer.Deserialize<SaveData>(gameSave.SaveJson);

            if (latestSave is null)
            {
                return Results.Problem("セーブデータを読み込めませんでした。");
            }

            db.GameSaves.Remove(gameSave);
        }

        List<AuthSession> sessions = await db.AuthSessions.Where(session => session.UserId == user.Id).ToListAsync();

        db.AuthSessions.RemoveRange(sessions);

        user.IsDeleted = true;
        user.EquippedTitleId = null;

        await db.SaveChangesAsync();

        return Results.Ok(new DeleteAccountResponse(latestSave));
    });

app.MapGet("/api/admin/users",
    async (HttpRequest request, AppDbContext db, AdminAuthService adminAuth) =>
    {
        if (!adminAuth.IsAuthorized(request))
        {
            return Results.Unauthorized();
        }

        List<AdminUserResponse> users = (await db.UserAccounts
            .AsNoTracking()
            .Select(
                user =>
                    new AdminUserResponse(
                        user.Id,
                        user.UserName,
                        user.CreatedAtUtc,
                        user.LastLoginAtUtc,
                        user.IsDisabled,
                        user.IsDeleted))
            .ToListAsync())
            .OrderBy(user => user.CreatedAtUtc)
            .ToList();

        return Results.Ok(users);
    });

app.MapPut("/api/admin/users/{userId:guid}/pin",
    async (
        Guid userId,
        AdminResetPinRequest resetRequest,
        HttpRequest request,
        AppDbContext db,
        AdminAuthService adminAuth,
        IPasswordHasher<UserAccount>
            passwordHasher) =>
    {
        if (!adminAuth.IsAuthorized(request))
        {
            return Results.Unauthorized();
        }

        string newPin = resetRequest.NewPin ?? string.Empty;

        bool validPin = newPin.Length == 4 && newPin.All(c => c is >= '0' and <= '9');

        if (!validPin)
        {
            return Results.BadRequest(new ApiErrorResponse("PINは数字4桁で入力してください。"));
        }

        UserAccount? user = await db.UserAccounts.SingleOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            return Results.NotFound();
        }

        if (user.IsDeleted)
        {
            return Results.BadRequest(new ApiErrorResponse("削除済みアカウントです。"));
        }

        user.PinHash =
            passwordHasher.HashPassword(user, newPin);

        //
        // PIN変更後は全端末をログアウト
        //
        List<AuthSession> sessions = await db.AuthSessions.Where(session => session.UserId == user.Id).ToListAsync();

        db.AuthSessions.RemoveRange(sessions);

        await db.SaveChangesAsync();

        return Results.NoContent();
    });

app.MapPut("/api/admin/users/{userId:guid}/disabled",
    async (
        Guid userId,
        AdminSetDisabledRequest disableRequest,
        HttpRequest request,
        AppDbContext db,
        AdminAuthService adminAuth) =>
    {
        if (!adminAuth.IsAuthorized(request))
        {
            return Results.Unauthorized();
        }

        UserAccount? user = await db.UserAccounts.SingleOrDefaultAsync(user => user.Id == userId);

        if (user is null)
        {
            return Results.NotFound();
        }

        if (user.IsDeleted)
        {
            return Results.BadRequest(new ApiErrorResponse("削除済みアカウントです。"));
        }

        user.IsDisabled = disableRequest.IsDisabled;

        //
        // 無効化するときは
        // 現在の全セッションも切る
        //
        if (disableRequest.IsDisabled)
        {
            List<AuthSession> sessions = await db.AuthSessions.Where(session => session.UserId == user.Id).ToListAsync();
            db.AuthSessions.RemoveRange(sessions);
        }

        await db.SaveChangesAsync();

        return Results.NoContent();
    });

app.Run();

static bool IsValidAscentProgress(SaveData save)
{
    if (save.Version < 5)
    {
        return true;
    }

    if (save.AscentCount < 0
        || save.TotalRootEarned < 0
        || save.CurrentRoot < 0)
    {
        return false;
    }

    if (save.RootAbundanceLevel is < 0 or > 30)
    {
        return false;
    }

    if (save.RootFertilityLevel is < 0 or > 20)
    {
        return false;
    }

    if (save.RootRetillLevel is < 0 or > 24)
    {
        return false;
    }

    if (save.RootSeedBlessingLevel is < 0 or > 24)
    {
        return false;
    }

    if (save.AutoBuyEnabled && !save.AutoBuyUnlocked)
    {
        return false;
    }

    if (save.AutoRetillEnabled && !save.AutoRetillUnlocked)
    {
        return false;
    }

    int usedRoot =
        save.RootAbundanceLevel
        + save.RootFertilityLevel
        + save.RootRetillLevel
        + save.RootSeedBlessingLevel
        + (save.AutoBuyUnlocked ? 1 : 0)
        + (save.AutoRetillUnlocked ? 1 : 0);

    return save.TotalRootEarned == save.AscentCount && save.CurrentRoot + usedRoot == save.TotalRootEarned;
}