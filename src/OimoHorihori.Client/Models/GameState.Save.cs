using OimoHorihori.Constants;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Models;

public partial class GameState
{
    public SaveData CreateSaveData()
    {
        return new SaveData
        {
            Version = GameConstants.SaveVersion,
            HasStarted = HasStarted,
            Potato = Potato,
            TotalPotato = TotalPotato,
            Farms = Farms
            .Select(farm => new FarmSaveData
            {
                Level = farm.Level,
                PurchaseCount = farm.PurchaseCount,
                RetillCount = farm.RetillCount
            })
            .ToList(),
            RunProducedPotato = RunProducedPotato,
            RunStartedAtUtc = RunStartedAtUtc,
            MaxRunProducedPotato = MaxRunProducedPotato,
            ReplantCount = ReplantCount,
            SeedPotato = SeedPotato,
            TotalSeedPotatoEarned = TotalSeedPotatoEarned,
            TotalSeedPotatoSpent = TotalSeedPotatoSpent,
            MaxSeedPotatoPerReplant = MaxSeedPotatoPerReplant,
            ProductionUpgradeLevel = ProductionUpgradeLevel,
            DigUpgradeLevel = DigUpgradeLevel,
            OfflineUpgradeLevel = OfflineUpgradeLevel,
            TotalConsumedPotato = TotalConsumedPotato,
            BestProductionPerSecond = BestProductionPerSecond,
            Farm1TotalPurchases = Farm1TotalPurchases,
            Farm2TotalPurchases = Farm2TotalPurchases,
            Farm3TotalPurchases = Farm3TotalPurchases,
            Farm4TotalPurchases = Farm4TotalPurchases,
            Farm5TotalPurchases = Farm5TotalPurchases,
            Farm6TotalPurchases = Farm6TotalPurchases,
            Farm7TotalPurchases = Farm7TotalPurchases,
            Farm8TotalPurchases = Farm8TotalPurchases,
            Farm1BestLevel = Farm1BestLevel,
            Farm2BestLevel = Farm2BestLevel,
            Farm3BestLevel = Farm3BestLevel,
            Farm4BestLevel = Farm4BestLevel,
            Farm5BestLevel = Farm5BestLevel,
            Farm6BestLevel = Farm6BestLevel,
            Farm7BestLevel = Farm7BestLevel,
            Farm8BestLevel = Farm8BestLevel,
            TotalOfflineProducedPotato = TotalOfflineProducedPotato,
            MaxOfflineProducedPotato = MaxOfflineProducedPotato,
            DigButtonCount = DigButtonCount,
            GameStartedAtUtc = GameStartedAtUtc,
            TotalPlayTimeSeconds = TotalPlayTimeSeconds,
            LastSaveTimeUtc = DateTimeOffset.UtcNow,
            AchievementUnlockedAtUtc = new Dictionary<string, DateTimeOffset>(AchievementUnlockedAtUtc),
            OimoDiscoveryCounts = new Dictionary<string, int>(OimoDiscoveryCounts),
            OimoDiscoveryElapsedSeconds = OimoDiscoveryElapsedSeconds,
            HasUsedTenPurchaseMode = HasUsedTenPurchaseMode,
            HasUsedMaxPurchaseMode = HasUsedMaxPurchaseMode,
            ShortestReplantSeconds = ShortestReplantSeconds,
            ReplantHistory = ReplantHistory.Select(entry => new ReplantHistoryEntry
            {
                ReplantedAtUtc = entry.ReplantedAtUtc,
                RunDurationSeconds = entry.RunDurationSeconds,
                RunProducedPotato = entry.RunProducedPotato,
                EarnedSeedPotato = entry.EarnedSeedPotato
            }).ToList(),
            RetillEfficiencyUpgradeLevel = RetillEfficiencyUpgradeLevel,
            FieldCostReductionUpgradeLevel = FieldCostReductionUpgradeLevel,
            AscentCount = AscentCount,
            CurrentRoot = CurrentRoot,
            TotalRootEarned = TotalRootEarned,
            RootAbundanceLevel = RootAbundanceLevel,
            RootFertilityLevel = RootFertilityLevel,
            RootRetillLevel = RootRetillLevel,
            RootSeedBlessingLevel = RootSeedBlessingLevel,
            AutoBuyUnlocked = AutoBuyUnlocked,
            AutoBuyEnabled = AutoBuyEnabled,
            AutoRetillUnlocked = AutoRetillUnlocked,
            AutoRetillEnabled = AutoRetillEnabled,
            CurrentAscentReplantCount = CurrentAscentReplantCount,
            CurrentAscentStartedAtUtc = CurrentAscentStartedAtUtc,
            BestAscentSeconds = BestAscentSeconds,
            AscentHistory = AscentHistory.Select(entry => new AscentHistoryEntry
            {
                AscentNumber = entry.AscentNumber,
                AscendedAtUtc = entry.AscendedAtUtc,
                AscentDurationSeconds = entry.AscentDurationSeconds,
                ReplantCount = entry.ReplantCount,
                FinalRunProducedPotato = entry.FinalRunProducedPotato
            }).ToList(),
        };
    }

