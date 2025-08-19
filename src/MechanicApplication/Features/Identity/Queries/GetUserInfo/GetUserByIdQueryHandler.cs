using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Identity.DTOs;
using MediatR;
using Microsoft.Extensions.Logging;

namespace MechanicApplication.Features.Identity.Queries.GetUserInfo;

public sealed class GetUserByIdQueryHandler(
    ILogger<GetUserByIdQueryHandler> logger,
    IIdenttiyService identtiyService)
    : IRequestHandler<GetUserByIdQuery, ErrorOr<AppUserDTO>>
{
    private readonly ILogger<GetUserByIdQueryHandler> _logger = logger;
    private readonly IIdenttiyService _identtiyService = identtiyService;

    public async Task<ErrorOr<AppUserDTO>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _identtiyService.GetUserByIdAsync(request.UserId);
         user.SwitchFirst(
            user => { },
            error =>  _logger.LogWarning("Failed to retrieve user with ID {UserId}: {Error}", request.UserId, error.Description) 
            );
        return user;

    }
}
