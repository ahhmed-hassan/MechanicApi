namespace MechanicApplication.Features.Identity.Queries;

public sealed record TokenResponse(string? AccessToken, string? RefreshToken, DateTime ExpiresOnUtc);

