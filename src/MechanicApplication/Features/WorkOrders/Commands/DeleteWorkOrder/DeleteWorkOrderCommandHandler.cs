using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.WorkOrders.Commands.DeleteWorkOrder;

public sealed class DeleteWrokOrderCommandHandler
    (
    IAppDbContext context,
    ILogger<DeleteWorkOrderCommand> logger,
    HybridCache cache
    )
    : IRequestHandler<DeleteWorkOrderCommand, ErrorOr<Deleted>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<DeleteWorkOrderCommand> _logger = logger;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<Deleted>> Handle(DeleteWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var workOrder = await _context.WorkOrders
            .FirstOrDefaultAsync(workOrder => workOrder.Id == request.workOrderId, cancellationToken);

        if (workOrder is null)
        {
            _logger.LogError("WorkOrder with ID {WorkOrderId} not found for deletion.", request.workOrderId);
            return ApplicationErrors.WorkOrderNotFound;
        }

        if (workOrder.State is not WorkOrderState.Scheduled)
        {
            _logger.LogError(
                "Deletion failed: only 'Scheduled'  WorkOrders can be deleted. Current status: {Status}",
                workOrder.State);

            return WorkOrderErrors.Readonly;
        }

        _context.WorkOrders.Remove(workOrder);
        workOrder.AddDomainEvent(new MechanicDomain.WorkOrders.Events.WorkOrderCollectionModified());
        await _context.SaveChangesAsync(cancellationToken);
        await _cache.RemoveAsync(Constants.Cache.WorkOrders.Single, cancellationToken);
        return new Deleted();

    }
}
