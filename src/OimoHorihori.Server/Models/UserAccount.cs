namespace OimoHorihori.Server.Models;

public class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public string PinHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastLoginAtUtc { get; set; }
    public bool IsDisabled { get; set; }
    public bool IsDeleted { get; set; }
    public string? EquippedTitleId { get; set; }
}