
using System.ComponentModel.DataAnnotations;

namespace MechanicContracts.Requests.WorkOrders;

public sealed record PageRequest
{
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;
    [Range(1, 100)]
    public int PageSize { get; init; } = 10;
}
