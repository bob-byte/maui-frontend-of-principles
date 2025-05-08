using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Principles.Core.Services;

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
        
        string url = $"{UrlBuilder.ProgressOfHabit}/{progressOfHabit.Id}";
        UpdateProgressDto dto = new()
        {
            Id = progressOfHabit.Id,
            Date = progressOfHabit.Date,
            Value = progressOfHabit.Value,
            HabitId = habit.Id
        };
        SaveProgressOfHabitResponse response = await RequestProvider.PostAsync<UpdateProgressDto, SaveProgressOfHabitResponse>(
            url,
            dto,
            SettingsService.AuthAccessToken
        );
        progressOfHabit.Id = response.Id;
    }
}
