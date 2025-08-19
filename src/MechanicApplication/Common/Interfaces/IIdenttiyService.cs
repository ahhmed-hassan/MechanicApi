using ErrorOr;
using MechanicApplication.Features.Identity.DTOs;

namespace MechanicApplication.Common.Interfaces;

public interface IIdenttiyService
{
    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string? policyName);

    Task<ErrorOr<AppUserDTO>> AuthenticateAsync(string email, string password);

    Task<ErrorOr<AppUserDTO>> GetUserByIdAsync(string userId);

    Task<string?> GetUserNameAsync(string userId);
}

