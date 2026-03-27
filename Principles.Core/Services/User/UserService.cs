using System;

namespace Principles.Core.Services;

public class UserService : BaseEntityService<User>, IUserService
{
    public UserService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        RemoteApi = serviceProvider.GetRequiredService<IUserRemoteApi>();
    }

    private IUserRemoteApi RemoteApi { get; }

    public async Task<User> GetCurrentUserAsync()
    {
        User? user = (await Database.GetAllAsync<User>().ConfigureAwait( false )).FirstOrDefault();
        if (user is null)
        {
            user = await RemoteApi.GetCurrentUserAsync().ConfigureAwait( false );
            await Database.SaveAsync( user ).ConfigureAwait( false );
        }

        return user;
    }

    public Task SaveUserNameAsync( string userName )
    {
        var saveRequest = new { UserName = userName };
        return LocalRemoteExecutor.ExecuteAsync<User>(
            localCall: async () =>
            {
                // TODO: just save user name without loading user, because we don't need 
                // other user data here. But for that we need to implement partial update 
                // in database.
                User user = await GetCurrentUserAsync().ConfigureAwait( false );
                user.Name = userName;
                await Database.SaveAsync( user ).ConfigureAwait( false );
            },
            remoteCall: () => RemoteApi.SaveUserNameAsync( userName ),
            operation: OperationKind.Save,
            data: saveRequest
        );
    }

    public Task SaveMainSloganAsync( string mainSlogan )
    {
        var saveRequest = new { MainSlogan = mainSlogan };
        return LocalRemoteExecutor.ExecuteAsync<User>(
            localCall: async () =>
            {
                // TODO: just save main slogan without loading user, because we don't need 
                // other user data here. But for that we need to implement partial update 
                // in database.
                User user = await GetCurrentUserAsync().ConfigureAwait( false );
                user.MainSlogan = mainSlogan;
                await Database.SaveAsync( user ).ConfigureAwait( false );
            },
            remoteCall: () => RemoteApi.SaveMainSloganAsync( mainSlogan ),
            operation: OperationKind.Save,
            data: saveRequest
        );
    }

    public Task SaveMissionAsync( string mission )
    {
        var saveRequest = new { Mission = mission };
        return LocalRemoteExecutor.ExecuteAsync<User>(
            localCall: async () =>
            {
                // TODO: just save mission without loading user, because we don't need 
                // other user data here. But for that we need to implement partial update 
                // in database.
                User user = await GetCurrentUserAsync().ConfigureAwait( false );
                user.Mission = mission;
                await Database.SaveAsync( user ).ConfigureAwait( false );
            },
            remoteCall: () => RemoteApi.SaveMissionAsync( mission ),
            operation: OperationKind.Save,
            data: saveRequest
        );
    }

    public async Task DeleteAccountAsync()
    {
        await RemoteApi.DeleteAccountAsync();
        IDatabaseConnectionProvider connectionProvider = ServiceLocator.Current!.GetRequiredService<IDatabaseConnectionProvider>();
        SQLiteAsyncConnection db = connectionProvider.AsyncConnection;
        List<string> tables = await db.QueryScalarsAsync<string>(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';"
        );
    
        foreach (string table in tables)
        {
            await db.ExecuteAsync($"DROP TABLE IF EXISTS {table};");
        }
    }
}
