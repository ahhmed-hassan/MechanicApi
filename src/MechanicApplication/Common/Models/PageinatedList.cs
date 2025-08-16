using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicApplication.Common.Models;

/// <summary>
/// Represents a paginated list of items of type <typeparamref name="T"/>.
/// Contains pagination metadata such as page number, page size, total pages, and total item count.
/// </summary>
/// <typeparam name="T">The type of items in the list. Must be a class.</typeparam>
public record PageinatedList<T>(
    int PageNumber,
    int PageSize,
    int TotalPages,
    int TotalCount,
    IReadOnlyCollection<T>? Items)
    where T : class;
