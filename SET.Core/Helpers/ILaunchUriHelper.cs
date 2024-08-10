namespace SET.Core.Helpers;

public interface ILaunchUriHelper
{
    Task<LaunchExternalOAuthResult> LaunchResult { get; }
    bool TryHandle( Uri uri );
    void Reset();
}
