namespace OimoHorihori.Shared.Admin;

public sealed record AdminUserResponse(
    Guid UserId,
    string UserName,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? LastLoginAtUtc,
    bool IsDisabled,
    bool IsDeleted);