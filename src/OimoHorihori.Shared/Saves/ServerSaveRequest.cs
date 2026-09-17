namespace OimoHorihori.Shared.Saves;

public sealed record ServerSaveRequest(long Revision, SaveData Save);