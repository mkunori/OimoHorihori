using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Services;
using OimoHorihori.Shared.Accounts;
using OimoHorihori.Shared.Saves;
using System.Text.Json;

namespace OimoHorihori.Server.Endpoints;

public static class AccountEndpoints
{
    public static void MapAccountEndpoints(this WebApplication app)
    {
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
    }
}