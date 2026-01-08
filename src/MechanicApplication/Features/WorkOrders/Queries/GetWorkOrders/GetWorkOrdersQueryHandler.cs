using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Common.Models;
using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.WorkOrders.Dtos;
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
            .WhereIf(request.State.HasValue,wo => wo.State == request.State!.Value);
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
