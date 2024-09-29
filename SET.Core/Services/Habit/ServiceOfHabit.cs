
namespace SET.Core.Services;

public class ServiceOfHabit : BaseRemoteService, IServiceOfHabit
{
    public ServiceOfHabit(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        //do nothing
    }

    public List<UserHabit>? StoredUserHabits { get; set; }

    public async Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval )
    {
        string url = $"{UrlBuilder.HabitsInProgress}";

        TimeOnly zeroTime = TimeOnly.FromTimeSpan( TimeSpan.Zero );
        DateTime startIntervalAsDateTime = startInterval.ToDateTime( zeroTime );
        DateTime endIntervalAsDateTime = endInterval.ToDateTime( zeroTime );
        int intervalLength = endIntervalAsDateTime.Subtract( startIntervalAsDateTime ).Days + 1;

        List<UserHabit> result = await RequestProvider.GetAsync<List<UserHabit>>(
            url,
            SettingsService.AuthAccessToken
        ).DefaultConfigureAwait();

        Stopwatch timeWatcher = Stopwatch.StartNew();
        foreach( UserHabit habit in result )
        {
            bool wasProgressesEmpty = habit.Progresses == null || habit.Progresses.Count == 0;
            habit.Progresses ??= new ObservableCollectionEx<ProgressOfHabit>();

            List<ProgressOfHabit> progressesInInterval = habit.
                Progresses.
                Where( p => startInterval <= p.Date && p.Date <= endInterval ).
                OrderByDescending( p => p.Date ).
                ToList();
            if (progressesInInterval.Count < intervalLength)
            {
                for (DateOnly date = endInterval; date >= startInterval; date = date.AddDays( value: -1 ))
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

            habit.Progresses = new ObservableCollectionEx<ProgressOfHabit>( habit.Progresses.OrderByDescending( u => u.Date ) );

            int whenToManyProgresses = 150;
            if (habit.Progresses.Count > whenToManyProgresses)
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

            if (!wasProgressesEmpty)
            {
                Recompute( habit );
            }
        }

        timeWatcher.Stop();

        if (SettingsService.IsDebug)
        {
            LoggingService.LogInfo( $"Time of habits initialization: {timeWatcher.ElapsedMilliseconds} milliseconds" );
        }

        return result;
    }

    public void Recompute( UserHabit habit )
    {
        if (habit.Progresses == null || habit.Progresses.Count < 2)
        {
            throw new ArgumentException( message: "Progresses prop is null or empty", paramName: nameof( habit ) );
        }

        DateOnly from = habit.Progresses[habit.Progresses.Count - 1].Date;
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
        if(id == default)
        {
            throw new ArgumentException( message: "Is default value", paramName: nameof( id ) );
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

    public void ResetPriorities( IEnumerable<UserHabit> habits )
    {
        UserHabit[] habitsAsArray = habits.ToArray();
        for (int priority = 1; priority <= habitsAsArray.Length; priority++)
        {
            habitsAsArray[priority - 1].Priority = priority;
        }
    }

    public bool CanAddNewHabit( UserHabit newHabit, IEnumerable<UserHabit> allHabits )
    {
        bool? result = null;
        List<UserHabit> activeHabits = allHabits.
            Where( h => h.PercentageAchieved < 0.4 && h.Status == StatusOfHabit.InProgress ).
            ToList();
        if (activeHabits.Count > 2)
        {
            result = false;
        }

        result ??= true;
        return result.Value;
    }

    public bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull(progress, nameof(progress));
        return progress.Value is not ProgressValue.YES_AUTO and ProgressValue.YES_MANUAL;
    }

    public async Task DeleteAsync(long id )
    {
        string url = $"{UrlBuilder.Habits}/{id}";
        await RequestProvider.DeleteAsync( url, SettingsService.AuthAccessToken );
    }
}
