using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool showReplant;
    private bool showReplantConfirmation;

    private void OpenReplant()
    {
        showReplant = true;
    }

    private void CloseReplant()
    {
        showReplant = false;
    }

    private void OpenReplantConfirmation()
    {
        if (!game.CanReplant)
        {
            return;
        }

        showReplant = false;
        showReplantConfirmation = true;
    }

    private void CancelReplant()
    {
        showReplantConfirmation = false;
        showReplant = true;
    }

    private async Task ExecuteReplantAsync()
    {
        int earnedSeedPotato = game.Replant();

        if (earnedSeedPotato <= 0)
        {
            showReplantConfirmation = false;

            return;
        }

        CheckAchievements();

        showReplantConfirmation = false;
        showReplant = false;
        purchaseMode = PurchaseMode.One;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        lastUpdateTime = now;
        lastAutoSaveTime = now;
        lastAutoActionTime = now;

        await SaveGameAsync();
    }
}