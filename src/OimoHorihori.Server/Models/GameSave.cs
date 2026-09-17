namespace OimoHorihori.Server.Models;

public class GameSave
{
    public Guid UserId { get; set; }
    public long Revision { get; set; }
    public string SaveJson { get; set; } = string.Empty;
    public DateTimeOffset UpdatedAtUtc { get; set; }
}