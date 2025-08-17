
using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Customers.DTOMappers;
using MechanicApplication.Features.Customers.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MechanicApplication.Features.Customers.Queries.GetCustomers;

public class GetCustomersQueryHandler(IAppDbContext context
    )
    : IRequestHandler<GetCustomersQuery, ErrorOr<List<CustomerDTO>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<ErrorOr<List<CustomerDTO>>> Handle(GetCustomersQuery _ , CancellationToken ct)
    {
        var customers = await _context.Customers.Include(c => c.Vehicles).AsNoTracking().ToListAsync(ct);

        return customers.ToDtos();
    }
}