    public bool TryLoadSaveData(SaveData save)
    {
        return save.Version switch
        {
            1 => TryLoadVersion1(save),
            2 => TryLoadVersion2(save),
            3 => TryLoadVersion3(save),
            4 => TryLoadVersion4(save),
            5 => TryLoadVersion5(save),
            _ => false
        };
    }

    private bool TryLoadVersion1(SaveData save)
    {
        if (!IsValidVersion1SaveData(save))
        {
            return false;
        }

        DateTimeOffset migrationTime = DateTimeOffset.UtcNow;
        HasStarted = save.HasStarted;
        Potato = save.Potato;
        TotalPotato = save.TotalPotato;
        Farms[0].Level = save.Farm1Level;
        Farms[1].Level = save.Farm2Level;
        Farms[2].Level = save.Farm3Level;
        Farms[3].Level = 0;
        Farms[4].Level = 0;
        Farms[5].Level = 0;
        Farms[6].Level = 0;
        Farms[7].Level = 0;
        Farms[0].PurchaseCount = Farms[0].Level;
        Farms[1].PurchaseCount = Farms[1].Level;
        Farms[2].PurchaseCount = Farms[2].Level;
        Farms[3].PurchaseCount = 0;
        Farms[4].PurchaseCount = 0;
        Farms[5].PurchaseCount = 0;
        Farms[6].PurchaseCount = 0;
        Farms[7].PurchaseCount = 0;
        RunProducedPotato = save.TotalPotato;
        MaxRunProducedPotato = save.TotalPotato;
        RunStartedAtUtc = migrationTime;
        ReplantCount = 0;
        SeedPotato = 0;
        TotalSeedPotatoEarned = 0;
        TotalSeedPotatoSpent = 0;
        MaxSeedPotatoPerReplant = 0;
        ProductionUpgradeLevel = (int)Math.Round(save.ProductionUpgradeLevel / 4.0, MidpointRounding.AwayFromZero);
        DigUpgradeLevel = 0;
        OfflineUpgradeLevel = Math.Min(GameConstants.OfflineUpgradeMaxLevel, (int)Math.Round(save.OfflineUpgradeLevel / 2.0, MidpointRounding.AwayFromZero));
        TotalConsumedPotato = Math.Max(0, save.TotalPotato - save.Potato);
        Farm1TotalPurchases = save.Farm1Level;
        Farm2TotalPurchases = save.Farm2Level;
        Farm3TotalPurchases = save.Farm3Level;
        Farm1BestLevel = save.Farm1Level;
        Farm2BestLevel = save.Farm2Level;
        Farm3BestLevel = save.Farm3Level;
        TotalOfflineProducedPotato = 0;
        MaxOfflineProducedPotato = 0;
        DigButtonCount = save.HasStarted ? 1 : 0;
        GameStartedAtUtc = migrationTime;
        TotalPlayTimeSeconds = 0;
        BestProductionPerSecond = ProductionPerSecond;
        RetillEfficiencyUpgradeLevel = 0;
        FieldCostReductionUpgradeLevel = 0;

        return true;
    }

