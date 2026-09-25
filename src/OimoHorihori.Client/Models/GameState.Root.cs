using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public partial class GameState
{
    public int UsedRoot => RootAbundanceLevel + RootFertilityLevel + RootRetillLevel + RootSeedBlessingLevel + (AutoBuyUnlocked ? 1 : 0) + (AutoRetillUnlocked ? 1 : 0);
    public int RootPower => UsedRoot;
    public double RootAbundanceMultiplier => GetRootAbundanceMultiplier(RootAbundanceLevel);
    public double RootFertilityMultiplier => GetRootFertilityMultiplier(RootFertilityLevel);
    public double RootAdjustedRetillBase => GetRootAdjustedRetillBase(RootRetillLevel);
    public double RootSeedBlessingMultiplier => GetRootSeedBlessingMultiplier(RootSeedBlessingLevel);
    public bool IsRootAbundanceMax => RootAbundanceLevel >= GameConstants.RootAbundanceMaxLevel;
    public bool IsRootFertilityMax => RootFertilityLevel >= GameConstants.RootFertilityMaxLevel;
    public bool IsRootRetillMax => RootRetillLevel >= GameConstants.RootRetillMaxLevel;
    public bool IsRootSeedBlessingMax => RootSeedBlessingLevel >= GameConstants.RootSeedBlessingMaxLevel;

    private bool TrySpendRoot(int cost)
    {
        if (cost <= 0 || CurrentRoot < cost)
        {
            return false;
        }

        CurrentRoot -= cost;

        return true;
    }

    public bool BuyRootAbundanceUpgrade()
    {
        if (IsRootAbundanceMax)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootAbundanceLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyRootFertilityUpgrade()
    {
        if (IsRootFertilityMax)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootFertilityLevel++;

        return true;
    }

    public bool BuyRootRetillUpgrade()
    {
        if (IsRootRetillMax)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootRetillLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyRootSeedBlessingUpgrade()
    {
        if (IsRootSeedBlessingMax)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootSeedBlessingLevel++;

        return true;
    }

    public double GetRootAbundanceMultiplier(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootAbundanceMaxLevel);

        return Math.Pow(GameConstants.RootAbundanceMultiplierPerLevel, safeLevel);
    }

    public double GetRootFertilityMultiplier(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootFertilityMaxLevel);

        return 1.0 - safeLevel * GameConstants.RootFertilityReductionPerLevel;
    }

    public double GetRootAdjustedRetillBase(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootRetillMaxLevel);

        return GameConstants.RetillProductionMultiplier + safeLevel * GameConstants.RootRetillBaseBonusPerLevel;
    }

    public double GetRootSeedBlessingMultiplier(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootSeedBlessingMaxLevel);

        return Math.Pow(GameConstants.RootSeedBlessingMultiplierPerLevel, safeLevel);
    }
}