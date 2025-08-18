using Asp.Versioning;
using MechanicApplication.Features.Customers.DTOs;
using MechanicApplication.Features.Customers.Queries.GetCustomers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicApi.Controllers;

[Route("api/v{version:apiversion}/customers")]
[ApiVersion("1.0")]
[ApiController]
[Authorize]
public class CustomersController(ISender sender) : ApiBaseController

{
    [HttpGet]
    [ProducesResponseType(typeof(List<CustomerDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves all the customers")]
    [EndpointDescription("Returns all the custmers associated with the current user")]
    [EndpointName("GetCustomers")]
    [MapToApiVersion("1.0")]
    [ProducesDefaultResponseType]
    [OutputCache(Duration = 60)]
    public async Task<ActionResult<List<CustomerDTO>>> GetCustomers(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomersQuery(), cancellationToken); 
        return result.Match(
            Ok,
            Problem);
        
    }
}