    private bool TryLoadVersion2(SaveData save)
    {
        if (!IsValidSaveData(save))
        {
            return false;
        }

        HasStarted = save.HasStarted;
        Potato = save.Potato;
        TotalPotato = save.TotalPotato;
        Farms[0].Level = save.Farm1Level;
        Farms[1].Level = save.Farm2Level;
        Farms[2].Level = save.Farm3Level;
        Farms[3].Level = 0;
        Farms[4].Level = 0;
        Farms[5].Level = 0;
        Farms[6].Level = 0;
        Farms[7].Level = 0;
        Farms[0].PurchaseCount = Farms[0].Level;
        Farms[1].PurchaseCount = Farms[1].Level;
        Farms[2].PurchaseCount = Farms[2].Level;
        Farms[3].PurchaseCount = 0;
        Farms[4].PurchaseCount = 0;
        Farms[5].PurchaseCount = 0;
        Farms[6].PurchaseCount = 0;
        Farms[7].PurchaseCount = 0;
        RunProducedPotato = save.RunProducedPotato;
        RunStartedAtUtc = save.RunStartedAtUtc;
        MaxRunProducedPotato = save.MaxRunProducedPotato;
        ReplantCount = save.ReplantCount;
        SeedPotato = save.SeedPotato;
        TotalSeedPotatoEarned = save.TotalSeedPotatoEarned;
        TotalSeedPotatoSpent = save.TotalSeedPotatoSpent;
        MaxSeedPotatoPerReplant = save.MaxSeedPotatoPerReplant;
        ProductionUpgradeLevel = (int)Math.Round(save.ProductionUpgradeLevel / 4.0, MidpointRounding.AwayFromZero);
        DigUpgradeLevel = save.DigUpgradeLevel;
        OfflineUpgradeLevel = Math.Min(GameConstants.OfflineUpgradeMaxLevel, (int)Math.Round(save.OfflineUpgradeLevel / 2.0, MidpointRounding.AwayFromZero));
        TotalConsumedPotato = save.TotalConsumedPotato;
        BestProductionPerSecond = save.BestProductionPerSecond;
        Farm1TotalPurchases = save.Farm1TotalPurchases;
        Farm2TotalPurchases = save.Farm2TotalPurchases;
        Farm3TotalPurchases = save.Farm3TotalPurchases;
        Farm1BestLevel = save.Farm1BestLevel;
        Farm2BestLevel = save.Farm2BestLevel;
        Farm3BestLevel = save.Farm3BestLevel;
        TotalOfflineProducedPotato = save.TotalOfflineProducedPotato;
        MaxOfflineProducedPotato = save.MaxOfflineProducedPotato;
        DigButtonCount = save.DigButtonCount;
        GameStartedAtUtc = save.GameStartedAtUtc;
        TotalPlayTimeSeconds = save.TotalPlayTimeSeconds;

        AchievementUnlockedAtUtc.Clear();

        if (save.AchievementUnlockedAtUtc is not null)
        {
            foreach (KeyValuePair<string, DateTimeOffset> pair in save.AchievementUnlockedAtUtc)
            {
                AchievementUnlockedAtUtc[pair.Key] = pair.Value;
            }
        }

        OimoDiscoveryCounts.Clear();

        if (save.OimoDiscoveryCounts is not null)
        {
            foreach (KeyValuePair<string, int> pair in save.OimoDiscoveryCounts)
            {
                if (pair.Value > 0)
                {
                    OimoDiscoveryCounts[pair.Key] = pair.Value;
                }
            }
        }

        OimoDiscoveryElapsedSeconds = save.OimoDiscoveryElapsedSeconds;
        HasUsedTenPurchaseMode = save.HasUsedTenPurchaseMode;
        HasUsedMaxPurchaseMode = save.HasUsedMaxPurchaseMode;
        ShortestReplantSeconds = 0;
        RetillEfficiencyUpgradeLevel = 0;
        FieldCostReductionUpgradeLevel = 0;

        ReplantHistory.Clear();

        return true;
    }

