

using ErrorOr;
using MechanicApplication.Features.Identity.DTOs;
using MechanicApplication.Features.Identity.Queries;
using System.Security.Claims;

namespace MechanicApplication.Common.Interfaces;

//Document

public interface ITokenProvidere
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user information used to generate the token.</param>
    /// <param name="ct">Optional cancellation token.</param>
    /// <returns>
    /// An <see cref="ErrorOr{TokenResponse}"/> containing the generated token response or an error.
    /// </returns>
    Task<ErrorOr<TokenResponse>> GenerateJwtTokenAsync(AppUserDTO user, CancellationToken ct = default);

    /// <summary>
    /// Retrieves the <see cref="ClaimsPrincipal"/> from an expired JWT token.
    /// </summary>
    /// <param name="token">The expired JWT token.</param>
    /// <returns>
    /// The <see cref="ClaimsPrincipal"/> extracted from the token, or <c>null</c> if invalid.
    /// </returns>
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
