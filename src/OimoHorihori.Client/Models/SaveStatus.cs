namespace OimoHorihori.Models;

public enum SaveStatus
{
    None,
    Saving,
    LocalSaved,
    ServerSaved,
    ServerUnavailable,
    Conflict,
    Error
}