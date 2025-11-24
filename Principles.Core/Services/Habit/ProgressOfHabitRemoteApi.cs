using System;

namespace Principles.Core.Services;

public interface IProgressOfHabitRemoteApi
{
    Task SaveAsync( ProgressOfHabit progress );
}


public class ProgressOfHabitRemoteApi : RemoteApiService<ProgressOfHabit>, IProgressOfHabitRemoteApi
{
    public ProgressOfHabitRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        //do nothing
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        OperationKind operation = new( queueItem.Operation );
        if (queueItem.PayloadJson is null)
        {
            throw new InvalidOperationException( "PayloadJson is null for ProgressOfHabitRemoteApi.HandleQueueItemAsync" );
        }

        if (operation == OperationKind.Save)
        {
            ProgressOfHabit? progress = JsonSerializer.Deserialize<ProgressOfHabit>( queueItem.PayloadJson );
            if (progress is null)
            {
                throw new InvalidOperationException( "Cannot deserialize ProgressOfHabit from payloadJson" );
            }
            else
            {
                await SaveAsync( progress );
            }
        }
        else
        {
            throw new NotSupportedException( $"Operation {operation} is not supported in ProgressOfHabitRemoteApi.HandleQueueItemAsync" );
        }
    }

    public async Task SaveAsync( ProgressOfHabit progress )
    {
        //server ID can be already set (user can save habit multiple times before sync)
        long serverId = await Database.GetFieldAsync<ProgressOfHabit, long>( progress.LocalId, "Id" );
        progress.Id = serverId;

        long habitId = await Database.GetFieldAsync<UserHabit, long>( progress.HabitLocalId, "Id" );
        if (habitId == 0)
        {
            throw new ArgumentException( message: $"{nameof( progress )}.{nameof( progress.HabitLocalId )} has no corresponding server ID" );
        }

        string url = $"{UrlBuilder.ProgressOfHabit}/{progress.Id}";
        UpdateProgressDto dto = new()
        {
            Id = progress.Id,
            Date = progress.Date,
            Value = progress.Value,
            HabitId = habitId,
        };
        SaveProgressOfHabitResponse response = await RequestProvider.PostAsync<UpdateProgressDto, SaveProgressOfHabitResponse>(
            url,
            dto,
            SettingsService.AuthAccessToken!
        );
        progress.Id = response.Id;

        await Database.UpdateFieldAsync<ProgressOfHabit, long>( progress.LocalId, "Id", response.Id );
    }
}
