using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Services;
using OimoHorihori.Shared.Auth;
using OimoHorihori.Shared.Profiles;
using OimoHorihori.Shared.Saves;
using System.Text.Json;

namespace OimoHorihori.Server.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this WebApplication app)
    {
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
                        gameStartedAtUtc));
            });
    }
}