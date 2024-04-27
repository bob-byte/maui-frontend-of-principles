using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SET.Core.Services;

public class ProgressOfHabitService : BaseRemoteService, IProgressOfHabitService
{
    private IServiceOfHabit m_serviceOfHabit;

    public ProgressOfHabitService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        m_serviceOfHabit = serviceProvider.GetRequiredService<IServiceOfHabit>();
    }

    public async Task UpdateAsync( ProgressOfHabit progressOfHabit )
    {
        #region Check parameter
        if (progressOfHabit.Id == default)
        {
            throw new ArgumentException( message: $"{nameof( progressOfHabit )}.{nameof( progressOfHabit.Id )} is default" );
        }
        
        if(progressOfHabit.Habit == null)
        {
            throw new ArgumentException( message: $"{nameof( progressOfHabit )}.{nameof( progressOfHabit.Habit )} is null" );
        }

        if (progressOfHabit.Habit.Progresses == null)
        {
            throw new ArgumentException( message: $"{nameof( progressOfHabit )}.{nameof( progressOfHabit.Habit )}.{nameof( progressOfHabit.Habit.Progresses )} is null" );
        }

        if (progressOfHabit.Habit.Frequency == null)
        {
            throw new ArgumentException( message: $"{nameof( progressOfHabit )}.{nameof( progressOfHabit.Habit )}.{nameof( progressOfHabit.Habit.Frequency )} is null" );
        }
        #endregion

        UserHabit habit = progressOfHabit.Habit;

        string isCompletedAsStr = progressOfHabit.Value == ProgressValue.YES_MANUAL ? "check" : "uncheck";
        double beforeProgress = habit.PercentageAchieved;

        m_serviceOfHabit.Recompute( habit );

        string url = $"{UrlBuilder.ProgressOfHabit}/{progressOfHabit.Id}";
        UpdateProgressDto dto = new()
        {
            Id = progressOfHabit.Id,
            PercentageAchieved = habit.PercentageAchieved,
            Date = progressOfHabit.Date,
            Value = (int)progressOfHabit.Value,
            HabitId = habit.Id
        };
        await RequestProvider.PutAsync(
            url,
            dto,
            SettingsService.AuthAccessToken
        );

        LoggingService.LogInfo( $"Before progress of habit: {beforeProgress};{Environment.NewLine}" +
            $"After {isCompletedAsStr}: {habit.PercentageAchieved}." );
    }

    public double ComputeScore( double frequency, double previousScore, double checkmarkValue, int complexity )
    {
        double coefOfComplexity = complexity switch
        {
            1 => 5.6,
            2 => 3.33,
            3 => 2.3,
            4 => 1.7,
            5 => 1.525,
            6 => 1.4,
            7 => 1.0,
            8 => 0.77,
            9 => 0.524,
            10 => 0.392,
            _ => throw new ArgumentException( message: $"Complexity {complexity} of habit is not supported",
            paramName: nameof( complexity ) )
        };

        double exponentialSmoothingFactor = 0.5;
        double scalingFactor = 13.0;
        double decayFactor = Math.Pow( exponentialSmoothingFactor, Math.Sqrt( frequency ) * coefOfComplexity / scalingFactor );

        double score = previousScore * decayFactor;
        score += checkmarkValue * (1 - decayFactor);
        return score;
    }

    

    public int ConvertScoreToPercentage( double score )
    {
        return (int)Math.Round( score * 100.0, MidpointRounding.ToEven );
        //return Math.Ceiling( score * 100 - 0.5 ) / 100.0;
    }
}
