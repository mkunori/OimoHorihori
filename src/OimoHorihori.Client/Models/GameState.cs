

using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Models;

public partial class GameState
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
        new Farm("畑2", 1.000e2,  1.000e0),
        new Farm("畑3", 1.000e3,  1.000e1),
        new Farm("畑4", 8.000e3,  1.000e2),
        new Farm("畑5", 6.000e4,  1.000e3),
        new Farm("畑6", 5.000e5,  1.000e4),
        new Farm("畑7", 3.000e6,  1.000e5),
        new Farm("畑8", 2.500e7,  1.000e6)
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
    public HashSet<string> UnlockedOimoPowerIds { get; } = new();
    public double TotalOimoPowerSpentPotato { get; set; }
}