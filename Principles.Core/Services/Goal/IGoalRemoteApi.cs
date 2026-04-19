namespace Principles.Core.Services;

public interface IGoalRemoteApi
{
    Task DeleteAsync( long goalId );
    Task<List<UserGoal>> GetAllAsync();
    Task<DtoWithId> SaveAsync( UserGoal goal );
}
