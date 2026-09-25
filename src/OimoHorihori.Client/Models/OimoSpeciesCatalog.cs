namespace OimoHorihori.Models;

public static class OimoSpeciesCatalog
{
    public static IReadOnlyList<OimoSpeciesDefinition> All { get; }
        = new List<OimoSpeciesDefinition>
        {
            new(
                "oimo_01",
                "ダンシャクイモ",
                1.000e10,
                OimoPowerEffectType.AllProductionMultiplier,
                1.05,
                "全生産 ×1.05"),

            new(
                "oimo_02",
                "メークインヌ",
                2.000e10,
                OimoPowerEffectType.FarmProductionMultiplier,
                1.08,
                "全畑生産 ×1.08"),

            new(
                "oimo_03",
                "キタアカリィ",
                4.000e10,
                OimoPowerEffectType.SeedGainMultiplier,
                1.05,
                "種芋獲得 ×1.05"),

            new(
                "oimo_04",
                "トヨシロウ",
                8.000e10,
                OimoPowerEffectType.FieldCostMultiplier,
                0.99,
                "畑価格 ×0.99"),

            new(
                "oimo_05",
                "インカノメザメ",
                1.500e11,
                OimoPowerEffectType.RetillFinalMultiplier,
                1.08,
                "RETILL最終生産 ×1.08"),

            new(
                "oimo_06",
                "ベニアズマァ",
                2.500e11,
                OimoPowerEffectType.AllProductionMultiplier,
                1.05,
                "全生産 ×1.05"),

            new(
                "oimo_07",
                "ベニハルカナ",
                4.000e11,
                OimoPowerEffectType.FarmProductionMultiplier,
                1.08,
                "全畑生産 ×1.08"),

            new(
                "oimo_08",
                "シルクスイートォ",
                7.000e11,
                OimoPowerEffectType.SeedGainMultiplier,
                1.05,
                "種芋獲得 ×1.05"),

            new(
                "oimo_09",
                "アンノウイモン",
                1.000e12,
                OimoPowerEffectType.FieldCostMultiplier,
                0.99,
                "畑価格 ×0.99"),

            new(
                "oimo_10",
                "ムラサキマサリーヌ",
                2.000e12,
                OimoPowerEffectType.RetillFinalMultiplier,
                1.08,
                "RETILL最終生産 ×1.08"),

            new(
                "oimo_11",
                "コガネセンガンヌ",
                4.000e12,
                OimoPowerEffectType.AllProductionMultiplier,
                1.05,
                "全生産 ×1.05"),

            new(
                "oimo_12",
                "ナルットキントキ",
                8.000e12,
                OimoPowerEffectType.FarmProductionMultiplier,
                1.08,
                "全畑生産 ×1.08"),

            new(
                "oimo_13",
                "サトイモォ",
                1.500e13,
                OimoPowerEffectType.SeedGainMultiplier,
                1.05,
                "種芋獲得 ×1.05"),

            new(
                "oimo_14",
                "ヤマノイーモ",
                3.000e13,
                OimoPowerEffectType.FieldCostMultiplier,
                0.99,
                "畑価格 ×0.99"),

            new(
                "oimo_15",
                "ナガイモーン",
                6.000e13,
                OimoPowerEffectType.RetillFinalMultiplier,
                1.08,
                "RETILL最終生産 ×1.08"),

            new(
                "oimo_16",
                "黄金いも",
                1.000e14,
                OimoPowerEffectType.AllProductionMultiplier,
                1.25,
                "全生産 ×1.25"),

            new(
                "oimo_17",
                "透明いも",
                1.500e14,
                OimoPowerEffectType.FieldCostMultiplier,
                0.90,
                "畑価格 ×0.90"),

            new(
                "oimo_18",
                "逆に土",
                2.500e14,
                OimoPowerEffectType.RetillBaseBonus,
                0.01,
                "RETILL基礎倍率 +0.01"),

            new(
                "oimo_19",
                "芋ではない何か",
                4.000e14,
                OimoPowerEffectType.SeedGainMultiplier,
                1.25,
                "種芋獲得 ×1.25"),

            new(
                "oimo_20",
                "オイモザウルス",
                6.000e14,
                OimoPowerEffectType.AllProductionMultiplier,
                2.00,
                "全生産 ×2.00")
        };
}