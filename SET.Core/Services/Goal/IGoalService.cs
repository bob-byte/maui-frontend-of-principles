namespace SET.Core.Services;
public interface IGoalService
{
    Task<DtoWithId> SaveGoalAsync( UserGoal goal );
    Task<List<UserGoal>> UserGoalsAsync();
    Task DeleteGoalAsync( UserGoal goal );
}
