namespace OimoHorihori.Pages;

public partial class Home
{
    private enum HomeView
    {
        Horihori,
        Menu,
        SeedUpgrades,
        Ascent,
        AscentHistory,
        Statistics,
        Achievements,
        OimoBook,
        ReplantHistory,
        Ranking,
        Profile,
        PublicProfile,
        Account,
        Settings
    }

    private HomeView currentView = HomeView.Horihori;

    private void OpenHorihori()
    {
        shareResultMessage = null;

        currentView = HomeView.Horihori;
    }

    private void OpenMenu()
    {
        currentView = HomeView.Menu;
    }

    private void OpenSeedUpgrades()
    {
        currentView = HomeView.SeedUpgrades;
    }

    private void OpenStatistics()
    {
        currentView = HomeView.Statistics;
    }

    private void OpenAchievements()
    {
        currentView = HomeView.Achievements;
    }

    private void OpenOimoBook()
    {
        currentView = HomeView.OimoBook;
    }

    private void OpenSettings()
    {
        currentView = HomeView.Settings;
    }

    private void OpenAccount()
    {
        currentView = HomeView.Account;
    }

    private void OpenReplantHistory()
    {
        currentView = HomeView.ReplantHistory;
    }

    private void OpenAscent()
    {
        currentView = HomeView.Ascent;
    }

    private void OpenAscentHistory()
    {
        currentView = HomeView.AscentHistory;
    }

    private void BackToRanking()
    {
        currentView = HomeView.Ranking;
    }

    private string GetBottomNavClass(bool horihori)
    {
        bool selected = horihori ? currentView == HomeView.Horihori : currentView != HomeView.Horihori;

        return selected ? "bottom-nav-button selected" : "bottom-nav-button";
    }
}