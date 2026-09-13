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
        ProductionPerLevel = productionPerLevel;
    }

    public double NextCost
    {
        get
        {
            double cost =
                BaseCost *
                Math.Pow(
                    GameConstants.FarmCostMultiplier,
                    Level);

            return Math.Ceiling(cost);
        }
    }

    public double ProductionPerSecond =>
        ProductionPerLevel * Level;
}