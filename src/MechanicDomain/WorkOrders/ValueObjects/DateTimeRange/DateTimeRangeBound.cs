using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MechanicDomain.WorkOrders.ValueObjects.DateTimeRange;

internal abstract record DateTimeRangeBound : IComparable<DateTimeOffset>
{
    private DateTimeRangeBound() { } // Sealed hierarchy

    public sealed record Bounded(DateTimeOffset Value) 
        : DateTimeRangeBound, IComparable<Bounded>
    {
        public override int CompareTo(DateTimeOffset other) => Value.CompareTo(other);

        public int CompareTo(Bounded? other)
        {
            return other is null ? 1 : Value.CompareTo(other.Value);
        }
    }

    public sealed record Unbounded : DateTimeRangeBound
    {
        public static readonly Unbounded Instance = new();
        private Unbounded() { }

        public override int CompareTo(DateTimeOffset other) => 1;
    }

    // Helper methods for pattern matching
    public bool IsBounded => this is Bounded;
    public bool IsUnbounded => this is Unbounded;

    // Convenience methods
    public DateTimeOffset? ToNullableDateTime() => this switch
    {
        Bounded bounded => bounded.Value,
        Unbounded => null,
        _ => throw new InvalidOperationException("Unknown bound type")
    };

    public static DateTimeRangeBound FromNullableDateTime(DateTimeOffset? value) =>
        value.HasValue ? new Bounded(value.Value) : Unbounded.Instance;

    public abstract int CompareTo(DateTimeOffset other);
   
}