    private bool TryLoadVersion3(SaveData save)
    {
        if (!IsValidSaveData(save))
        {
            return false;
        }

        if (!double.IsFinite(save.ShortestReplantSeconds) || save.ShortestReplantSeconds < 0)
        {
            return false;
        }

        if (save.ReplantHistory is null || save.ReplantHistory.Count > 10)
        {
            return false;
        }

        foreach (ReplantHistoryEntry entry in save.ReplantHistory)
        {
            if (!double.IsFinite(entry.RunDurationSeconds) || entry.RunDurationSeconds < 0)
            {
                return false;
            }

            if (!double.IsFinite(entry.RunProducedPotato) || entry.RunProducedPotato < 0)
            {
                return false;
            }

            if (entry.EarnedSeedPotato < 0)
            {
                return false;
            }
        }

        TotalPotato = save.TotalPotato;
        HasStarted = false;
        Potato = 0;
        RunProducedPotato = 0;
        RunStartedAtUtc = default;
        foreach (Farm farm in Farms)
        {
            farm.Level = 0;
            farm.PurchaseCount = 0;
            farm.RetillCount = 0;
        }
        MaxRunProducedPotato = save.MaxRunProducedPotato;
        ReplantCount = save.ReplantCount;
        SeedPotato = save.SeedPotato;
        TotalSeedPotatoEarned = save.TotalSeedPotatoEarned;
        TotalSeedPotatoSpent = save.TotalSeedPotatoSpent;
        MaxSeedPotatoPerReplant = save.MaxSeedPotatoPerReplant;
        ProductionUpgradeLevel = (int)Math.Round(save.ProductionUpgradeLevel / 4.0, MidpointRounding.AwayFromZero);
        DigUpgradeLevel = save.DigUpgradeLevel;
        OfflineUpgradeLevel = Math.Min(GameConstants.OfflineUpgradeMaxLevel, (int)Math.Round(save.OfflineUpgradeLevel / 2.0, MidpointRounding.AwayFromZero));
        TotalConsumedPotato = save.TotalConsumedPotato;
        BestProductionPerSecond = save.BestProductionPerSecond;
        Farm1TotalPurchases = save.Farm1TotalPurchases;
        Farm2TotalPurchases = save.Farm2TotalPurchases;
        Farm3TotalPurchases = save.Farm3TotalPurchases;
        Farm1BestLevel = save.Farm1BestLevel;
        Farm2BestLevel = save.Farm2BestLevel;
        Farm3BestLevel = save.Farm3BestLevel;
        TotalOfflineProducedPotato = save.TotalOfflineProducedPotato;
        MaxOfflineProducedPotato = save.MaxOfflineProducedPotato;
        DigButtonCount = save.DigButtonCount;
        GameStartedAtUtc = save.GameStartedAtUtc;
        TotalPlayTimeSeconds = save.TotalPlayTimeSeconds;

        AchievementUnlockedAtUtc.Clear();

        if (save.AchievementUnlockedAtUtc is not null)
        {
            foreach (KeyValuePair<string, DateTimeOffset> pair in save.AchievementUnlockedAtUtc)
            {
                AchievementUnlockedAtUtc[pair.Key] = pair.Value;
            }
        }

        OimoDiscoveryCounts.Clear();

        if (save.OimoDiscoveryCounts is not null)
        {
            foreach (KeyValuePair<string, int> pair in save.OimoDiscoveryCounts)
            {
                if (pair.Value > 0)
                {
                    OimoDiscoveryCounts[pair.Key] = pair.Value;
                }
            }
        }

        OimoDiscoveryElapsedSeconds = save.OimoDiscoveryElapsedSeconds;
        HasUsedTenPurchaseMode = save.HasUsedTenPurchaseMode;
        HasUsedMaxPurchaseMode = save.HasUsedMaxPurchaseMode;
        ShortestReplantSeconds = save.ShortestReplantSeconds;
        RetillEfficiencyUpgradeLevel = 0;
        FieldCostReductionUpgradeLevel = 0;

        ReplantHistory.Clear();

        foreach (ReplantHistoryEntry entry in save.ReplantHistory)
        {
            ReplantHistory.Add(new ReplantHistoryEntry
            {
                ReplantedAtUtc = entry.ReplantedAtUtc,
                RunDurationSeconds = entry.RunDurationSeconds,
                RunProducedPotato = entry.RunProducedPotato,
                EarnedSeedPotato = entry.EarnedSeedPotato
            });
        }

        return true;
    }

