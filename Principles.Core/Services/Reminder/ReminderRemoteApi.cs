namespace Principles.Core.Services;

public class ReminderRemoteApi : RemoteApiService<Reminder>, IReminderRemoteApi
{
    public ReminderRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
    }

    public Task<Reminder> GetHabitsReportReminderAsync()
    {
        return RequestProvider.GetAsync<Reminder>( UrlBuilder.HabitsReportReminder, SettingsService.AuthAccessToken! );
    }

    public Task<SaveHabitsReportReminderResponse> SaveHabitsReportReminderAsync( Reminder reminder )
    {
        return RequestProvider.PostAsync<Reminder, SaveHabitsReportReminderResponse>(
            $"{UrlBuilder.HabitsReportReminder}/{reminder.Id}",
            reminder,
            SettingsService.AuthAccessToken!
        );
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        OperationKind operation = new( queueItem.Operation );
        if (operation != OperationKind.Save)
        {
            throw new NotSupportedException( $"Operation '{queueItem.Operation}' is not supported for reminders." );
        }

        Reminder reminder = await LoadReminderForQueueItemAsync( queueItem ).ConfigureAwait( false );

        SaveHabitsReportReminderResponse response = await SaveHabitsReportReminderAsync( reminder ).ConfigureAwait( false );
        reminder.Id = response.Id;
        reminder.UserNotificationRequestId = response.UserNotificationRequestId;

        if (reminder.LocalId != 0)
        {
            await Database.UpdateFieldAsync<Reminder, long>( reminder.LocalId, nameof( reminder.Id ), response.Id ).ConfigureAwait( false );
            await Database.UpdateFieldAsync<Reminder, int>( reminder.LocalId, nameof( reminder.UserNotificationRequestId ), response.UserNotificationRequestId ).ConfigureAwait( false );
        }
    }

    private async Task<Reminder> LoadReminderForQueueItemAsync( SyncQueueItem queueItem )
    {
        if (queueItem.EntityLocalId is long localId && localId != 0)
        {
            Reminder? localReminder = await Database.GetByIdAsync<Reminder>( localId ).ConfigureAwait( false );
            if (localReminder is not null)
            {
                return localReminder;
            }
        }

        if (queueItem.PayloadJson is null)
        {
            throw new InvalidOperationException( "PayloadJson is null for queued reminder." );
        }

        return JsonSerializer.Deserialize<Reminder>( queueItem.PayloadJson, Principles.Constants.Constants.JsonOptions )
            ?? throw new InvalidOperationException( "Cannot deserialize queued reminder." );
    }
}
