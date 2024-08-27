namespace SET.Core.Services;

public interface ISettingsService
{
    bool IsDebug { get; }
    string? AuthAccessToken { get; }

    Task<string> GetAuthAccessTokenAsync();
    Task SetAuthAccessTokenAsync( string value );
}