    private bool TryLoadVersion4(SaveData save)
    {
        if (!IsValidVersion4SaveData(save))
        {
            return false;
        }

        LoadVersion4Fields(save);
        InitializeVersion5MigrationState();

        return true;
    }

    private bool TryLoadVersion5(SaveData save)
    {
        if (!IsValidVersion5SaveData(save))
        {
            return false;
        }

        LoadVersion4Fields(save);

        AscentCount = save.AscentCount;
        CurrentRoot = save.CurrentRoot;
        TotalRootEarned = save.TotalRootEarned;
        RootAbundanceLevel = save.RootAbundanceLevel;
        RootFertilityLevel = save.RootFertilityLevel;
        RootRetillLevel = save.RootRetillLevel;
        RootSeedBlessingLevel = save.RootSeedBlessingLevel;
        AutoBuyUnlocked = save.AutoBuyUnlocked;
        AutoBuyEnabled = save.AutoBuyEnabled;
        AutoRetillUnlocked = save.AutoRetillUnlocked;
        AutoRetillEnabled = save.AutoRetillEnabled;
        CurrentAscentReplantCount = save.CurrentAscentReplantCount;
        CurrentAscentStartedAtUtc = save.CurrentAscentStartedAtUtc;
        BestAscentSeconds = save.BestAscentSeconds;

        AscentHistory.Clear();

        foreach (AscentHistoryEntry entry in save.AscentHistory)
        {
            AscentHistory.Add(new AscentHistoryEntry
            {
                AscentNumber = entry.AscentNumber,
                AscendedAtUtc = entry.AscendedAtUtc,
                AscentDurationSeconds = entry.AscentDurationSeconds,
                ReplantCount = entry.ReplantCount,
                FinalRunProducedPotato = entry.FinalRunProducedPotato
            });
        }

        return true;
    }

    private void LoadVersion4Fields(SaveData save)
    {
        HasStarted = save.HasStarted;
        Potato = save.Potato;
        TotalPotato = save.TotalPotato;
        RunProducedPotato = save.RunProducedPotato;
        RunStartedAtUtc = save.RunStartedAtUtc;
        MaxRunProducedPotato = save.MaxRunProducedPotato;
        ReplantCount = save.ReplantCount;
        SeedPotato = save.SeedPotato;
        TotalSeedPotatoEarned = save.TotalSeedPotatoEarned;
        TotalSeedPotatoSpent = save.TotalSeedPotatoSpent;
        MaxSeedPotatoPerReplant = save.MaxSeedPotatoPerReplant;
        ProductionUpgradeLevel = save.ProductionUpgradeLevel;
        DigUpgradeLevel = save.DigUpgradeLevel;
        OfflineUpgradeLevel = save.OfflineUpgradeLevel;
        RetillEfficiencyUpgradeLevel = save.RetillEfficiencyUpgradeLevel;
        FieldCostReductionUpgradeLevel = save.FieldCostReductionUpgradeLevel;
        for (int i = 0; i < Farms.Count; i++)
        {
            FarmSaveData savedFarm = save.Farms[i];
            Farms[i].Level = savedFarm.Level;
            Farms[i].PurchaseCount = savedFarm.PurchaseCount;
            Farms[i].RetillCount = savedFarm.RetillCount;
        }
        TotalConsumedPotato = save.TotalConsumedPotato;
        BestProductionPerSecond = save.BestProductionPerSecond;
        Farm1TotalPurchases = save.Farm1TotalPurchases;
        Farm2TotalPurchases = save.Farm2TotalPurchases;
        Farm3TotalPurchases = save.Farm3TotalPurchases;
        Farm4TotalPurchases = save.Farm4TotalPurchases;
        Farm5TotalPurchases = save.Farm5TotalPurchases;
        Farm6TotalPurchases = save.Farm6TotalPurchases;
        Farm7TotalPurchases = save.Farm7TotalPurchases;
        Farm8TotalPurchases = save.Farm8TotalPurchases;
        Farm1BestLevel = save.Farm1BestLevel;
        Farm2BestLevel = save.Farm2BestLevel;
        Farm3BestLevel = save.Farm3BestLevel;
        Farm4BestLevel = save.Farm4BestLevel;
        Farm5BestLevel = save.Farm5BestLevel;
        Farm6BestLevel = save.Farm6BestLevel;
        Farm7BestLevel = save.Farm7BestLevel;
        Farm8BestLevel = save.Farm8BestLevel;
        TotalOfflineProducedPotato = save.TotalOfflineProducedPotato;
        MaxOfflineProducedPotato = save.MaxOfflineProducedPotato;
        DigButtonCount = save.DigButtonCount;
        GameStartedAtUtc = save.GameStartedAtUtc;
        TotalPlayTimeSeconds = save.TotalPlayTimeSeconds;
        OimoDiscoveryElapsedSeconds = save.OimoDiscoveryElapsedSeconds;
        HasUsedTenPurchaseMode = save.HasUsedTenPurchaseMode;
        HasUsedMaxPurchaseMode = save.HasUsedMaxPurchaseMode;
        ShortestReplantSeconds = save.ShortestReplantSeconds;

        AchievementUnlockedAtUtc.Clear();

        foreach (var pair in save.AchievementUnlockedAtUtc)
        {
            AchievementUnlockedAtUtc[pair.Key] = pair.Value;
        }

        OimoDiscoveryCounts.Clear();

        foreach (var pair in save.OimoDiscoveryCounts)
        {
            if (pair.Value > 0)
            {
                OimoDiscoveryCounts[pair.Key] = pair.Value;
            }
        }

        ReplantHistory.Clear();

        foreach (ReplantHistoryEntry entry in save.ReplantHistory)
        {
            ReplantHistory.Add(new ReplantHistoryEntry
            {
                ReplantedAtUtc = entry.ReplantedAtUtc,
                RunDurationSeconds = entry.RunDurationSeconds,
                RunProducedPotato = entry.RunProducedPotato,
                EarnedSeedPotato = entry.EarnedSeedPotato
            });
        }
    }

