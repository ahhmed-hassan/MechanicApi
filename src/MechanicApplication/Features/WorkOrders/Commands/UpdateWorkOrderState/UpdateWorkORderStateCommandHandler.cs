

using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.WorkOrders.Commands.UpdateWorkOrderState;

public class UpdateWorkORderStateCommandHandler
    (
    IAppDbContext context, 
    ILogger<UpdateWorkORderStateCommandHandler> logger, 
    HybridCache cache, 
    TimeProvider timeProvider
    )
    : IRequestHandler<UpdateWorkOrderState, ErrorOr<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdateWorkORderStateCommandHandler> _logger = logger;
    private readonly HybridCache _cache = cache;
    private readonly TimeProvider _timeProvider = timeProvider;
    public async Task<ErrorOr<Updated>> Handle(UpdateWorkOrderState request, CancellationToken cancellationToken)
    {
        var workOrder = await  _context.WorkOrders.FirstOrDefaultAsync(wo => wo.Id == request.WorkOrderId, cancellationToken);
        if (workOrder is null)
        {
            _logger.LogWarning("Work order with ID {WorkOrderId} not found.", request.WorkOrderId);
            return ApplicationErrors.WorkOrderNotFound; 
        }
        if(workOrder.StartAtUtc > _timeProvider.GetUtcNow() )
        {
            _logger.LogError("State transition for WorkOrder Id '{WorkOrderId}` is not allowed before the work order�s scheduled start time.", request.WorkOrderId);

            return WorkOrderErrors.StateTransitionNotAllowed(workOrder.StartAtUtc); 
        }
        var updaTed = workOrder.UpdateState(request.State);

        return await updaTed
        .ThenDoAsync(async _ =>
        {
            await _cache.RemoveByTagAsync(Constants.Cache.WorkOrders.Single, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            workOrder.AddDomainEvent(new WorkOrderCollectionModified());
            if(workOrder.State == MechanicDomain.WorkOrders.Enums.WorkOrderState.Completed)
            {
                workOrder.AddDomainEvent(new WorkOrderCompleted (workOrder.Id));
            }
        })

         .Else( error =>
         {
             _logger.LogWarning("Failed to update work order state: {Error}", error.First());
             return error;
         })
         ;

        
    }
}
