

using System.Security.Claims;

namespace MechanicApplication.Features.Identity.DTOs;

public sealed record AppUserDTO(string UserId, string Email, IList<string> Roles, IList<Claim> Claims);