    private void InitializeVersion5MigrationState()
    {
        AscentCount = 0;
        CurrentRoot = 0;
        TotalRootEarned = 0;
        RootAbundanceLevel = 0;
        RootFertilityLevel = 0;
        RootRetillLevel = 0;
        RootSeedBlessingLevel = 0;
        AutoBuyUnlocked = false;
        AutoBuyEnabled = false;
        AutoRetillUnlocked = false;
        AutoRetillEnabled = false;
        CurrentAscentReplantCount = 0;

        // v5移行後からASCENT時間を計測する。
        CurrentAscentStartedAtUtc = DateTimeOffset.UtcNow;

        BestAscentSeconds = 0;

        AscentHistory.Clear();
    }

    private static bool IsValidVersion5SaveData(SaveData save)
    {
        if (!IsValidVersion4SaveData(save))
        {
            return false;
        }

        if (save.AscentCount < 0 || save.CurrentRoot < 0 || save.TotalRootEarned < 0 || save.CurrentAscentReplantCount < 0)
        {
            return false;
        }

        if (save.RootAbundanceLevel < 0 || save.RootAbundanceLevel > GameConstants.RootAbundanceMaxLevel)
        {
            return false;
        }

        if (save.RootFertilityLevel < 0 || save.RootFertilityLevel > GameConstants.RootFertilityMaxLevel)
        {
            return false;
        }

        if (save.RootRetillLevel < 0 || save.RootRetillLevel > GameConstants.RootRetillMaxLevel)
        {
            return false;
        }

        if (save.RootSeedBlessingLevel < 0 || save.RootSeedBlessingLevel > GameConstants.RootSeedBlessingMaxLevel)
        {
            return false;
        }

        if (!double.IsFinite(save.BestAscentSeconds) || save.BestAscentSeconds < 0)
        {
            return false;
        }

        if (save.AscentHistory is null || save.AscentHistory.Count > GameConstants.AscentHistoryMaxCount)
        {
            return false;
        }

        int usedRoot =
            save.RootAbundanceLevel + save.RootFertilityLevel + save.RootRetillLevel + save.RootSeedBlessingLevel + (save.AutoBuyUnlocked ? 1 : 0) + (save.AutoRetillUnlocked ? 1 : 0);

        if (usedRoot > save.TotalRootEarned)
        {
            return false;
        }

        if (save.CurrentRoot != save.TotalRootEarned - usedRoot)
        {
            return false;
        }

        foreach (AscentHistoryEntry entry in save.AscentHistory)
        {
            if (entry.AscentNumber <= 0 || entry.ReplantCount < 0 || !double.IsFinite(entry.AscentDurationSeconds) || entry.AscentDurationSeconds < 0 || !double.IsFinite(entry.FinalRunProducedPotato) || entry.FinalRunProducedPotato < 0)
            {
                return false;
            }
        }

        if (!save.AutoBuyUnlocked && save.AutoBuyEnabled)
        {
            return false;
        }

        if (!save.AutoRetillUnlocked && save.AutoRetillEnabled)
        {
            return false;
        }

        if (save.TotalRootEarned != save.AscentCount)
        {
            return false;
        }

        if (save.CurrentAscentReplantCount > save.ReplantCount)
        {
            return false;
        }

        if (save.AscentHistory.Count > save.AscentCount)
        {
            return false;
        }

        return true;
    }

