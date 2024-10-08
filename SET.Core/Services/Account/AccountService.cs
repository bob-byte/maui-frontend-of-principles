using SET.Core.Services;

using System;
namespace SET.Core.Services;

public class AccountService : BaseRemoteService, IAccountService
{
    public AccountService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }

    public async Task DeleteAccountAsync()
    {
        string url = $"{UrlBuilder.Account}";
        await RequestProvider.DeleteAsync(url, SettingsService.AuthAccessToken);
    }
}
