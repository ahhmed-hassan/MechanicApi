using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Customers.DTOs;

namespace MechanicApplication.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : ICachedQuery<ErrorOr<List<CustomerDTO>>>
{
    public string CacheKey => Features.Constants.Cache.Customers.Single;
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    string[] ICachedQuery.Tags => [Features.Constants.Cache.Customers.Plural];
}
