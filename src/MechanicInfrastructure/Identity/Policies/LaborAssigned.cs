
using MechanicApplication.Common.Interfaces;
using MechanicDomain.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MechanicInfrastructure.Identity.Policies;


public class LaborAssigned : IAuthorizationRequirement; 

public class LaborAssignedHandler(
    IAppDbContext context, 
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<LaborAssigned>
{
    private readonly IAppDbContext _dbContext = context; 
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    protected async override Task HandleRequirementAsync(AuthorizationHandlerContext context, LaborAssigned requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            // If user ID is not found, fail the requirement
            context.Fail();
            return;
        }
        //Extract workorder dynamically from the route
        var possibleWorkOrderId = _httpContextAccessor.HttpContext?.Request.RouteValues["WorkOrderId"]?.ToString();
        if (!Guid.TryParse(possibleWorkOrderId, out var workOrderId))
        {
            // If the work order ID is not valid, fail the requirement
            context.Fail();
            return;
        }

        var guidUserId = Guid.Parse(userId);
        var isAssignedToLabor = await _dbContext.WorkOrders
            .AnyAsync(wo => wo.Id == workOrderId && wo.LaborId == guidUserId,
                _httpContextAccessor.HttpContext?.RequestAborted ?? default);

        if (isAssignedToLabor || context.User.IsInRole(nameof(Role.Manager)))
        {
            // If the user is assigned to the labor or has manager role, succeed the requirement
            context.Succeed(requirement);
            return;
        }
        context.Fail();
        return;
    }
}

