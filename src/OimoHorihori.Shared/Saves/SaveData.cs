namespace OimoHorihori.Shared.Saves;

public class SaveData
{
    public int Version { get; set; } = 4;
    public bool HasStarted { get; set; }
    public double Potato { get; set; }
    public double TotalPotato { get; set; }
    public int Farm1Level { get; set; }
    public int Farm2Level { get; set; }
    public int Farm3Level { get; set; }
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
    public DateTimeOffset LastSaveTimeUtc { get; set; }
    public Dictionary<string, DateTimeOffset> AchievementUnlockedAtUtc { get; set; } = new();
    public Dictionary<string, int> OimoDiscoveryCounts { get; set; } = new();
    public double OimoDiscoveryElapsedSeconds { get; set; }
    public bool HasUsedTenPurchaseMode { get; set; }
    public bool HasUsedMaxPurchaseMode { get; set; }
    public double ShortestReplantSeconds { get; set; }
    public List<ReplantHistoryEntry> ReplantHistory { get; set; } = new();
    public List<FarmSaveData> Farms { get; set; } = new();
    public int FieldEfficiencyUpgradeLevel { get; set; }
    public int FieldCostReductionUpgradeLevel { get; set; }
}