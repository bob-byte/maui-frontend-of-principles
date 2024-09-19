namespace SET.Core.Services;

public interface ISettingsService
{
    bool IsDebug { get; }
    string? AuthAccessToken { get; }
    string UserId { get; set; }
    double NormalPageWidth { get; set; }
    double NormalPageHeight { get; set; }

    Task<string> GetAuthAccessTokenAsync();
    Task SetAuthAccessTokenAsync( string value );
}