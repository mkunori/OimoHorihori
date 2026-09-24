using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public class Farm
{
    public string Name { get; }
    public int Level { get; set; }
    public int PurchaseCount { get; set; }
    public int RetillCount { get; set; }
    public double BaseCost { get; }
    public double ProductionPerLevel { get; }
    public int MaxLevel => GameConstants.InitialFarmMaxLevel + RetillCount * GameConstants.RetillLevelCapBonus;
    public bool IsMaxLevel => Level >= MaxLevel;
    public bool IsRetillMax => RetillCount >= GameConstants.RetillMaxCount;
    public bool CanRetill => !IsRetillMax && Level >= MaxLevel;
    public double BaseProductionPerSecond => ProductionPerLevel * Level;

    public Farm(string name, double baseCost, double productionPerLevel)
    {
        Name = name;
        BaseCost = baseCost;
        ProductionPerLevel = productionPerLevel;
    }

    public double GetCostAtPurchaseCount(int purchaseCount, double costMultiplier = 1.0)
    {
        double rawCost = BaseCost * Math.Pow(GameConstants.FarmCostMultiplier, purchaseCount);
        double finalCost = rawCost * costMultiplier;

        return Math.Ceiling(finalCost);
    }

    public double GetCostForLevels(int levels, double costMultiplier = 1.0)
    {
        if (levels <= 0)
        {
            return 0;
        }

        double totalCost = 0;

        for (int i = 0; i < levels; i++)
        {
            double cost = GetCostAtPurchaseCount(PurchaseCount + i, costMultiplier);

            if (!double.IsFinite(cost))
            {
                return double.PositiveInfinity;
            }

            totalCost += cost;

            if (!double.IsFinite(totalCost))
            {
                return double.PositiveInfinity;
            }
        }

        return totalCost;
    }

    public int GetAffordableLevels(double potato, int maxLevels, double costMultiplier = 1.0)
    {
        if (!double.IsFinite(potato) || potato < 0 || maxLevels <= 0)
        {
            return 0;
        }

        int remainingLevels = MaxLevel - Level;
        if (remainingLevels <= 0)
        {
            return 0;
        }

        int purchaseLimit = Math.Min(maxLevels, remainingLevels);

        double remaining = potato;
        int affordableLevels = 0;

        while (affordableLevels < purchaseLimit)
        {
            double cost = GetCostAtPurchaseCount(PurchaseCount + affordableLevels, costMultiplier);

            if (!double.IsFinite(cost) || remaining < cost)
            {
                break;
            }

            remaining -= cost;
            affordableLevels++;
        }

        return affordableLevels;
    }

    public bool TryRetill()
    {
        if (!CanRetill)
        {
            return false;
        }

        Level = 0;
        RetillCount++;

        return true;
    }

    public double GetNextCost(double costMultiplier = 1.0)
    {
        return GetCostAtPurchaseCount(PurchaseCount, costMultiplier);
    }
}