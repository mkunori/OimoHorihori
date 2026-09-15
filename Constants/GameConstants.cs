namespace OimoHorihori.Constants;

public static class GameConstants
{
    public const int SaveVersion = 2;

    public const double BaseProductionPerSecond = 0.1;

    public const double FarmCostMultiplier = 1.25;

    public const double OfflineLimitSeconds = 28_800;

    public const double AutoSaveIntervalSeconds = 5;

    public const int GameLoopIntervalMilliseconds = 250;

    public const double ReplantBaseProduction = 10_000_000;

    public const double ProductionUpgradeBonusPerLevel =
    0.005;

    public const double DigUpgradeBonusPerLevel = 0.1;

    public const double InitialOfflineLimitSeconds = 28_800;

    public const double OfflineUpgradeSecondsPerLevel = 1_800;

    public const double MaxOfflineLimitSeconds = 86_400;

    public const int OfflineUpgradeMaxLevel = 32;

    public const double OimoOfflineDiscoveryLimitSeconds = 86_400;

    public const double OimoDiscoveryIntervalSeconds = 30;

    public const double OimoDiscoveryChance = 0.00035;
}