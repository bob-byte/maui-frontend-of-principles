namespace Principles.Core.Services;

public class GoalService : BaseEntityService<UserGoal>, IGoalService
{
    private readonly INetworkService m_networkService;
    private readonly IGoalRemoteApi m_remoteApi;

    public GoalService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
        m_remoteApi = serviceProvider.GetRequiredService<IGoalRemoteApi>();
    }

    public ObservableCollectionEx<UserGoal>? StoredGoals { get; set; }

    public async Task<ObservableCollectionEx<UserGoal>> UserGoalsAsync()
    {
        if (StoredGoals?.Any() == true)
        {
            return StoredGoals;
        }

        List<UserGoal> goals = await Database.GetAllAsync<UserGoal>().ConfigureAwait( false );
        if (goals.Count == 0 && m_networkService.IsConnected)
        {
            goals = await m_remoteApi.GetAllAsync().ConfigureAwait( false );
            if (goals.Count > 0)
            {
                await Database.SaveRangeAsync( goals ).ConfigureAwait( false );
            }
        }

        StoredGoals = new ObservableCollectionEx<UserGoal>( goals.OrderBy( g => g.Name ) );
        return StoredGoals;
    }

    public async Task SaveGoalAsync( UserGoal goal )
    {
        ArgumentNullException.ThrowIfNull( goal );

        goal.LastModified = DateTime.UtcNow;
        await Database.SaveAsync( goal ).ConfigureAwait( false );

        if (m_networkService.IsConnected)
        {
            try
            {
                DtoWithId response = await m_remoteApi.SaveAsync( goal ).ConfigureAwait( false );
                if (goal.Id != response.Id)
                {
                    goal.Id = response.Id;
                    await Database.SaveAsync( goal ).ConfigureAwait( false );
                }
                return;
            }
            catch
            {
                // Fall back to queued sync and keep the local change as source of truth.
            }
        }

        await SyncQueueService.AddToQueueAsync( goal, OperationKind.Save ).ConfigureAwait( false );
    }

    public async Task DeleteGoalAsync( UserGoal goal )
    {
        ArgumentNullException.ThrowIfNull( goal );

        await Database.DeleteAsync( goal ).ConfigureAwait( false );

        if (goal.Id == 0)
        {
            return;
        }

        if (m_networkService.IsConnected)
        {
            try
            {
                await m_remoteApi.DeleteAsync( goal.Id ).ConfigureAwait( false );
                return;
            }
            catch
            {
                // Keep queued delete if immediate sync failed.
            }
        }

        await SyncQueueService.AddToQueueAsync( goal, OperationKind.Delete ).ConfigureAwait( false );
    }
}
