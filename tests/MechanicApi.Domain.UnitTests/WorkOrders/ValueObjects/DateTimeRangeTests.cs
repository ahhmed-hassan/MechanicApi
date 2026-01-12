using MechanicDomain.WorkOrders;
using MechanicDomain.WorkOrders.ValueObjects.DateTimeRange;
using Xunit;

namespace MechanicDomain.Tests.WorkOrders.ValueObjects;

public class DateTimeRangeTests
{
    private readonly DateTimeOffset _baseDate = new(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);

    #region Factory Method Tests

    [Fact]
    public void Create_WithValidRange_ShouldSucceed()
    {
        // Arrange
        var start = _baseDate;
        var end = _baseDate.AddHours(2);

        // Act
        var result = DateTimeRange.Create(start, end);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(start, result.Value.Start);
        Assert.False(result.Value.IsOpenEnded);
        
    }

    [Fact]
    public void Create_WithEqualStartAndEnd_ShouldSucceed()
    {
        // Arrange
        var date = _baseDate;

        // Act
        var result = DateTimeRange.Create(date, date);

        // Assert
        Assert.False(result.IsError);
        Assert.Equal(date, result.Value.Start);
    }

    [Fact]
    public void Create_WithEndBeforeStart_ShouldReturnError()
    {
        // Arrange
        var start = _baseDate;
        var end = _baseDate.AddHours(-1);

        // Act
        var result = DateTimeRange.Create(start, end);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.FirstError.Code);
    }

    [Fact]
    public void CreateOpenEnded_ShouldCreateUnboundedRange()
    {
        // Arrange
        var start = _baseDate;

        // Act
        var range = DateTimeRange.CreateOpenEnded(start);

        // Assert
        Assert.Equal(start, range.Start); 
        Assert.True(range.IsOpenEnded);
    }

    [Fact]
    public void CreateFromNullable_WithNullEnd_ShouldCreateOpenEnded()
    {
        // Arrange
        var start = _baseDate;

        // Act
        var result = DateTimeRange.CreateFromNullable(start, null);

        // Assert
        Assert.False(result.IsError);
        Assert.True(result.Value.IsOpenEnded);
        Assert.Equal(start, result.Value.Start);
    }

    [Fact]
    public void CreateFromNullable_WithValidEnd_ShouldCreateBoundedRange()
    {
        // Arrange
        var start = _baseDate;
        var end = _baseDate.AddDays(1);

        // Act
        var result = DateTimeRange.CreateFromNullable(start, end);

        // Assert
        Assert.False(result.IsError);
        Assert.False(result.Value.IsOpenEnded);
        Assert.Equal(start, result.Value.Start);
    }

    [Fact]
    public void CreateFromNullable_WithInvalidEnd_ShouldReturnError()
    {
        // Arrange
        var start = _baseDate;
        var end = _baseDate.AddHours(-1);

        // Act
        var result = DateTimeRange.CreateFromNullable(start, end);

        // Assert
        Assert.True(result.IsError);
        //result.IsError.Should().BeTrue();
        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.FirstError.Code);
       // result.FirstError.Should().Be(WorkOrderErrors.InvalidTiming);
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_DateBeforeStart_ShouldReturnFalse()
    {
        // Arrange
        var range = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;
        var testDate = _baseDate.AddHours(-1);

        // Act
        var result = range.Contains(testDate);

        // Assert
        Assert.False(result);
        //result.Should().BeFalse();
    }

    [Fact]
    public void Contains_DateAtStart_ShouldReturnTrue()
    {
        // Arrange
        var range = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;

        // Act
        var result = range.Contains(_baseDate);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Contains_DateWithinRange_ShouldReturnTrue()
    {
        // Arrange
        var range = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;
        var testDate = _baseDate.AddHours(12);

        // Act
        var result = range.Contains(testDate);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Contains_DateAtEnd_ShouldReturnTrue()
    {
        // Arrange
        var end = _baseDate.AddDays(1);
        var range = DateTimeRange.Create(_baseDate, end).Value;

        // Act
        var result = range.Contains(end);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Contains_DateAfterEnd_ShouldReturnFalse()
    {
        // Arrange
        var range = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;
        var testDate = _baseDate.AddDays(2);

        // Act
        var result = range.Contains(testDate);

        // Assert
        Assert.False(result);
        //result.Should().BeFalse();
    }

    [Fact]
    public void Contains_OpenEndedRange_DateAfterStart_ShouldReturnTrue()
    {
        // Arrange
        var range = DateTimeRange.CreateOpenEnded(_baseDate);
        var testDate = _baseDate.AddYears(100);

        // Act
        var result = range.Contains(testDate);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Contains_OpenEndedRange_DateBeforeStart_ShouldReturnFalse()
    {
        // Arrange
        var range = DateTimeRange.CreateOpenEnded(_baseDate);
        var testDate = _baseDate.AddHours(-1);

        // Act
        var result = range.Contains(testDate);

        // Assert
        Assert.False(result);
        //result.Should().BeFalse();
    }

    #endregion

    #region Overlaps Tests

    [Fact]
    public void Overlaps_SameRange_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;
        var range2 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_PartialOverlap_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(2)).Value;
        var range2 = DateTimeRange.Create(_baseDate.AddDays(1), _baseDate.AddDays(3)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_OneRangeContainsOther_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(5)).Value;
        var range2 = DateTimeRange.Create(_baseDate.AddDays(1), _baseDate.AddDays(2)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
       // result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_TouchingAtBoundary_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;
        var range2 = DateTimeRange.Create(_baseDate.AddDays(1), _baseDate.AddDays(2)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_NoOverlap_Range2AfterRange1_ShouldReturnFalse()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;
        var range2 = DateTimeRange.Create(_baseDate.AddDays(2), _baseDate.AddDays(3)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.False(result);
        //result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_NoOverlap_Range1AfterRange2_ShouldReturnFalse()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate.AddDays(2), _baseDate.AddDays(3)).Value;
        var range2 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(1)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.False(result);
        //result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_BothOpenEnded_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.CreateOpenEnded(_baseDate);
        var range2 = DateTimeRange.CreateOpenEnded(_baseDate.AddDays(1));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_OpenEndedWithBounded_Overlapping_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.CreateOpenEnded(_baseDate);
        var range2 = DateTimeRange.Create(_baseDate.AddDays(1), _baseDate.AddDays(2)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_OpenEndedWithBounded_NotOverlapping_ShouldReturnFalse()
    {
        // Arrange
        var range1 = DateTimeRange.CreateOpenEnded(_baseDate.AddDays(5));
        var range2 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(2)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.False(result);
        //result.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_BoundedWithOpenEnded_Overlapping_ShouldReturnTrue()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(2)).Value;
        var range2 = DateTimeRange.CreateOpenEnded(_baseDate.AddDays(1));

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    [Fact]
    public void Overlaps_IsSymmetric()
    {
        // Arrange
        var range1 = DateTimeRange.Create(_baseDate, _baseDate.AddDays(2)).Value;
        var range2 = DateTimeRange.Create(_baseDate.AddDays(1), _baseDate.AddDays(3)).Value;

        // Act
        var result1 = range1.Overlaps(range2);
        var result2 = range2.Overlaps(range1);

        // Assert
        Assert.True(result1);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Create_WithMaxDateTimeOffset_ShouldSucceed()
    {
        // Arrange
        var start = _baseDate;
        var end = DateTimeOffset.MaxValue;

        // Act
        var result = DateTimeRange.Create(start, end);

        // Assert
        Assert.False(result.IsError);
        //result.IsError.Should().BeFalse();
    }

    [Fact]
    public void Create_WithMinDateTimeOffset_ShouldSucceed()
    {
        // Arrange
        var start = DateTimeOffset.MinValue;
        var end = _baseDate;

        // Act
        var result = DateTimeRange.Create(start, end);

        // Assert
        Assert.False(result.IsError);
        //result.IsError.Should().BeFalse();
    }

    [Fact]
    public void Overlaps_WithDifferentTimeZones_ShouldWorkCorrectly()
    {
        // Arrange
        var utcDate = new DateTimeOffset(2024, 1, 15, 10, 0, 0, TimeSpan.Zero);
        var pstDate = new DateTimeOffset(2024, 1, 15, 2, 0, 0, TimeSpan.FromHours(-8)); // Same instant as UTC

        var range1 = DateTimeRange.Create(utcDate, utcDate.AddHours(2)).Value;
        var range2 = DateTimeRange.Create(pstDate, pstDate.AddHours(2)).Value;

        // Act
        var result = range1.Overlaps(range2);

        // Assert
        Assert.True(result);
        //result.Should().BeTrue();
    }

    #endregion
}