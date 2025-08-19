using Asp.Versioning;
using MechanicApplication.Features.Identity.DTOs;
using MechanicApplication.Features.Identity.Queries;
using MechanicApplication.Features.Identity.Queries.GenerateTokens;
using MechanicApplication.Features.Identity.Queries.GetUserInfo;
using MechanicApplication.Features.Identity.Queries.RefreshTokens;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;
using System.Transactions;

namespace MechanicApi.Controllers;

[Route("identity")]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender) : ApiBaseController
{

    /// <summary>
    /// Generates a token for the user.
    /// 
    /// 
    [HttpPost("token/generate")]
    [ProducesResponseType<TokenResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Generate an accesss and refresh token for the user")]
    [EndpointDescription("Authenticates a user using provided credentials and returns a JWT token pair.")]
    [EndpointName("GenerateToken")]
 
    public async Task<ActionResult<TokenResponse>> GenerateTokenQuery([FromBody] GenerateTokenQuery request,
        CancellationToken cancellation)
    {
        var result = await sender.Send(request, cancellation);
        return result.Match(Ok,Problem);
    }
    [HttpPost("token/refresh-token")]
    [ProducesResponseType<TokenResponse>( StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>( StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Refreshes access token using a valid refresh token.")]
    [EndpointDescription("Exchanges an expired access token and a valid refresh token for a new token pair.")]
    [EndpointName("RefreshToken")]
    public async Task<ActionResult<TokenResponse>> RefreshTokenQuery(
        [FromBody] RefreshTokenQuery request,
        CancellationToken cancellation)
    {
        var result = await sender.Send(request, cancellation);
        return result.Match(Ok, Problem);
    }
    /// <summary>
    /// Returns user information for the currently authenticated user.
    /// </summary>
    /// <returns>An ActionResult containing the user information or an error.</returns>
    [HttpGet("current-user/claims")]
    [Authorize]
    [ProducesResponseType(typeof(AppUserDTO), StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointDescription("Get current user claims Returns the claims of the currently authenticated user.")]
    [EndpointSummary("Get the current authenticated user's claims.")]
    [EndpointName("GetCurrentUserClaims")]
    public async Task<ActionResult<AppUserDTO>> GetCurrentUserInfor(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.Match(
            Ok,
            Problem);
    }
}
