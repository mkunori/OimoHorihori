namespace OimoHorihori.Shared.Profiles;

public sealed record PublicProfileResponse(
    Guid UserId,
    string UserName,
    string? EquippedTitleName,
    double TotalPotato,
    double BestProductionPerSecond,
    int ReplantCount,
    int AscentCount,
    int RootPower,
    int AchievementCount,
    int OimoSpeciesCount,
    DateTimeOffset? GameStartedAtUtc);