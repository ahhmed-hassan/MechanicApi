using ErrorOr;

namespace MechanicApplication.Common.Interfaces;

public interface IIdenttiyService
{
    Task<bool> IsInRoleAsync(string userId, string role);

    Task<bool> AuthorizeAsync(string userId, string? policyName);

    Task<ErrorOr<AppUserDto>> AuthenticateAsync(string email, string password);

    Task<ErrorOr<AppUserDto>> GetUserByIdAsync(string userId);

    Task<string?> GetUserNameAsync(string userId);
}

//TODO: Deelte this soon
public class AppUserDto
{
}