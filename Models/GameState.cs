using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public class GameState
{
    public bool HasStarted { get; set; }

    public double Potato { get; set; }

    public double TotalPotato { get; set; }

    public List<Farm> Farms { get; } = new()
    {
        new Farm(
            "畑1",
            10,
            0.1),

        new Farm(
            "畑2",
            1_000,
            2),

        new Farm(
            "畑3",
            100_000,
            50)
    };

    public double ProductionPerSecond
    {
        get
        {
            double total =
                GameConstants.BaseProductionPerSecond;

            foreach (Farm farm in Farms)
            {
                total +=
                    farm.ProductionPerSecond;
            }

            return total;
        }
    }

    public bool CanBuyFarm(Farm farm)
    {
        return
            double.IsFinite(farm.NextCost)
            && Potato >= farm.NextCost;
    }

    public bool BuyFarm(Farm farm)
    {
        if (!CanBuyFarm(farm))
        {
            return false;
        }

        double cost =
            farm.NextCost;

        Potato -= cost;

        farm.Level++;

        return true;
    }

    public void ProducePotato(double amount)
    {
        if (!double.IsFinite(amount)
            || amount <= 0)
        {
            return;
        }

        Potato += amount;
        TotalPotato += amount;
    }

    public void ProduceForSeconds(
        double seconds)
    {
        if (!double.IsFinite(seconds)
            || seconds <= 0)
        {
            return;
        }

        double amount =
            ProductionPerSecond
            * seconds;

        ProducePotato(amount);
    }

    public double ProduceOffline(
        double seconds)
    {
        if (!double.IsFinite(seconds)
            || seconds <= 0)
        {
            return 0;
        }

        double cappedSeconds =
            Math.Min(
                seconds,
                GameConstants
                    .OfflineLimitSeconds);

        double amount =
            ProductionPerSecond
            * cappedSeconds;

        ProducePotato(amount);

        return amount;
    }

    public SaveData CreateSaveData()
    {
        return new SaveData
        {
            Version =
                GameConstants.SaveVersion,

            HasStarted =
                HasStarted,

            Potato =
                Potato,

            TotalPotato =
                TotalPotato,

            Farm1Level =
                Farms[0].Level,

            Farm2Level =
                Farms[1].Level,

            Farm3Level =
                Farms[2].Level,

            LastSaveTimeUtc =
                DateTimeOffset.UtcNow
        };
    }

    public bool TryLoadSaveData(
        SaveData save)
    {
        if (save.Version !=
            GameConstants.SaveVersion)
        {
            return false;
        }

        if (!IsValidSaveData(save))
        {
            return false;
        }

        HasStarted =
            save.HasStarted;

        Potato =
            save.Potato;

        TotalPotato =
            save.TotalPotato;

        Farms[0].Level =
            save.Farm1Level;

        Farms[1].Level =
            save.Farm2Level;

        Farms[2].Level =
            save.Farm3Level;

        return true;
    }

    private static bool IsValidSaveData(
        SaveData save)
    {
        if (!double.IsFinite(save.Potato)
            || save.Potato < 0)
        {
            return false;
        }

        if (!double.IsFinite(
                save.TotalPotato)
            || save.TotalPotato < 0)
        {
            return false;
        }

        if (save.TotalPotato
            < save.Potato)
        {
            return false;
        }

        if (save.Farm1Level < 0
            || save.Farm2Level < 0
            || save.Farm3Level < 0)
        {
            return false;
        }

        return true;
    }
}