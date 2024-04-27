
using System;
using System.IO;

namespace SET.Tests.Unit;

public class ProgressOfHabitServiceTest
{
    private const double PRECISION = 1e-6;

    private readonly IProgressOfHabitService m_service;

    public ProgressOfHabitServiceTest()
    {
        m_service = Config.ServiceProvider.GetRequiredService<IProgressOfHabitService>();
    }

    [Fact]
    public void ComputeScore_IsHabitAchievedAfterDefaultContinuousRepeatNumber_Yes()
    {
        ComputeScore_IsHabitAchievedAfterSeveralRepeatsContinuousRepeats_Yes( HabitConstants.DEFAULT_HABIT_COMPLEXITY );
    }

    [Fact]
    public void ComputeScore_IsHabitAchievedAfterMinContinuousRepeats_Yes()
    {
        ComputeScore_IsHabitAchievedAfterSeveralRepeatsContinuousRepeats_Yes( HabitConstants.MIN_HABIT_COMPLEXITY );
    }

    [Fact]
    public void ComputeScore_IsHabitAchievedAfterMaxContinuousRepeats_Yes()
    {
        ComputeScore_IsHabitAchievedAfterSeveralRepeatsContinuousRepeats_Yes( HabitConstants.MAX_HABIT_COMPLEXITY );
    }

    private void ComputeScore_IsHabitAchievedAfterSeveralRepeatsContinuousRepeats_Yes( int complexity )
    {
        //Arrange
        int frequency = 1;//each day
        double habitIsCompletedMark = 1;
        double score = 0.0;

        List<double> scores = new();
        for (double numComplexity = 18.0; scores.Count < 10; numComplexity += (254.0 - 18.0) / 9.0)
        {
            scores.Add(numComplexity);
        }

        //Act
        for (int numDay = 1; numDay < 10000; numDay++)
        {
            score = m_service.ComputeScore( frequency, score, habitIsCompletedMark, 1 );

            int percentage = m_service.ConvertScoreToPercentage( score );
            if (percentage == 100)
            {
                throw new Exception( $"stopped at {numDay} day" );
            }
        }

        //Assert
        score.Should().BeApproximately( 1.0, 0.05);
    }

    [Fact]
    public void ComputeScore_IsCorrectProgressChangeForDailyHabit_Yes()
    {
        //Arrange
        double check = 1;
        double frequency = 1;//each day
        int complexity = 7;
        List<double> scoresOfChecked = new();
        List<double> scoresOfUnchecked = new();

        //Act
        scoresOfChecked.Add( m_service.ComputeScore( frequency, previousScore: 0.0, check, complexity ) );
        scoresOfChecked.Add( m_service.ComputeScore( frequency, 0.5, check, complexity ) );
        scoresOfChecked.Add( m_service.ComputeScore( frequency, 0.75, check, complexity ) );

        check = 0;
        scoresOfUnchecked.Add( m_service.ComputeScore( frequency, previousScore: 0.0, check, complexity ) );
        scoresOfUnchecked.Add( m_service.ComputeScore( frequency, 0.5, check, complexity ) );
        scoresOfUnchecked.Add( m_service.ComputeScore( frequency, 0.75, check, complexity ) );

        //Assert
        scoresOfChecked[0].Should().BeApproximately( expectedValue: 0.051922, PRECISION );
        scoresOfChecked[1].Should().BeApproximately( 0.525961, PRECISION );
        scoresOfChecked[2].Should().BeApproximately( 0.762981, PRECISION );

        scoresOfUnchecked[0].Should().BeApproximately( 0.0, PRECISION );
        scoresOfUnchecked[1].Should().BeApproximately( 0.474039, PRECISION );
        scoresOfUnchecked[2].Should().BeApproximately( 0.711058, PRECISION );
    }
}

