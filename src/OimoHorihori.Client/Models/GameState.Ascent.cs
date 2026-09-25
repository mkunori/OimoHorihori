using OimoHorihori.Constants;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Models;

public partial class GameState
{
    public bool CanAscent => CurrentAscentReplantCount >= 1 && RunProducedPotato >= GameConstants.AscentTargetProduction;

    public double CurrentAscentElapsedSeconds
    {
        get
        {
            if (CurrentAscentStartedAtUtc == default)
            {
                return 0;
            }

            double seconds = (DateTimeOffset.UtcNow - CurrentAscentStartedAtUtc).TotalSeconds;

            return Math.Max(0, seconds);
        }
    }

    public bool Ascent()
    {
        if (!CanAscent)
        {
            return false;
        }

        DateTimeOffset ascendedAtUtc = DateTimeOffset.UtcNow;

        double ascentDurationSeconds = 0;

        if (CurrentAscentStartedAtUtc != default)
        {
            ascentDurationSeconds = Math.Max(0, (ascendedAtUtc - CurrentAscentStartedAtUtc).TotalSeconds);
        }

        double finalRunProducedPotato = RunProducedPotato;
        int ascentReplantCount = CurrentAscentReplantCount;
        int nextAscentNumber = AscentCount + 1;

        if (ascentDurationSeconds > 0)
        {
            if (BestAscentSeconds <= 0 || ascentDurationSeconds < BestAscentSeconds)
            {
                BestAscentSeconds = ascentDurationSeconds;
            }
        }

        AscentHistory.Insert(0, new AscentHistoryEntry
        {
            AscentNumber = nextAscentNumber,
            AscendedAtUtc = ascendedAtUtc,
            AscentDurationSeconds = ascentDurationSeconds,
            ReplantCount = ascentReplantCount,
            FinalRunProducedPotato = finalRunProducedPotato
        });

        if (AscentHistory.Count > GameConstants.AscentHistoryMaxCount)
        {
            AscentHistory.RemoveRange(GameConstants.AscentHistoryMaxCount, AscentHistory.Count - GameConstants.AscentHistoryMaxCount);
        }

        AscentCount++;
        CurrentRoot++;
        TotalRootEarned++;

        ResetRunProgress();
        ResetReplantLayer();

        CurrentAscentStartedAtUtc = ascendedAtUtc;

        return true;
    }

    private void ResetReplantLayer()
    {
        SeedPotato = 0;
        ProductionUpgradeLevel = 0;
        DigUpgradeLevel = 0;
        OfflineUpgradeLevel = 0;
        RetillEfficiencyUpgradeLevel = 0;
        FieldCostReductionUpgradeLevel = 0;
        CurrentAscentReplantCount = 0;
    }
}