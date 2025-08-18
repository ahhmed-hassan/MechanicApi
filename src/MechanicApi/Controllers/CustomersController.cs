using Asp.Versioning;
using MechanicApplication.Features.Customers.DTOs;
using MechanicApplication.Features.Customers.Queries.GetCustoemerByID;
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

    [HttpGet("{customerId:guid}", Name = "GetCustomerById")]
    [ProducesResponseType<CustomerDTO>( StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Retrieves a customer by ID")]
    [EndpointDescription("Returns a customer associated with the current user by ID")]
    [EndpointName("GetCustomerById")]
    [MapToApiVersion("1.0")]
    //It does not worth it to cce for just one customer
    //[OutputCache(Duration =60, VaryByQueryKeys = new[] { "customerId" })]
    public async Task<ActionResult<CustomerDTO>> GetCustomerById(Guid customerId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(customerId), cancellationToken);
        return result.Match(
            Ok,
            Problem);
    }
}
