using OimoHorihori.Server.Models;
using OimoHorihori.Server.Services;
using OimoHorihori.Shared.Auth;
using OimoHorihori.Shared.Rankings;

namespace OimoHorihori.Server.Endpoints;

public static class RankingEndpoints
{
    public static void MapRankingEndpoints(this WebApplication app)
    {
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
    }
}