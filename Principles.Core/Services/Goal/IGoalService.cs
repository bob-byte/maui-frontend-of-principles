namespace Principles.Core.Services;

public interface IGoalService
{
    ObservableCollectionEx<UserGoal>? StoredGoals { get; set; }

    Task SaveGoalAsync( UserGoal goal );
    Task<ObservableCollectionEx<UserGoal>> UserGoalsAsync( bool forceReload = false );
    Task<UserGoal?> GetGoalByLocalIdAsync( long localId );
    Task DeleteGoalAsync( UserGoal goal );
}
