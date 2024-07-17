
namespace SET.Core.Services;
public class GoalService : BaseRemoteService, IGoalService
{
    public GoalService( IServiceProvider serviceProvider ) 
        : base( serviceProvider )
    {
        //do nothing
    }

    public async Task<List<UserGoal>> LoadGoalsAsync( string userId )
    {
        string url = $"{UrlBuilder.Goal}?userId={userId}";
        var userGoals = await RequestProvider.GetAsync<List<UserGoal>>( url, SettingsService.AuthAccessToken );
        return userGoals;
    }

    public async Task SaveGoalAsync( UserGoal goal )
    {
        string url = $"{UrlBuilder.Goal}/{goal.Id}?userId={SettingsService.UserId}";
        DtoWithId response = await RequestProvider.PostAsync<UserGoal, DtoWithId>( url, goal, SettingsService.AuthAccessToken ).DefaultConfigureAwait();
        goal.Id = response.Id;
    }

    public async Task DeleteGoalAsync( UserGoal existedGoal )
    {
        string url = $"{UrlBuilder.Goal}/{existedGoal.Id}";
        await RequestProvider.DeleteAsync( url, SettingsService.AuthAccessToken );
    }
}
