namespace Principles.Core.Services;

public interface ISettingsService
{
    bool IsDebug { get; }
    string? AuthAccessToken { get; }
    DateTime? LastSuccessfulSyncAt { get; set; }
    DateTime? LastFailedSyncAt { get; set; }
    double NormalPageWidth { get; set; }
    double NormalPageHeight { get; set; }
    string CurrentCulture { get; set; }

    Task<string> GetAuthAccessTokenAsync();
    Task SetAuthAccessTokenAsync( string value );
}
