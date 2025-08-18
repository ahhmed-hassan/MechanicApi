using Asp.Versioning;
using ErrorOr;
using MechanicApplication.Features.Customers.Commands.CreateCustomer;
using MechanicApplication.Features.Customers.Commands.UpdateCustomer;
using MechanicApplication.Features.Customers.DTOs;
using MechanicApplication.Features.Customers.Queries.GetCustoemerByID;
using MechanicApplication.Features.Customers.Queries.GetCustomers;
using MechanicContracts.Requests.Customers;
using MechanicDomain.Identity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using System.Collections.Immutable;

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
    [ProducesResponseType<CustomerDTO>(StatusCodes.Status200OK)]
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
    [HttpPost]
    [Authorize(Policy = "ManagerOnly")]
    [ProducesResponseType<CustomerDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new customer")]
    [EndpointDescription("Creates a new customer ")]
    [EndpointName("CreateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<CustomerDTO>> CreateCustomer([FromBody] CreateCustomerRequest command, CancellationToken cancellationToken)
    {
        var vehiclesCommands = command.Vehicles.Select(v => new CreateVehicleCommand(
            v.Make,
            v.Model,
            v.Year,
            v.LicensePlate)).ToList();

        var result = await sender.Send(
            new CreateCustomerCommand(
                command.Name,
                command.PhoneNumber,
                command.Email,
                vehiclesCommands)
            , cancellationToken);

        var currentVersion = HttpContext.GetRequestedApiVersion()!.ToString()?? "1.0";

        return result.Match(
            customer => CreatedAtRoute("GetCustomerById", new { version = currentVersion,  customerId = customer.CustomerId }, customer),
            Problem);
    }
    [HttpPut("{customerId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType<CustomerDTO>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing customer")]
    [EndpointDescription("Updates an existing customer associated with the current user")]
    [EndpointName("UpdateCustomer")]
    [MapToApiVersion("1.0")]
    public async Task<ActionResult<Updated>> Update(Guid customerId, [FromBody] UpdateCustomerRequest request, CancellationToken cancellationToken)
    {
        var updateVehiclesRequest = request.Vehicles.Select(v => new UpdateVehicleCommand(
            v.VehicleId,
            v.Make,
            v.Model,
            v.Year,
            v.LicensePlate)).ToList();
        var result = await sender.Send(new UpdateCustomerCommand(
            customerId,
            request.Name,
            request.PhoneNumber,
            request.Email,
            updateVehiclesRequest.ToImmutableList())
            , cancellationToken);
        return result.Match(
           _ => NoContent(),
            Problem);
    }
}
