
namespace Principles.Core.Services;
public class VersionCheckerService : BaseRemoteService, IVersionCheckerService
{
    public VersionCheckerService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }
    public async Task<AppVersionInfo> GetAppVersionAsync(string language)
    {
        string url = $"{UrlBuilder.VersionCheck}?language={language}&osPlatform={DeviceInfo.Platform}";
        AppVersionInfo appVersion = await RequestProvider.GetAsync<AppVersionInfo>(
            url
        ).DefaultConfigureAwait();
        return appVersion;
    }
}
