using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;
public class VersionCheckerService : BaseRemoteService, IVersionCheckerService
{
    public VersionCheckerService( IServiceProvider serviceProvider )
        : base( serviceProvider )
    {
        //do nothing
    }
    public async Task<AppVersionInfo> GetAppVersionAsync(string language )
    {
        string url = $"{UrlBuilder.VersionCheck}?language={language}";
        AppVersionInfo appVertion = await RequestProvider.GetAsync<AppVersionInfo>(
            url
        ).DefaultConfigureAwait();
        return appVertion;
    }
}
