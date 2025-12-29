using Asp.Versioning;
using ErrorOr;
using MechanicApplication.Common.Constants;
using MechanicApplication.Features.WorkOrders.Commands.AssignLabor;
using MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderState;
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
    }
}
