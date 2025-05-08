using Principles.Core.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using Principles.Core.Services.Managers;

namespace Principles.Core.Services;
public class SyncService : BaseRemoteService, ISyncService
{
    //private readonly IDatabaseService _localDb;
    private readonly IUserManagerService _userManager;

    public SyncService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //_localDb = serviceProvider.GetRequiredService<IDatabaseService>();
        _userManager = serviceProvider.GetRequiredService<IUserManagerService>();
    }

    public async Task SyncUsersAsync()
    {
        string url = $"{UrlBuilder.Sync}";

        List<UserDto> remoteUsers = await RequestProvider.GetAsync<List<UserDto>>( url );
        if (remoteUsers is null || !remoteUsers.Any())
            return;

        foreach (var user in remoteUsers)
        {
            await _userManager.SaveUserFromDtoAsync( user ); 
        }
    }
}

