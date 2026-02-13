namespace Principles.Core.Services;

public interface ISettingsService
{
    bool IsDebug { get; }
    string? AuthAccessToken { get; }
    double NormalPageWidth { get; set; }
    double NormalPageHeight { get; set; }
    string? CurrentCulture { get; set; }
    bool IsAdsEnabled { get; }

    Task<string> GetAuthAccessTokenAsync();
    Task SetAuthAccessTokenAsync( string value );
}