using Principles.Core.Constants;

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
    private IConfiguration m_configuration;

    public ApiKeyService( IServiceProvider serviceProvider )
    {
        m_requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
    }

    public async Task<string> RestoreApiKeyAsync()
    {
        string result;

        string? encryptedApiKey = await SecureStorage.GetAsync( PreferenceKeys.API_KEY ).DefaultConfigureAwait();
        
        if (string.IsNullOrWhiteSpace( encryptedApiKey ))
        {
            string url = $"{m_urlBuilder.ApiKey}";
            ApiKeyResponse response = await m_requestProvider.GetAsync<ApiKeyResponse>( url, m_settingsService.AuthAccessToken! ).DefaultConfigureAwait();
            
            result = DecryptTextHelper.DecryptText( response.Value, m_configuration["ApiKeyEncryptionSettings:FirstKey"]!, m_configuration["ApiKeyEncryptionSettings:SecondKey"]! );
            await SecureStorage.SetAsync( PreferenceKeys.API_KEY, response.Value ).DefaultConfigureAwait();
        }
        else
        {
            result = DecryptTextHelper.DecryptText( encryptedApiKey, m_configuration["ApiKeyEncryptionSettings:FirstKey"]!, m_configuration["ApiKeyEncryptionSettings:SecondKey"]! );
        }
        
        return result;
    }
}
