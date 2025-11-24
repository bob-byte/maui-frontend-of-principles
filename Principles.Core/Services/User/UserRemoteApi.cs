using System;

namespace Principles.Core.Services;

public class UserRemoteApi : RemoteApiService<User>, IUserRemoteApi
{
    public UserRemoteApi( IServiceProvider serviceProvider ) : base( serviceProvider )
    {
    }

    public Task<User> GetCurrentUserAsync()
    {
        string url = UrlBuilder.Profile;
        return RequestProvider.GetAsync<User>( url, SettingsService.AuthAccessToken! );
    }

    public Task SaveUserNameAsync( string userName )
    {
        string url = UrlBuilder.UserName;
        return RequestProvider.PutAsync( url, userName, SettingsService.AuthAccessToken! );
    }

    public Task SaveMainSloganAsync( string mainSlogan )
    {
        string url = UrlBuilder.UserMainSlogan;
        return RequestProvider.PutAsync( url, mainSlogan, SettingsService.AuthAccessToken! );
    }

    public Task SaveMissionAsync( string mission )
    {
        string url = UrlBuilder.UserMission;
        return RequestProvider.PutAsync( url, mission, SettingsService.AuthAccessToken! );
    }

    public Task DeleteAccountAsync()
    {
        string url = $"{UrlBuilder.Account}";
        return RequestProvider.DeleteAsync(url, SettingsService.AuthAccessToken!);
    }

    public override async Task HandleQueueItemAsync( SyncQueueItem queueItem )
    {
        string operation = queueItem.Operation;
        string? payloadJson = queueItem.PayloadJson;

        if (operation == "SaveUserName")
        {
            string userName = payloadJson ?? string.Empty;
            await SaveUserNameAsync( userName );
        }
        else if (operation == "SaveMainSlogan")
        {
            string mainSlogan = payloadJson ?? string.Empty;
            await SaveMainSloganAsync( mainSlogan );
        }
        else if (operation == "SaveMission")
        {
            string mission = payloadJson ?? string.Empty;
            await SaveMissionAsync( mission );
        }
        else
        {
            throw new ArgumentException( $"Operation '{operation}' is not supported in UserRemoteApi." );
        }
    }
}