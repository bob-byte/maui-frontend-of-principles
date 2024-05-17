using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;

public class ServiceOfHabit : BaseRemoteService, IServiceOfHabit
{
    public ServiceOfHabit(IServiceProvider serviceProvider)
        : base(serviceProvider)
    {
        //do nothing
    }

    public async Task<List<UserHabit>> ActiveHabitsAsync( DateOnly startInterval, DateOnly endInterval )
    {
        string url = $"{UrlBuilder.HabitsInProgress}/{SettingsService.UserId}";

        TimeOnly zeroTime = TimeOnly.FromTimeSpan( TimeSpan.Zero );
        DateTime startIntervalAsDateTime = startInterval.ToDateTime( zeroTime );
        DateTime endIntervalAsDateTime = endInterval.ToDateTime( zeroTime );
        int intervalLength = endIntervalAsDateTime.Subtract( startIntervalAsDateTime ).Days + 1;

        List<UserHabit> result = await RequestProvider.GetAsync<List<UserHabit>>( url, SettingsService.AuthAccessToken );

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
        LoggingService.LogInfo( $"Time of habits initialization: {timeWatcher.ElapsedMilliseconds} milliseconds" );

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

        string url = $"{UrlBuilder.Habits}/{habit.Id}?userId={SettingsService.UserId}";
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

        string url = $"{UrlBuilder.HabitsPriorities}/{SettingsService.UserId}";
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

    public Task<UserHabit> PostHabitAsync( UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull( habit, nameof( habit ) );

        string url = $"{UrlBuilder.Habits}";
        return RequestProvider.PostAsync<UserHabit, UserHabit>( url, habit, SettingsService.AuthAccessToken );
    }

    public bool CanAddNewHabit( UserHabit newHabit, IEnumerable<UserHabit> allHabits )
    {
        bool? result = null;
        if (newHabit.AreasOfLife != null && newHabit.AreasOfLife.Any())
        {
            foreach (UserAreaOfLife sphere in newHabit.AreasOfLife!)
            {
                List<UserHabit> habitsInSphere = allHabits.
                    Where( h => h.AreasOfLife!.Any( a => a.Id == sphere.Id ) && h.PercentageAchieved < 0.5 && h.Status == StatusOfHabit.InProgress ).
                    ToList();
                if (habitsInSphere.Count >= 3)
                {
                    result = false;
                    break;
                }

                //Assess the overall complexity of habits in this area
                double overallComplexity = habitsInSphere.Sum( h => h.Complexity ) + newHabit.Complexity;
                double maxOverallComplexity = HabitConstants.MAX_HABIT_COMPLEXITY + (HabitConstants.MAX_HABIT_COMPLEXITY / 5.0);
                if (overallComplexity >= maxOverallComplexity)
                {
                    result = false;
                    break;
                }

                //Estimate the overall frequency of habits in this area
                double overallFrequency = habitsInSphere.Sum( h => h.Frequency!.Value ) + newHabit.Frequency!.Value;
                if (overallFrequency > 2)
                {
                    result = false;
                    break;
                }
            }
        }
        else
        {
            List<UserHabit> habitsWithoutAnyArea = allHabits.
                Where( h => (h.AreasOfLife == null || !h.AreasOfLife.Any()) && h.PercentageAchieved < 0.5 && h.Status == StatusOfHabit.InProgress ).
                ToList();
            if (habitsWithoutAnyArea.Count >= 3)
            {
                result = false;
            }

            //Assess the overall complexity of habits in this area
            double overallComplexity = habitsWithoutAnyArea.Sum( h => h.Complexity ) + newHabit.Complexity;
            double maxOverallComplexity = HabitConstants.MAX_HABIT_COMPLEXITY + (HabitConstants.MAX_HABIT_COMPLEXITY / 5.0);
            if (overallComplexity >= maxOverallComplexity)
            {
                result = false;
            }

            //Estimate the overall frequency of habits in this area
            double overallFrequency = habitsWithoutAnyArea.Sum( h => h.Frequency!.Value ) + newHabit.Frequency!.Value;
            if (overallFrequency > 2)
            {
                result = false;
            }
        }

        result ??= true;
        return result.Value;
    }

    public bool ShouldHabitBeFollowed( ProgressOfHabit progress, UserHabit habit )
    {
        #region Check parameters
        ArgumentNullException.ThrowIfNull( progress, nameof( progress ) );
        ArgumentNullException.ThrowIfNull( habit, nameof( habit ) );

        if (habit.Frequency == null)
        {
            throw new ArgumentException( "Frequency of habit is null", nameof( habit ) );
        }

        if (habit.Progresses == null)
        {
            throw new ArgumentException( "Progresses of habit is null", nameof( habit ) );
        }
        #endregion

        FrequencyOfHabit frequency = habit.Frequency;

        bool shouldHabitBeFollowed;
        int goalOfFollowedCount = habit.Frequency.Repeats;
        var zeroTime = TimeOnly.FromTimeSpan( TimeSpan.Zero );
        var dateTimeOfProgress = progress.Date.ToDateTime( zeroTime );

        switch (habit.Frequency.IntervalType)
        {
            case IntervalType.Day:
                {
                    shouldHabitBeFollowed = true;
                    break;
                }

            case IntervalType.Week:
                {
                    int daysToStartOfWeek = dateTimeOfProgress.DayOfWeek == DayOfWeek.Sunday
                        ? 6
                        : dateTimeOfProgress.DayOfWeek - DayOfWeek.Monday;

                    var firstDateOfCurrentWeek = DateOnly.FromDateTime( dateTimeOfProgress.AddDays( -daysToStartOfWeek ) );

                    int daysUntilEndOfWeek = dateTimeOfProgress.DayOfWeek == DayOfWeek.Sunday
                        ? 0
                        : (7 - (int)dateTimeOfProgress.DayOfWeek);

                    var lastDayOfCurrentWeek = DateOnly.FromDateTime( dateTimeOfProgress.AddDays( daysUntilEndOfWeek ) );

                    int followedCountThisWeek = habit.Progresses.Count( p => p.IsCompleted() && firstDateOfCurrentWeek <= p.Date && p.Date <= lastDayOfCurrentWeek );

                    //including date of progress, so "+ 1"
                    int leftChances = daysUntilEndOfWeek + 1;
                    shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountThisWeek;
                    break;
                }

            case IntervalType.Month:
                {
                    DateTime firstDayOfMonth = new( dateTimeOfProgress.Year, dateTimeOfProgress.Month, 1 );
                    var firstDayOnlyOfMonth = DateOnly.FromDateTime( firstDayOfMonth );

                    DateTime lastDayOfMonth = firstDayOfMonth.AddMonths( 1 ).AddDays( -1 );
                    var lastDayOnlyOfMonth = DateOnly.FromDateTime( lastDayOfMonth );

                    int followedCountThisMonth = habit.Progresses.Count( p => p.IsCompleted() && firstDayOnlyOfMonth <= p.Date && p.Date <= lastDayOnlyOfMonth );

                    int daysUntilEndOfMonth = (lastDayOfMonth - dateTimeOfProgress).Days;

                    //including current day
                    int leftChances = daysUntilEndOfMonth + 1;
                    shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountThisMonth;

                    break;
                }

            default:
                {
                    if (habit.Frequency.IntervalType == IntervalType.Year)
                    {
                        int year = progress.Date.Year;

                        DateTime firstDateOfYear = new( year, month: 1, day: 1 );
                        DateOnly firstDateOfYearAsDateOnly = DateOnly.FromDateTime( firstDateOfYear );

                        DateTime earliestDateTime = habit.Progresses.Select( p => p.Date ).Min().ToDateTime( zeroTime );
                        DateTime largestDateTime = habit.Progresses.Select( p => p.Date ).Max().ToDateTime( zeroTime );

                        int diffOfEarliestAndLargest = (largestDateTime - earliestDateTime).Days;
                        int diffOfDateAndYearStart = (dateTimeOfProgress - firstDateOfYear).Days;

                        DateTime lastDayOfInterval = new( year + 1, 1, 1 );
                        lastDayOfInterval = lastDayOfInterval.AddDays( -1 );
                        var lastDayOfIntervalAsDateOnly = DateOnly.FromDateTime( lastDayOfInterval );

                        int daysUntilEndOfInterval = (lastDayOfInterval - dateTimeOfProgress).Days;
                        int followedCountWithinInterval;

                        if (diffOfEarliestAndLargest >= diffOfDateAndYearStart)
                        {
                            followedCountWithinInterval = habit.Progresses.Count( p => p.IsCompleted() && firstDateOfYearAsDateOnly <= p.Date && p.Date <= lastDayOfIntervalAsDateOnly );
                        }
                        else
                        {
                            if (habit.CountOfFollowedPerSpecificInterval == null)
                            {
                                throw new InvalidOperationException( $"{nameof( habit.CountOfFollowedPerSpecificInterval )} cannot be null" );
                            }

                            followedCountWithinInterval = (int)habit.CountOfFollowedPerSpecificInterval;
                        }

                        //including current day
                        int leftChances = daysUntilEndOfInterval + 1;
                        shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountWithinInterval;
                    }
                    else if (habit.Frequency.IntervalType == IntervalType.Other)
                    {
                        ProgressOfHabit? firstFollowedProgress = habit.Progresses.
                            Where( p => p.IsCompleted() ).
                            OrderBy( p => p.Date ).
                            FirstOrDefault();
                        if (firstFollowedProgress == null || progress.Date < firstFollowedProgress.Date)
                        {
                            shouldHabitBeFollowed = true;
                        }
                        else
                        {
                            DateOnly lastDayOfInterval = default;

                            int dayInterval = habit.Frequency.IntervalLengthInDays;

                            DateOnly date = firstFollowedProgress.Date;
                            while (lastDayOfInterval == default)
                            {
                                date = date.AddDays( dayInterval );
                                if (date > progress.Date)
                                {
                                    lastDayOfInterval = date.AddDays( -1 );
                                }
                            }

                            DateOnly firstDayOfInterval = lastDayOfInterval.AddDays( -dayInterval + 1 );
                            int followedCountWithinInterval = habit.Progresses.Count( p => p.IsCompleted() && firstDayOfInterval <= p.Date && p.Date <= lastDayOfInterval );
                            int daysUntilEndOfInterval = (lastDayOfInterval.ToDateTime( zeroTime ) - dateTimeOfProgress).Days;

                            //including current day
                            int leftChances = daysUntilEndOfInterval + 1;
                            shouldHabitBeFollowed = leftChances <= goalOfFollowedCount - followedCountWithinInterval;
                        }
                    }
                    else
                    {
                        throw new ArgumentException( $"Invalid value of {nameof( ProgressOfHabit )}.{nameof( ProgressOfHabit.Habit )}.{nameof( UserHabit.Frequency )}.{nameof( FrequencyOfHabit.IntervalType )} in UncheckedProgressToImgConverter.ConvertFrom" );
                    }

                    break;
                }
        }

        bool result = shouldHabitBeFollowed;
        return result;
    }

    public async Task DeleteAsync(long id )
    {
        string url = $"{UrlBuilder.Habits}/{id}";
        await RequestProvider.DeleteAsync( url, SettingsService.AuthAccessToken );
    }
}
