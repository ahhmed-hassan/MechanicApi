using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Labors.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicApplication.Features.Labors.Queries.GetLabors;

public sealed class GetLaborsQueryHandler(IAppDbContext dbContext)
    : IRequestHandler<GetLaborsQuery, ErrorOr<List<LaborDTO>>>
{
    private readonly IAppDbContext _dbContext = dbContext;
    public async Task<ErrorOr<List<LaborDTO>>> Handle(GetLaborsQuery request, CancellationToken cancellationToken)
    {
        var labors = _dbContext.Employees.AsNoTracking()
            .Where(e => e.Role == MechanicDomain.Identity.Role.Labor)
            .ToListAsync(cancellationToken);
        return LaborDTO.fromEmployees(await labors); 
    }
}
