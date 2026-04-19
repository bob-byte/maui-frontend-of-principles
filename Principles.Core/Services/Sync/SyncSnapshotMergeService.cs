namespace Principles.Core.Services;

public class SyncSnapshotMergeService : ISyncSnapshotMergeService
{
    private readonly IDatabase m_database;
    private readonly ISyncQueueService m_syncQueueService;
    private readonly ServiceOfHabit m_habitService;
    private readonly IGoalService m_goalService;
    private readonly IReminderService m_reminderService;
    private readonly ISyncStateNotifier m_stateNotifier;

    public SyncSnapshotMergeService( IServiceProvider serviceProvider )
    {
        m_database = serviceProvider.GetRequiredService<IDatabase>();
        m_syncQueueService = serviceProvider.GetRequiredService<ISyncQueueService>();
        m_habitService = serviceProvider.GetRequiredService<ServiceOfHabit>();
        m_goalService = serviceProvider.GetRequiredService<IGoalService>();
        m_reminderService = serviceProvider.GetRequiredService<IReminderService>();
        m_stateNotifier = serviceProvider.GetRequiredService<ISyncStateNotifier>();
    }

    public async Task MergeAsync( SyncBootstrapResponse snapshot )
    {
        ArgumentNullException.ThrowIfNull( snapshot );

        List<SyncQueueItem> userQueueItems = await m_syncQueueService.GetBlockingItemsAsync( nameof( User ) ).ConfigureAwait( false );
        List<SyncQueueItem> goalQueueItems = await m_syncQueueService.GetBlockingItemsAsync( nameof( UserGoal ) ).ConfigureAwait( false );
        List<SyncQueueItem> habitQueueItems = await m_syncQueueService.GetBlockingItemsAsync( nameof( UserHabit ) ).ConfigureAwait( false );
        List<SyncQueueItem> progressQueueItems = await m_syncQueueService.GetBlockingItemsAsync( nameof( ProgressOfHabit ) ).ConfigureAwait( false );
        List<SyncQueueItem> reminderQueueItems = await m_syncQueueService.GetBlockingItemsAsync( nameof( Reminder ) ).ConfigureAwait( false );

        await MergeUserAsync( snapshot.User, userQueueItems ).ConfigureAwait( false );
        await MergeGoalsAsync( snapshot.Goals ?? [], goalQueueItems ).ConfigureAwait( false );
        await MergeHabitsAsync(
            (snapshot.ActiveHabits ?? []).Concat( snapshot.ArchivedHabits ?? [] ).ToList(),
            habitQueueItems,
            progressQueueItems
        ).ConfigureAwait( false );
        await MergeHabitsReportReminderAsync( snapshot.HabitsReportReminder, reminderQueueItems ).ConfigureAwait( false );
        await RefreshCachesAsync().ConfigureAwait( false );
    }

    private async Task MergeUserAsync( User? serverUser, IReadOnlyCollection<SyncQueueItem> queueItems )
    {
        if (serverUser is null)
        {
            return;
        }

        User? localUser = (await m_database.GetAllAsync<User>().ConfigureAwait( false )).FirstOrDefault();
        if (localUser is not null && queueItems.Count > 0)
        {
            return;
        }

        NormalizeUser( serverUser );

        if (localUser is not null)
        {
            if (localUser.LastModified > serverUser.LastModified)
            {
                return;
            }

            serverUser.LocalId = localUser.LocalId;
        }

        serverUser.IsAllDataSyncedOnFirstStart = true;
        await m_database.SaveAsync( serverUser ).ConfigureAwait( false );
        m_stateNotifier.NotifyUserChanged( serverUser );
    }

    private async Task MergeGoalsAsync( IReadOnlyCollection<UserGoal> serverGoals, IReadOnlyCollection<SyncQueueItem> queueItems )
    {
        List<UserGoal> localGoals = await m_database.GetAllAsync<UserGoal>().ConfigureAwait( false );
        Dictionary<long, UserGoal> localGoalsById = localGoals
            .Where( goal => goal.Id != 0 )
            .ToDictionary( goal => goal.Id );

        foreach (UserGoal serverGoal in serverGoals)
        {
            NormalizeGoal( serverGoal );

            if (!localGoalsById.TryGetValue( serverGoal.Id, out UserGoal? localGoal ))
            {
                await m_database.SaveAsync( serverGoal ).ConfigureAwait( false );
                continue;
            }

            if (HasPendingGoal( localGoal, queueItems ) || localGoal.LastModified > serverGoal.LastModified)
            {
                continue;
            }

            serverGoal.LocalId = localGoal.LocalId;
            await m_database.SaveAsync( serverGoal ).ConfigureAwait( false );
        }

        HashSet<long> serverGoalIds = serverGoals.Select( goal => goal.Id ).ToHashSet();
        foreach (UserGoal localGoal in localGoals.Where( goal => goal.Id != 0 && !serverGoalIds.Contains( goal.Id ) ))
        {
            if (HasPendingGoal( localGoal, queueItems ))
            {
                continue;
            }

            await DetachGoalFromHabitsAsync( localGoal.LocalId ).ConfigureAwait( false );
            await m_database.DeleteAsync( localGoal ).ConfigureAwait( false );
        }
    }

