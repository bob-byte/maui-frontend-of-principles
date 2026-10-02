using Principles.Core.Constants;

namespace Principles.Core.Services.AiKey;

public class ApiKeyService : IApiKeyService
{
    private readonly IRequestProvider m_requestProvider;
    private readonly IUrlBuilder m_urlBuilder;
    private readonly ISettingsService m_settingsService;
    private readonly IConfiguration m_configuration;
    private readonly ISecureStorageService m_secureStorage;

    public ApiKeyService( IServiceProvider serviceProvider )
    {
        m_requestProvider = serviceProvider.GetRequiredService<IRequestProvider>();
        m_urlBuilder = serviceProvider.GetRequiredService<IUrlBuilder>();
        m_settingsService = serviceProvider.GetRequiredService<ISettingsService>();
        m_configuration = serviceProvider.GetRequiredService<IConfiguration>();
        m_secureStorage = serviceProvider.GetRequiredService<ISecureStorageService>();
    }

    public async Task<string> RestoreApiKeyAsync()
    {
        string result;

        string? encryptedApiKey = await m_secureStorage.GetAsync( CacheKeys.API_KEY ).DefaultConfigureAwait();

        if (string.IsNullOrWhiteSpace( encryptedApiKey ))
        {
            string url = $"{m_urlBuilder.ApiKey}";
            ApiKeyResponse response = await m_requestProvider.GetAsync<ApiKeyResponse>( url, m_settingsService.AuthAccessToken! ).DefaultConfigureAwait();

            result = DecryptTextHelper.DecryptText( response.Value, m_configuration["ApiKeyEncryptionSettings:FirstKey"]!, m_configuration["ApiKeyEncryptionSettings:SecondKey"]! );
            await m_secureStorage.SetAsync( CacheKeys.API_KEY, response.Value ).DefaultConfigureAwait();
        }
        else
        {
            result = DecryptTextHelper.DecryptText( encryptedApiKey, m_configuration["ApiKeyEncryptionSettings:FirstKey"]!, m_configuration["ApiKeyEncryptionSettings:SecondKey"]! );
        }

        return result;
    }
}
