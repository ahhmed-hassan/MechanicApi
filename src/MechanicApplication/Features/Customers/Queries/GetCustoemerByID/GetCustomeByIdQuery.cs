using ErrorOr;
using MechanicApplication.Common.Interfaces;
using MechanicApplication.Features.Customers.DTOs;

namespace MechanicApplication.Features.Customers.Queries.GetCustoemerByID;

public sealed record GetCustomerByIdQuery(Guid CustomerId) : ICachedQuery<ErrorOr<CustomerDTO>>
{
    public string CacheKey => $"{Features.Constants.Cache.Customers.Single}_{CustomerId}";

    public TimeSpan Expiration => TimeSpan.FromMinutes(10);

    public string[] Tags => [Features.Constants.Cache.Customers.Single];
}