    private async Task MergeHabitsAsync(
        IReadOnlyCollection<UserHabit> serverHabits,
        IReadOnlyCollection<SyncQueueItem> habitQueueItems,
        IReadOnlyCollection<SyncQueueItem> progressQueueItems )
    {
        List<UserHabit> localHabits = await LoadAllLocalHabitsAsync().ConfigureAwait( false );
        Dictionary<long, UserHabit> localHabitsById = localHabits
            .Where( habit => habit.Id != 0 )
            .ToDictionary( habit => habit.Id );

        foreach (UserHabit serverHabit in serverHabits)
        {
            NormalizeServerHabitGraph( serverHabit );

            if (!localHabitsById.TryGetValue( serverHabit.Id, out UserHabit? localHabit ))
            {
                await AttachKnownLocalIdsAsync( serverHabit ).ConfigureAwait( false );
                await m_habitService.SaveLocalGraphAsync( serverHabit, preserveLastModified: true ).ConfigureAwait( false );
                continue;
            }

            if (HasPendingHabit( localHabit, habitQueueItems ) ||
                localHabit.LastModified > serverHabit.LastModified)
            {
                localHabit.Progresses = MergeProgressesByTimestamp( localHabit, localHabit, serverHabit, progressQueueItems );
                await m_habitService.SaveLocalGraphAsync( localHabit, preserveLastModified: true ).ConfigureAwait( false );
                continue;
            }

            await AttachKnownLocalIdsAsync( serverHabit, localHabit ).ConfigureAwait( false );
            serverHabit.Progresses = MergeProgressesByTimestamp( serverHabit, localHabit, serverHabit, progressQueueItems );
            await m_habitService.SaveLocalGraphAsync( serverHabit, preserveLastModified: true ).ConfigureAwait( false );
        }

        HashSet<long> serverHabitIds = serverHabits.Select( habit => habit.Id ).ToHashSet();
        foreach (UserHabit localHabit in localHabits.Where( habit => habit.Id != 0 && !serverHabitIds.Contains( habit.Id ) ))
        {
            if (HasPendingHabit( localHabit, habitQueueItems ) || HasPendingProgresses( localHabit, progressQueueItems ))
            {
                continue;
            }

            await m_habitService.DeleteLocalGraphAsync( localHabit ).ConfigureAwait( false );
        }
    }

    private async Task MergeHabitsReportReminderAsync( Reminder? serverReminder, IReadOnlyCollection<SyncQueueItem> queueItems )
    {
        Reminder? localReminder = (await m_database.GetAllAsync<Reminder>().ConfigureAwait( false )).FirstOrDefault();
        if (queueItems.Count > 0 && localReminder is not null)
        {
            return;
        }

        if (serverReminder is null || IsEmptyReminder( serverReminder ))
        {
            if (localReminder is null)
            {
                return;
            }

            if (localReminder.UserNotificationRequestId != 0)
            {
                await m_reminderService.CancelLocallyAsync( localReminder.UserNotificationRequestId ).ConfigureAwait( false );
            }

            await m_database.DeleteAsync( localReminder ).ConfigureAwait( false );
            return;
        }

        NormalizeReminder( serverReminder );

        if (localReminder is not null)
        {
            if (localReminder.LastModified > serverReminder.LastModified)
            {
                return;
            }

            serverReminder.LocalId = localReminder.LocalId;
        }

        await m_database.SaveAsync( serverReminder ).ConfigureAwait( false );
        await ApplyReminderLocallyAsync( serverReminder ).ConfigureAwait( false );
    }

