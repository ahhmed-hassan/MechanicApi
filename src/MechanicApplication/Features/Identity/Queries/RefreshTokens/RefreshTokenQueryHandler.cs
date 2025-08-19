using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace MechanicApplication.Features.Identity.Queries.RefreshTokens;

public sealed class RefreshTokenQueryHandler(
    ILogger<RefreshTokenQueryHandler> logger,
    IIdenttiyService identtiyService, 
    ITokenProvidere tokenProvider, 
    IAppDbContext appDbContext)
    : IRequestHandler<RefreshTokenQuery, ErrorOr<TokenResponse>>
{
    private readonly ILogger<RefreshTokenQueryHandler> _logger = logger; 
    private readonly IIdenttiyService _identtiyService = identtiyService;
    private readonly ITokenProvidere _tokenProvider = tokenProvider;
    private readonly IAppDbContext _appDbContext = appDbContext;

    /// <summary>
    /// Handles the refresh token request by validating the provided refresh token,
    /// retrieving the associated user, and generating a new access token if valid.
    /// </summary>
    /// <param name="request">The refresh token query containing the refresh token.</param>
    /// <param name="cancellationToken">A cancellation token for the async operation.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TokenResponse}"/> containing the new token response if successful,
    /// or an error if validation fails.
    /// </returns>
    public async Task<ErrorOr<TokenResponse>> Handle(RefreshTokenQuery request, CancellationToken cancellationToken)
    {
        // Extract principal from the expired token
        var principal = _tokenProvider.GetPrincipalFromExpiredToken(request.RefreshToken);
        if (principal is null)
        {
            _logger.LogError("Invalid refresh token provided.");
            return ApplicationErrors.ExpiredAccessTokenInvalid;
        }

        // Retrieve user ID from token claims
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if ((userId is null))
        {
            _logger.LogError("Invlaid userId claim" );
            return ApplicationErrors.UserIdClaimInvalid;
        }

        // Fetch user by ID
        var user = await _identtiyService.GetUserByIdAsync(userId);
        if(user.IsError)
        {
            _logger.LogError("Failed to retrieve user with ID {UserId}: {Error}", userId, user.FirstError.Description);
            return user.Errors;
        }

        // Validate refresh token from database
        var refreshToken = await _appDbContext.RefreshTokens
            .FirstOrDefaultAsync(refreshToken => refreshToken.Token == request.RefreshToken && refreshToken.UserId == userId
            , cancellationToken);
        if (refreshToken is null || refreshToken.ExpiresOnUtc< DateTime.UtcNow)
        {
            _logger.LogError("Refresg token has expired");
            return ApplicationErrors.RefreshTokenExpired;
            
        }
        // Generate new access token for the user
        var newAccessToken = await _tokenProvider.GenerateJwtTokenAsync(user.Value);
        newAccessToken.SwitchFirst(
            token => { }, 
            error => _logger.LogError("Failed to generate new access token for user {UserId}: {Error}", userId, error.Description)
        );
        return newAccessToken;


    }
}
