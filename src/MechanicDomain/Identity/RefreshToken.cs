using ErrorOr;
using MechanicDomain.Abstractions;
using System.Security.AccessControl;

namespace MechanicDomain.Identity;

public sealed class RefreshToken : AuditableEntity
{
    public string? Token { get; }
    public string? UserId { get; }
    public DateTimeOffset ExpiresOnUtc { get; }

    private RefreshToken()
    { }

    private RefreshToken(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc)
        : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresOnUtc = expiresOnUtc;
    }

    public static ErrorOr<RefreshToken> Create(Guid id, string? token, string? userId, DateTimeOffset expiresOnUtc)
    {
        if (id == Guid.Empty) return RefreshTokenErrors.IdRequired;


        if (string.IsNullOrWhiteSpace(token)) return RefreshTokenErrors.TokenRequired;


        if (string.IsNullOrWhiteSpace(userId)) return RefreshTokenErrors.UserIdRequired;


        if (expiresOnUtc <= DateTimeOffset.UtcNow) return RefreshTokenErrors.ExpiryInvalid;

        return new RefreshToken(id, token, userId, expiresOnUtc);
    }
}
