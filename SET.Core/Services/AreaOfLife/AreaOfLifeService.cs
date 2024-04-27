using System;
namespace SET.Core.Services;

public class AreaOfLifeService : BaseRemoteService, IAreaOfLifeService
{
    public AreaOfLifeService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        //do nothing
    }

    public async Task<List<UserAreaOfLife>> UserAreasOfLife()
    {
        string url = $"{UrlBuilder.AreasOfLife}?userId={SettingsService.UserId}";
        List<UserAreaOfLife> result = await RequestProvider.GetAsync<List<UserAreaOfLife>>( url, SettingsService.AuthAccessToken );
        return result;
    }
}

