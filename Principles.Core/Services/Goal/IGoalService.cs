namespace Principles.Core.Services;

public interface IGoalService
{
    ObservableCollectionEx<UserGoal>? StoredGoals { get; set; }

    Task SaveGoalAsync( UserGoal goal );
    Task<ObservableCollectionEx<UserGoal>> UserGoalsAsync();
    Task DeleteGoalAsync( UserGoal goal );
}
