namespace OimoHorihori.Models;

public class SaveData
{
    public int Version { get; set; } = 2;

    // 基本状態
    public bool HasStarted { get; set; }

    public double Potato { get; set; }

    public double TotalPotato { get; set; }

    public int Farm1Level { get; set; }

    public int Farm2Level { get; set; }

    public int Farm3Level { get; set; }


    // 周回
    public double RunProducedPotato { get; set; }

    public DateTimeOffset RunStartedAtUtc { get; set; }

    public double MaxRunProducedPotato { get; set; }

    public int ReplantCount { get; set; }


    // 種芋
    public int SeedPotato { get; set; }

    public int TotalSeedPotatoEarned { get; set; }

    public int TotalSeedPotatoSpent { get; set; }

    public int MaxSeedPotatoPerReplant { get; set; }


    // 恒久強化
    public int ProductionUpgradeLevel { get; set; }

    public int DigUpgradeLevel { get; set; }

    public int OfflineUpgradeLevel { get; set; }


    // 芋統計
    public double TotalConsumedPotato { get; set; }

    public double BestProductionPerSecond { get; set; }


    // 畑統計
    public int Farm1TotalPurchases { get; set; }

    public int Farm2TotalPurchases { get; set; }

    public int Farm3TotalPurchases { get; set; }

    public int Farm1BestLevel { get; set; }

    public int Farm2BestLevel { get; set; }

    public int Farm3BestLevel { get; set; }


    // オフライン統計
    public double TotalOfflineProducedPotato { get; set; }

    public double MaxOfflineProducedPotato { get; set; }


    // 操作統計
    public int DigButtonCount { get; set; }


    // 時間統計
    public DateTimeOffset GameStartedAtUtc { get; set; }

    public double TotalPlayTimeSeconds { get; set; }


    // セーブ
    public DateTimeOffset LastSaveTimeUtc { get; set; }

    public Dictionary<string, DateTimeOffset> AchievementUnlockedAtUtc { get; set; } = new();

    public Dictionary<string, int> OimoDiscoveryCounts { get; set; } = new();

    public double OimoDiscoveryElapsedSeconds { get; set; }

    public bool HasUsedTenPurchaseMode { get; set; }

    public bool HasUsedMaxPurchaseMode { get; set; }
}