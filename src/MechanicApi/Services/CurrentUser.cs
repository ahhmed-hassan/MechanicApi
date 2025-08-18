using MechanicApplication.Common.Interfaces;
using System.Security.Claims;

namespace MechanicApi.Services;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : IUser
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    public string? Id => _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

}
