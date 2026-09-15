using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public class Farm
{
    public string Name { get; }

    public int Level { get; set; }

    public double BaseCost { get; }

    public double ProductionPerLevel { get; }

    public Farm(
        string name,
        double baseCost,
        double productionPerLevel)
    {
        Name = name;
        BaseCost = baseCost;
        ProductionPerLevel =
            productionPerLevel;
    }

    public double GetCostAtLevel(
        int level)
    {
        double cost =
            BaseCost *
            Math.Pow(
                GameConstants
                    .FarmCostMultiplier,
                level);

        return Math.Ceiling(cost);
    }

    public double NextCost =>
        GetCostAtLevel(Level);

    public double GetCostForLevels(
        int levels)
    {
        if (levels <= 0)
        {
            return 0;
        }

        double totalCost = 0;

        for (int i = 0;
            i < levels;
            i++)
        {
            double cost =
                GetCostAtLevel(
                    Level + i);

            if (!double.IsFinite(cost))
            {
                return
                    double.PositiveInfinity;
            }

            totalCost += cost;

            if (!double.IsFinite(totalCost))
            {
                return
                    double.PositiveInfinity;
            }
        }

        return totalCost;
    }

    public int GetAffordableLevels(
        double potato,
        int maxLevels)
    {
        if (!double.IsFinite(potato)
            || potato < 0
            || maxLevels <= 0)
        {
            return 0;
        }

        double remaining =
            potato;

        int affordableLevels = 0;

        while (affordableLevels
            < maxLevels)
        {
            double cost =
                GetCostAtLevel(
                    Level
                    + affordableLevels);

            if (!double.IsFinite(cost)
                || remaining < cost)
            {
                break;
            }

            remaining -= cost;

            affordableLevels++;
        }

        return affordableLevels;
    }

    public double ProductionPerSecond =>
        ProductionPerLevel * Level;
}