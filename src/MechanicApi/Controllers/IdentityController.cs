using Asp.Versioning;
using MechanicApplication.Features.Identity.DTOs;
using MechanicApplication.Features.Identity.Queries.GetUserInfo;
using MediatR;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MechanicApi.Controllers;

[Route("identity")]
[ApiVersionNeutral]
public sealed class IdentityController(ISender sender) : ApiBaseController
{
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
    public async Task<IActionResult> GetCurrentUserInfor(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var result = await sender.Send(new GetUserByIdQuery(userId), cancellationToken);
        return result.Match(
            Ok,
            Problem);
    }
}
