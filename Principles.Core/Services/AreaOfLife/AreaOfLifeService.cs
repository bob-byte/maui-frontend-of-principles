namespace Principles.Core.Services;

public class AreaOfLifeService : BaseEntityService<UserAreaOfLife>, IAreaOfLifeService
{
    private readonly INetworkService m_networkService;

    public AreaOfLifeService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
    }

    public async Task<List<UserAreaOfLife>> UserAreasOfLife()
    {
        List<UserAreaOfLife> localAreas = await Database.GetAllAsync<UserAreaOfLife>().ConfigureAwait( false );
        if (localAreas.Count > 0)
        {
            return localAreas.OrderBy( a => a.Name ).ToList();
        }

        if (!m_networkService.IsConnected)
        {
            return [];
        }

        List<UserAreaOfLife> result = await RequestProvider
            .GetAsync<List<UserAreaOfLife>>( UrlBuilder.AreasOfLife, SettingsService.AuthAccessToken )
            .ConfigureAwait( false );

        if (result.Count > 0)
        {
            foreach (UserAreaOfLife area in result)
            {
                area.LastModified = DateTime.UtcNow;
            }

            await Database.SaveRangeAsync( result ).ConfigureAwait( false );
        }

        return result.OrderBy( a => a.Name ).ToList();
    }
}