    private async Task RefreshCachesAsync()
    {
        List<UserGoal> localGoals = (await m_database.GetAllAsync<UserGoal>().ConfigureAwait( false ))
            .OrderBy( goal => goal.Name )
            .ToList();

        if (m_goalService.StoredGoals is not null)
        {
            m_goalService.StoredGoals.Reload( localGoals );
        }

        if (m_habitService.StoredUserHabits is not null)
        {
            List<UserHabit> activeHabits = await m_habitService.ActiveHabitsAsync().ConfigureAwait( false );
            m_habitService.StoredUserHabits.Reload( activeHabits.OrderBy( habit => habit.Priority ).ThenBy( habit => habit.Name ) );
        }
    }

    private async Task<List<UserHabit>> LoadAllLocalHabitsAsync()
    {
        List<UserHabit> roots = await m_database.GetAllAsync<UserHabit>().ConfigureAwait( false );
        List<UserHabit> result = new( roots.Count );

        foreach (UserHabit root in roots)
        {
            UserHabit habit = await m_habitService.UserHabitAsync( root.LocalId ).ConfigureAwait( false );
            result.Add( habit );
        }

        return result;
    }

    private async Task AttachKnownLocalIdsAsync( UserHabit serverHabit, UserHabit? localHabit = null )
    {
        if (localHabit is not null)
        {
            serverHabit.LocalId = localHabit.LocalId;
        }

        if (serverHabit.Frequency is not null && localHabit?.Frequency is not null && localHabit.Frequency.Id == serverHabit.Frequency.Id)
        {
            serverHabit.Frequency.LocalId = localHabit.Frequency.LocalId;
            serverHabit.FrequencyLocalId = localHabit.FrequencyLocalId;
        }

        if (serverHabit.Goal is not null)
        {
            UserGoal? localGoal = localHabit?.Goal?.Id == serverHabit.Goal.Id
                ? localHabit.Goal
                : (await m_database.WhereAsync<UserGoal>( goal => goal.Id == serverHabit.Goal.Id ).ConfigureAwait( false )).FirstOrDefault();

            if (localGoal is not null)
            {
                serverHabit.Goal.LocalId = localGoal.LocalId;
                serverHabit.GoalLocalId = localGoal.LocalId;
            }
        }

        Dictionary<long, UserAreaOfLife> storedAreasById = (await m_database.GetAllAsync<UserAreaOfLife>().ConfigureAwait( false ))
            .Where( area => area.Id != 0 )
            .GroupBy( area => area.Id )
            .ToDictionary( group => group.Key, group => group.First() );

        if (serverHabit.AreasOfLife is not null)
        {
            foreach (UserAreaOfLife area in serverHabit.AreasOfLife)
            {
                if (area.Id != 0 && storedAreasById.TryGetValue( area.Id, out UserAreaOfLife? localArea ))
                {
                    area.LocalId = localArea.LocalId;
                }
            }
        }

        Dictionary<long, UserHabitReminder> localRemindersById = localHabit?.Reminders?
            .Where( reminder => reminder.Id != 0 )
            .ToDictionary( reminder => reminder.Id ) ?? [];

        if (serverHabit.Reminders is not null)
        {
            foreach (UserHabitReminder reminder in serverHabit.Reminders)
            {
                if (reminder.Id != 0 && localRemindersById.TryGetValue( reminder.Id, out UserHabitReminder? localReminder ))
                {
                    reminder.LocalId = localReminder.LocalId;

                    Dictionary<long, WeekDay> localWeekDaysById = localReminder.DaysOfWeek?
                        .Where( day => day.Id != 0 )
                        .ToDictionary( day => day.Id ) ?? [];

                    if (reminder.DaysOfWeek is null)
                    {
                        continue;
                    }

                    foreach (WeekDay weekDay in reminder.DaysOfWeek)
                    {
                        if (weekDay.Id != 0 && localWeekDaysById.TryGetValue( weekDay.Id, out WeekDay? localWeekDay ))
                        {
                            weekDay.LocalId = localWeekDay.LocalId;
                        }
                    }
                }
            }
        }

        Dictionary<long, ProgressOfHabit> localProgressesById = localHabit?.Progresses?
            .Where( progress => progress.Id != 0 )
            .ToDictionary( progress => progress.Id ) ?? [];
        Dictionary<int, ProgressOfHabit> localProgressesByDate = localHabit?.Progresses?
            .GroupBy( progress => progress.DateAsInt )
            .ToDictionary( group => group.Key, group => group.First() ) ?? [];

        if (serverHabit.Progresses is not null)
        {
            foreach (ProgressOfHabit progress in serverHabit.Progresses)
            {
                if (progress.Id != 0 && localProgressesById.TryGetValue( progress.Id, out ProgressOfHabit? localProgress ))
                {
                    progress.LocalId = localProgress.LocalId;
                }
                else if (localProgressesByDate.TryGetValue( progress.DateAsInt, out localProgress ))
                {
                    progress.LocalId = localProgress.LocalId;
                }
            }
        }
    }

