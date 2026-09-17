using Microsoft.EntityFrameworkCore;
using OimoHorihori.Server.Data;
using OimoHorihori.Server.Models;
using OimoHorihori.Server.Security;

namespace OimoHorihori.Server.Services;

public class SessionService
{
    private readonly AppDbContext db;

    public SessionService(AppDbContext db)
    {
        this.db = db;
    }

    public async Task<UserAccount?> GetCurrentUserAsync(HttpRequest request)
    {
        string authorization = request.Headers.Authorization.ToString();

        const string prefix = "Bearer ";

        if (!authorization.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string token = authorization[prefix.Length..].Trim();

        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        string tokenHash = SessionTokenHelper.HashToken(token);

        DateTimeOffset now = DateTimeOffset.UtcNow;

        AuthSession? session = await db.AuthSessions.SingleOrDefaultAsync(s => s.TokenHash == tokenHash);

        if (session is null)
        {
            return null;
        }

        if (session.ExpiresAtUtc <= now)
        {
            db.AuthSessions.Remove(session);
            await db.SaveChangesAsync();

            return null;
        }

        if (session is null)
        {
            return null;
        }

        UserAccount? user = await db.UserAccounts.SingleOrDefaultAsync(u => u.Id == session.UserId && !u.IsDisabled && !u.IsDeleted);

        return user;
    }
}