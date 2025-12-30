
using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicDomain.Abstractions;
using MechanicDomain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.WorkOrders.Commands.RelocateWorkOrder
{
    public class RelocateWorkOrderCommandHandler(
        IAppDbContext context, 
        ILogger<RelocateWorkOrderCommandHandler> logger, 
        IWorkOrderPolicy workOrderPolicy, 
        HybridCache cache

        ) : IRequestHandler<RelocateWorkOrderCommand, ErrorOr<Updated>>

    {
        private readonly IAppDbContext _context = context;
        private readonly ILogger<RelocateWorkOrderCommandHandler> _logger = logger;
        private readonly HybridCache _cache = cache;
        private readonly IWorkOrderPolicy _availabilityChecker = workOrderPolicy;    
        public async Task<ErrorOr<Updated>> Handle(RelocateWorkOrderCommand request, CancellationToken cancellationToken)
        {
            var workOrder = await _context.WorkOrders
                .Include(wo => wo.RepairTasks)
                .Include(wo => wo.Labor)
                .Include(wo => wo.Vehicle)
                .FirstOrDefaultAsync(wo => wo.Id == request.Id, cancellationToken);
            if (workOrder is null)
            {
                _logger.LogError("Work order with ID {WorkOrderId} not found.", request.Id);
                return ApplicationErrors.WorkOrderNotFound; 
            }
            var duration = workOrder.EndAtUtc.Subtract(workOrder.StartAtUtc).Duration();
            var endAt = request.NewStartAt.Add(duration);
            var available = await _availabilityChecker.CheckSpotAvailabilityAsync(
                request.NewSpot, 
                request.NewStartAt, 
                endAt, 
                workOrder.Id, 
                cancellationToken);
            available.Switch(_ => { },
                _ => _logger.LogError("spot {Spot} is not available", workOrder.Spot.ToString())
                );
            return await available.FailIf(_ =>  _availabilityChecker.IsLaborOccupied(workOrder.LaborId,
                                                                  workOrder.Id,
                                                                  workOrder.StartAtUtc,
                                                                  endAt).Result, ApplicationErrors.LaborOccupied)
                .FailIf(_ => _availabilityChecker.IsVehicleAlreadyScheduled(workOrder.VehicleId,
                                                                      workOrder.StartAtUtc,
                                                                      endAt).Result
                , ApplicationErrors.VehicleSchedulingConflict)
                .Then(_ => workOrder.UpdateTiming(request.NewStartAt))
                .Else(error => { _logger.LogError("Failed to update timing {Error} ", error.First()); return error; })
                .Then(_ => workOrder.UpdateSpot(request.NewSpot))
                .Else(error => { _logger.LogError("Failed to update spot : {Error}", error.First()); return error; })
                .ThenDoAsync(async _ => 
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    workOrder.AddDomainEvent(new WorkOrderCollectionModified());
                    await _cache.RemoveByTagAsync(Constants.Cache.WorkOrders.Single, cancellationToken);
                });

            
        }
    }
}
