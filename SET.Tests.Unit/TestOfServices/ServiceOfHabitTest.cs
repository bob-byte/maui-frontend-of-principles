using System;
namespace SET.Tests.Unit;

public class ServiceOfHabitTest
{
    private readonly IServiceOfHabit m_service;
    private readonly IProgressOfHabitService m_progressOfHabitService;

    public ServiceOfHabitTest()
    {
        m_service = Config.ServiceProvider.GetRequiredService<IServiceOfHabit>();
        m_progressOfHabitService = Config.ServiceProvider.GetRequiredService<IProgressOfHabitService>();
    }

    [Fact]
    public void RecomputedPercentageAchieved_HabitWithDifferentComplexityShouldBeFullyCompletedAfterKnownPeriod_ItIs()
    {
        //Arrange
        Dictionary<int, int> complexityWithDaysToAutomateDict = new()
        {
            { 1, 18 },
            { 2, 30 },
            { 3, 44 },
            { 4, 59 },
            { 5, 66 },
            { 6, 71 },
            { 7, 100 },
            { 8, 130 },
            { 9, 190 },
            { 10, 254 }
        };

        //Act and assert
        foreach (KeyValuePair<int, int> complexityWithDays in complexityWithDaysToAutomateDict)
        {
            TestIsHabitAutomated( habitComplexity: complexityWithDays.Key, daysToAutomateHabit: complexityWithDays.Value );
        }
    }

    private void TestIsHabitAutomated(int habitComplexity, int daysToAutomateHabit)
    {
        //Arrange
        ObservableCollectionEx<ProgressOfHabit> originalProgresses = new();
        DateOnly currentDate = DateOnly.FromDateTime( DateTime.Today );

        UserHabit habit = new()
        {
            Complexity = habitComplexity,
            Frequency = new FrequencyOfHabit
            {
                IntervalLengthInDays = 1,
                Repeats = 1
            }
        };

        for (int numDay = 0; numDay < daysToAutomateHabit; numDay++, currentDate = currentDate.AddDays( -1 ))
        {
            originalProgresses.Add( new ProgressOfHabit
            {
                Date = currentDate,
                Value = ProgressValue.YES_MANUAL,
                Habit = habit
            } );
        }

        habit.Progresses = originalProgresses;

        //Act
        double score = m_service.RecomputedScoreAchieved(
            habit,
            originalProgresses.Last().Date,
            originalProgresses.First().Date
        );
        int percentageAchieved = m_progressOfHabitService.ConvertScoreToPercentage( score );

        //Assert
        percentageAchieved.Should().Be( 100, $"percentage is not 100 for habit complexity {habitComplexity}" );
    }
}
