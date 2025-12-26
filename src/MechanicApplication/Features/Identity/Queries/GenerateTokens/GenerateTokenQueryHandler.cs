using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
//using Microsoft.IdentityModel.Tokens;

namespace MechanicApplication.Features.Identity.Queries.GenerateTokens;

public sealed class GenerateTokenQueryHandler(
    ILogger<GenerateTokenQueryHandler> logger,
    IIdenttiyService identityService,
    ITokenProvidere tokenProvider
    ) : IRequestHandler<GenerateTokenQuery, ErrorOr<TokenResponse>>
{
    private readonly ILogger<GenerateTokenQueryHandler> _logger = logger;
    private readonly IIdenttiyService _identityService = identityService;
    private readonly ITokenProvidere _tokenProvider = tokenProvider;
    public async Task<ErrorOr<TokenResponse>> Handle(GenerateTokenQuery request, CancellationToken cancellationToken)
    {
        var userResponse = await _identityService.AuthenticateAsync(request.Email, request.Password);

        var generatedtoken = await userResponse.ThenAsync(
            async user => await _tokenProvider.GenerateJwtTokenAsync(user, cancellationToken));

        generatedtoken.SwitchFirst(
            _ => { },
            error => _logger.LogError("Failed to generate token: {Error}", error)
        );
        return generatedtoken;
    }
}
