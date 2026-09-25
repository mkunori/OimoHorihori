using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Security;
using OimoHorihori.Server.Services;
using OimoHorihori.Shared.Auth;

namespace OimoHorihori.Server.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
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

                DateTimeOffset expiresAtUtc =
                    now.AddDays(30);

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
    }
}