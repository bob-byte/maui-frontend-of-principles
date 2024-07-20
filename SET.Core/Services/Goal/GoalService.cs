
namespace SET.Core.Services;
public class GoalService : BaseRemoteService, IGoalService
{
    public GoalService( IServiceProvider serviceProvider ) 
        : base( serviceProvider )
    {
        //do nothing
    }

    public Task<List<UserGoal>> UserGoalsAsync()
    {
        string url = $"{UrlBuilder.Goal}?userId={SettingsService.UserId}";
        return RequestProvider.GetAsync<List<UserGoal>>( url, SettingsService.AuthAccessToken );
    }

    public Task<DtoWithId> SaveGoalAsync( UserGoal goal )
    {
        string url = $"{UrlBuilder.Goal}/{goal.Id}?userId={SettingsService.UserId}";
        return RequestProvider.PostAsync<UserGoal, DtoWithId>( url, goal, SettingsService.AuthAccessToken );
    }

    public async Task DeleteGoalAsync( UserGoal existedGoal )
    {
        string url = $"{UrlBuilder.Goal}/{existedGoal.Id}";
        await RequestProvider.DeleteAsync( url, SettingsService.AuthAccessToken );
    }
}
