namespace OimoHorihori.Shared.Profiles;

public sealed record TitleSettingsResponse(string? EquippedTitleId, List<TitleOptionResponse> Titles);