using OimoHorihori.Constants;

namespace OimoHorihori.Models;

public static class AchievementCatalog
{

    public static IReadOnlyList<AchievementDefinition> All { get; } = new List<AchievementDefinition>
        {
            new(
                "first_dig",
                "はじめの一掘り",
                "初めて「掘る！」を押す",
                game => game.DigButtonCount >= 1),

            new(
                "total_1e3",
                "芋が増えてきた",
                "総生産 1.000e+3 芋",
                game => game.TotalPotato >= 1.0e3),

            new(
                "total_1e6",
                "芋の山",
                "総生産 1.000e+6 芋",
                game => game.TotalPotato >= 1.0e6),

            new(
                "total_1e9",
                "芋の海",
                "総生産 1.000e+9 芋",
                game => game.TotalPotato >= 1.0e9),

            new(
                "total_1e12",
                "芋とは何か",
                "総生産 1.000e+12 芋",
                game => game.TotalPotato >= 1.0e12),

            new(
                "total_1e15",
                "指数表記の世界",
                "総生産 1.000e+15 芋",
                game => game.TotalPotato >= 1.0e15),

            new(
                "farm1_level_10",
                "畑仕事入門",
                "畑1 Lv.10",
                game => game.Farm1BestLevel >= 10),

            new(
                "farm1_level_100",
                "畑1の主",
                "畑1 Lv.100",
                game => game.Farm1BestLevel >= 100),

            new(
                "farm2_first",
                "二枚目の畑",
                "畑2を初購入",
                game => game.Farm2TotalPurchases >= 1),

            new(
                "farm2_level_100",
                "畑2の主",
                "畑2 Lv.100",
                game => game.Farm2BestLevel >= 100),

            new(
                "farm3_first",
                "三枚目の畑",
                "畑3を初購入",
                game => game.Farm3TotalPurchases >= 1),

            new(
                "farm3_level_100",
                "畑3の主",
                "畑3 Lv.100",
                game => game.Farm3BestLevel >= 100),

            new(
                "farm4_first",
                "四枚目の畑",
                "畑4を初購入",
                game => game.Farm4TotalPurchases >= 1),

            new(
                "farm4_level_100",
                "畑4の主",
                "畑4 Lv.100",
                game => game.Farm4BestLevel >= 100),

            new(
                "farm5_first",
                "五枚目の畑",
                "畑5を初購入",
                game => game.Farm5TotalPurchases >= 1),

            new(
                "farm5_level_100",
                "畑5の主",
                "畑5 Lv.100",
                game => game.Farm5BestLevel >= 100),

            new(
                "farm6_first",
                "六枚目の畑",
                "畑6を初購入",
                game => game.Farm6TotalPurchases >= 1),

            new(
                "farm6_level_100",
                "畑6の主",
                "畑6 Lv.100",
                game => game.Farm6BestLevel >= 100),

            new(
                "farm7_first",
                "七枚目の畑",
                "七7を初購入",
                game => game.Farm7TotalPurchases >= 1),

            new(
                "farm7_level_100",
                "畑7の主",
                "畑7 Lv.100",
                game => game.Farm7BestLevel >= 100),

            new(
                "farm8_first",
                "八枚目の畑",
                "畑8を初購入",
                game => game.Farm8TotalPurchases >= 1),

            new(
                "farm8_level_100",
                "畑8の主",
                "畑8 Lv.100",
                game => game.Farm8BestLevel >= 100),

            new(
                "production_1e1",
                "1秒で芋10個",
                "生産速度 1.000e+1 /sec",
                game => game.BestProductionPerSecond >= 1.0e1),


            new(
                "production_1e3",
                "高速HORIHORI",
                "生産速度 1.000e+3 /sec",
                game => game.BestProductionPerSecond >= 1.0e3),

            new(
                "production_1e6",
                "芋工業化",
                "生産速度 1.000e+6 /sec",
                game => game.BestProductionPerSecond >= 1.0e6),

            new(
                "seed_available",
                "最初の種芋",
                "初めて種芋を1個以上獲得可能になる",
                game => game.MaxRunProducedPotato >= GameConstants.ReplantBaseProduction),

            new(
                "replant_1",
                "もう一度掘る",
                "REPLANTを1回行う",
                game => game.ReplantCount >= 1),

            new(
                "replant_10",
                "植え直し職人",
                "REPLANT 10回",
                game => game.ReplantCount >= 10),

            new(
                "replant_100",
                "何度でもHORIHORI",
                "REPLANT 100回",
                game => game.ReplantCount >= 100),

            new(
                "seed_total_10",
                "種芋コレクター",
                "累計種芋10個獲得",
                game => game.TotalSeedPotatoEarned >= 10),

            new(
                "seed_total_100",
                "種芋長者",
                "累計種芋100個獲得",
                game => game.TotalSeedPotatoEarned >= 100),

            new(
                "production_upgrade_1",
                "永久に2%",
                "生産力強化 Lv.1",
                game => game.ProductionUpgradeLevel >= 1),

            new(
                "production_upgrade_10",
                "積み重なる20%",
                "生産力強化 Lv.10",
                game => game.ProductionUpgradeLevel >= 10),

            new(
                "dig_upgrade_10",
                "掘る力",
                "掘り出し強化 Lv.10",
                game => game.DigUpgradeLevel >= 10),

            new(
                "offline_upgrade_32",
                "24時間戦えます",
                "放置強化 Lv.16到達",
                game => game.OfflineUpgradeLevel >= 16),

            new(
                "run_seed_5",
                "まだ植え直さない",
                "1周で種芋5個分まで到達",
                game => game.MaxRunProducedPotato >= GameConstants.ReplantBaseProduction * 25),

            new(
                "run_1e9",
                "長期栽培",
                "1周の生産量 1.000e+21",
                game => game.MaxRunProducedPotato >= 1.0e21),

            new(
                "purchase_ten",
                "まとめ買い",
                "+10購入を初めて使用",
                game => game.HasUsedTenPurchaseMode),

            new(
                "purchase_max",
                "全部ください",
                "MAX購入を初めて使用",
                game => game.HasUsedMaxPurchaseMode),

            new(
                "oimo_first",
                "いも発見！",
                "図鑑で初めていもを発見",
                game => game.TotalOimoDiscoveries >= 1),

            new(
                "oimo_10",
                "いも収集家",
                "図鑑10種類発見",
                game => game.DiscoveredOimoSpeciesCount >= 10),

            new(
                "oimo_complete",
                "いも博士",
                "図鑑20種類発見",
                game => game.DiscoveredOimoSpeciesCount >= 20),

            new(
                "days_7",
                "一週間の芋",
                "ゲーム開始から7日",
                game => HasElapsedDays(game, 7)),

            new(
                "days_30",
                "一か月の芋",
                "ゲーム開始から30日",
                game =>HasElapsedDays(game, 30)),

            new(
                "days_100",
                "百日の芋",
                "ゲーム開始から100日",
                game => HasElapsedDays(game, 100)),

            new(
                "retill_1",
                "耕し直し",
                "初めてRETILLを行う",
                game => game.Farms.Any(
                    farm => farm.RetillCount >= 1)),

            new(
                "retill_10",
                "土づくりの極み",
                "1つの畑でRETILL 10回",
                game => game.Farms.Any(
                    farm => farm.RetillCount >= 10)),

            new(
                "retill_efficiency_10",
                "耕すほど強く",
                "RETILL効率強化 Lv.10",
                game => game.RetillEfficiencyUpgradeLevel >= 10),

            new(
                "field_cost_reduction_10",
                "節約農法",
                "畑コスト軽減 Lv.10",
                game => game.FieldCostReductionUpgradeLevel >= 10),

        };

    private static bool HasElapsedDays(GameState game, int days)
    {
        if (game.GameStartedAtUtc == default)
        {
            return false;
        }

        return DateTimeOffset.UtcNow - game.GameStartedAtUtc >= TimeSpan.FromDays(days);
    }
}