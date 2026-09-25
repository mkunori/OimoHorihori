using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Security;
using OimoHorihori.Shared.Admin;
using OimoHorihori.Shared.Auth;

namespace OimoHorihori.Server.Endpoints;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(
        this WebApplication app)
    {
        app.MapGet("/api/admin/users",
            async (HttpRequest request, AppDbContext db, AdminAuthService adminAuth) =>
            {
                if (!adminAuth.IsAuthorized(request))
                {
                    return Results.Unauthorized();
                }

                List<AdminUserResponse> users =
                    (await db.UserAccounts
                        .AsNoTracking()
                        .Select(user => new AdminUserResponse(user.Id, user.UserName, user.CreatedAtUtc, user.LastLoginAtUtc, user.IsDisabled, user.IsDeleted))
                        .ToListAsync())
                    .OrderBy(user => user.CreatedAtUtc)
                    .ToList();

                return Results.Ok(users);
            });

        app.MapPut(
            "/api/admin/users/{userId:guid}/pin",
            async (Guid userId, AdminResetPinRequest resetRequest, HttpRequest request, AppDbContext db, AdminAuthService adminAuth, IPasswordHasher<UserAccount> passwordHasher) =>
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

                user.PinHash = passwordHasher.HashPassword(user, newPin);

                //
                // PIN変更後は全端末をログアウト
                //
                List<AuthSession> sessions = await db.AuthSessions.Where(session => session.UserId == user.Id).ToListAsync();

                db.AuthSessions.RemoveRange(sessions);

                await db.SaveChangesAsync();

                return Results.NoContent();
            });

        app.MapPut(
            "/api/admin/users/{userId:guid}/disabled",
            async (Guid userId, AdminSetDisabledRequest disableRequest, HttpRequest request, AppDbContext db, AdminAuthService adminAuth) =>
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
                    return Results.BadRequest(
                        new ApiErrorResponse("削除済みアカウントです。"));
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
    }
}