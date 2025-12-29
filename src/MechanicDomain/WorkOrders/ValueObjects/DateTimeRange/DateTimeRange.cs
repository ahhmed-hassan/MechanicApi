
using ErrorOr;

namespace MechanicDomain.WorkOrders.ValueObjects.DateTimeRange;

public sealed record DateTimeRange
{
    public DateTimeOffset Start { get; }
    private DateTimeRangeBound End { get; }

    private DateTimeRange(DateTimeOffset start, DateTimeRangeBound end)
    {
        Start = start;
        End = end;
    }

    // Factory methods
    public static ErrorOr<DateTimeRange> Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end < start)
            return WorkOrderErrors.InvalidTiming; 

        return new DateTimeRange(start, new DateTimeRangeBound.Bounded(end));
    }

    public static DateTimeRange CreateOpenEnded(DateTimeOffset start) 
        => new DateTimeRange(start, DateTimeRangeBound.Unbounded.Instance);

    public static ErrorOr<DateTimeRange> CreateFromNullable(DateTimeOffset start, DateTimeOffset? end)
    {
        if (end.HasValue && end.Value < start)
            return WorkOrderErrors.InvalidTiming;

        return new DateTimeRange(start, DateTimeRangeBound.FromNullableDateTime(end));
    }

    // Domain logic
    public bool Contains(DateTimeOffset date) => End switch
    {
        DateTimeRangeBound.Bounded bounded => date >= Start && date <= bounded.Value,
        DateTimeRangeBound.Unbounded => date >= Start,
        _ => throw new InvalidOperationException("Unknown bound type")
    };

    public bool Overlaps(DateTimeRange other)
    {
        //var thisEnd = End is DateTimeRangeBound.Bounded boundedThis ? boundedThis.Value : DateTimeOffset.MaxValue;
        //var otherEnd = other.End is DateTimeRangeBound.Bounded boundedOther ? boundedOther.Value : DateTimeOffset.MaxValue;
        var result = (other.End.CompareTo(Start) >= 0 ) && End.CompareTo(other.Start) >=0;
        //return Start <= otherEnd && other.Start <= thisEnd;
        return result;
    }

    public bool IsOpenEnded => End.IsUnbounded;
}
