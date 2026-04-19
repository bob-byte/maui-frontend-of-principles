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
        if (operation == OperationKind.Save)
        {
            ProgressOfHabit progress = await LoadProgressForQueueItemAsync( queueItem ).ConfigureAwait( false );
            await SaveAsync( progress ).ConfigureAwait( false );
        }
        else
        {
            throw new NotSupportedException( $"Operation {operation} is not supported in ProgressOfHabitRemoteApi.HandleQueueItemAsync" );
        }
    }

    private async Task<ProgressOfHabit> LoadProgressForQueueItemAsync( SyncQueueItem queueItem )
    {
        if (queueItem.EntityLocalId is long localId && localId != 0)
        {
            ProgressOfHabit? localProgress = await Database.GetByIdAsync<ProgressOfHabit>( localId ).ConfigureAwait( false );
            if (localProgress is not null)
            {
                return localProgress;
            }
        }

        if (queueItem.PayloadJson is null)
        {
            throw new InvalidOperationException( "PayloadJson is null for ProgressOfHabitRemoteApi.HandleQueueItemAsync" );
        }

        return JsonSerializer.Deserialize<ProgressOfHabit>( queueItem.PayloadJson, Principles.Constants.Constants.JsonOptions )
            ?? throw new InvalidOperationException( "Cannot deserialize ProgressOfHabit from payloadJson" );
    }

    public async Task SaveAsync( ProgressOfHabit progress )
    {
        //server ID can be already set (user can save habit multiple times before sync)
        long serverId = await Database.GetFieldAsync<ProgressOfHabit, long>( progress.LocalId, "Id" );
        progress.Id = serverId;

        long habitId = await Database.GetFieldAsync<UserHabit, long>( progress.UserHabitLocalId, nameof( UserHabit.Id ) );
        if (habitId == 0)
        {
            throw new ArgumentException( message: $"{nameof( progress )}.{nameof( progress.UserHabitLocalId )} has no corresponding server ID" );
        }

        string url = $"{UrlBuilder.ProgressOfHabit}/{progress.Id}";
        UpdateProgressDto dto = new()
        {
            Id = progress.Id,
            Date = progress.Date,
            Value = progress.Value,
            HabitId = habitId,
            LastModified = progress.LastModified
        };
        SaveProgressOfHabitResponse response = await RequestProvider.PostAsync<UpdateProgressDto, SaveProgressOfHabitResponse>(
            url,
            dto,
            SettingsService.AuthAccessToken!
        );
        progress.Id = response.Id;

        await Database.UpdateFieldAsync<ProgressOfHabit, long>( progress.LocalId, nameof( progress.Id ), response.Id );
    }
}
