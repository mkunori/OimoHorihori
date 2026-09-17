namespace OimoHorihori.Shared.Rankings;

public sealed record RankingResponse(RankingCategory Category, List<RankingEntryResponse> TopEntries, RankingEntryResponse? MyEntry);