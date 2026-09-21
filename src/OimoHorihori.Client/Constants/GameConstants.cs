namespace OimoHorihori.Constants;

public static class GameConstants
{
    public const int SaveVersion = 4;
    public const double BaseProductionPerSecond = 0.1;
    public const double FarmCostMultiplier = 1.005;
    public const double AutoSaveIntervalSeconds = 5;
    public const int GameLoopIntervalMilliseconds = 250;
    public const double ReplantBaseProduction = 9.000e10;
    public const double ProductionUpgradeBonusPerLevel = 0.02;
    public const double DigUpgradeBonusPerLevel = 0.5;
    public const double InitialOfflineLimitSeconds = 28_800;
    public const double OfflineUpgradeSecondsPerLevel = 3_600;
    public const double MaxOfflineLimitSeconds = 86_400;
    public const int OfflineUpgradeMaxLevel = 16;
    public const double RetillEfficiencyBonusPerLevelPerRetill = 0.005;
    public const double FieldCostReductionPerLevel = 0.0025;
    public const int FieldCostReductionMaxLevel = 40;
    public const double OimoOfflineDiscoveryLimitSeconds = 86_400;
    public const double OimoDiscoveryIntervalSeconds = 30;
    public const double OimoDiscoveryChance = 0.00035;
    public const int InitialFarmMaxLevel = 100;
    public const int RetillMaxCount = 10;
    public const int RetillLevelCapBonus = 10;
    public const double RetillProductionMultiplier = 1.25;
}