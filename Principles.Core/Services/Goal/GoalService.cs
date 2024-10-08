
namespace Principles.Core.Services;
public class GoalService : BaseRemoteService, IGoalService
{
    public GoalService( IServiceProvider serviceProvider ) 
        : base( serviceProvider )
    {
        //do nothing
    }

    public ObservableCollectionEx<UserGoal>? StoredGoals { get; set; }

    public async Task<ObservableCollectionEx<UserGoal>> UserGoalsAsync()
    {
        ObservableCollectionEx<UserGoal> result;

        if (StoredGoals?.Any() == true)
        {
            result = StoredGoals;
        }
        else
        {
            string url = $"{UrlBuilder.Goal}";

            result = await RequestProvider.GetAsync<ObservableCollectionEx<UserGoal>>( url, SettingsService.AuthAccessToken ).DefaultConfigureAwait();

            if(result is null)
            {
                result = new ObservableCollectionEx<UserGoal>();
            }
            else
            {
                StoredGoals = result;
            }
        }

        return result;
    }

    public Task<DtoWithId> SaveGoalAsync( UserGoal goal )
    {
        string url = $"{UrlBuilder.Goal}/{goal.Id}";
        return RequestProvider.PostAsync<UserGoal, DtoWithId>( url, goal, SettingsService.AuthAccessToken );
    }

    public async Task DeleteGoalAsync( UserGoal existedGoal )
    {
        string url = $"{UrlBuilder.Goal}/{existedGoal.Id}";
        await RequestProvider.DeleteAsync( url, SettingsService.AuthAccessToken );
    }
}
