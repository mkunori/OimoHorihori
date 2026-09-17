namespace OimoHorihori.Shared.Saves;

public sealed record ServerSaveResponse(long Revision, SaveData Save, DateTimeOffset SavedAtUtc);