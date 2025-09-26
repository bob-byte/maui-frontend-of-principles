
namespace Principles.Core.Services;

public class ServiceOfHabit : BaseEntityService<UserHabit>, IServiceOfHabit
{
    public ServiceOfHabit(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.NUMBER_OF_DAYS_IN_PROGRESS + 1 );

        ReminderService = serviceProvider.GetRequiredService<IReminderService>();
        NetworkService = serviceProvider.GetRequiredService<INetworkService>();
    }
    
    private IReminderService ReminderService { get; }
    private INetworkService NetworkService { get; }

    public ObservableCollectionEx<UserHabit>? StoredUserHabits { get; set; }

    public DateOnly StartProgressInterval { get; }
    
    public DateOnly EndProgressInterval { get; }

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
                        Id = 0, Date = date, Value = ProgressValue.UNKNOWN, Habit = habit
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

    public async Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval )
    {
        List<UserHabit> result = await OfflineApiService.GetAllAsync();
        result = result.Where( h => !h.IsArchived ).ToList();
        
        foreach (UserHabit habit in result)
        {
            InitializeHabitProgresses( habit, startInterval, endInterval );
        }
        
        return result;
    }

    public async Task<UserHabit> UserHabitAsync( long id )
    {
        if(id == 0)
        {
            throw new ArgumentException( message: "is zero", paramName: nameof( id ) );
        }

        UserHabit result = await OfflineRepository.GetRequiredByIdAsync<UserHabit>( id );
        return result;
    }

    public async Task UpdateHabitAsync(UserHabit habit)
    {
        ArgumentNullException.ThrowIfNull(habit, nameof(habit));
        
        if (habit.Reminders?.Any() == true)
        {
            foreach (UserHabitReminder habitReminder in habit.Reminders)
            {
                for (int numWeekDay = 0; numWeekDay < habitReminder.DaysOfWeek?.Count; numWeekDay++)
                {
                    WeekDay weekDay = habitReminder.DaysOfWeek[numWeekDay];
                    await AddNotificationToDeviceAsync( habit, habitReminder, weekDay );
                }
            }
        }

        await OfflineApiService.SaveAsync( habit );
    }
    
    
    private async Task AddNotificationToDeviceAsync( UserHabit habit, UserHabitReminder reminder, WeekDay weekDay )
    {
        if (reminder.IsEnabled)
        {
            DateTime currentDate = DateTime.Now;
            TimeSpan currentTime = currentDate.TimeOfDay;

            int reminderDayIndex = (int)weekDay.Type;

            int currentDayIndex = (int)currentDate.DayOfWeek;

            int daysUntilNextReminder = (reminderDayIndex - currentDayIndex + 7) % 7;

            if (daysUntilNextReminder == 0 && reminder.Time.ToTimeSpan() < currentTime)
            {
                daysUntilNextReminder = 7;
            }

            DateTime notifyDateTime = currentDate.Date
                .AddDays( daysUntilNextReminder )
                .Add( reminder.Time.ToTimeSpan() );

            await ReminderService.SaveLocallyAsync(
                weekDay.UserNotificationRequestId,
                reminder.Title,
                reminder.Description,
                notifyDateTime,
                ReminderRepeat.Weekly
            );
        }
        else if (!habit.IsNew() && weekDay.UserNotificationRequestId != 0)
        {
            //TODO: test how it works for new habit
            await ReminderService.CancelLocallyAsync( weekDay.UserNotificationRequestId );
        }
    }

    public async Task SetHabitArchiveStatusAsync(UserHabit habit)
    {
        HabitArchiveStatus dto = new() { HabitId = habit.Id, IsArchived = habit.IsArchived, };
        await OfflineApiService.ExecuteAsync(
            () => OfflineRepository.UpdateFieldAsync<UserHabit, bool>( habit.LocalId, nameof(habit.IsArchived),
                habit.IsArchived ), nameof(UserHabit), "SetArchiveStatus", dto );
    }

    public Task<List<ArсhivedHabit>> GetArchivedHabits()
    {
        return OfflineRepository.QueryAsync<ArсhivedHabit>(
            "SELECT Id, LocalId, Name, ArchivingTime FROM UserHabit WHERE IsArchived = true" );
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

    public async Task DeleteAsync(UserHabit habit)
    {
        long[] reminderIds =
            (await OfflineRepository.WhereAsync<UserHabitReminder>( r => r.UserHabitId == habit.LocalId )
                .DefaultConfigureAwait()).Select( r => r.LocalId ).ToArray();
        
        List<int> notificationRequests = new();
        foreach (long idOfReminder in reminderIds)
        {
            List<WeekDay> weekDaysOfReminder = await OfflineRepository.WhereAsync<WeekDay>( w => w.UserHabitReminderId == idOfReminder ).DefaultConfigureAwait();
            notificationRequests.AddRange( weekDaysOfReminder.Select( w => w.UserNotificationRequestId ) );
        }
        
        await OfflineApiService.DeleteAsync( habit ).DefaultConfigureAwait();
        foreach (int notification in notificationRequests)
        {
            await ReminderService.CancelLocallyAsync( notification ).DefaultConfigureAwait();
        }
    }
    
    public int GetDaysUntilFullAutomation( UserHabit habit )
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
}
