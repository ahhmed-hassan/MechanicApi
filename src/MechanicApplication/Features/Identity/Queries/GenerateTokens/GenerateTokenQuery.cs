using ErrorOr;
using MediatR;

namespace MechanicApplication.Features.Identity.Queries.GenerateTokens;

public sealed record GenerateTokenQuery(string Email , string Password): IRequest<ErrorOr<TokenResponse>>;

