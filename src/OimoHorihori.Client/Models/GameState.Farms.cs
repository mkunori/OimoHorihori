namespace OimoHorihori.Models;

public partial class GameState
{
    public int BuyFarm(Farm farm, int maxLevels)
    {
        int levels = farm.GetAffordableLevels(Potato, maxLevels, FieldCostMultiplier);

        if (levels <= 0)
        {
            return 0;
        }

        double cost = farm.GetCostForLevels(levels, FieldCostMultiplier);

        if (!double.IsFinite(cost) || cost > Potato)
        {
            return 0;
        }

        Potato -= cost;
        TotalConsumedPotato += cost;
        farm.Level += levels;
        farm.PurchaseCount += levels;

        UpdateFarmStatistics(farm, levels);

        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return levels;
    }

    private void UpdateFarmStatistics(Farm farm, int purchasedLevels)
    {
        switch (Farms.IndexOf(farm))
        {
            case 0:
                Farm1TotalPurchases += purchasedLevels;
                Farm1BestLevel = Math.Max(Farm1BestLevel, farm.Level);
                break;

            case 1:
                Farm2TotalPurchases += purchasedLevels;
                Farm2BestLevel = Math.Max(Farm2BestLevel, farm.Level);
                break;

            case 2:
                Farm3TotalPurchases += purchasedLevels;
                Farm3BestLevel = Math.Max(Farm3BestLevel, farm.Level);
                break;

            case 3:
                Farm4TotalPurchases += purchasedLevels;
                Farm4BestLevel = Math.Max(Farm4BestLevel, farm.Level);
                break;

            case 4:
                Farm5TotalPurchases += purchasedLevels;
                Farm5BestLevel = Math.Max(Farm5BestLevel, farm.Level);
                break;

            case 5:
                Farm6TotalPurchases += purchasedLevels;
                Farm6BestLevel = Math.Max(Farm6BestLevel, farm.Level);
                break;

            case 6:
                Farm7TotalPurchases += purchasedLevels;
                Farm7BestLevel = Math.Max(Farm7BestLevel, farm.Level);
                break;

            case 7:
                Farm8TotalPurchases += purchasedLevels;
                Farm8BestLevel = Math.Max(Farm8BestLevel, farm.Level);
                break;
        }
    }

    public double GetFarmProductionPerSecond(Farm farm)
    {
        double farmBase = farm.BaseProductionPerSecond;
        double retillMultiplier = GetRetillBaseMultiplier(farm);
        double seedRetillEfficiencyMultiplier = GetRetillEfficiencyMultiplier(farm);

        return farmBase * retillMultiplier * seedRetillEfficiencyMultiplier * OimoFarmProductionMultiplier * GetOimoRetillFinalMultiplier(farm);
    }
}