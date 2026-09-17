using OimoHorihori.Shared.Saves;

namespace OimoHorihori.Services;

public sealed record ServerSaveResult(bool Success, bool Conflict, ServerSaveResponse? Save);