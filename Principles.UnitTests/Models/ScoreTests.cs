namespace Principles.UnitTests.Models;

public class ScoreTests
{
    private const double Precision = 1e-6;

    [Fact]
    public void Compute_DailyHabitCheckAndUncheck_MatchesKnownProgressDeltas()
    {
        const double frequency = 1;
        const int complexity = 7;

        Score.Compute( frequency, previousScore: 0.0, checkmarkValue: 1, complexity )
            .Should().BeApproximately( 0.051922, Precision );
        Score.Compute( frequency, previousScore: 0.5, checkmarkValue: 1, complexity )
            .Should().BeApproximately( 0.525961, Precision );
        Score.Compute( frequency, previousScore: 0.75, checkmarkValue: 1, complexity )
            .Should().BeApproximately( 0.762981, Precision );

        Score.Compute( frequency, previousScore: 0.0, checkmarkValue: 0, complexity )
            .Should().BeApproximately( 0.0, Precision );
        Score.Compute( frequency, previousScore: 0.5, checkmarkValue: 0, complexity )
            .Should().BeApproximately( 0.474039, Precision );
        Score.Compute( frequency, previousScore: 0.75, checkmarkValue: 0, complexity )
            .Should().BeApproximately( 0.711058, Precision );
    }

    [Theory]
    [InlineData( 0.0, 0 )]
    [InlineData( 0.5, 50 )]
    [InlineData( 0.994, 99 )]
    [InlineData( 1.0, 100 )]
    public void Round_ConvertsScoreToPercentage( double score, int expectedPercentage )
    {
        Score.Round( score ).Should().Be( expectedPercentage );
    }

    [Theory]
    [InlineData( HabitConstants.MIN_HABIT_COMPLEXITY )]
    [InlineData( HabitConstants.DEFAULT_HABIT_COMPLEXITY )]
    [InlineData( HabitConstants.MAX_HABIT_COMPLEXITY )]
    public void Get_DailyYesManualForAutomationWindow_ReachesFullScore( int complexity )
    {
        FrequencyOfHabit frequency = DailyFrequency();
        int days = HabitComplexity.ToDaysCount( complexity );
        List<int> values = Enumerable.Repeat( ProgressValue.YES_MANUAL, days ).ToList();

        double score = Score.Get( complexity, frequency, values );

        Score.Round( score ).Should().Be( 100 );
    }

    [Fact]
    public void Get_EmptyValues_ReturnsZero()
    {
        double score = Score.Get(
            HabitConstants.DEFAULT_HABIT_COMPLEXITY,
            DailyFrequency(),
            [] );

        score.Should().Be( 0 );
    }

    private static FrequencyOfHabit DailyFrequency() =>
        new()
        {
            IntervalLengthInDays = 1,
            Repeats = 1
        };
}
