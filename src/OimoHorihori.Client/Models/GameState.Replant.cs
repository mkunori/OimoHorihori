using OimoHorihori.Constants;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Models;

public partial class GameState
{
    public int BaseReplantSeedPotato
    {
        get
        {
            if (RunProducedPotato < GameConstants.ReplantBaseProduction)
            {
                return 0;
            }

            double value = Math.Sqrt(RunProducedPotato / GameConstants.ReplantBaseProduction);

            return (int)Math.Floor(value);
        }
    }
    public int ReplantSeedPotato
    {
        get
        {
            double value = BaseReplantSeedPotato * RootSeedBlessingMultiplier;

            return (int)Math.Floor(value);
        }
    }
    public double NextSeedPotatoRequiredProduction
    {
        get
        {
            int nextBaseSeedPotato = BaseReplantSeedPotato + 1;

            return GameConstants.ReplantBaseProduction * nextBaseSeedPotato * nextBaseSeedPotato;
        }
    }

    public double ProductionUntilNextSeedPotato => Math.Max(0, NextSeedPotatoRequiredProduction - RunProducedPotato);
    public bool CanReplant => BaseReplantSeedPotato >= 1;

    public int Replant()
    {
        int earnedSeedPotato = ReplantSeedPotato;

        if (earnedSeedPotato <= 0)
        {
            return 0;
        }

        DateTimeOffset replantedAtUtc = DateTimeOffset.UtcNow;

        double runDurationSeconds = 0;

        if (RunStartedAtUtc != default)
        {
            runDurationSeconds = Math.Max(0, (replantedAtUtc - RunStartedAtUtc).TotalSeconds);
        }

        double runProducedPotato = RunProducedPotato;

        if (runDurationSeconds > 0)
        {
            if (ShortestReplantSeconds <= 0 || runDurationSeconds < ShortestReplantSeconds)
            {
                ShortestReplantSeconds = runDurationSeconds;
            }
        }

        ReplantHistory.Insert(0, new ReplantHistoryEntry
        {
            ReplantedAtUtc = replantedAtUtc,
            RunDurationSeconds = runDurationSeconds,
            RunProducedPotato = runProducedPotato,
            EarnedSeedPotato = earnedSeedPotato
        });

        if (ReplantHistory.Count > 10)
        {
            ReplantHistory.RemoveRange(10, ReplantHistory.Count - 10);
        }

        SeedPotato += earnedSeedPotato;
        TotalSeedPotatoEarned += earnedSeedPotato;
        MaxSeedPotatoPerReplant = Math.Max(MaxSeedPotatoPerReplant, earnedSeedPotato);
        ReplantCount++;
        CurrentAscentReplantCount++;

        ResetRunProgress();

        return earnedSeedPotato;
    }

    private void ResetRunProgress()
    {
        Potato = 0;

        foreach (Farm farm in Farms)
        {
            farm.Level = 0;
            farm.PurchaseCount = 0;
            farm.RetillCount = 0;
        }

        RunProducedPotato = 0;
        RunStartedAtUtc = default;
        HasStarted = false;
    }
}