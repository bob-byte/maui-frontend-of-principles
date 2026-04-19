namespace Principles.Core.Services;

public class GoalRemoteApi : RemoteApiService<UserGoal>, IGoalRemoteApi
{
    public GoalRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
    }

    public Task<List<UserGoal>> GetAllAsync()
    {
        return RequestProvider.GetAsync<List<UserGoal>>( UrlBuilder.Goal, SettingsService.AuthAccessToken! );
    }

    public async Task<DtoWithId> SaveAsync( UserGoal goal )
    {
        DtoWithId response = await RequestProvider.PostAsync<UserGoal, DtoWithId>(
            $"{UrlBuilder.Goal}/{goal.Id}",
            goal,
            SettingsService.AuthAccessToken!
        ).ConfigureAwait( false );

        if (goal.LocalId != 0)
        {
            await Database.UpdateFieldAsync<UserGoal, long>( goal.LocalId, nameof( goal.Id ), response.Id ).ConfigureAwait( false );
        }

        return response;
    }

    public Task DeleteAsync( long goalId )
    {
        return RequestProvider.DeleteAsync( $"{UrlBuilder.Goal}/{goalId}", SettingsService.AuthAccessToken! );
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        OperationKind operation = new( queueItem.Operation );
        UserGoal goal = await LoadGoalForQueueItemAsync( queueItem ).ConfigureAwait( false );

        if (operation == OperationKind.Save)
        {
            await SaveAsync( goal ).ConfigureAwait( false );
        }
        else if (operation == OperationKind.Delete)
        {
            if (goal.Id != 0)
            {
                await DeleteAsync( goal.Id ).ConfigureAwait( false );
            }
        }
        else
        {
            throw new NotSupportedException( $"Operation {operation} is not supported in GoalRemoteApi." );
        }
    }

    private async Task<UserGoal> LoadGoalForQueueItemAsync( SyncQueueItem queueItem )
    {
        if (queueItem.EntityLocalId is long localId && localId != 0)
        {
            UserGoal? localGoal = await Database.GetByIdAsync<UserGoal>( localId ).ConfigureAwait( false );
            if (localGoal is not null)
            {
                return localGoal;
            }
        }

        if (queueItem.PayloadJson is null)
        {
            throw new InvalidOperationException( "PayloadJson is null for GoalRemoteApi queue item." );
        }

        return JsonSerializer.Deserialize<UserGoal>( queueItem.PayloadJson, Principles.Constants.Constants.JsonOptions )
            ?? throw new InvalidOperationException( "Cannot deserialize queued UserGoal." );
    }
}
