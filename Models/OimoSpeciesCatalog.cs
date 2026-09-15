namespace OimoHorihori.Models;

public static class OimoSpeciesCatalog
{
    public static IReadOnlyList<OimoSpeciesDefinition>
        All
    { get; } =
        new List<OimoSpeciesDefinition>
        {
            new("oimo_01", "ダンシャクイモ"),
            new("oimo_02", "メークインヌ"),
            new("oimo_03", "キタアカリィ"),
            new("oimo_04", "トヨシロウ"),
            new("oimo_05", "インカノメザメ"),
            new("oimo_06", "ベニアズマァ"),
            new("oimo_07", "ベニハルカナ"),
            new("oimo_08", "シルクスイートォ"),
            new("oimo_09", "アンノウイモン"),
            new("oimo_10", "ムラサキマサリーヌ"),
            new("oimo_11", "コガネセンガンヌ"),
            new("oimo_12", "ナルットキントキ"),
            new("oimo_13", "サトイモォ"),
            new("oimo_14", "ヤマノイーモ"),
            new("oimo_15", "ナガイモーン"),
            new("oimo_16", "黄金いも"),
            new("oimo_17", "透明いも"),
            new("oimo_18", "逆に土"),
            new("oimo_19", "芋ではない何か"),
            new("oimo_20", "オイモザウルス")
        };
}