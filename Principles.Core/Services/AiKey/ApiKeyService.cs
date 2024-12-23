using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Core.Services.AiKey;
public class ApiKeyService : IApiKeyService
{
    private IRequestProvider m_requestProvider;
    private IUrlBuilder m_urlBuilder;
    private ISettingsService m_settingsService;

    public ApiKeyService( IServiceProvider serviceProvider )
    {
        m_requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
    }

    public async Task<string> GetApiKeyAsync()
    {
        string url = $"{m_urlBuilder.ApiKey}";
        ApiKeyResponse response = await m_requestProvider.GetAsync<ApiKeyResponse>( url, m_settingsService.AuthAccessToken ).DefaultConfigureAwait();
   
        return DecryptTextHelper.DecryptText( response.Value, "FirstKey", "SecondKey" );
    }
}
