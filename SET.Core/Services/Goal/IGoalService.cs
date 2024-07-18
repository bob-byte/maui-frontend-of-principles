namespace SET.Core.Services;
public interface IGoalService
{
    Task SaveGoalAsync( UserGoal goal );
    Task<List<UserGoal>> UserGoalsAsync();
    Task DeleteGoalAsync( UserGoal goal );
}
