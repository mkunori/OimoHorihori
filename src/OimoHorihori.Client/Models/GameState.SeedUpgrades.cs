using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public partial class GameState
{
    public int ProductionUpgradeCost => GetUpgradeCost(ProductionUpgradeLevel);
    public int DigUpgradeCost => GetUpgradeCost(DigUpgradeLevel);
    public int OfflineUpgradeCost => GetUpgradeCost(OfflineUpgradeLevel);
    public bool IsOfflineUpgradeMax => OfflineUpgradeLevel >= GameConstants.OfflineUpgradeMaxLevel;
    public int FieldCostReductionUpgradeCost => GetUpgradeCost(FieldCostReductionUpgradeLevel);
    public bool IsFieldCostReductionUpgradeMax => FieldCostReductionUpgradeLevel >= GameConstants.FieldCostReductionMaxLevel;
    public double FieldCostMultiplier => SeedFieldCostMultiplier * RootFertilityMultiplier * OimoFieldCostMultiplier;

    private static int GetUpgradeCost(int currentLevel)
    {
        int nextLevel = currentLevel + 1;

        double cost = (double)nextLevel * (nextLevel + 1) / 3;

        return (int)Math.Round(cost);
    }

    private bool TrySpendSeedPotato(int cost)
    {
        if (cost <= 0 || SeedPotato < cost)
        {
            return false;
        }

        SeedPotato -= cost;
        TotalSeedPotatoSpent += cost;

        return true;
    }

    public bool BuyProductionUpgrade()
    {
        int cost =
            ProductionUpgradeCost;

        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        ProductionUpgradeLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyDigUpgrade()
    {
        int cost = DigUpgradeCost;

        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        DigUpgradeLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyOfflineUpgrade()
    {
        if (IsOfflineUpgradeMax)
        {
            return false;
        }

        if (!TrySpendSeedPotato(OfflineUpgradeCost))
        {
            return false;
        }

        OfflineUpgradeLevel++;

        return true;
    }

    public bool BuyFieldCostReductionUpgrade()
    {
        if (IsFieldCostReductionUpgradeMax)
        {
            return false;
        }

        int cost = FieldCostReductionUpgradeCost;

        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        FieldCostReductionUpgradeLevel++;

        return true;
    }

    public double SeedFieldCostMultiplier
    {
        get
        {
            double reduction = FieldCostReductionUpgradeLevel * GameConstants.FieldCostReductionPerLevel;

            reduction = Math.Min(reduction, 0.40);

            return 1.0 - reduction;
        }
    }
}