using OimoHorihori.Constants;
using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Models;

public class GameState
{
    public bool HasStarted { get; set; }
    public double Potato { get; set; }
    public double TotalPotato { get; set; }
    public double RunProducedPotato { get; set; }
    public DateTimeOffset RunStartedAtUtc { get; set; }
    public double MaxRunProducedPotato { get; set; }
    public int ReplantCount { get; set; }
    public int SeedPotato { get; set; }
    public int TotalSeedPotatoEarned { get; set; }
    public int TotalSeedPotatoSpent { get; set; }
    public int MaxSeedPotatoPerReplant { get; set; }
    public int ProductionUpgradeLevel { get; set; }
    public int DigUpgradeLevel { get; set; }
    public int OfflineUpgradeLevel { get; set; }
    public int RetillEfficiencyUpgradeLevel { get; set; }
    public int FieldCostReductionUpgradeLevel { get; set; }
    public double TotalConsumedPotato { get; set; }
    public double BestProductionPerSecond { get; set; }
    public int Farm1TotalPurchases { get; set; }
    public int Farm2TotalPurchases { get; set; }
    public int Farm3TotalPurchases { get; set; }
    public int Farm4TotalPurchases { get; set; }
    public int Farm5TotalPurchases { get; set; }
    public int Farm6TotalPurchases { get; set; }
    public int Farm7TotalPurchases { get; set; }
    public int Farm8TotalPurchases { get; set; }
    public int Farm1BestLevel { get; set; }
    public int Farm2BestLevel { get; set; }
    public int Farm3BestLevel { get; set; }
    public int Farm4BestLevel { get; set; }
    public int Farm5BestLevel { get; set; }
    public int Farm6BestLevel { get; set; }
    public int Farm7BestLevel { get; set; }
    public int Farm8BestLevel { get; set; }
    public double TotalOfflineProducedPotato { get; set; }
    public double MaxOfflineProducedPotato { get; set; }
    public int DigButtonCount { get; set; }
    public DateTimeOffset GameStartedAtUtc { get; set; }
    public double TotalPlayTimeSeconds { get; set; }
    public double OimoDiscoveryElapsedSeconds { get; set; }
    public bool HasUsedTenPurchaseMode { get; set; }
    public bool HasUsedMaxPurchaseMode { get; set; }
    public Dictionary<string, DateTimeOffset> AchievementUnlockedAtUtc { get; } = new();
    public Dictionary<string, int> OimoDiscoveryCounts { get; } = new();
    public List<Farm> Farms { get; } = new()
    {
        new Farm("畑1", 1.000e1,  1.000e-1),
        new Farm("畑2", 1.500e2,  1.000e0),
        new Farm("畑3", 3.000e3,  1.000e1),
        new Farm("畑4", 4.500e4,  1.000e2),
        new Farm("畑5", 6.000e5,  1.000e3),
        new Farm("畑6", 1.300e7,  1.000e4),
        new Farm("畑7", 2.000e8,  1.000e5),
        new Farm("畑8", 1.100e10, 1.000e6)
    };
    public double ShortestReplantSeconds { get; set; }
    public List<ReplantHistoryEntry> ReplantHistory { get; } = new();
    public int AscentCount { get; set; }
    public int CurrentRoot { get; set; }
    public int TotalRootEarned { get; set; }
    public int RootAbundanceLevel { get; set; }
    public int RootFertilityLevel { get; set; }
    public int RootRetillLevel { get; set; }
    public int RootSeedBlessingLevel { get; set; }
    public bool AutoBuyUnlocked { get; set; }
    public bool AutoBuyEnabled { get; set; }
    public bool AutoRetillUnlocked { get; set; }
    public bool AutoRetillEnabled { get; set; }
    public int CurrentAscentReplantCount { get; set; }
    public DateTimeOffset CurrentAscentStartedAtUtc { get; set; }
    public double BestAscentSeconds { get; set; }
    public List<AscentHistoryEntry> AscentHistory { get; } = new();

