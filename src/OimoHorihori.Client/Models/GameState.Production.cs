using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public partial class GameState
{
    public double BaseProductionPerSecond => GameConstants.BaseProductionPerSecond + DigUpgradeLevel * GameConstants.DigUpgradeBonusPerLevel;

    public double ProductionMultiplier => 1.0 + ProductionUpgradeLevel * GameConstants.ProductionUpgradeBonusPerLevel;

    public double ProductionPerSecond
    {
        get
        {
            double total = BaseProductionPerSecond;

            foreach (Farm farm in Farms)
            {
                total += GetFarmProductionPerSecond(farm);
            }

            return total * ProductionMultiplier * RootAbundanceMultiplier * OimoAllProductionMultiplier;
        }
    }

    public void ProducePotato(double amount)
    {
        if (!double.IsFinite(amount) || amount <= 0)
        {
            return;
        }

        Potato += amount;
        TotalPotato += amount;
        RunProducedPotato += amount;

        MaxRunProducedPotato = Math.Max(MaxRunProducedPotato, RunProducedPotato);
    }

    public void ProduceForSeconds(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0)
        {
            return;
        }

        double amount = ProductionPerSecond * seconds;

        ProducePotato(amount);
    }

    public double ProduceOffline(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0)
        {
            return 0;
        }

        double cappedSeconds = Math.Min(seconds, OfflineLimitSeconds);

        double amount = ProductionPerSecond * cappedSeconds;

        ProducePotato(amount);

        TotalOfflineProducedPotato += amount;

        MaxOfflineProducedPotato = Math.Max(MaxOfflineProducedPotato, amount);

        return amount;
    }

    public double OfflineLimitSeconds
    {
        get
        {
            double seconds = GameConstants.InitialOfflineLimitSeconds + OfflineUpgradeLevel * GameConstants.OfflineUpgradeSecondsPerLevel;

            return Math.Min(seconds, GameConstants.MaxOfflineLimitSeconds);
        }
    }
}