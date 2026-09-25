using OimoHorihori.Constants;
using OimoHorihori.Utilities;

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
                total = GameMath.Add(total, GetFarmProductionPerSecond(farm));
            }

            total = GameMath.Multiply(total, ProductionMultiplier);
            total = GameMath.Multiply(total, RootAbundanceMultiplier);
            total = GameMath.Multiply(total, OimoAllProductionMultiplier);

            return total;
        }
    }

    public void ProducePotato(double amount)
    {
        if (!double.IsFinite(amount)
            || amount <= 0)
        {
            return;
        }

        Potato = GameMath.Add(Potato, amount);
        TotalPotato = GameMath.Add(TotalPotato, amount);
        RunProducedPotato = GameMath.Add(RunProducedPotato, amount);

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