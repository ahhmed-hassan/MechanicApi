using Asp.Versioning;
using MechanicApplication.Features.Labors.DTOs;
using MechanicApplication.Features.Labors.Queries.GetLabors;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicApi.Controllers;

[Route("api/v{version:apiVersion}/labors")]
[ApiVersion("1.0")]
[Authorize]
public sealed class LaborsController (ISender sender) : ApiBaseController
{

    [HttpGet]

    [OutputCache(Duration = 60)]
    [ProducesResponseType<List<LaborDTO>>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a lsit of available labors")]
    [EndpointDescription("Returning all labor records labor records associated with the system, accessible only to users with the Manager role")]

    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var laborsQuery = new GetLaborsQuery();
        var result =  await sender.Send(laborsQuery, ct);
        return result.Match(
            labors =>  Ok(labors),
            Problem);
        
    }

}
