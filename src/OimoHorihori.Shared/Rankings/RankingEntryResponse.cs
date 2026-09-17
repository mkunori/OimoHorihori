namespace OimoHorihori.Shared.Rankings;

public sealed record RankingEntryResponse(Guid UserId, int Rank, string UserName, double Value, bool IsCurrentUser);