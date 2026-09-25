using OimoHorihori.Models;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Pages;

public partial class Home
{
    private bool showAscentConfirmation;
    private bool showAscentResult;
    private AscentHistoryEntry? lastAscentResult;

    private async Task UnlockAutoBuyAsync()
    {
        if (!game.UnlockAutoBuy())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task UnlockAutoRetillAsync()
    {
        if (!game.UnlockAutoRetill())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task ToggleAutoBuyAsync()
    {
        if (!game.AutoBuyUnlocked)
        {
            return;
        }

        if (!game.SetAutoBuyEnabled(!game.AutoBuyEnabled))
        {
            return;
        }

        lastAutoActionTime = DateTimeOffset.UtcNow;

        await SaveGameAsync();
    }

    private async Task ToggleAutoRetillAsync()
    {
        if (!game.AutoRetillUnlocked)
        {
            return;
        }

        if (!game.SetAutoRetillEnabled(!game.AutoRetillEnabled))
        {
            return;
        }

        lastAutoActionTime = DateTimeOffset.UtcNow;

        await SaveGameAsync();
    }

    private async Task BuyRootAbundanceAsync()
    {
        if (!game.BuyRootAbundanceUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyRootFertilityAsync()
    {
        if (!game.BuyRootFertilityUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyRootRetillAsync()
    {
        if (!game.BuyRootRetillUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyRootSeedBlessingAsync()
    {
        if (!game.BuyRootSeedBlessingUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private void OpenAscentConfirmation()
    {
        if (!game.CanAscent)
        {
            return;
        }

        showAscentConfirmation = true;
    }

    private void CancelAscent()
    {
        showAscentConfirmation = false;
    }

    private async Task ExecuteAscentAsync()
    {
        if (!game.CanAscent)
        {
            showAscentConfirmation = false;

            return;
        }

        bool ascended =
            game.Ascent();

        if (!ascended)
        {
            showAscentConfirmation = false;

            return;
        }

        lastAscentResult = game.AscentHistory.FirstOrDefault();
        showAscentConfirmation = false;
        showAscentResult = true;
        purchaseMode = PurchaseMode.One;

        DateTimeOffset now = DateTimeOffset.UtcNow;

        ResetRuntimeTimestamps(now);
        CheckAchievements();

        await SaveGameAsync();
    }

    private void CloseAscentResult()
    {
        showAscentResult = false;
        lastAscentResult = null;

        currentView = HomeView.Horihori;
    }
}