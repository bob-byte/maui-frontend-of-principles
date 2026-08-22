using Principles.Exceptions;

namespace Principles.Core.Services;

public class ServiceOfHabit : BaseEntityService<UserHabit>, IServiceOfHabit
{
    private static readonly OperationKind SetArchiveStatusOperation = OperationKind.Custom( "SetArchiveStatus" );

    public ServiceOfHabit( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        EndProgressInterval = DateOnly.FromDateTime( DateTime.Today );
        StartProgressInterval = EndProgressInterval.AddDays( -HabitConstants.NUMBER_OF_DAYS_IN_PROGRESS + 1 );

        ReminderService = serviceProvider.GetRequiredService<IReminderService>();
        NetworkService = serviceProvider.GetRequiredService<INetworkService>();
        RemoteApi = serviceProvider.GetRequiredService<IHabitRemoteApi>();
    }

    private IReminderService ReminderService { get; }
    private INetworkService NetworkService { get; }

    public ObservableCollectionEx<UserHabit>? StoredUserHabits { get; set; }

    public DateOnly StartProgressInterval { get; }
    public DateOnly EndProgressInterval { get; }
    public IHabitRemoteApi RemoteApi { get; }

    public void InitializeHabitProgresses( UserHabit habit, DateOnly startInterval, DateOnly endInterval )
    {
        ArgumentNullException.ThrowIfNull( habit );
        EnsureHabitRuntimeDefaults( habit );

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
                        Habit = habit,
                        UserHabitLocalId = habit.LocalId
                    };
                    habit.Progresses.Add( progress );
                }
            }
        }

        habit.Progresses = new ObservableCollectionEx<ProgressOfHabit>( habit.Progresses.OrderByDescending( u => u.Date ) );
        foreach (ProgressOfHabit progress in habit.Progresses)
        {
            progress.Habit = habit;
            progress.UserHabitLocalId = habit.LocalId;
        }

        Recompute( habit );
    }

    public void Recompute( UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull( habit );
        EnsureHabitRuntimeDefaults( habit );

        if (habit.Progresses == null || habit.Progresses.Count < 2)
        {
            habit.PercentageAchieved = 0;
            return;
        }

        DateOnly from = habit.Progresses[^1].Date;
        DateOnly to = habit.Progresses[0].Date;
        habit.PercentageAchieved = RecomputedScoreAchieved( habit, from, to );
    }

    public double RecomputedScoreAchieved( UserHabit habit, DateOnly from, DateOnly to )
    {
        ArgumentNullException.ThrowIfNull( habit );
        EnsureHabitRuntimeDefaults( habit );

        FrequencyOfHabit frequency = habit.Frequency!;
        List<ProgressOfHabit> knownProgresses = habit.Progresses
            .Where( p => p.Value == ProgressValue.YES_MANUAL || p.Value == ProgressValue.NO )
            .ToList();

        habit.ComputedProgresses.RecomputeFrom( knownProgresses, frequency, isNumerical: false );
        habit.ScoreList.Recompute( habit.Complexity, frequency, habit.ComputedProgresses, from, to );

        return habit.ScoreList.Get( to ).Value;
    }

    public async Task<List<UserHabit>> ActiveHabitsAsync()
    {
        List<UserHabit> habits = await LoadHabitsAsync( isArchived: false ).ConfigureAwait( false );
        if (habits.Count == 0 && NetworkService.IsConnected)
        {
            List<UserHabit> remoteHabits = await RemoteApi.GetAllAsync().ConfigureAwait( false );
            foreach (UserHabit habit in remoteHabits)
            {
                await SaveHabitGraphAsync( habit ).ConfigureAwait( false );
            }

            habits = await LoadHabitsAsync( isArchived: false ).ConfigureAwait( false );
        }

        foreach (UserHabit habit in habits)
        {
            InitializeHabitProgresses( habit, StartProgressInterval, EndProgressInterval );
        }

        return habits.OrderBy( h => h.Priority ).ThenBy( h => h.Name ).ToList();
    }

    public async Task<UserHabit> UserHabitAsync( long localId )
    {
        if (localId == 0)
        {
            throw new ArgumentException( "LocalId is zero.", nameof( localId ) );
        }

        UserHabit? habit = await LoadHabitByLocalIdAsync( localId ).ConfigureAwait( false );
        if (habit is null)
        {
            throw new KeyNotFoundException( $"Habit with local id {localId} was not found." );
        }

        await RepairMissingFrequencyAsync( habit ).ConfigureAwait( false );
        return habit;
    }

    public async Task UpdateHabitAsync( UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull( habit );

        await SaveLocalGraphAsync( habit, includeProgresses: false ).ConfigureAwait( false );

        if (NetworkService.IsConnected)
        {
            try
            {
                await RemoteApi.SaveAsync( habit ).ConfigureAwait( false );
                return;
            }
            catch
            {
                // Queue below.
            }
        }

        await SyncQueueService.AddToQueueAsync( habit, OperationKind.Save ).ConfigureAwait( false );
    }

    public async Task SaveLocalGraphAsync( UserHabit habit, bool includeProgresses = true, bool preserveLastModified = false )
    {
        List<int> previousNotificationIds = habit.LocalId == 0
            ? []
            : await GetNotificationIdsAsync( habit.LocalId ).ConfigureAwait( false );

        await SaveHabitGraphAsync( habit, includeProgresses, preserveLastModified ).ConfigureAwait( false );
        await SyncNotificationsAsync( habit, previousNotificationIds ).ConfigureAwait( false );
    }

    public async Task SetHabitArchiveStatusAsync( UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull( habit );

        habit.LastModified = DateTime.UtcNow;
        await Database.UpdateFieldAsync<UserHabit, bool>( habit.LocalId, nameof( habit.IsArchived ), habit.IsArchived ).ConfigureAwait( false );
        await Database.UpdateFieldAsync<UserHabit, DateTime?>( habit.LocalId, nameof( habit.ArchivingTime ), habit.IsArchived ? DateTime.UtcNow : null ).ConfigureAwait( false );

        await SyncNotificationsAsync( habit ).ConfigureAwait( false );

        HabitArchiveStatus dto = new() { HabitId = habit.Id, IsArchived = habit.IsArchived, LastModified = habit.LastModified };
        if (habit.Id == 0)
        {
            await SyncQueueService.AddToQueueAsync( habit, OperationKind.Save ).ConfigureAwait( false );
            return;
        }

        if (NetworkService.IsConnected)
        {
            try
            {
                await RemoteApi.SetArchiveStatusAsync( dto ).ConfigureAwait( false );
                return;
            }
            catch
            {
                // Queue below.
            }
        }

        await SyncQueueService.AddToQueueAsync( nameof( UserHabit ), SetArchiveStatusOperation, dto, habit.Id, habit.LocalId ).ConfigureAwait( false );
    }

    public async Task<List<ArсhivedHabitDto>> GetArchivedHabits()
    {
        List<ArсhivedHabitDto> archivedHabits = (await LoadHabitsAsync( isArchived: true ).ConfigureAwait( false ))
            .Select( h => new ArсhivedHabitDto
            {
                Id = h.Id,
                LocalId = h.LocalId,
                Name = h.Name!,
                ArchivingTime = h.ArchivingTime ?? DateTime.MinValue
            } )
            .OrderByDescending( h => h.ArchivingTime )
            .ToList();

        if (archivedHabits.Count == 0 && NetworkService.IsConnected)
        {
            List<ArсhivedHabitDto> remoteArchived = await RemoteApi.GetArchivedAsync().ConfigureAwait( false );
            foreach (ArсhivedHabitDto archived in remoteArchived)
            {
                UserHabit? storedHabit = (await Database.WhereAsync<UserHabit>( h => h.Id == archived.Id ).ConfigureAwait( false ))
                    .FirstOrDefault();

                if (storedHabit is null && archived.Id != 0)
                {
                    UserHabit remoteHabit = await RemoteApi.GetByIdAsync( archived.Id ).ConfigureAwait( false );
                    remoteHabit.IsArchived = true;
                    await SaveHabitGraphAsync( remoteHabit ).ConfigureAwait( false );
                    archived.LocalId = remoteHabit.LocalId;
                }
                else if (storedHabit is not null)
                {
                    archived.LocalId = storedHabit.LocalId;
                }
            }

            archivedHabits = (await LoadHabitsAsync( isArchived: true ).ConfigureAwait( false ))
                .Select( h => new ArсhivedHabitDto
                {
                    Id = h.Id,
                    LocalId = h.LocalId,
                    Name = h.Name!,
                    ArchivingTime = h.ArchivingTime ?? DateTime.MinValue
                } )
                .OrderByDescending( h => h.ArchivingTime )
                .ToList();
        }

        return archivedHabits;
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
                UserHabit? habit = allHabits.FirstOrDefault( h => h.Equals( newHabit ) );
                if (habit is not null)
                {
                    allHabits.Remove( habit );
                }
            }

            List<UserHabit> activeHabits = allHabits
                .Where( h => h.PercentageAchieved < 0.4 && h.Status == StatusOfHabit.InProgress && !h.IsArchived )
                .ToList();
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
        ArgumentNullException.ThrowIfNull( progress );
        return progress.Value is not ProgressValue.YES_AUTO and ProgressValue.YES_MANUAL;
    }

    public async Task DeleteAsync( UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull( habit );

        await DeleteLocalGraphAsync( habit ).ConfigureAwait( false );

        if (habit.Id == 0)
        {
            return;
        }

        if (NetworkService.IsConnected)
        {
            try
            {
                await RemoteApi.DeleteAsync( habit.Id ).ConfigureAwait( false );
                return;
            }
            catch
            {
                // Queue below.
            }
        }

        await SyncQueueService.AddToQueueAsync( habit, OperationKind.Delete ).ConfigureAwait( false );
    }

    public async Task DeleteLocalGraphAsync( UserHabit habit )
    {
        List<int> notificationRequests = await GetNotificationIdsAsync( habit.LocalId ).ConfigureAwait( false );

        await Database.ExecuteAsync( $"DELETE FROM {nameof( UserAreaOfLifeUserHabit )} WHERE UserHabitLocalId = ?;", habit.LocalId ).ConfigureAwait( false );
        await Database.ExecuteAsync( $"DELETE FROM {nameof( ProgressOfHabit )} WHERE UserHabitLocalId = ?;", habit.LocalId ).ConfigureAwait( false );
        await Database.ExecuteAsync( $"DELETE FROM {nameof( WeekDay )} WHERE UserHabitReminderLocalId IN (SELECT LocalId FROM {nameof( UserHabitReminder )} WHERE UserHabitLocalId = ?);", habit.LocalId ).ConfigureAwait( false );
        await Database.ExecuteAsync( $"DELETE FROM {nameof( UserHabitReminder )} WHERE UserHabitLocalId = ?;", habit.LocalId ).ConfigureAwait( false );
        await Database.DeleteAsync( habit ).ConfigureAwait( false );

        foreach (int notificationId in notificationRequests)
        {
            await ReminderService.CancelLocallyAsync( notificationId ).ConfigureAwait( false );
        }
    }

    public int GetDaysUntilFullAutomation( UserHabit habit )
    {
        ArgumentNullException.ThrowIfNull( habit );
        EnsureHabitRuntimeDefaults( habit );

        if (habit.Progresses.Count == 0)
        {
            return 0;
        }

        FrequencyOfHabit frequency = habit.Frequency!;
        List<int> values = habit.ComputedProgresses
            .GetByInterval( habit.Progresses[^1].Date, habit.Progresses[0].Date )
            .Select( x => x.Value )
            .ToList();
        int extraDays = 0;

        while (true)
        {
            double value = Score.Get( habit.Complexity, frequency, values );
            int roundedScore = Score.Round( value );
            if (roundedScore >= 100)
            {
                break;
            }

            values.Insert( 0, ProgressValue.YES_MANUAL );
            extraDays++;

            if (extraDays > 365)
            {
                throw new InvalidOperationException( "Cannot define days until the habit is complete." );
            }
        }

        return extraDays;
    }
    private async Task<List<UserHabit>> LoadHabitsAsync( bool isArchived )
    {
        List<UserHabit> habits = await Database.WhereAsync<UserHabit>( h => h.IsArchived == isArchived ).ConfigureAwait( false );
        List<UserHabit> result = new( habits.Count );

        foreach (UserHabit habit in habits)
        {
            UserHabit? hydrated = await LoadHabitByLocalIdAsync( habit.LocalId ).ConfigureAwait( false );
            if (hydrated is not null)
            {
                await RepairMissingFrequencyAsync( hydrated ).ConfigureAwait( false );
                result.Add( hydrated );
            }
        }

        return result;
    }

    private async Task<UserHabit?> LoadHabitByLocalIdAsync( long localId )
    {
        UserHabit? habit = await Database.GetByIdAsync<UserHabit>( localId ).ConfigureAwait( false );
        if (habit is null)
        {
            return null;
        }

        if (habit.FrequencyLocalId != 0)
        {
            habit.Frequency = await Database.GetByIdAsync<FrequencyOfHabit>( habit.FrequencyLocalId ).ConfigureAwait( false );
        }

        if (habit.GoalLocalId is long goalLocalId && goalLocalId != 0)
        {
            habit.Goal = await Database.GetByIdAsync<UserGoal>( goalLocalId ).ConfigureAwait( false );
        }

        List<UserHabitReminder> reminders = await Database.WhereAsync<UserHabitReminder>( r => r.UserHabitLocalId == habit.LocalId ).ConfigureAwait( false );
        foreach (UserHabitReminder reminder in reminders)
        {
            reminder.DaysOfWeek = (await Database.WhereAsync<WeekDay>( d => d.UserHabitReminderLocalId == reminder.LocalId ).ConfigureAwait( false ))
                .OrderBy( d => d.Type )
                .ToList();
        }

        habit.Reminders = new ObservableCollectionEx<UserHabitReminder>( reminders );
        habit.Progresses = new ObservableCollectionEx<ProgressOfHabit>(
            (await Database.WhereAsync<ProgressOfHabit>( p => p.UserHabitLocalId == habit.LocalId ).ConfigureAwait( false ))
            .OrderByDescending( p => p.Date )
            .ToList()
        );

        habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>(
            await LoadAreasOfLifeForHabitAsync( habit.LocalId ).ConfigureAwait( false )
        );

        return habit;
    }

    private async Task RepairMissingFrequencyAsync( UserHabit habit )
    {
        EnsureFrequencyDefaults( habit );

        if (habit.LocalId == 0)
        {
            return;
        }

        if (habit.Frequency!.LocalId == 0)
        {
            await Database.SaveAsync( habit.Frequency ).ConfigureAwait( false );
        }
        else
        {
            await Database.SaveAsync( habit.Frequency ).ConfigureAwait( false );
        }

        if (habit.FrequencyLocalId != habit.Frequency.LocalId)
        {
            habit.FrequencyLocalId = habit.Frequency.LocalId;
            await Database.UpdateFieldAsync<UserHabit, long>(
                habit.LocalId,
                nameof( habit.FrequencyLocalId ),
                habit.FrequencyLocalId
            ).ConfigureAwait( false );
        }
    }

    private async Task<List<UserAreaOfLife>> LoadAreasOfLifeForHabitAsync( long habitLocalId )
    {
        List<UserAreaOfLifeUserHabit> links = await Database.WhereAsync<UserAreaOfLifeUserHabit>( l => l.UserHabitLocalId == habitLocalId ).ConfigureAwait( false );
        List<UserAreaOfLife> result = new( links.Count );

        foreach (UserAreaOfLifeUserHabit link in links)
        {
            UserAreaOfLife? area = await Database.GetByIdAsync<UserAreaOfLife>( link.UserAreaOfLifeLocalId ).ConfigureAwait( false );
            if (area is not null)
            {
                result.Add( area );
            }
        }

        return result;
    }

    private async Task EnsureLocalRelationsAsync( UserHabit habit, bool preserveLastModified = false )
    {
        if (!preserveLastModified || habit.LastModified == default)
        {
            habit.LastModified = DateTime.UtcNow;
        }

        EnsureFrequencyDefaults( habit );
        FrequencyOfHabit frequency = habit.Frequency!;
        if (!preserveLastModified || frequency.LastModified == default)
        {
            frequency.LastModified = habit.LastModified;
        }

        await Database.SaveAsync( frequency ).ConfigureAwait( false );
        habit.FrequencyLocalId = frequency.LocalId;

        if (habit.Goal is not null)
        {
            if (!preserveLastModified || habit.Goal.LastModified == default)
            {
                habit.Goal.LastModified = habit.LastModified;
            }

            await Database.SaveAsync( habit.Goal ).ConfigureAwait( false );
            habit.GoalLocalId = habit.Goal.LocalId;
        }
        else
        {
            habit.GoalLocalId = null;
        }
    }

    private async Task SaveHabitGraphAsync( UserHabit habit, bool includeProgresses = true, bool preserveLastModified = false )
    {
        PrepareHabitForSave( habit, includeProgresses );
        await EnsureLocalRelationsAsync( habit, preserveLastModified ).ConfigureAwait( false );
        await Database.SaveAsync( habit ).ConfigureAwait( false );

        await Database.ExecuteAsync( $"DELETE FROM {nameof( UserAreaOfLifeUserHabit )} WHERE UserHabitLocalId = ?;", habit.LocalId ).ConfigureAwait( false );
        if (habit.AreasOfLife?.Any() == true)
        {
            foreach (UserAreaOfLife area in habit.AreasOfLife)
            {
                if (!preserveLastModified || area.LastModified == default)
                {
                    area.LastModified = habit.LastModified;
                }

                await Database.SaveAsync( area ).ConfigureAwait( false );
                await Database.SaveAsync( new UserAreaOfLifeUserHabit
                {
                    UserAreaOfLifeLocalId = area.LocalId,
                    UserHabitLocalId = habit.LocalId
                } ).ConfigureAwait( false );
            }
        }

        await Database.ExecuteAsync( $"DELETE FROM {nameof( WeekDay )} WHERE UserHabitReminderLocalId IN (SELECT LocalId FROM {nameof( UserHabitReminder )} WHERE UserHabitLocalId = ?);", habit.LocalId ).ConfigureAwait( false );
        await Database.ExecuteAsync( $"DELETE FROM {nameof( UserHabitReminder )} WHERE UserHabitLocalId = ?;", habit.LocalId ).ConfigureAwait( false );
        if (habit.Reminders?.Any() == true)
        {
            foreach (UserHabitReminder reminder in habit.Reminders)
            {
                if (!preserveLastModified || reminder.LastModified == default)
                {
                    reminder.LastModified = habit.LastModified;
                }

                reminder.UserHabitLocalId = habit.LocalId;
                await Database.SaveAsync( reminder ).ConfigureAwait( false );

                if (reminder.DaysOfWeek?.Any() != true)
                {
                    continue;
                }

                foreach (WeekDay weekDay in reminder.DaysOfWeek)
                {
                    if (weekDay.UserNotificationRequestId == 0)
                    {
                        weekDay.UserNotificationRequestId = GenerateNotificationRequestId();
                    }

                    if (!preserveLastModified || weekDay.LastModified == default)
                    {
                        weekDay.LastModified = reminder.LastModified;
                    }

                    weekDay.UserHabitReminderLocalId = reminder.LocalId;
                    await Database.SaveAsync( weekDay ).ConfigureAwait( false );
                }
            }
        }

        if (includeProgresses && habit.Progresses?.Any() == true)
        {
            foreach (ProgressOfHabit progress in habit.Progresses)
            {
                if (!preserveLastModified || progress.LastModified == default)
                {
                    progress.LastModified = habit.LastModified;
                }

                progress.UserHabitLocalId = habit.LocalId;
                await Database.SaveAsync( progress ).ConfigureAwait( false );
            }
        }
    }

    private async Task<List<int>> GetNotificationIdsAsync( long habitLocalId )
    {
        List<int> result = new();
        List<UserHabitReminder> reminders = await Database.WhereAsync<UserHabitReminder>( r => r.UserHabitLocalId == habitLocalId ).ConfigureAwait( false );

        foreach (UserHabitReminder reminder in reminders)
        {
            List<WeekDay> weekDays = await Database.WhereAsync<WeekDay>( d => d.UserHabitReminderLocalId == reminder.LocalId ).ConfigureAwait( false );
            result.AddRange( weekDays.Select( w => w.UserNotificationRequestId ) );
        }

        return result;
    }

    private async Task SyncNotificationsAsync( UserHabit habit, IReadOnlyCollection<int>? previousNotificationIds = null )
    {
        HashSet<int> currentNotificationIds = habit.Reminders?
            .SelectMany( reminder => reminder.DaysOfWeek ?? [] )
            .Select( weekDay => weekDay.UserNotificationRequestId )
            .Where( notificationId => notificationId != 0 )
            .ToHashSet()
            ?? [];

        if (previousNotificationIds?.Count > 0)
        {
            foreach (int notificationId in previousNotificationIds.Where( notificationId => !currentNotificationIds.Contains( notificationId ) ))
            {
                await ReminderService.CancelLocallyAsync( notificationId ).ConfigureAwait( false );
            }
        }

        if (habit.Reminders?.Any() != true)
        {
            return;
        }

        foreach (UserHabitReminder reminder in habit.Reminders)
        {
            if (reminder.DaysOfWeek?.Any() != true)
            {
                continue;
            }

            foreach (WeekDay weekDay in reminder.DaysOfWeek)
            {
                await AddNotificationToDeviceAsync( habit, reminder, weekDay ).ConfigureAwait( false );
            }
        }
    }

    private async Task AddNotificationToDeviceAsync( UserHabit habit, UserHabitReminder reminder, WeekDay weekDay )
    {
        if (habit.IsArchived || !reminder.IsEnabled)
        {
            if (weekDay.UserNotificationRequestId != 0)
            {
                await ReminderService.CancelLocallyAsync( weekDay.UserNotificationRequestId ).ConfigureAwait( false );
            }

            return;
        }

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
            string.IsNullOrWhiteSpace( reminder.Title ) ? "Reminder" : reminder.Title,
            reminder.Description ?? string.Empty,
            notifyDateTime,
            ReminderRepeat.Weekly
        ).ConfigureAwait( false );
    }

    private static void PrepareHabitForSave( UserHabit habit, bool includeProgresses )
    {
        List<string> validationErrors = [];

        habit.Name = TrimToNull( habit.Name );
        habit.Description = TrimToNull( habit.Description );
        habit.ColorName = TrimToNull( habit.ColorName );
        EnsureHabitRuntimeDefaults( habit );

        ValidateHabit( habit, validationErrors );
        SanitizeGoal( habit );
        SanitizeAreasOfLife( habit );
        SanitizeReminders( habit );

        if (includeProgresses || habit.Progresses?.Any() == true)
        {
            SanitizeProgresses( habit );
        }

        if (validationErrors.Count > 0)
        {
            throw new HabitSaveValidationException( validationErrors );
        }
    }

    private static void ValidateHabit( UserHabit habit, ICollection<string> validationErrors )
    {
        if (habit.Name is null)
        {
            validationErrors.Add( "Habit name is required." );
        }

        if (!Enum.IsDefined( habit.Type ) || habit.Type == TypeOfHabit.None)
        {
            validationErrors.Add( "Habit type is invalid." );
        }

        if (!Enum.IsDefined( habit.Status ))
        {
            validationErrors.Add( "Habit status is invalid." );
        }

        if (habit.Complexity < HabitConstants.MIN_HABIT_COMPLEXITY || habit.Complexity > HabitConstants.MAX_HABIT_COMPLEXITY)
        {
            validationErrors.Add( $"Habit complexity must be between {HabitConstants.MIN_HABIT_COMPLEXITY} and {HabitConstants.MAX_HABIT_COMPLEXITY}." );
        }

        ValidateFrequency( habit.Frequency!, validationErrors );
    }

    private static void ValidateFrequency( FrequencyOfHabit frequency, ICollection<string> validationErrors )
    {
        if (!Enum.IsDefined( frequency.Type ))
        {
            validationErrors.Add( "Habit frequency type is invalid." );
        }

        if (frequency.Repeats <= 0)
        {
            validationErrors.Add( "Habit frequency repeats must be greater than zero." );
        }

        if (frequency.IntervalLengthInDays <= 0)
        {
            validationErrors.Add( "Habit frequency interval length must be greater than zero." );
        }
    }

    private static void EnsureFrequencyDefaults( UserHabit habit )
    {
        habit.Frequency ??= new FrequencyOfHabit();

        if (!Enum.IsDefined( habit.Frequency.Type ))
        {
            habit.Frequency.Type = FrequencyType.EveryDay;
        }

        if (habit.Frequency.IntervalLengthInDays <= 0)
        {
            habit.Frequency.IntervalLengthInDays = 1;
        }

        if (habit.Frequency.Repeats <= 0)
        {
            habit.Frequency.Repeats = 1;
        }

        if (habit.Frequency.Type == FrequencyType.EveryDay)
        {
            habit.Frequency.IntervalLengthInDays = 1;
            habit.Frequency.Repeats = 1;
        }
        else if (habit.Frequency.Type == FrequencyType.EverySeveralDays)
        {
            habit.Frequency.Repeats = 1;
        }
    }

    private static void EnsureHabitRuntimeDefaults( UserHabit habit )
    {
        EnsureFrequencyDefaults( habit );

        if (habit.Complexity < HabitConstants.MIN_HABIT_COMPLEXITY || habit.Complexity > HabitConstants.MAX_HABIT_COMPLEXITY)
        {
            habit.Complexity = HabitConstants.DEFAULT_HABIT_COMPLEXITY;
        }

        habit.Progresses ??= new ObservableCollectionEx<ProgressOfHabit>();
        habit.ComputedProgresses ??= new ListOfProgressOfHabit( habit );
        habit.ScoreList ??= new ScoreList();
    }

    private static void SanitizeGoal( UserHabit habit )
    {
        if (habit.Goal is null)
        {
            habit.GoalLocalId = null;
            return;
        }

        habit.Goal.Name = TrimToNull( habit.Goal.Name );
        if (habit.Goal.Name is null)
        {
            habit.Goal = null;
            habit.GoalLocalId = null;
            return;
        }

        if (habit.Goal.Id == 0 && habit.Goal.LocalId == 0)
        {
            habit.GoalLocalId = null;
        }
    }

    private static void SanitizeAreasOfLife( UserHabit habit )
    {
        if (habit.AreasOfLife is null)
        {
            return;
        }

        List<UserAreaOfLife> sanitizedAreas = [];
        HashSet<long> seenLocalIds = [];
        HashSet<long> seenRemoteIds = [];
        HashSet<string> seenNames = new( StringComparer.OrdinalIgnoreCase );

        foreach (UserAreaOfLife area in habit.AreasOfLife)
        {
            if (area is null)
            {
                continue;
            }

            area.Name = TrimToNull( area.Name );
            if (area.Name is null)
            {
                continue;
            }

            if (area.Id == 0 && area.LocalId == 0)
            {
                continue;
            }

            if (area.LocalId != 0 && !seenLocalIds.Add( area.LocalId ))
            {
                continue;
            }

            if (area.Id != 0 && !seenRemoteIds.Add( area.Id ))
            {
                continue;
            }

            if (!seenNames.Add( area.Name ))
            {
                continue;
            }

            sanitizedAreas.Add( area );
        }

        habit.AreasOfLife = new ObservableCollectionEx<UserAreaOfLife>( sanitizedAreas );
    }

    private static void SanitizeReminders( UserHabit habit )
    {
        if (habit.Reminders is null)
        {
            return;
        }

        List<UserHabitReminder> sanitizedReminders = [];
        foreach (UserHabitReminder reminder in habit.Reminders)
        {
            if (reminder is null)
            {
                continue;
            }

            reminder.Title = TrimToNull( reminder.Title );
            reminder.Description = TrimToNull( reminder.Description );
            reminder.DaysOfWeek = SanitizeWeekDays( reminder.DaysOfWeek );

            if (reminder.Title is null || reminder.DaysOfWeek.Count == 0)
            {
                continue;
            }

            sanitizedReminders.Add( reminder );
        }

        habit.Reminders = new ObservableCollectionEx<UserHabitReminder>( sanitizedReminders );
    }

    private static List<WeekDay> SanitizeWeekDays( IEnumerable<WeekDay>? weekDays )
    {
        if (weekDays is null)
        {
            return [];
        }

        List<WeekDay> sanitizedWeekDays = [];
        HashSet<DayOfWeek> seenDays = [];

        foreach (WeekDay weekDay in weekDays)
        {
            if (weekDay is null || !Enum.IsDefined( weekDay.Type ) || !seenDays.Add( weekDay.Type ))
            {
                continue;
            }

            sanitizedWeekDays.Add( weekDay );
        }

        return sanitizedWeekDays
            .OrderBy( weekDay => weekDay.Type )
            .ToList();
    }

    private static void SanitizeProgresses( UserHabit habit )
    {
        if (habit.Progresses is null)
        {
            return;
        }

        List<ProgressOfHabit> sanitizedProgresses = [];
        HashSet<int> usedDates = [];

        for (int progressIndex = habit.Progresses.Count - 1; progressIndex >= 0; progressIndex--)
        {
            ProgressOfHabit progress = habit.Progresses[progressIndex];
            if (progress is null || !IsValidProgress( progress ) || !usedDates.Add( progress.DateAsInt ))
            {
                continue;
            }

            progress.Habit = habit;
            progress.UserHabitLocalId = habit.LocalId;
            progress.Notes = TrimToNull( progress.Notes );
            sanitizedProgresses.Add( progress );
        }

        sanitizedProgresses.Reverse();
        habit.Progresses = new ObservableCollectionEx<ProgressOfHabit>(
            sanitizedProgresses.OrderByDescending( progress => progress.DateAsInt )
        );
    }

    private static bool IsValidProgress( ProgressOfHabit progress )
    {
        return progress.DateAsInt >= DateOnly.MinValue.DayNumber &&
            progress.Value is ProgressValue.UNKNOWN or ProgressValue.NO or ProgressValue.YES_AUTO or ProgressValue.YES_MANUAL or ProgressValue.SKIP;
    }

    private static string? TrimToNull( string? value )
    {
        return string.IsNullOrWhiteSpace( value ) ? null : value.Trim();
    }

    private static int GenerateNotificationRequestId()
    {
        return Math.Abs( Random.Shared.Next( 1, int.MaxValue ) );
    }
}