    public double BaseProductionPerSecond => GameConstants.BaseProductionPerSecond + DigUpgradeLevel * GameConstants.DigUpgradeBonusPerLevel;
    public double ProductionMultiplier => 1.0 + ProductionUpgradeLevel * GameConstants.ProductionUpgradeBonusPerLevel;
    public double ProductionUntilNextSeedPotato => Math.Max(0, NextSeedPotatoRequiredProduction - RunProducedPotato);
    public int ProductionUpgradeCost => GetUpgradeCost(ProductionUpgradeLevel);
    public int DigUpgradeCost => GetUpgradeCost(DigUpgradeLevel);
    public int OfflineUpgradeCost => GetUpgradeCost(OfflineUpgradeLevel);
    public bool IsOfflineUpgradeMax => OfflineUpgradeLevel >= GameConstants.OfflineUpgradeMaxLevel;
    public int FieldCostReductionUpgradeCost => GetUpgradeCost(FieldCostReductionUpgradeLevel);
    public bool IsFieldCostReductionUpgradeMax => FieldCostReductionUpgradeLevel >= GameConstants.FieldCostReductionMaxLevel;
    public int RetillEfficiencyUpgradeCost => GetUpgradeCost(RetillEfficiencyUpgradeLevel);
    public int UsedRoot => RootAbundanceLevel + RootFertilityLevel + RootRetillLevel + RootSeedBlessingLevel + (AutoBuyUnlocked ? 1 : 0) + (AutoRetillUnlocked ? 1 : 0);
    public int RootPower => UsedRoot;
    public bool CanAscent => RunProducedPotato >= GameConstants.AscentTargetProduction;
    public double RootAbundanceMultiplier => GetRootAbundanceMultiplier(RootAbundanceLevel);
    public double RootFertilityMultiplier => GetRootFertilityMultiplier(RootFertilityLevel);
    public double RootAdjustedRetillBase => GetRootAdjustedRetillBase(RootRetillLevel);
    public double RootSeedBlessingMultiplier => GetRootSeedBlessingMultiplier(RootSeedBlessingLevel);
    public bool IsRootAbundanceMax => RootAbundanceLevel >= GameConstants.RootAbundanceMaxLevel;
    public bool IsRootFertilityMax => RootFertilityLevel >= GameConstants.RootFertilityMaxLevel;
    public bool IsRootRetillMax => RootRetillLevel >= GameConstants.RootRetillMaxLevel;
    public bool IsRootSeedBlessingMax => RootSeedBlessingLevel >= GameConstants.RootSeedBlessingMaxLevel;

    public double ProductionPerSecond
    {
        get
        {
            double total = BaseProductionPerSecond;

            foreach (Farm farm in Farms)
            {
                total += GetFarmProductionPerSecond(farm);
            }

            return total * ProductionMultiplier * RootAbundanceMultiplier;
        }
    }

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

    public bool CanReplant => BaseReplantSeedPotato >= 1;

    public double NextSeedPotatoRequiredProduction
    {
        get
        {
            int nextBaseSeedPotato = BaseReplantSeedPotato + 1;

            return GameConstants.ReplantBaseProduction * nextBaseSeedPotato * nextBaseSeedPotato;
        }
    }

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

