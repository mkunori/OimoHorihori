namespace OimoHorihori.Shared.Profiles;

public static class TitleCatalog
{
    public static IReadOnlyList<TitleDefinition> All { get; }
        = new List<TitleDefinition>
        {
            new("first_dig", "はじめの一掘り", "first_dig"),
            new("farm1_level_10", "畑仕事入門", "farm1_level_10"),

            new("farm1_level_100", "畑1の主", "farm1_level_100"),
            new("farm2_level_100", "畑2の主", "farm2_level_100"),
            new("farm3_level_100", "畑3の主", "farm3_level_100"),

            new("production_1e3", "高速HORIHORI", "production_1e3"),
            new("production_1e6", "芋工業化", "production_1e6"),

            new("seed_available", "最初の種芋", "seed_available"),

            new("replant_10", "植え直し職人", "replant_10"),
            new("replant_100", "何度でもHORIHORI", "replant_100"),

            new("seed_total_10", "種芋コレクター", "seed_total_10"),
            new("seed_total_100", "種芋長者", "seed_total_100"),

            new("run_1e9", "長期栽培", "run_1e9"),

            new("days_100", "百日の芋", "days_100")
        };
}