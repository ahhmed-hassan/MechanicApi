using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.Identity.Queries.RefreshTokens;

public sealed record RefreshTokenQuery(string RefreshToken, string ExpiredAccessToken) : IRequest<ErrorOr<TokenResponse>>;

