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
    public int Farm1BestLevel { get; set; }
    public int Farm2BestLevel { get; set; }
    public int Farm3BestLevel { get; set; }
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
        new Farm("畑2", 1.600e3,  1.000e1),
        new Farm("畑3", 2.600e5,  1.000e3),
        new Farm("畑4", 4.200e7,  1.000e5),
        new Farm("畑5", 6.800e9,  1.000e7),
        new Farm("畑6", 1.100e12, 1.000e9),
        new Farm("畑7", 1.800e14, 1.000e11),
        new Farm("畑8", 2.900e16, 1.000e13)
    };
    public double ShortestReplantSeconds { get; set; }
    public List<ReplantHistoryEntry> ReplantHistory { get; } = new();
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

    public double ProductionPerSecond
    {
        get
        {
            double total = BaseProductionPerSecond;
            foreach (Farm farm in Farms)
            {
                total += farm.ProductionPerSecond * GetRetillEfficiencyMultiplier(farm);
            }

            return total * ProductionMultiplier;
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
            Farm1BestLevel = Farm1BestLevel,
            Farm2BestLevel = Farm2BestLevel,
            Farm3BestLevel = Farm3BestLevel,
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
        Farm1BestLevel = save.Farm1BestLevel;
        Farm2BestLevel = save.Farm2BestLevel;
        Farm3BestLevel = save.Farm3BestLevel;
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

    public int ReplantSeedPotato
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

    public bool CanReplant => ReplantSeedPotato >= 1;

    public double NextSeedPotatoRequiredProduction
    {
        get
        {
            int nextSeedPotato = ReplantSeedPotato + 1;

            return GameConstants.ReplantBaseProduction * nextSeedPotato * nextSeedPotato;
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
        return farm.ProductionPerSecond * GetRetillEfficiencyMultiplier(farm);
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

    public double FieldCostMultiplier
    {
        get
        {
            double reduction = FieldCostReductionUpgradeLevel * GameConstants.FieldCostReductionPerLevel;

            reduction = Math.Min(reduction, 0.10);

            return 1.0 - reduction;
        }
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

    public double GetRetillEfficiencyMultiplier(Farm farm)
    {
        return 1.0 + farm.RetillCount * RetillEfficiencyUpgradeLevel * GameConstants.RetillEfficiencyBonusPerLevelPerRetill;
    }
}