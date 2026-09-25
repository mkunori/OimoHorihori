using OimoHorihori.Shared.Profiles;
using OimoHorihori.Shared.Rankings;

namespace OimoHorihori.Pages;

public partial class Home
{
    private RankingCategory rankingCategory = RankingCategory.TotalPotato;
    private RankingResponse? rankingResponse;
    private bool isRankingLoading;
    private string? rankingError;
    private PublicProfileResponse? selectedPublicProfile;
    private bool isPublicProfileLoading;
    private string? publicProfileError;

    private async Task OpenRankingAsync()
    {
        currentView = HomeView.Ranking;

        await LoadRankingAsync(rankingCategory);
    }

    private async Task LoadRankingAsync(RankingCategory category)
    {
        rankingCategory = category;
        isRankingLoading = true;
        rankingError = null;

        try
        {
            rankingResponse = await Rankings.GetAsync(category);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ranking load failed: {ex}");

            rankingError = "ランキングを読み込めませんでした。";
        }
        finally
        {
            isRankingLoading = false;
        }
    }

    private async Task OpenPublicProfileAsync(Guid userId)
    {
        currentView = HomeView.PublicProfile;

        selectedPublicProfile = null;
        publicProfileError = null;
        isPublicProfileLoading = true;

        try
        {
            selectedPublicProfile = await Profiles.GetPublicProfileAsync(userId);

            if (selectedPublicProfile is null)
            {
                publicProfileError = "プロフィールが見つかりませんでした。";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Public profile load failed: {ex}");

            publicProfileError = "プロフィールを読み込めませんでした。";
        }
        finally
        {
            isPublicProfileLoading = false;
        }
    }
}