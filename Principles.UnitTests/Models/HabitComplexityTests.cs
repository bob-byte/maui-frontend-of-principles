namespace Principles.UnitTests.Models;

public class HabitComplexityTests
{
    [Theory]
    [InlineData( 1, 18 )]
    [InlineData( 2, 30 )]
    [InlineData( 3, 44 )]
    [InlineData( 4, 59 )]
    [InlineData( 5, 66 )]
    [InlineData( 6, 71 )]
    [InlineData( 7, 100 )]
    [InlineData( 8, 130 )]
    [InlineData( 9, 190 )]
    [InlineData( 10, 254 )]
    public void ToDaysCount_KnownComplexities_ReturnExpectedWindows( double complexity, int expectedDays )
    {
        HabitComplexity.ToDaysCount( complexity ).Should().Be( expectedDays );
    }

    [Fact]
    public void ToDaysCount_UnsupportedValue_Throws()
    {
        Action act = () => HabitComplexity.ToDaysCount( 11 );

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData( 1, 5.6 )]
    [InlineData( 5, 1.525 )]
    [InlineData( 7, 1.0 )]
    [InlineData( 10, 0.392 )]
    public void ToCoefficient_KnownComplexities_ReturnExpectedCoefficients( int complexity, double expected )
    {
        HabitComplexity.ToCoefficient( complexity ).Should().Be( expected );
    }

    [Fact]
    public void ToCoefficient_UnsupportedValue_Throws()
    {
        Action act = () => HabitComplexity.ToCoefficient( 0 );

        act.Should().Throw<ArgumentException>()
            .WithParameterName( "complexity" );
    }
}
