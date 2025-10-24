
namespace Principles.Core.Services;

public class ServiceOfHabit : BaseRemoteService, IServiceOfHabit
{
    private IReminderService m_reminderService;

    public ServiceOfHabit(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        m_reminderService = ServiceLocator.Current!.GetRequiredService<IReminderService>();
    }

    public ObservableCollectionEx<UserHabit>? StoredUserHabits { get; set; }

    public async Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval )
    {
        string url = UrlBuilder.HabitsInProgress;
        
        List<UserHabit> result = await RequestProvider.GetAsync<List<UserHabit>>(
            url,
            SettingsService.AuthAccessToken
        ).DefaultConfigureAwait();
        
        foreach( UserHabit habit in result )
        {
            InitializeHabitProgresses( habit, startInterval, endInterval );
        }

        return result;
    }

    public void InitializeHabitProgresses( UserHabit habit, DateOnly startInterval, DateOnly endInterval )
    {
        bool wasProgressesEmpty = habit.Progresses == null || habit.Progresses.Count == 0;
        habit.Progresses ??= new ObservableCollectionEx<ProgressOfHabit>();

        int intervalLength =
            (endInterval.ToDateTime( TimeOnly.MinValue ) - startInterval.ToDateTime( TimeOnly.MinValue )).Days + 1;

        List<ProgressOfHabit> progressesInInterval = habit.Progresses
            .Where( p => startInterval <= p.Date && p.Date <= endInterval )
            .OrderByDescending( p => p.Date )
            .ToList();

        if (progressesInInterval.Count < intervalLength)
        {
            for (DateOnly date = endInterval; date >= startInterval; date = date.AddDays( -1 ))
            {
                if (wasProgressesEmpty || !progressesInInterval.Any( p => p.Date == date ))
                {
                    ProgressOfHabit progress = new()
                    {
                        Id = 0,
                        Date = date,
                        Value = ProgressValue.UNKNOWN,
                        Habit = habit
                    };
                    habit.Progresses.Add( progress );
                }
            }
        }

        habit.Progresses =
            new ObservableCollectionEx<ProgressOfHabit>( habit.Progresses.OrderByDescending( u => u.Date ) );

        int whenTooManyProgresses = 150;
        if (habit.Progresses.Count > whenTooManyProgresses)
        {
            habit.Progresses.AsParallel().ForAll( p => p.Habit = habit );
        }
        else
        {
            foreach (ProgressOfHabit progress in habit.Progresses)
            {
                progress.Habit = habit;
            }
        }

        Recompute( habit );
    }

    public void Recompute( UserHabit habit )
    {
        if (habit.Progresses == null || habit.Progresses.Count < 2)
        {
            throw new ArgumentException( message: "Progresses prop is null or empty", paramName: nameof( habit ) );
        }

        DateOnly from = habit.Progresses[^1].Date;
        DateOnly to = habit.Progresses[0].Date;
        habit.PercentageAchieved = RecomputedScoreAchieved( habit, from, to );
    }

    public double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to )
    {
        #region Check parameters
        if (habit.Progresses == null)
        {
            throw new ArgumentException( message: "Progresses prop is null or empty", paramName: nameof( habit ) );
        }
        if (habit.Frequency == null)
        {
            throw new ArgumentException( message: "Frequency prop is null or empty", paramName: nameof( habit ) );
        }
        #endregion

        List<ProgressOfHabit> knownProgresses = habit.
            Progresses.
            Where( p => p.Value == ProgressValue.YES_MANUAL || p.Value == ProgressValue.NO ).
            ToList();
        habit.ComputedProgresses.RecomputeFrom( knownProgresses, habit.Frequency, isNumerical: false );
        habit.ScoreList.Recompute( habit.Complexity, habit.Frequency, habit.ComputedProgresses, from, to );

        double result = habit.ScoreList.Get( to ).Value;

        return result;
    }

    public async Task<UserHabit> UserHabitAsync( long id )
    {
        if(id == 0)
        {
            throw new ArgumentException( message: "is zero", paramName: nameof( id ) );
        }

        string url = $"{UrlBuilder.Habits}/{id}";
        UserHabit result = await RequestProvider.GetAsync<UserHabit>( url, SettingsService.AuthAccessToken );
        result.Frequency ??= new FrequencyOfHabit();
        return result;
    }

    public async Task<SaveHabitResponse> UpdateHabitAsync(EditUserHabitDto habit)
    {
        ArgumentNullException.ThrowIfNull(habit, nameof(habit));

        string url = $"{UrlBuilder.Habits}/{habit.Id}";
        SaveHabitResponse result = await RequestProvider.PostAsync<EditUserHabitDto, SaveHabitResponse>( url, habit, SettingsService.AuthAccessToken );
    
        return result;
    }
    public async Task UpdatePrioritiesAsync( IEnumerable<UserHabitWithPriority> habitsWithPriorities )
    {
        ArgumentNullException.ThrowIfNull( habitsWithPriorities );
        if (!habitsWithPriorities.Any())
        {
            throw new ArgumentException( message: "is empty", nameof( habitsWithPriorities ) );
        }

        string url = $"{UrlBuilder.HabitsPriorities}";
        await RequestProvider.PutAsync( url, habitsWithPriorities.ToList(), SettingsService.AuthAccessToken );
    }

    public async Task SetHabitArchiveStatusAsync( HabitArchiveStatus habitArchiveStatus )
    {
        string url = $"{UrlBuilder.HabitArchiveStatus}";
        await RequestProvider.PostAsync( url, habitArchiveStatus, SettingsService.AuthAccessToken );
    }

    public async Task<List<ArсhivedHabitDto>> GetArchivedHabits()
    {
        string url = $"{UrlBuilder.Archive}";

        List<ArсhivedHabitDto> result = await RequestProvider.GetAsync<List<ArсhivedHabitDto>>(
            url,
            SettingsService.AuthAccessToken
        ).DefaultConfigureAwait();
        return result;
    }

    public bool IsItRecommendedToCreateNewHabit( UserHabit newHabit )
    {
        bool? result = null;

        if (newHabit.IsArchived)
        {
            result = true;
        }
        else
        {
            List<UserHabit> allHabits = StoredUserHabits?.ToList() ?? [];

            if (allHabits.Contains( newHabit ))
            {
                allHabits.Remove( newHabit );
            }
            else
            {
                UserHabit? habit = allHabits.FirstOrDefault( h => h.Id == newHabit.Id );
                if (habit is not null)
                {
                    allHabits.Remove( habit );
                }
            }

            List<UserHabit> activeHabits = allHabits
                .Where( h => h.PercentageAchieved < 0.4 && h.Status == StatusOfHabit.InProgress && !h.IsArchived ).ToList();
            if (activeHabits.Count >= 2)
            {
                result = false;
            }

            result ??= true;
        }

        return result.Value;
    }

    public bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull(progress, nameof(progress));
        return progress.Value is not ProgressValue.YES_AUTO and ProgressValue.YES_MANUAL;
    }

    public Task<HabitDeletionResponse?> DeleteAsync(long id )
    {
        string url = $"{UrlBuilder.Habits}/{id}";
        return RequestProvider.DeleteAsync<HabitDeletionResponse>( url, SettingsService.AuthAccessToken );
    }
    
    public int GetDaysUntilFullAutomation( UserHabit habit)
    {
        if (habit.Frequency is null)
        {
            throw new ArgumentException( "Frequency cannot be null." );
        }
        
        if (habit.Progresses is null)
        {
            throw new ArgumentException( "Progresses cannot be null." );
        }
        
        if (habit.ComputedProgresses is null)
        {
            throw new ArgumentException( "ComputedProgresses cannot be null." );
        }
            
        List<int> values = habit.ComputedProgresses
            .GetByInterval( habit.Progresses[^1].Date, habit.Progresses[0].Date ).Select( x => x.Value ).ToList();
        int extraDays = 0;

        while (true)
        {
            double value = Score.Get( habit.Complexity, habit.Frequency, values );
            int roundedScore = Score.Round( value );
            if ( roundedScore >= 100 )
            {
                break;
            }
            
            values.Insert( 0, ProgressValue.YES_MANUAL );
            extraDays++;

            // endless cycle without a limit is dangerous
            if (extraDays > 365)
            {
                throw new InvalidOperationException("Cannot define days until the habit is complete.");
            }
        }

        return extraDays;
    }
    
    public async Task RestoreRemindersOfHabit(UserHabit habit)
    {
        if (habit.Reminders?.Any() == true)
        {
            await m_reminderService.RequestAccessToSendNotificationsAsync();

            for (int numReminder = 0; numReminder < habit.Reminders.Count; numReminder++)
            {
                UserHabitReminder habitReminder = habit.Reminders[numReminder];

                for (int numWeekDay = 0; numWeekDay < habitReminder.DaysOfWeek?.Count; numWeekDay++)
                {
                    WeekDay weekDay = habitReminder.DaysOfWeek[numWeekDay];
                    bool isNewHabit = false;
                    await m_reminderService.AddNotificationToDeviceAsync( isNewHabit, habitReminder, weekDay );
                }
            }
        }
    }
    
    public void CancelAllRemindersOfHabit(UserHabit habit)
    {
        if (habit.Reminders is not null)
        {
            foreach (UserHabitReminder reminder in habit.Reminders)
            {
                reminder.IsEnabled = false;
                if (reminder.DaysOfWeek is not null)
                {
                    foreach (WeekDay weekDay in reminder.DaysOfWeek)
                    {
                        m_reminderService.Cancel( weekDay.UserNotificationRequestId );
                    }
                }
            }
        }
    }
}
