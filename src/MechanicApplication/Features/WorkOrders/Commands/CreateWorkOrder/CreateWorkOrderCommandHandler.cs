

using ErrorOr;
using MechanicApplication.Common.Errors;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.WorkOrders.Dtos;
using MechanicApplication.Features.WorkOrders.Mappers;
using MechanicDomain.Customers.Vehicles;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MechanicApplication.Features.WorkOrders.Commands.CreateWorkOrder;

public sealed class CreateWorkOrderCommandHandler(
    ILogger<CreateWorkOrderCommandHandler> logger,
    IWorkOrderPolicy workOrderPolicy,
    IAppDbContext appDbContext,
    HybridCache cache
    ) : IRequestHandler<CreateWorkOrderCommand, ErrorOr<Dtos.WorkOrderDTO>>
{
    private readonly ILogger<CreateWorkOrderCommandHandler> _logger = logger;
    private readonly IWorkOrderPolicy _workOrderPolicy = workOrderPolicy;
    private readonly IAppDbContext _appDbContext = appDbContext;
    private readonly HybridCache _cache = cache;
    public async Task<ErrorOr<Dtos.WorkOrderDTO>> Handle(CreateWorkOrderCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await _appDbContext.Vehicles
            .FirstOrDefaultAsync(v => v.Id == request.VehicleId, cancellationToken);
        if (vehicle is null)
        {
            _logger.LogError("Vehicle with Id {VehicleId} not found", request.VehicleId);
            return ApplicationErrors.VehicleNotFound;
        }


        var labor = await _appDbContext.Employees
            .FirstOrDefaultAsync(e => e.Id == request.LaborId, cancellationToken);
        if (labor is null)
        {
            _logger.LogError("Labor with Id {LaborId} not found", request.LaborId);
            return ApplicationErrors.LaborNotFound;
        }

        var repaitrTasks = await _appDbContext.RepairTasks
            .Where(rt => request.RepairTasksIds.Contains(rt.Id))
            .ToListAsync(cancellationToken);
        var endAt = request.StartAt.AddMinutes(repaitrTasks.Sum(rt => (double)rt.EstimatedDurationInMins));

        if (_workOrderPolicy.IsOutsideOperatingHours(request.StartAt, endAt))
        {
            _logger.LogError("WorkOrder time {StartAt} to {EndAt} is outside operating hours", request.StartAt, endAt);
            return ApplicationErrors.WorkOrderOutsideOperatingHour(request.StartAt, endAt);
        }

        //Vechicle can belong to many workOrders, but of course only one of them at any specific time
        var theSameVehicleIsAssignedToSomeWorkOrderAtOverlappingTime = await _appDbContext.WorkOrders
            .AnyAsync(wo =>
            wo.VehicleId == request.VehicleId &&
            wo.StartAtUtc < endAt &&
            wo.EndAtUtc > request.StartAt
            , cancellationToken
            );

        if(theSameVehicleIsAssignedToSomeWorkOrderAtOverlappingTime)
        {
            _logger.LogError("Vehicle with Id '{VehicleId}' already has an overlapping WorkOrder.", request.VehicleId);
            return Error.Conflict(
               code: "Vehicle_Overlapping_WorkOrders",
               description: "The vehicle already has an overlapping WorkOrder for the given start and end UTC time.");
        }

        var isLaborOccupied = await _workOrderPolicy.IsLaborOccupied(
            request.LaborId,
            null,
            request.StartAt,
            endAt);
        if (isLaborOccupied)
        {
            _logger.LogError("Labor with Id {LaborId} is occupied between {StartAt} and {EndAt}", request.LaborId, request.StartAt, endAt);
            return ApplicationErrors.LaborOccupied;
        }

        ErrorOr<Success> fullfillMinumumDuration =
            _workOrderPolicy.ValidateMinimumRequirement(request.StartAt, endAt);

        var isSpotAvailable = await
            fullfillMinumumDuration
            .ThenAsync(_ =>
                _workOrderPolicy.CheckSpotAvailabilityAsync(request.Spot, request.StartAt, endAt,
                 null, cancellationToken)
            );

        var workOrderResult = isSpotAvailable
        .Then(_ =>
        WorkOrder.Create(
        Guid.NewGuid(),
        request.VehicleId,
        request.StartAt,
        laborId: request.LaborId,
        spot: request.Spot,
        repairTasks: repaitrTasks,
        endAt: endAt
    ));
        return await workOrderResult

           .ThenDoAsync(async workOrder =>
           {
               _appDbContext.WorkOrders.Add(workOrder);
               workOrder.AddDomainEvent(new WorkOrderCollectionModified());
               await _appDbContext.SaveChangesAsync(cancellationToken);
               _logger.LogInformation("WorkOrder with Id {WorkOrderId} was created", workOrder.Id);
               await _cache.RemoveByTagAsync(Constants.Cache.WorkOrders.Single, cancellationToken);
           }
           )
           .ThenDo(workOrder => { workOrder.Vehicle = vehicle; })
           .Then(WorkOrderMapper.ToDto)
           .Else(error =>
            {
                _logger.LogError("Failed to create Workorder {Error}", error.First().Description);
                return error;
            })
            ;


    }
}
