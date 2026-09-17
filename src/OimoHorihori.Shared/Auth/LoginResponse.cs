namespace OimoHorihori.Shared.Auth;

public sealed record LoginResponse(Guid UserId, string UserName, string SessionToken, DateTimeOffset ExpiresAtUtc);