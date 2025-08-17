using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Customers.DTOs;

namespace MechanicApplication.Features.Customers.Queries.GetCustomers;

public sealed record GetCustomersQuery : ICachedQuery<ErrorOr<List<CustomerDTO>>>
{
    public string CacheKey => Constants.Cache.Customers.Single;
    public TimeSpan Expiration => TimeSpan.FromMinutes(10);
    string[] ICachedQuery.Tags => [Constants.Cache.Customers.Plural];
}
