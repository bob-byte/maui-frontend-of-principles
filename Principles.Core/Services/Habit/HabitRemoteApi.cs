namespace Principles.Core.Services;

public class HabitRemoteApi: RemoteApiService<UserHabit>
{
    public HabitRemoteApi(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }
    
    public override async Task<List<UserHabit>> GetAllAsync(bool forceRefresh = false)
    {
        string url = UrlBuilder.HabitsInProgress;
        
        List<UserHabit> result = await RequestProvider.GetAsync<List<UserHabit>>(
            url,
            SettingsService.AuthAccessToken!
        ).DefaultConfigureAwait();

        return result;
    }

    public override async Task SaveAsync( UserHabit habit )
    {
        long serverId = await OfflineRepository.GetFieldAsync<UserHabit, long>( habit.LocalId, "Id" );
        habit.Id = serverId;
        
        string url = $"{UrlBuilder.Habits}/{habit.Id}";
        SaveHabitResponse response = await RequestProvider.PostAsync<UserHabit, SaveHabitResponse>( url, habit, SettingsService.AuthAccessToken! );
        
        await OfflineRepository.UpdateFieldAsync<UserHabit, long>( habit.LocalId, "Id", response.Id );
        habit.Id = response.Id;
        await OfflineRepository.UpdateFieldAsync<FrequencyOfHabit, long>( habit.Frequency!.LocalId, "Id", response.FrequencyId );

        if (response.ReminderIds is not null && habit.Reminders?.Any() == true)
        {
            for (int numReminder = 0; numReminder < response.ReminderIds.Count; numReminder++)
            {
                SaveHabitResponse.Reminder dtoOfReminder = response.ReminderIds[numReminder];
                UserHabitReminder habitReminder = habit.Reminders[numReminder];
                
                await OfflineRepository.UpdateFieldAsync<UserHabitReminder, long>( habitReminder.LocalId, "Id", response.ReminderIds[numReminder].Id );

                for (int numWeekDay = 0; numWeekDay < dtoOfReminder.DaysOfWeek?.Count; numWeekDay++)
                {
                    SaveHabitResponse.WeekDay dtoOfWeekDay = dtoOfReminder.DaysOfWeek[numWeekDay];
                    WeekDay? weekDay = habitReminder.DaysOfWeek.FirstOrDefault( d => d.Type == dtoOfWeekDay.Type );
                    if (weekDay is not null)
                    {
                        await OfflineRepository.UpdateFieldAsync<WeekDay, long>( weekDay.LocalId, "Id", dtoOfWeekDay.Id );
                    }
                }
            }
        }
    }

    public override async Task ExecuteAsync( string operation, string? payloadJson )
    {
        if (operation == "SetArchiveStatus" && payloadJson is not null)
        {
            HabitArchiveStatus? dto = JsonSerializer.Deserialize<HabitArchiveStatus>( payloadJson );
            if (dto is not null)
            {
                string url = $"{UrlBuilder.HabitArchiveStatus}";
                await RequestProvider.PostAsync( url, dto, SettingsService.AuthAccessToken! );
            }
        }
    }

    public override async Task DeleteAsync( UserHabit item )
    {
        string url = $"{UrlBuilder.Habits}/{item.Id}";
        await RequestProvider.DeleteAsync<HabitDeletionResponse>( url, SettingsService.AuthAccessToken );
    }

    public override async Task SaveAsync( long entityId, long localId, string payloadJson )
    {
        UserHabit habit = JsonSerializer.Deserialize<UserHabit>( payloadJson )!;
        
        await SaveAsync( habit );
    }

    public override async Task DeleteAsync( long entityId, string payloadJson )
    {
        string url = $"{UrlBuilder.Habits}/{entityId}";
        await RequestProvider.DeleteAsync<HabitDeletionResponse>( url, SettingsService.AuthAccessToken! );
    }
}