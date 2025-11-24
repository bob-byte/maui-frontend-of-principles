namespace Principles.Core.Services;

public interface IHabitRemoteApi
{
    Task DeleteAsync( long entityId );
    Task<List<UserHabit>> GetAllAsync();
    Task SaveAsync( UserHabit habit );
    Task SetArchiveStatusAsync( HabitArchiveStatus status );
}

public class HabitRemoteApi : RemoteApiService<UserHabit>, IHabitRemoteApi
{
    public HabitRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        
    }

    public async Task<List<UserHabit>> GetAllAsync()
    {
        string url = UrlBuilder.HabitsInProgress;

        List<UserHabit> result = await RequestProvider.GetAsync<List<UserHabit>>(
            url,
            SettingsService.AuthAccessToken!
        ).DefaultConfigureAwait();

        return result;
    }

    public async Task SaveAsync( UserHabit habit )
    {
        //server ID can be already set (user can save habit multiple times before sync)
        long serverId = await Database.GetFieldAsync<UserHabit, long>( habit.LocalId, "Id" );
        habit.Id = serverId;

        string url = $"{UrlBuilder.Habits}/{habit.Id}";
        SaveHabitResponse response = await RequestProvider.PostAsync<UserHabit, SaveHabitResponse>( url, habit, SettingsService.AuthAccessToken! );

        await Database.UpdateFieldAsync<UserHabit, long>( habit.LocalId, "Id", response.Id );
        habit.Id = response.Id;
        await Database.UpdateFieldAsync<FrequencyOfHabit, long>( habit.Frequency!.LocalId, "Id", response.FrequencyId );

        if (response.ReminderIds is not null && habit.Reminders?.Any() == true)
        {
            for (int numReminder = 0; numReminder < response.ReminderIds.Count; numReminder++)
            {
                SaveHabitResponse.Reminder dtoOfReminder = response.ReminderIds[numReminder];
                UserHabitReminder habitReminder = habit.Reminders[numReminder];

                await Database.UpdateFieldAsync<UserHabitReminder, long>( habitReminder.LocalId, "Id", response.ReminderIds[numReminder].Id );

                for (int numWeekDay = 0; numWeekDay < dtoOfReminder.DaysOfWeek?.Count; numWeekDay++)
                {
                    SaveHabitResponse.WeekDay dtoOfWeekDay = dtoOfReminder.DaysOfWeek[numWeekDay];
                    WeekDay? weekDay = habitReminder.DaysOfWeek.FirstOrDefault( d => d.Type == dtoOfWeekDay.Type );
                    if (weekDay is not null)
                    {
                        await Database.UpdateFieldAsync<WeekDay, long>( weekDay.LocalId, "Id", dtoOfWeekDay.Id );
                    }
                }
            }
        }
    }

    public async Task SetArchiveStatusAsync( HabitArchiveStatus status )
    {
        string url = $"{UrlBuilder.HabitArchiveStatus}";
        await RequestProvider.PostAsync( url, status, SettingsService.AuthAccessToken! );
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        OperationKind operation = new( queueItem.Operation );
        string? payloadJson = queueItem.PayloadJson;

        if (operation == "SetArchiveStatus")
        {
            if (payloadJson is null)
            {
                throw new InvalidOperationException( "PayloadJson is null for SetArchiveStatus operation" );
            }

            HabitArchiveStatus? status = JsonSerializer.Deserialize<HabitArchiveStatus>( payloadJson ) ??
                throw new InvalidOperationException( "Cannot deserialize HabitArchiveStatus from payloadJson" );

            await SetArchiveStatusAsync( status );
        }
        else if (operation == OperationKind.Save)
        {
            if (payloadJson is null)
            {
                throw new InvalidOperationException( "PayloadJson is null for SetArchiveStatus operation" );
            }

            UserHabit? habit = JsonSerializer.Deserialize<UserHabit>( payloadJson );
            if (habit is null)
            {
                throw new InvalidOperationException( "Cannot deserialize UserHabit from payloadJson" );
            }
            else
            {
                await SaveAsync( habit );
            }
        }
        else if (operation == OperationKind.Delete)
        {
            if (queueItem.EntityId is null)
            {
                throw new InvalidOperationException( "EntityId is null for Delete operation" );
            }


            await DeleteAsync( queueItem.EntityId.Value );
        }
    }

    public async Task DeleteAsync( long entityId )
    {
        string url = $"{UrlBuilder.Habits}/{entityId}";
        await RequestProvider.DeleteAsync<HabitDeletionResponse>( url, SettingsService.AuthAccessToken! );
    }
}