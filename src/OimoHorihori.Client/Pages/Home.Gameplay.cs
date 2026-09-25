using OimoHorihori.Models;

namespace OimoHorihori.Pages;

public partial class Home
{
    private PurchaseMode purchaseMode = PurchaseMode.One;

    private void SetPurchaseMode(PurchaseMode mode)
    {
        purchaseMode = mode;
    }

    private int GetPurchaseMaxLevels()
    {
        return purchaseMode switch
        {
            PurchaseMode.One => 1,
            PurchaseMode.Ten => 10,
            PurchaseMode.Max => int.MaxValue,
            _ => 1
        };
    }

    private async Task BuyFarm(Farm farm)
    {
        int maxLevels = GetPurchaseMaxLevels();
        int purchasedLevels = game.BuyFarm(farm, maxLevels);

        if (purchasedLevels <= 0)
        {
            return;
        }

        if (purchaseMode == PurchaseMode.Ten)
        {
            game.HasUsedTenPurchaseMode = true;
        }

        if (purchaseMode == PurchaseMode.Max)
        {
            game.HasUsedMaxPurchaseMode = true;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyProductionUpgradeAsync()
    {
        if (!game.BuyProductionUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyDigUpgradeAsync()
    {
        if (!game.BuyDigUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyOfflineUpgradeAsync()
    {
        if (!game.BuyOfflineUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task ExecuteRetillAsync(Farm farm)
    {
        if (!game.RetillFarm(farm))
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyRetillEfficiencyUpgradeAsync()
    {
        if (!game.BuyRetillEfficiencyUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }

    private async Task BuyFieldCostReductionUpgradeAsync()
    {
        if (!game.BuyFieldCostReductionUpgrade())
        {
            return;
        }

        CheckAchievements();

        await SaveGameAsync();
    }
}