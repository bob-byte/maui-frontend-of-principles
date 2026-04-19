namespace Principles.Core.Services;

public class UserRemoteApi : RemoteApiService<User>, IUserRemoteApi
{
    public UserRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
    }

    public Task<User> GetCurrentUserAsync()
    {
        return RequestProvider.GetAsync<User>( UrlBuilder.Profile, SettingsService.AuthAccessToken! );
    }

    public Task SaveUserNameAsync( string userName, DateTime lastModified = default )
    {
        return RequestProvider.PutAsync(
            UrlBuilder.UserName,
            new SaveUserNameRequest( userName, NormalizeTimestamp( lastModified ) ),
            SettingsService.AuthAccessToken!
        );
    }

    public Task SaveMainSloganAsync( string mainSlogan, DateTime lastModified = default )
    {
        return RequestProvider.PutAsync(
            UrlBuilder.UserMainSlogan,
            new SaveMainSloganRequest( mainSlogan, NormalizeTimestamp( lastModified ) ),
            SettingsService.AuthAccessToken!
        );
    }

    public Task SaveMissionAsync( string mission, DateTime lastModified = default )
    {
        return RequestProvider.PutAsync(
            UrlBuilder.UserMission,
            new SaveMissionRequest( mission, NormalizeTimestamp( lastModified ) ),
            SettingsService.AuthAccessToken!
        );
    }

    public Task DeleteAccountAsync()
    {
        return RequestProvider.DeleteAsync( UrlBuilder.Account, SettingsService.AuthAccessToken! );
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        OperationKind operation = new( queueItem.Operation );
        string? payloadJson = queueItem.PayloadJson;

        if (payloadJson is null)
        {
            throw new InvalidOperationException( "PayloadJson is null for queued user update." );
        }

        if (operation == "SaveUserName")
        {
            SaveUserNameRequest request = JsonSerializer.Deserialize<SaveUserNameRequest>( payloadJson )
                ?? throw new InvalidOperationException( "Cannot deserialize SaveUserNameRequest." );
            await SaveUserNameAsync( request.UserName, request.LastModified ).ConfigureAwait( false );
        }
        else if (operation == "SaveMainSlogan")
        {
            SaveMainSloganRequest request = JsonSerializer.Deserialize<SaveMainSloganRequest>( payloadJson )
                ?? throw new InvalidOperationException( "Cannot deserialize SaveMainSloganRequest." );
            await SaveMainSloganAsync( request.MainSlogan, request.LastModified ).ConfigureAwait( false );
        }
        else if (operation == "SaveMission")
        {
            SaveMissionRequest request = JsonSerializer.Deserialize<SaveMissionRequest>( payloadJson )
                ?? throw new InvalidOperationException( "Cannot deserialize SaveMissionRequest." );
            await SaveMissionAsync( request.Mission, request.LastModified ).ConfigureAwait( false );
        }
        else
        {
            throw new ArgumentException( $"Operation '{operation}' is not supported in UserRemoteApi." );
        }
    }

    private static DateTime NormalizeTimestamp( DateTime timestamp )
    {
        return timestamp == default ? DateTime.UtcNow : timestamp;
    }
}