        ReplantHistory.Insert(
            0,
            new ReplantHistoryEntry
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

    private static int GetUpgradeCost(int currentLevel)
    {
        int nextLevel = currentLevel + 1;
        double cost = (double)nextLevel * (nextLevel + 1) / 3;
        return (int)Math.Round(cost);
    }

    private bool TrySpendSeedPotato(int cost)
    {
        if (cost <= 0 || SeedPotato < cost)
        {
            return false;
        }

        SeedPotato -= cost;
        TotalSeedPotatoSpent += cost;

        return true;
    }

    public bool BuyProductionUpgrade()
    {
        int cost = ProductionUpgradeCost;

        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        ProductionUpgradeLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyDigUpgrade()
    {
        int cost = DigUpgradeCost;

        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        DigUpgradeLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyOfflineUpgrade()
    {
        if (IsOfflineUpgradeMax)
        {
            return false;
        }

        if (!TrySpendSeedPotato(OfflineUpgradeCost))
        {
            return false;
        }

        OfflineUpgradeLevel++;

        return true;
    }

    public double OfflineLimitSeconds
    {
        get
        {
            double seconds = GameConstants.InitialOfflineLimitSeconds + OfflineUpgradeLevel * GameConstants.OfflineUpgradeSecondsPerLevel;
            return Math.Min(seconds, GameConstants.MaxOfflineLimitSeconds);
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return AchievementUnlockedAtUtc.ContainsKey(achievementId);
    }

    public DateTimeOffset? GetAchievementUnlockedAtUtc(string achievementId)
    {
        if (AchievementUnlockedAtUtc.TryGetValue(achievementId, out DateTimeOffset unlockedAt))
        {
            return unlockedAt;
        }

        return null;
    }

    public IReadOnlyList<AchievementDefinition> CheckAchievements()
    {
        List<AchievementDefinition> unlockedAchievements = new();

        foreach (AchievementDefinition achievement in AchievementCatalog.All)
        {
            if (IsAchievementUnlocked(achievement.Id))
            {
                continue;
            }

            if (!achievement.Condition(this))
            {
                continue;
            }

            AchievementUnlockedAtUtc[achievement.Id] = DateTimeOffset.UtcNow;
            unlockedAchievements.Add(achievement);
        }

        return unlockedAchievements;
    }

    public int GetOimoDiscoveryCount(string speciesId)
    {
        return OimoDiscoveryCounts.TryGetValue(speciesId, out int count) ? count : 0;
    }

    public bool IsOimoDiscovered(string speciesId)
    {
        return GetOimoDiscoveryCount(speciesId) > 0;
    }

    public int DiscoveredOimoSpeciesCount
    {
        get
        {
            int count = 0;

            foreach (OimoSpeciesDefinition species in OimoSpeciesCatalog.All)
            {
                if (IsOimoDiscovered(species.Id))
                {
                    count++;
                }
            }

            return count;
        }
    }

    public int TotalOimoDiscoveries
    {
        get
        {
            int total = 0;

            foreach (OimoSpeciesDefinition species in OimoSpeciesCatalog.All)
            {
                total += GetOimoDiscoveryCount(species.Id);
            }

            return total;
        }
    }

    public OimoSpeciesDefinition? TryDiscoverOimo()
    {
        double roll = Random.Shared.NextDouble();

        if (roll >= GameConstants.OimoDiscoveryChance)
        {
            return null;
        }

        return DiscoverRandomOimo();
    }

    private void AddOimoDiscovery(OimoSpeciesDefinition species)
    {
        if (!OimoDiscoveryCounts.TryAdd(species.Id, 1))
        {
            OimoDiscoveryCounts[species.Id]++;
        }
    }

    private OimoSpeciesDefinition DiscoverRandomOimo()
    {
        int index = Random.Shared.Next(OimoSpeciesCatalog.All.Count);

        OimoSpeciesDefinition species = OimoSpeciesCatalog.All[index];
        AddOimoDiscovery(species);

        return species;
    }

    public IReadOnlyList<OimoSpeciesDefinition> AdvanceOimoDiscovery(double seconds)
    {
        if (!double.IsFinite(seconds) || seconds <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        OimoDiscoveryElapsedSeconds += seconds;
        int trialCount = (int)Math.Floor(OimoDiscoveryElapsedSeconds / GameConstants.OimoDiscoveryIntervalSeconds);
        if (trialCount <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }
        OimoDiscoveryElapsedSeconds -= trialCount * GameConstants.OimoDiscoveryIntervalSeconds;

        List<OimoSpeciesDefinition> discoveries = new();
        for (int i = 0; i < trialCount; i++)
        {
            OimoSpeciesDefinition? discovered = TryDiscoverOimo();

            if (discovered is not null)
            {
                discoveries.Add(discovered);
            }
        }

        return discoveries;
    }

    public IReadOnlyList<OimoSpeciesDefinition> ProduceOfflineOimoDiscoveries(double elapsedSeconds)
    {
        if (!double.IsFinite(elapsedSeconds) || elapsedSeconds <= 0)
        {
            return Array.Empty<OimoSpeciesDefinition>();
        }

        double cappedSeconds = Math.Min(elapsedSeconds, GameConstants.OimoOfflineDiscoveryLimitSeconds);

        return AdvanceOimoDiscovery(
            cappedSeconds);
    }

    public bool RetillFarm(Farm farm)
    {
        bool retilled = farm.TryRetill();

        if (!retilled)
        {
            return false;
        }

        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public double GetFarmProductionPerSecond(Farm farm)
    {
        double farmBase = farm.BaseProductionPerSecond;
        double retillMultiplier = GetRetillBaseMultiplier(farm);
        double seedRetillEfficiencyMultiplier = GetRetillEfficiencyMultiplier(farm);

        return farmBase * retillMultiplier * seedRetillEfficiencyMultiplier;
    }

    public bool BuyRetillEfficiencyUpgrade()
    {
        int cost = RetillEfficiencyUpgradeCost;
        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        RetillEfficiencyUpgradeLevel++;

        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyFieldCostReductionUpgrade()
    {
        if (IsFieldCostReductionUpgradeMax)
        {
            return false;
        }

        int cost = FieldCostReductionUpgradeCost;
        if (!TrySpendSeedPotato(cost))
        {
            return false;
        }

        FieldCostReductionUpgradeLevel++;

        return true;
    }

    public double SeedFieldCostMultiplier
    {
        get
        {
            double reduction = FieldCostReductionUpgradeLevel * GameConstants.FieldCostReductionPerLevel;

            reduction = Math.Min(reduction, 0.10);

            return 1.0 - reduction;
        }
    }

    public double FieldCostMultiplier => SeedFieldCostMultiplier * RootFertilityMultiplier;

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

    public double GetRetillEfficiencyMultiplier(Farm farm)
    {
        return 1.0 + farm.RetillCount * RetillEfficiencyUpgradeLevel * GameConstants.RetillEfficiencyBonusPerLevelPerRetill;
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
            if (BestAscentSeconds <= 0
                || ascentDurationSeconds < BestAscentSeconds)
            {
                BestAscentSeconds = ascentDurationSeconds;
            }
        }

        AscentHistory.Insert(
            0,
            new AscentHistoryEntry
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

    public double GetRetillBaseMultiplier(Farm farm)
    {
        return GetRetillBaseMultiplier(farm.RetillCount);
    }

    public double GetRetillBaseMultiplier(int retillCount)
    {
        return Math.Pow(RootAdjustedRetillBase, retillCount);
    }

    private bool TrySpendRoot(int cost)
    {
        if (cost <= 0 || CurrentRoot < cost)
        {
            return false;
        }

        CurrentRoot -= cost;

        return true;
    }

    public bool BuyRootAbundanceUpgrade()
    {
        if (RootAbundanceLevel >= GameConstants.RootAbundanceMaxLevel)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootAbundanceLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyRootFertilityUpgrade()
    {
        if (RootFertilityLevel >= GameConstants.RootFertilityMaxLevel)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootFertilityLevel++;

        return true;
    }

    public bool BuyRootRetillUpgrade()
    {
        if (RootRetillLevel >= GameConstants.RootRetillMaxLevel)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootRetillLevel++;
        BestProductionPerSecond = Math.Max(BestProductionPerSecond, ProductionPerSecond);

        return true;
    }

    public bool BuyRootSeedBlessingUpgrade()
    {
        if (RootSeedBlessingLevel >= GameConstants.RootSeedBlessingMaxLevel)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        RootSeedBlessingLevel++;

        return true;
    }

    public bool UnlockAutoBuy()
    {
        if (AutoBuyUnlocked)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        AutoBuyUnlocked = true;
        AutoBuyEnabled = false;

        return true;
    }

    public bool SetAutoBuyEnabled(bool enabled)
    {
        if (!AutoBuyUnlocked)
        {
            AutoBuyEnabled = false;
            return false;
        }

        if (AutoBuyEnabled == enabled)
        {
            return false;
        }

        AutoBuyEnabled = enabled;

        return true;
    }

    public bool UnlockAutoRetill()
    {
        if (AutoRetillUnlocked)
        {
            return false;
        }

        if (!TrySpendRoot(1))
        {
            return false;
        }

        AutoRetillUnlocked = true;
        AutoRetillEnabled = false;

        return true;
    }

    public bool SetAutoRetillEnabled(bool enabled)
    {
        if (!AutoRetillUnlocked)
        {
            AutoRetillEnabled = false;
            return false;
        }

        if (AutoRetillEnabled == enabled)
        {
            return false;
        }

        AutoRetillEnabled = enabled;

        return true;
    }

    public bool ProcessAutoBuy()
    {
        if (!AutoBuyUnlocked || !AutoBuyEnabled)
        {
            return false;
        }

        for (int i = Farms.Count - 1; i >= 0; i--)
        {
            Farm farm = Farms[i];

            if (farm.IsMaxLevel)
            {
                continue;
            }

            int affordableLevels = farm.GetAffordableLevels(Potato, 1, FieldCostMultiplier);

            if (affordableLevels <= 0)
            {
                continue;
            }

            int purchasedLevels = BuyFarm(farm, 1);

            return purchasedLevels > 0;
        }

        return false;
    }

    public int ProcessAutoRetill()
    {
        if (!AutoRetillUnlocked || !AutoRetillEnabled)
        {
            return 0;
        }

        int retillCount = 0;

        foreach (Farm farm in Farms)
        {
            if (!farm.CanRetill)
            {
                continue;
            }

            if (RetillFarm(farm))
            {
                retillCount++;
            }
        }

        return retillCount;
    }

    public bool ProcessAutomation()
    {
        bool changed = false;

        // すでにLv上限なら先にRETILLする
        if (ProcessAutoRetill() > 0)
        {
            changed = true;
        }

        // 1回の処理につき購入は1Lvだけ
        if (ProcessAutoBuy())
        {
            changed = true;
        }

        // 今回の+1でLv上限に届いた場合、
        // 同じ処理内でRETILLする
        if (ProcessAutoRetill() > 0)
        {
            changed = true;
        }

        return changed;
    }

    public double GetRootAbundanceMultiplier(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootAbundanceMaxLevel);

        return Math.Pow(GameConstants.RootAbundanceMultiplierPerLevel, safeLevel);
    }

    public double GetRootFertilityMultiplier(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootFertilityMaxLevel);

        return 1.0 - safeLevel * GameConstants.RootFertilityReductionPerLevel;
    }

    public double GetRootAdjustedRetillBase(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootRetillMaxLevel);

        return GameConstants.RetillProductionMultiplier + safeLevel * GameConstants.RootRetillBaseBonusPerLevel;
    }

    public double GetRootSeedBlessingMultiplier(int level)
    {
        int safeLevel = Math.Clamp(level, 0, GameConstants.RootSeedBlessingMaxLevel);
        return Math.Pow(
            GameConstants.RootSeedBlessingMultiplierPerLevel, safeLevel);
    }

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
}