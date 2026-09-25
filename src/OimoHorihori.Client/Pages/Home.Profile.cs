using OimoHorihori.Shared.Profiles;

namespace OimoHorihori.Pages;

public partial class Home
{
    private TitleSettingsResponse? titleSettings;
    private bool isProfileLoading;
    private string? profileError;

    private async Task OpenProfileAsync()
    {
        currentView = HomeView.Profile;

        if (currentUser is null)
        {
            return;
        }

        await LoadProfileTitlesAsync();
    }

    private async Task LoadProfileTitlesAsync()
    {
        isProfileLoading = true;
        profileError = null;

        try
        {
            titleSettings = await Profiles.GetTitlesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Profile load failed: {ex}");

            profileError = "プロフィールを読み込めませんでした。";
        }
        finally
        {
            isProfileLoading = false;
        }
    }

    private async Task EquipTitleAsync(string? titleId)
    {
        try
        {
            await Profiles.EquipTitleAsync(titleId);

            titleSettings = await Profiles.GetTitlesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Title update failed: {ex}");

            profileError = "称号を変更できませんでした。";
        }
    }
}