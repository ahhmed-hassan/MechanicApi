using ErrorOr;
using MechanicApplication.Features.Identity.DTOs;
using MediatR;

namespace MechanicApplication.Features.Identity.Queries.GetUserInfo;

public sealed record GetUserByIdQuery(string? UserId) : IRequest<ErrorOr<AppUserDTO>>;


