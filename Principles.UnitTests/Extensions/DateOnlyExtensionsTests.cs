namespace Principles.UnitTests.Extensions;

public class DateOnlyExtensionsTests
{
    [Fact]
    public void DaysUntil_LaterDate_ReturnsPositiveSpan()
    {
        DateOnly start = new( 2026, 1, 1 );
        DateOnly end = new( 2026, 1, 11 );

        start.DaysUntil( end ).Should().Be( 10 );
    }

    [Fact]
    public void DaysUntil_EarlierDate_ReturnsNegativeSpan()
    {
        DateOnly start = new( 2026, 1, 11 );
        DateOnly end = new( 2026, 1, 1 );

        start.DaysUntil( end ).Should().Be( -10 );
    }

    [Fact]
    public void DaysUntil_SameDate_ReturnsZero()
    {
        DateOnly day = new( 2026, 3, 15 );

        day.DaysUntil( day ).Should().Be( 0 );
    }
}