    private static bool IsValidSaveData(SaveData save)
    {
        if (!IsValidVersion1SaveData(save))
        {
            return false;
        }

        if (!double.IsFinite(save.RunProducedPotato) || save.RunProducedPotato < 0)
        {
            return false;
        }

        if (save.ReplantCount < 0 || save.SeedPotato < 0 || save.TotalSeedPotatoEarned < 0 || save.TotalSeedPotatoSpent < 0)
        {
            return false;
        }

        if (save.ProductionUpgradeLevel < 0 || save.DigUpgradeLevel < 0 || save.OfflineUpgradeLevel < 0)
        {
            return false;
        }

        if (save.OfflineUpgradeLevel > 32)
        {
            return false;
        }

        return true;
    }

    private static bool IsValidVersion4SaveData(SaveData save)
    {
        if (!double.IsFinite(save.Potato) || save.Potato < 0)
        {
            return false;
        }

        if (!double.IsFinite(save.TotalPotato) || save.TotalPotato < 0)
        {
            return false;
        }

        if (!double.IsFinite(save.RunProducedPotato) || save.RunProducedPotato < 0)
        {
            return false;
        }

        if (save.ReplantCount < 0 || save.SeedPotato < 0 || save.TotalSeedPotatoEarned < 0 || save.TotalSeedPotatoSpent < 0)
        {
            return false;
        }

        if (save.ProductionUpgradeLevel < 0 || save.DigUpgradeLevel < 0 || save.OfflineUpgradeLevel < 0 || save.RetillEfficiencyUpgradeLevel < 0 || save.FieldCostReductionUpgradeLevel < 0)
        {
            return false;
        }

        if (save.OfflineUpgradeLevel > GameConstants.OfflineUpgradeMaxLevel)
        {
            return false;
        }

        if (save.FieldCostReductionUpgradeLevel > GameConstants.FieldCostReductionMaxLevel)
        {
            return false;
        }

        if (save.Farms is null || save.Farms.Count != 8)
        {
            return false;
        }

        foreach (FarmSaveData farm in save.Farms)
        {
            if (farm.RetillCount < 0 || farm.RetillCount > GameConstants.RetillMaxCount)
            {
                return false;
            }

            int maxLevel = GameConstants.InitialFarmMaxLevel + farm.RetillCount * GameConstants.RetillLevelCapBonus;

            if (farm.Level < 0
                || farm.Level > maxLevel)
            {
                return false;
            }

            if (farm.PurchaseCount < 0)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsValidVersion1SaveData(SaveData save)
    {
        if (!double.IsFinite(save.Potato) || save.Potato < 0)
        {
            return false;
        }

        if (!double.IsFinite(save.TotalPotato) || save.TotalPotato < 0)
        {
            return false;
        }

        if (save.TotalPotato < save.Potato)
        {
            return false;
        }

        if (save.Farm1Level < 0 || save.Farm2Level < 0 || save.Farm3Level < 0)
        {
            return false;
        }

        return true;
    }
}