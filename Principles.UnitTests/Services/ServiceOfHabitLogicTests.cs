namespace Principles.UnitTests.Services;

public class ServiceOfHabitLogicTests
{
    private readonly ServiceOfHabit m_service = ServiceOfHabitFactory.Create();

    [Theory]
    [InlineData( HabitConstants.MIN_HABIT_COMPLEXITY )]
    [InlineData( HabitConstants.DEFAULT_HABIT_COMPLEXITY )]
    [InlineData( HabitConstants.MAX_HABIT_COMPLEXITY )]
    public void RecomputedScoreAchieved_DailyYesManualForAutomationWindow_ReachesFullScore( int complexity )
    {
        int days = HabitComplexity.ToDaysCount( complexity );
        UserHabit habit = CreateDailyHabit( complexity, days );

        double score = m_service.RecomputedScoreAchieved(
            habit,
            habit.Progresses![^1].Date,
            habit.Progresses[0].Date );

        Score.Round( score ).Should().Be( 100 );
    }

    [Fact]
    public void ShouldHabitBeFollowed_YesManual_IsTrue()
    {
        ProgressOfHabit progress = new() { Value = ProgressValue.YES_MANUAL };

        m_service.ShouldHabitBeFollowed( progress, new UserHabit() ).Should().BeTrue();
    }

    [Theory]
    [InlineData( ProgressValue.YES_AUTO )]
    [InlineData( ProgressValue.NO )]
    [InlineData( ProgressValue.UNKNOWN )]
    [InlineData( ProgressValue.SKIP )]
    public void ShouldHabitBeFollowed_NonManualValues_AreFalse( int value )
    {
        ProgressOfHabit progress = new() { Value = value };

        m_service.ShouldHabitBeFollowed( progress, new UserHabit() ).Should().BeFalse();
    }

    [Fact]
    public void IsItRecommendedToCreateNewHabit_ArchivedHabit_IsTrue()
    {
        UserHabit habit = new() { IsArchived = true };

        m_service.IsItRecommendedToCreateNewHabit( habit ).Should().BeTrue();
    }

    [Fact]
    public void IsItRecommendedToCreateNewHabit_TwoLowProgressActiveHabits_IsFalse()
    {
        m_service.StoredUserHabits =
        [
            new UserHabit { Name = "A", PercentageAchieved = 0.1, Status = StatusOfHabit.InProgress },
            new UserHabit { Name = "B", PercentageAchieved = 0.2, Status = StatusOfHabit.InProgress }
        ];

        UserHabit candidate = new() { Name = "C", Status = StatusOfHabit.InProgress };

        m_service.IsItRecommendedToCreateNewHabit( candidate ).Should().BeFalse();
    }

    [Fact]
    public void IsItRecommendedToCreateNewHabit_FewerThanTwoActiveLowProgress_IsTrue()
    {
        m_service.StoredUserHabits =
        [
            new UserHabit { Name = "A", PercentageAchieved = 0.1, Status = StatusOfHabit.InProgress },
            new UserHabit { Name = "B", PercentageAchieved = 0.9, Status = StatusOfHabit.InProgress }
        ];

        UserHabit candidate = new() { Name = "C", Status = StatusOfHabit.InProgress };

        m_service.IsItRecommendedToCreateNewHabit( candidate ).Should().BeTrue();
    }

    [Fact]
    public void Recompute_FewerThanTwoProgresses_SetsPercentageToZero()
    {
        UserHabit habit = new()
        {
            Complexity = HabitConstants.DEFAULT_HABIT_COMPLEXITY,
            Frequency = new FrequencyOfHabit { IntervalLengthInDays = 1, Repeats = 1 },
            Progresses = [new ProgressOfHabit { Date = DateOnly.FromDateTime( DateTime.Today ), Value = ProgressValue.YES_MANUAL }]
        };

        m_service.Recompute( habit );

        habit.PercentageAchieved.Should().Be( 0 );
    }

    private static UserHabit CreateDailyHabit( int complexity, int days )
    {
        UserHabit habit = new()
        {
            Complexity = complexity,
            Frequency = new FrequencyOfHabit
            {
                IntervalLengthInDays = 1,
                Repeats = 1
            },
            Progresses = []
        };

        DateOnly currentDate = DateOnly.FromDateTime( DateTime.Today );
        for (int numDay = 0; numDay < days; numDay++, currentDate = currentDate.AddDays( -1 ))
        {
            habit.Progresses.Add( new ProgressOfHabit
            {
                Date = currentDate,
                Value = ProgressValue.YES_MANUAL,
                Habit = habit
            } );
        }

        return habit;
    }
}
