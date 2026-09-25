using OimoHorihori.Constants;
using OimoHorihori.Utilities;

namespace OimoHorihori.Models;

public partial class GameState
{
    public int RetillEfficiencyUpgradeCost => GetUpgradeCost(RetillEfficiencyUpgradeLevel);

    public bool IsRetillUnlockedThisRun => Farms.Count >= 8 && Farms[7].PurchaseCount > 0;

    public bool CanRetillFarm(Farm farm)
    {
        return IsRetillUnlockedThisRun && farm.CanRetill;
    }

    public bool RetillFarm(Farm farm)
    {
        if (!CanRetillFarm(farm))
        {
            return false;
        }

        bool retilled = farm.TryRetill();

        if (!retilled)
        {
            return false;
        }

        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public double GetRetillBaseMultiplier(Farm farm)
    {
        return GetRetillBaseMultiplier(farm.RetillCount);
    }

    public double GetRetillBaseMultiplier(int retillCount)
    {
        if (retillCount < 0)
        {
            return 1.0;
        }

        return GameMath.Pow(RootAdjustedRetillBase, retillCount);
    }

    public double GetRetillEfficiencyMultiplier(Farm farm)
    {
        double bonus = farm.RetillCount * (double)RetillEfficiencyUpgradeLevel * GameConstants.RetillEfficiencyBonusPerLevelPerRetill;

        if (!double.IsFinite(bonus))
        {
            return double.MaxValue;
        }

        return GameMath.Add(1.0, bonus);
    }

    public bool BuyRetillEfficiencyUpgrade()
    {
        int cost = RetillEfficiencyUpgradeCost;

        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        RetillEfficiencyUpgradeLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }
}