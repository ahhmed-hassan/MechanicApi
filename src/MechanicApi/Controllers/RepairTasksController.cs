using Asp.Versioning;
using ErrorOr;
using MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicApplication.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicApplication.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicApplication.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicApplication.Features.RepairTasks.Queries.GetRepiarTaskById;
using MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicContracts.Requests.RepairTasks;
using MechanicDomain.Identity;
using MechanicDomain.RepairTasks.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace MechanicApi.Controllers;

[Route("api/v{version:apiversion}/repair-tasks")]
[ApiVersion("1.0")]
[Authorize]
public sealed class RepairTasksController(ISender sender) : ApiBaseController
{
    [HttpGet]
    [ProducesResponseType<List<RepairTaskDTO>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets all repair tasks")]
    [EndpointDescription("Gets all repair tasks including parts")]
    [EndpointName("GetRepairTasks")]
    [MapToApiVersion("1.0")]
    [OutputCache(Duration = 60)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRepairTasksQuery(), cancellationToken);
        return result
                .Match(Ok, Problem);
    }
    [HttpPost]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType<RepairTaskDTO>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Creates a new repair task")]
    [EndpointDescription("Creates a repair task and optionallz includes parts")]
    [EndpointName("CreateRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Create([FromBody] CreateRepairTaskRequest request, CancellationToken cancellation)
    {
        var createPartCommands = request.Parts
            .ConvertAll(p => new CreateRepairTaskPaskPartCommand(p.Name, p.Cost, p.Quantity));
        var createRepairTaskCommand = new CreateRepairTaskCommand(
            request.Name,
            request.LaborCost,
            request.EstimatedDurationInMins is not null ? (RepairDurationInMinutes)request.EstimatedDurationInMins : null,
            createPartCommands);

        var result = await sender.Send(createRepairTaskCommand, cancellation);
        return result.Match(
            response => CreatedAtAction(
                actionName: nameof(GetById),
                routeValues: new { repairTaskId = response.RepairTaskId },
                value: response),
            Problem
            );

    }
    [HttpPut("{repairTaskId: guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType<RepairTaskDTO>(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Updates an existing repait task")]
    [EndpointDescription("Updates a repair task and its associated parts")]
    [EndpointName("UpdateRepairTask")]
    public async Task<IActionResult> Update(Guid repairTaskId, [FromBody] UpdateRepairTaskRequest request, CancellationToken cancellationToken)
    {
        var commandParts = request.Parts
            .ConvertAll(p => new UpdateRepairTaskPartCommand(PartId: p.PartId, Name: p.Name, Cost: p.Cost, Quantity: p.Quantity));
        var updateCommand = new UpdateRepairTaskCommand(
            repairTaskId,
            request.Name,
            request.LaborCost,
            (RepairDurationInMinutes)request.EstimatedDurationInMins,
            commandParts
            );
        var result = await sender.Send(updateCommand, cancellationToken);
        return result.Match(response => Ok(response), Problem);


    }

    [HttpGet("{repairTaskId:guid}", Name = nameof(GetById))]
    [ProducesResponseType<RepairTaskDTO>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Gets a repair task by id")]
    [EndpointDescription("Gets a repair task by id including parts")]
    [EndpointName("GetRepairTaskById")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetById(Guid repairTaskId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId), cancellationToken);
        return result.Match(Ok, Problem);
    }

    [HttpDelete("{repairTaskId:guid}")]
    [Authorize(Roles = nameof(Role.Manager))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Deletes a repair task by id")]
    [EndpointDescription("Deletes a repair task by id")]
    [EndpointName("DeleteRepairTask")]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> Delete(Guid repairTaskId, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RemoveRepairTaskCommand(repairTaskId), cancellationToken);
        return result.Match(_ => NoContent(), Problem);
    }

    
}
