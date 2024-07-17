namespace SET.Core.Services;
public interface IGoalService
{
    Task SaveGoalAsync( UserGoal newGoal );
    Task<List<UserGoal>> LoadGoalsAsync(string userId);
    Task DeleteGoalAsync( UserGoal existedGoal );
}
