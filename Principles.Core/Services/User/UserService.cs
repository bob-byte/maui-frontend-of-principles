using Plugin.LocalNotification;

namespace Principles.Core.Services;

public class UserService : BaseEntityService<User>, IUserService
{
    private static readonly OperationKind SaveUserNameOperation = OperationKind.Custom( "SaveUserName" );
    private static readonly OperationKind SaveMainSloganOperation = OperationKind.Custom( "SaveMainSlogan" );
    private static readonly OperationKind SaveMissionOperation = OperationKind.Custom( "SaveMission" );

    private readonly INetworkService m_networkService;

    public UserService( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
        RemoteApi = serviceProvider.GetRequiredService<IUserRemoteApi>();
        m_networkService = serviceProvider.GetRequiredService<INetworkService>();
    }

    private IUserRemoteApi RemoteApi { get; }

    public async Task<User> GetCurrentUserAsync()
    {
        User? user = (await Database.GetAllAsync<User>().ConfigureAwait( false )).FirstOrDefault();
        if (user is not null)
        {
            return user;
        }

        user = await RemoteApi.GetCurrentUserAsync().ConfigureAwait( false );
        await SaveSingleUserAsync( user ).ConfigureAwait( false );

        return user;
    }

    public Task SaveUserNameAsync( string userName )
    {
        DateTime lastModified = DateTime.UtcNow;
        SaveUserNameRequest request = new( userName, lastModified );
        return LocalRemoteExecutor.ExecuteAsync<User>(
            () => UpdateCurrentUserAsync( user => user.Name = userName, lastModified ),
            () => RemoteApi.SaveUserNameAsync( userName, lastModified ),
            SaveUserNameOperation,
            request
        );
    }

    public Task SaveMainSloganAsync( string mainSlogan )
    {
        DateTime lastModified = DateTime.UtcNow;
        SaveMainSloganRequest request = new( mainSlogan, lastModified );
        return LocalRemoteExecutor.ExecuteAsync<User>(
            () => UpdateCurrentUserAsync( user => user.MainSlogan = mainSlogan, lastModified ),
            () => RemoteApi.SaveMainSloganAsync( mainSlogan, lastModified ),
            SaveMainSloganOperation,
            request
        );
    }

    public Task SaveMissionAsync( string mission )
    {
        DateTime lastModified = DateTime.UtcNow;
        SaveMissionRequest request = new( mission, lastModified );
        return LocalRemoteExecutor.ExecuteAsync<User>(
            () => UpdateCurrentUserAsync( user => user.Mission = mission, lastModified ),
            () => RemoteApi.SaveMissionAsync( mission, lastModified ),
            SaveMissionOperation,
            request
        );
    }

    public async Task DeleteAccountAsync()
    {
        await RemoteApi.DeleteAccountAsync().ConfigureAwait( false );
        await ClearLocalDataAsync().ConfigureAwait( false );
    }

    public async Task ClearLocalDataAsync()
    {
        string[] tables =
        [
            nameof( UserAreaOfLifeUserHabit ),
            nameof( WeekDay ),
            nameof( UserHabitReminder ),
            nameof( ProgressOfHabit ),
            nameof( UserHabit ),
            nameof( FrequencyOfHabit ),
            nameof( UserGoal ),
            nameof( UserAreaOfLife ),
            nameof( Reminder ),
            nameof( SyncQueueItem ),
            nameof( User )
        ];

        foreach (string table in tables)
        {
            try
            {
                await Database.ExecuteAsync( $"DELETE FROM {table};" ).ConfigureAwait( false );
            }
            catch (SQLiteException ex) when (ex.Message.Contains( "no such table", StringComparison.OrdinalIgnoreCase ))
            {
                // The local schema may be incomplete on first app start after an interrupted migration.
            }
        }

        try
        {
            if (LocalNotificationCenter.Current.IsSupported)
            {
                LocalNotificationCenter.Current.CancelAll();
            }
        }
        catch (Exception ex)
        {
            LoggingService.LogError( ex, "Failed to cancel local notifications while clearing user data." );
        }
    }

    private async Task UpdateCurrentUserAsync( Action<User> update, DateTime lastModified )
    {
        User user = await GetStoredOrNewUserAsync().ConfigureAwait( false );
        update( user );
        user.LastModified = lastModified == default ? DateTime.UtcNow : lastModified;
        await SaveSingleUserAsync( user ).ConfigureAwait( false );
    }

    private async Task<User> GetStoredOrNewUserAsync()
    {
        User? user = (await Database.GetAllAsync<User>().ConfigureAwait( false )).FirstOrDefault();
        if (user is not null)
        {
            return user;
        }

        if (m_networkService.IsConnected)
        {
            return await RemoteApi.GetCurrentUserAsync().ConfigureAwait( false );
        }

        return new User();
    }

    private async Task SaveSingleUserAsync( User user )
    {
        User? existingUser = (await Database.GetAllAsync<User>().ConfigureAwait( false )).FirstOrDefault();
        if (existingUser is not null && user.LocalId == 0)
        {
            user.LocalId = existingUser.LocalId;
        }

        await Database.SaveAsync( user ).ConfigureAwait( false );
    }
}