    private async Task DetachGoalFromHabitsAsync( long goalLocalId )
    {
        List<UserHabit> habits = await m_database.WhereAsync<UserHabit>( habit => habit.GoalLocalId == goalLocalId ).ConfigureAwait( false );
        foreach (UserHabit habit in habits)
        {
            habit.GoalLocalId = null;
            await m_database.SaveAsync( habit ).ConfigureAwait( false );
        }
    }

    private async Task ApplyReminderLocallyAsync( Reminder reminder )
    {
        if (reminder.IsEnabled)
        {
            await m_reminderService.SaveLocallyAsync(
                reminder.UserNotificationRequestId,
                reminder.Title,
                reminder.Description,
                DateTime.Today.Add( reminder.Time.ToTimeSpan() ),
                ReminderRepeat.Daily
            ).ConfigureAwait( false );
        }
        else if (reminder.UserNotificationRequestId != 0)
        {
            await m_reminderService.CancelLocallyAsync( reminder.UserNotificationRequestId ).ConfigureAwait( false );
        }
    }

    private static bool IsEmptyReminder( Reminder reminder )
    {
        return reminder.Id == 0 &&
            string.IsNullOrWhiteSpace( reminder.Title ) &&
            string.IsNullOrWhiteSpace( reminder.Description ) &&
            !reminder.IsEnabled;
    }

    private static bool HasPendingGoal( UserGoal goal, IEnumerable<SyncQueueItem> queueItems )
    {
        return queueItems.Any( item =>
            (item.EntityId is long entityId && entityId != 0 && entityId == goal.Id) ||
            (item.EntityLocalId is long entityLocalId && entityLocalId != 0 && entityLocalId == goal.LocalId) );
    }

