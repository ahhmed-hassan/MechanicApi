using Asp.Versioning;
using MechanicApplication.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicApplication.Features.RepairTasks.DTOs;
using MechanicApplication.Features.RepairTasks.Queries.GetRepairTasks;
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

    private object GetById()
    {
        throw new NotImplementedException();
    }
}
