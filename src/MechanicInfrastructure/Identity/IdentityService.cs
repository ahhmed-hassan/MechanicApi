

using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Identity.DTOs;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
namespace MechanicInfrastructure.Identity;

public class IdentityService(
    UserManager<AppUser> userManager, 
    IUserClaimsPrincipalFactory<AppUser> userClaimsPrincipalFactory,
    IAuthorizationService authorizationService
    )
    : IIdenttiyService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly IUserClaimsPrincipalFactory<AppUser> _userClaimsPrincipalFactory = userClaimsPrincipalFactory;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    public async Task<ErrorOr<AppUserDTO>> AuthenticateAsync(string email, string password)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if(user is null)
            return Error.NotFound("User_Not_Found", $"User with email {MaskEmail(email)} not found.");
        if(!user.EmailConfirmed)
            return Error.Validation("Email_Not_Confirmed", "Email address is not confirmed.");
        if(!await _userManager.CheckPasswordAsync(user, password))
            return Error.Validation("Invalid_Credentials", "The provided credentials are invalid.");

        return new AppUserDTO(user.Id, user.Email!, await _userManager.GetRolesAsync(user),
                              await _userManager.GetClaimsAsync(user));

    }

    public async Task<bool> AuthorizeAsync(string userId, string? policyName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
            return false;
        var principal = await _userClaimsPrincipalFactory.CreateAsync(user);
        if(string.IsNullOrEmpty(policyName))
        {
            // If no policy is specified, we assume the user is authorized
            return true;
        }
        var result = await _authorizationService.AuthorizeAsync(principal, policyName);
        return result.Succeeded;
    }

    public async Task<ErrorOr<AppUserDTO>> GetUserByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId); 
        if (user is null)
            return Error.NotFound("User_Not_Found", $"User with ID {userId} not found.");

        return new AppUserDTO(user.Id, user.Email!, await _userManager.GetRolesAsync(user),
                              await _userManager.GetClaimsAsync(user));
    }

    public async Task<string?> GetUserNameAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user?.UserName;
    }

    public async Task<bool> IsInRoleAsync(string userId, string role)
    {
        var user = await _userManager.FindByIdAsync(userId);

        return user != null && await _userManager.IsInRoleAsync(user, role);
    }
    private static string MaskEmail(string email)
    {
        int atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return $"****{email.AsSpan(atIndex)}";
        }

        return email[0] + "****" + email[atIndex - 1] + email[atIndex..];
    }
}