    private static bool HasPendingHabit( UserHabit habit, IEnumerable<SyncQueueItem> queueItems )
    {
        foreach (SyncQueueItem item in queueItems)
        {
            if ((item.EntityId is long entityId && entityId != 0 && entityId == habit.Id) ||
                (item.EntityLocalId is long entityLocalId && entityLocalId != 0 && entityLocalId == habit.LocalId))
            {
                return true;
            }

            if (new OperationKind( item.Operation ) == "SetArchiveStatus" && item.PayloadJson is not null)
            {
                HabitArchiveStatus? status = JsonSerializer.Deserialize<HabitArchiveStatus>( item.PayloadJson );
                if (status?.HabitId == habit.Id && habit.Id != 0)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasPendingProgresses( UserHabit habit, IEnumerable<SyncQueueItem> queueItems )
    {
        HashSet<long> localProgressIds = habit.Progresses?.Where( progress => progress.LocalId != 0 ).Select( progress => progress.LocalId ).ToHashSet() ?? [];
        HashSet<long> serverProgressIds = habit.Progresses?.Where( progress => progress.Id != 0 ).Select( progress => progress.Id ).ToHashSet() ?? [];

        return queueItems.Any( item =>
            (item.EntityLocalId is long entityLocalId && entityLocalId != 0 && localProgressIds.Contains( entityLocalId )) ||
            (item.EntityId is long entityId && entityId != 0 && serverProgressIds.Contains( entityId )));
    }

    private static ObservableCollectionEx<ProgressOfHabit> MergeProgressesByTimestamp(
        UserHabit owner,
        UserHabit localHabit,
        UserHabit serverHabit,
        IEnumerable<SyncQueueItem> progressQueueItems )
    {
        Dictionary<int, ProgressOfHabit> localByDate = localHabit.Progresses?
            .GroupBy( progress => progress.DateAsInt )
            .ToDictionary( group => group.Key, group => group.First() ) ?? [];

        Dictionary<int, ProgressOfHabit> serverByDate = serverHabit.Progresses?
            .GroupBy( progress => progress.DateAsInt )
            .ToDictionary( group => group.Key, group => group.First() ) ?? [];

        List<ProgressOfHabit> merged = new();

        foreach (ProgressOfHabit serverProgress in serverByDate.Values)
        {
            if (localByDate.TryGetValue( serverProgress.DateAsInt, out ProgressOfHabit? localProgress ))
            {
                if (ShouldKeepLocalProgress( localProgress, serverProgress, progressQueueItems ))
                {
                    AttachProgressToHabit( localProgress, owner );
                    merged.Add( localProgress );
                    continue;
                }

                serverProgress.LocalId = localProgress.LocalId;
            }

            AttachProgressToHabit( serverProgress, owner );
            merged.Add( serverProgress );
        }

        foreach (ProgressOfHabit localOnlyProgress in localByDate.Values.Where( progress => !serverByDate.ContainsKey( progress.DateAsInt ) ))
        {
            if (localOnlyProgress.Id == 0 ||
                HasPendingProgress( localOnlyProgress, progressQueueItems ) ||
                NormalizeTimestamp( localOnlyProgress.LastModified ) > NormalizeTimestamp( owner.LastModified ))
            {
                AttachProgressToHabit( localOnlyProgress, owner );
                merged.Add( localOnlyProgress );
            }
        }

        return new ObservableCollectionEx<ProgressOfHabit>(
            merged
                .GroupBy( progress => progress.DateAsInt )
                .Select( group => group.OrderByDescending( progress => NormalizeTimestamp( progress.LastModified ) ).First() )
                .OrderByDescending( progress => progress.DateAsInt )
        );
    }

    private static bool ShouldKeepLocalProgress(
        ProgressOfHabit localProgress,
        ProgressOfHabit serverProgress,
        IEnumerable<SyncQueueItem> progressQueueItems )
    {
        return HasPendingProgress( localProgress, progressQueueItems ) ||
            NormalizeTimestamp( localProgress.LastModified ) > NormalizeTimestamp( serverProgress.LastModified );
    }

    private static bool HasPendingProgress( ProgressOfHabit progress, IEnumerable<SyncQueueItem> queueItems )
    {
        return queueItems.Any( item =>
            (item.EntityLocalId is long entityLocalId && entityLocalId != 0 && entityLocalId == progress.LocalId) ||
            (item.EntityId is long entityId && entityId != 0 && entityId == progress.Id) );
    }

    private static void AttachProgressToHabit( ProgressOfHabit progress, UserHabit habit )
    {
        progress.Habit = habit;
        progress.UserHabitLocalId = habit.LocalId;
    }

    private static void NormalizeUser( User user )
    {
        user.LastModified = NormalizeTimestamp( user.LastModified );
    }

    private static void NormalizeGoal( UserGoal goal )
    {
        goal.LastModified = NormalizeTimestamp( goal.LastModified );
    }

    private static void NormalizeReminder( Reminder reminder )
    {
        reminder.LastModified = NormalizeTimestamp( reminder.LastModified );
    }

    private static void NormalizeServerHabitGraph( UserHabit habit )
    {
        DateTime habitLastModified = NormalizeTimestamp( habit.LastModified );
        habit.LastModified = habitLastModified;

        if (habit.Frequency is not null)
        {
            habit.Frequency.LastModified = NormalizeTimestamp( habit.Frequency.LastModified, habitLastModified );
        }

        if (habit.Goal is not null)
        {
            habit.Goal.LastModified = NormalizeTimestamp( habit.Goal.LastModified, habitLastModified );
        }

        if (habit.AreasOfLife is not null)
        {
            foreach (UserAreaOfLife area in habit.AreasOfLife)
            {
                area.LastModified = NormalizeTimestamp( area.LastModified, habitLastModified );
            }
        }

        if (habit.Reminders is not null)
        {
            foreach (UserHabitReminder reminder in habit.Reminders)
            {
                reminder.LastModified = NormalizeTimestamp( reminder.LastModified, habitLastModified );

                if (reminder.DaysOfWeek is null)
                {
                    continue;
                }

                foreach (WeekDay weekDay in reminder.DaysOfWeek)
                {
                    weekDay.LastModified = NormalizeTimestamp( weekDay.LastModified, reminder.LastModified );
                }
            }
        }

        if (habit.Progresses is not null)
        {
            foreach (ProgressOfHabit progress in habit.Progresses)
            {
                progress.LastModified = NormalizeTimestamp( progress.LastModified, habitLastModified );
            }
        }
    }

    private static DateTime NormalizeTimestamp( DateTime timestamp, DateTime? fallback = null )
    {
        return timestamp == default ? fallback ?? DateTime.UtcNow : DateTime.SpecifyKind( timestamp, DateTimeKind.Utc );
    }
}
