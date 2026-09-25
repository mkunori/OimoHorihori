using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Services;
using OimoHorihori.Shared.Auth;
using OimoHorihori.Shared.Saves;
using System.Text.Json;

namespace OimoHorihori.Server.Endpoints;

public static class SaveEndpoints
{
    public static void MapSaveEndpoints(this WebApplication app)
    {
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

                    GameSave newSave = new()
                    {
                        UserId = user.Id,
                        Revision = 1,
                        SaveJson = JsonSerializer.Serialize(saveRequest.Save),
                        UpdatedAtUtc = now
                    };

                    db.GameSaves.Add(newSave);

                    await rankingService.UpdateFromSaveAsync(user, saveRequest.Save);
                    await db.SaveChangesAsync();

                    return Results.Ok(new ServerSaveResponse(newSave.Revision, saveRequest.Save, now));
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

                //
                // 新しいVersionのセーブを
                // 古いClientで上書きさせない
                //
                if (saveRequest.Save.Version < oldSave.Version)
                {
                    return Results.Conflict(new ServerSaveResponse(currentSave.Revision, oldSave, currentSave.UpdatedAtUtc));
                }

                if (saveRequest.Save.TotalPotato < oldSave.TotalPotato
                    || saveRequest.Save.BestProductionPerSecond < oldSave.BestProductionPerSecond
                    || saveRequest.Save.ReplantCount < oldSave.ReplantCount)
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

                //
                // 実績由来の称号も初期化
                //
                user.EquippedTitleId = null;

                await db.SaveChangesAsync();

                return Results.NoContent();
            });
    }

    private static bool IsValidAscentProgress(SaveData save)
    {
        if (save.Version < 5)
        {
            return true;
        }

        if (save.AscentCount < 0 || save.TotalRootEarned < 0 || save.CurrentRoot < 0)
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

        int usedRoot = save.RootAbundanceLevel + save.RootFertilityLevel + save.RootRetillLevel + save.RootSeedBlessingLevel + (save.AutoBuyUnlocked ? 1 : 0) + (save.AutoRetillUnlocked ? 1 : 0);

        return save.TotalRootEarned == save.AscentCount && save.CurrentRoot + usedRoot == save.TotalRootEarned;
    }
}