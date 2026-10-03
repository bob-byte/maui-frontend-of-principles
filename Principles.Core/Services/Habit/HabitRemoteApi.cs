namespace Principles.Core.Services;

public interface IHabitRemoteApi
{
    Task DeleteAsync( long entityId );
    Task<List<UserHabit>> GetAllAsync();
    Task<List<ArсhivedHabitDto>> GetArchivedAsync();
    Task<UserHabit> GetByIdAsync( long entityId );
    Task SaveAsync( UserHabit habit );
    Task SetArchiveStatusAsync( HabitArchiveStatus status );
}

public class HabitRemoteApi : RemoteApiService<UserHabit>, IHabitRemoteApi
{
    public HabitRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
    }

    public Task<List<UserHabit>> GetAllAsync()
    {
        return RequestProvider.GetAsync<List<UserHabit>>( UrlBuilder.HabitsInProgress, SettingsService.AuthAccessToken! );
    }

    public Task<List<ArсhivedHabitDto>> GetArchivedAsync()
    {
        return RequestProvider.GetAsync<List<ArсhivedHabitDto>>( UrlBuilder.Archive, SettingsService.AuthAccessToken! );
    }

    public Task<UserHabit> GetByIdAsync( long entityId )
    {
        return RequestProvider.GetAsync<UserHabit>( $"{UrlBuilder.Habits}/{entityId}", SettingsService.AuthAccessToken! );
    }

    public async Task SaveAsync( UserHabit habit )
    {
        habit.Id = await Database.GetFieldAsync<UserHabit, long>( habit.LocalId, nameof( habit.Id ) ).ConfigureAwait( false );
        if (habit.GoalLocalId is long goalLocalId && goalLocalId != 0)
        {
            long goalId = await Database.GetFieldAsync<UserGoal, long>( goalLocalId, nameof( UserGoal.Id ) ).ConfigureAwait( false );
            if (habit.Goal is not null)
            {
                habit.Goal.Id = goalId;
            }
        }

        EditUserHabitDto dto = await BuildDtoAsync( habit ).ConfigureAwait( false );
        SaveHabitResponse response = await RequestProvider.PostAsync<EditUserHabitDto, SaveHabitResponse>(
            $"{UrlBuilder.Habits}/{habit.Id}",
            dto,
            SettingsService.AuthAccessToken!
        ).ConfigureAwait( false );

        habit.Id = response.Id;
        await Database.UpdateFieldAsync<UserHabit, long>( habit.LocalId, nameof( habit.Id ), response.Id ).ConfigureAwait( false );
        await Database.UpdateFieldAsync<FrequencyOfHabit, long>( habit.Frequency!.LocalId, nameof( habit.Frequency.Id ), response.FrequencyId ).ConfigureAwait( false );

        if (response.ReminderIds is null || habit.Reminders?.Any() != true)
        {
            return;
        }

        for (int reminderIndex = 0; reminderIndex < response.ReminderIds.Count && reminderIndex < habit.Reminders.Count; reminderIndex++)
        {
            SaveHabitResponse.Reminder dtoReminder = response.ReminderIds[reminderIndex];
            UserHabitReminder reminder = habit.Reminders[reminderIndex];

            await Database.UpdateFieldAsync<UserHabitReminder, long>( reminder.LocalId, nameof( reminder.Id ), dtoReminder.Id ).ConfigureAwait( false );
            reminder.Id = dtoReminder.Id;

            if (dtoReminder.DaysOfWeek is null || reminder.DaysOfWeek is null)
            {
                continue;
            }

            for (int weekDayIndex = 0; weekDayIndex < dtoReminder.DaysOfWeek.Count; weekDayIndex++)
            {
                SaveHabitResponse.WeekDay dtoWeekDay = dtoReminder.DaysOfWeek[weekDayIndex];
                WeekDay? weekDay = reminder.DaysOfWeek.FirstOrDefault( d => d.Type == dtoWeekDay.Type );
                if (weekDay is null)
                {
                    continue;
                }

                weekDay.Id = dtoWeekDay.Id;
                weekDay.UserNotificationRequestId = dtoWeekDay.NotificationRequestId;

                await Database.UpdateFieldAsync<WeekDay, long>( weekDay.LocalId, nameof( weekDay.Id ), dtoWeekDay.Id ).ConfigureAwait( false );
                await Database.UpdateFieldAsync<WeekDay, int>( weekDay.LocalId, nameof( weekDay.UserNotificationRequestId ), dtoWeekDay.NotificationRequestId ).ConfigureAwait( false );
            }
        }
    }

    public Task SetArchiveStatusAsync( HabitArchiveStatus status )
    {
        return RequestProvider.PostAsync( UrlBuilder.HabitArchiveStatus, status, SettingsService.AuthAccessToken! );
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        OperationKind operation = new( queueItem.Operation );

        if (operation == "SetArchiveStatus")
        {
            if (queueItem.PayloadJson is null)
            {
                throw new InvalidOperationException( "PayloadJson is null for SetArchiveStatus operation." );
            }

            HabitArchiveStatus status = JsonSerializer.Deserialize<HabitArchiveStatus>( queueItem.PayloadJson, Principles.Constants.Constants.JsonOptions )
                ?? throw new InvalidOperationException( "Cannot deserialize HabitArchiveStatus." );
            await SetArchiveStatusAsync( status ).ConfigureAwait( false );
        }
        else if (operation == OperationKind.Save)
        {
            UserHabit habit = await LoadHabitForQueueItemAsync( queueItem ).ConfigureAwait( false );
            await SaveAsync( habit ).ConfigureAwait( false );
        }
        else if (operation == OperationKind.Delete)
        {
            if (queueItem.EntityId is long entityId && entityId != 0)
            {
                await DeleteAsync( entityId ).ConfigureAwait( false );
            }
        }
        else
        {
            throw new NotSupportedException( $"Operation {operation} is not supported in HabitRemoteApi." );
        }
    }

    private async Task<UserHabit> LoadHabitForQueueItemAsync( SyncQueueItem queueItem )
    {
        if (queueItem.EntityLocalId is long localId && localId != 0)
        {
            IServiceOfHabit habitService = ServiceProvider.GetRequiredService<IServiceOfHabit>();
            return await habitService.UserHabitAsync( localId ).ConfigureAwait( false );
        }

        if (queueItem.PayloadJson is null)
        {
            throw new InvalidOperationException( "PayloadJson is null for queued habit save." );
        }

        return JsonSerializer.Deserialize<UserHabit>( queueItem.PayloadJson, Principles.Constants.Constants.JsonOptions )
            ?? throw new InvalidOperationException( "Cannot deserialize UserHabit." );
    }

    public Task DeleteAsync( long entityId )
    {
        return RequestProvider.DeleteAsync( $"{UrlBuilder.Habits}/{entityId}", SettingsService.AuthAccessToken! );
    }

    private async Task<EditUserHabitDto> BuildDtoAsync( UserHabit habit )
    {
        List<UserHabit> prioritizedHabits = await Database.WhereAsync<UserHabit>( h => !h.IsArchived ).ConfigureAwait( false );

        return new EditUserHabitDto
        {
            Id = habit.Id,
            Name = habit.Name,
            Type = habit.Type,
            Description = habit.Description,
            Goal = habit.Goal,
            Status = habit.Status,
            IsArchived = habit.IsArchived,
            Frequency = habit.Frequency,
            Priority = habit.Priority,
            Complexity = habit.Complexity,
            ColorName = habit.ColorName,
            Reminders = habit.Reminders,
            LastModified = habit.LastModified,
            PrioritizedHabits = prioritizedHabits.Select( h => new UserHabitWithPriority
            {
                Id = h.Id,
                Priority = h.Priority
            } ).ToList()
        };
    }
}
