namespace SET.Core.Services;

public interface ISettingsService
{
    bool IsDebug { get; }
    string AuthAccessToken { get; set; }
    string UserId { get; set; }
}