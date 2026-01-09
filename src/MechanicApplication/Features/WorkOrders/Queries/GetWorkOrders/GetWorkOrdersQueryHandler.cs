using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Common.Models;
using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.WorkOrders.Dtos;
using MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders.Enums;
using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace MechanicApplication.Features.WorkOrders.Queries.GetWorkOrders;

public sealed class GetWorkOrdersQueryHandler(
    IAppDbContext context, 
    ILogger<GetWorkOrdersQueryHandler> logger
    )
    
    : IRequestHandler<GetWorkOrdersQuery, ErrorOr<PaginatedList<WorkOrderListItemDTO>>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetWorkOrdersQueryHandler> _logger = logger;
    public async Task<ErrorOr<PaginatedList<WorkOrderListItemDTO>>> Handle(GetWorkOrdersQuery request, CancellationToken cancellationToken)
    {
        var workOrdersQuery = _context.WorkOrders.AsNoTracking()
            .Include(wo => wo.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(wo => wo.Labor)
            .Include(wo => wo.Invoice)
            .Include(wo => wo.RepairTasks)
                .ThenInclude(rt => rt.Parts)
            .AsQueryable()
                ;
        workOrdersQuery = ApplyFilters(workOrdersQuery, request);
        workOrdersQuery = ApplySorting(workOrdersQuery, request.SortColumn, request.SortDirection);
        var count = await workOrdersQuery.CountAsync(cancellationToken: cancellationToken);

        var items = await workOrdersQuery
              .Skip((request.Page - 1) * request.PageSize)
              .Take(request.PageSize)
              .Select(wo => new WorkOrderListItemDTO(wo)
                     )
            .ToListAsync(cancellationToken);
        return new PaginatedList<WorkOrderListItemDTO>
        (
            PageNumber : request.Page,
            PageSize : request.PageSize,
            TotalCount : count,
            Items: items
        );

            
    }
    private static IQueryable<WorkOrder> ApplyFilters(IQueryable<WorkOrder> query, GetWorkOrdersQuery request)
    {
        return query
            .WhereIf(request.Spot.HasValue, wo=> wo.Spot == request.Spot)
            .WhereIf(request.State.HasValue,wo => wo.State == request.State!.Value)
            .WhereIf(request.VehicleId.GetValueOrDefault(Guid.Empty) != Guid.Empty 
                       , wo => wo.VehicleId == request.VehicleId)
            .WhereIf(request.LaborId.GetValueOrDefault(Guid.Empty) != Guid.Empty
                       , wo => wo.LaborId == request.LaborId)
            .WhereIf(request.StartDateFrom.HasValue, 
                wo => wo.StartAtUtc >= request.StartDateFrom!.Value.ToUniversalTime())
            .WhereIf(request.StartDateTo.HasValue,
                wo => wo.StartAtUtc <= request.StartDateTo!.Value.ToUniversalTime())
            .WhereIf(request.EndDateFrom.HasValue,
                wo => wo.EndAtUtc >= request.EndDateFrom!.Value.ToUniversalTime())
            .WhereIf(request.EndDateTo.HasValue,
                wo => wo.EndAtUtc <= request.EndDateTo!.Value.ToUniversalTime())
            ;
    }

    private static IQueryable<WorkOrder> ApplySorting(
    IQueryable<WorkOrder> query,
    WorkOrderSortColumn sortColumn,
    SortDirection sortDirection)
    {
        // ✅ Check once, then apply to the selected property
        return sortColumn switch
        {
            WorkOrderSortColumn.CreatedAt =>
                ApplyOrder(query, wo => wo.CreatedAtUtc, sortDirection),

            WorkOrderSortColumn.UpdatedAt =>
                ApplyOrder(query, wo => wo.LastModifiedUtc, sortDirection),

            WorkOrderSortColumn.StartAt =>
                ApplyOrder(query, wo => wo.StartAtUtc, sortDirection),

            WorkOrderSortColumn.EndAt =>
                ApplyOrder(query, wo => wo.EndAtUtc, sortDirection),

            WorkOrderSortColumn.State =>
                ApplyOrder(query, wo => wo.State, sortDirection),

            WorkOrderSortColumn.Spot =>
                ApplyOrder(query, wo => wo.Spot, sortDirection),

            WorkOrderSortColumn.Total =>
                ApplyOrder(query, wo => wo.Total, sortDirection),

            WorkOrderSortColumn.VehicleId =>
                ApplyOrder(query, wo => wo.VehicleId, sortDirection),

            WorkOrderSortColumn.LaborId =>
                ApplyOrder(query, wo => wo.LaborId, sortDirection),

            _ => query.OrderByDescending(wo => wo.CreatedAtUtc) // Default
        };
    }

    // ✅ Helper method - checks direction once
    private static IQueryable<WorkOrder> ApplyOrder<TKey>(
        IQueryable<WorkOrder> query,
        Expression<Func<WorkOrder, TKey>> keySelector,
        SortDirection sortDirection)
    {
        return sortDirection == SortDirection.Asc  
         ? query.OrderBy(keySelector)
         : query.OrderByDescending(keySelector);
    }
}

public static class QueryableWorkorderExtenstions
{
    /// <summary>
    /// Applies a WHERE clause only if the condition is true
    /// </summary>
    public static IQueryable<WorkOrder> WhereIf(
        this IQueryable<WorkOrder> query,
        bool condition,
        Expression<Func<WorkOrder, bool>> predicate
        )
        => condition ? query.Where(predicate) : query;
}
