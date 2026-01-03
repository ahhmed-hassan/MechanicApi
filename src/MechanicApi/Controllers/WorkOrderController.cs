using Asp.Versioning;
using ErrorOr;
using MechanicApi.ContractDomainMapping;
using MechanicApplication.Common.Constants;
using MechanicApplication.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicApplication.Features.WorkOrders.Commands.AssignLabor;
using MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicApplication.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderState;
using MechanicApplication.Features.WorkOrders.Dtos;
using MechanicContracts.Requests.WorkOrders;
using MechanicContracts.Shared;
using MechanicDomain.Identity;
using MechanicInfrastructure.Identity.Policies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.AccessControl;

namespace MechanicApi.Controllers
{
    [Authorize]
    [Route("api/v{version:apiVersion}/workorders")]
    [ApiVersion("1.0")]
    public class WorkOrderController(ISender sender) : ApiBaseController
    {

        [HttpPost]
        [Authorize(Policy = nameof(Role.Manager))]
        [ProducesResponseType<WorkOrderDTO>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Create a new work order")]
        [EndpointDescription("Create a new work order by providing spot, vehicle ID, start time, repair tasks, and labor ID. Only managers can perform this action.")]
        [EndpointName("CreateWorkOrder")]
        public async Task<ActionResult<WorkOrderDTO>> Create([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
        {
            var command = new CreateWorkOrderCommand(
                request.Spot.ToDomainSpot(),
                request.VehicleId,
                request.StartAtUtc,
                request.RepairTasksIds,
                request.LaborId
                );
            var result = await sender.Send(command, cancellationToken);
            var currentApiVersion = HttpContext.GetRequestedApiVersion()?.ToString();
            return result.Match( 
                workOrder => CreatedAtAction(
                    "GetWorkOrderById",
                    new { workOrderId = workOrder.WorkOrderId, apiVersion = currentApiVersion },
                    workOrder
                    ),
                Problem);
        }

        [HttpPost("{workOrderId:guid}/relocate")]
        [ProducesResponseType<NoContentResult>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [EndpointSummary("Relocate a work order to a new time and spot")]
        [EndpointDescription("Relocate a work order to a new time and spot, ensuring no conflicts with existing schedules. Only managers can do this action")]
        [EndpointName("RescheduleWorkOrder")]

        public async Task<ActionResult<NoContentResult>> Relocate(Guid workOrderId, RelocateWorkOrderRequest request, CancellationToken cancellationToken)
        {
            var relocateCommand = new RelocateWorkOrderCommand(workOrderId, request.NewStart, request.newSpot.ToDomainSpot());
            var result = await sender.Send(relocateCommand, cancellationToken);
            return result.Match(_ => NoContent(), Problem);

        }


        [HttpPut("{workOrderId:guid}/labor")]
        [Authorize(Policy = nameof(Role.Manager))]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
        [EndpointSummary("Assigns a labor to a workorder")]
        [EndpointDescription("assigning some work order (With the same time and spot and everything) to another labor")]
        [EndpointName("AssignLaborToWorkOrder")]
        
        public async Task<ActionResult<Updated>> AssignLabor(Guid workOrderId, AssignLaborRequest request, CancellationToken ct)
        {
            var assignLaborCommand = new AssignLaborCommand(workOrderId, Guid.Parse(request.LaborId));
            var result = await sender.Send(assignLaborCommand);
            return result.Match(_ => NoContent(), Problem);
        }

        [HttpPut("{workOrderId:guid}/state")]
        [Authorize(Policy = AuhtorizationConstants.SelfScopedWorkedOrderAccess)]
        [Authorize(Roles = nameof(Role.Labor) + "," + nameof(Role.Manager))]
        [ProducesResponseType<Updated>(StatusCodes.Status200OK)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [EndpointSummary("Updates the state of a work order")]
        [EndpointDescription("Updates the state of a work order, such as from 'Scheduled' to 'In Progress' or 'Completed' The due start date should not be of course in the future")]
        [EndpointName("UpdateWorkOrderState")]
        public async Task<ActionResult<Updated>> UpdateState(Guid workOrderId,UpdateWorkOrderStateRequest request , CancellationToken ct)
        {
            var updateWorkOrderCommand = new UpdateWorkOrderState(workOrderId,
                                                                  (MechanicDomain.WorkOrders.Enums.WorkOrderState)request.State);
            var result = await sender.Send(updateWorkOrderCommand, ct);
            return result.Match (_ => NoContent(), Problem);
            
        }

        [HttpPut("{workorderId:guid}/repair-tasks")]
        [Authorize(Policy = nameof(Role.Manager))]
        [ProducesResponseType<NoContentResult>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [EndpointSummary("Update repair tasks associated with a work order")]
        [EndpointDescription("Update the list of repair tasks associated with a specific work order. Only managers can perform this action.")]
        [EndpointName("UpdateWorkOrderRepairTasks")]
        public async Task<ActionResult<NoContentResult>> UpdateRepairTasks(Guid workorderId, ModifyRepairTasksRequest request, CancellationToken ct)
        {
            var updateRepairTasksCommand = new UpdateWorkOrderRepairTasksCommand(
                                                workorderId, request.RepairTasksIds);
            var result = await sender.Send(updateRepairTasksCommand, ct);
            return result.Match(_ => NoContent(), Problem);
        }
        [HttpDelete("{workOrderId:guid}")]
        [Authorize(Policy = nameof(Role.Manager))]
        [ProducesResponseType<NoContentResult>(StatusCodes.Status204NoContent)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
        [EndpointSummary("Delete a work order")]
        [EndpointName("DeleteWorkOrder")]
        [EndpointDescription("Delete a specific work order by its ID. Only managers have the authority to delete work orders.")]
        public async Task<ActionResult<NoContentResult>> Delete(Guid workOrderId, CancellationToken ct)
        {
            var deleteWorkOrderCommand = new DeleteWorkOrderCommand(workOrderId);
            var result = sender.Send(deleteWorkOrderCommand, ct);
            return await result.Match(_ => NoContent(), Problem);
        }
        
    }
}
