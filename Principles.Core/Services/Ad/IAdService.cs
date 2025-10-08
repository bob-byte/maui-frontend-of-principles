namespace Principles.Core.Services;
public interface IAdService
{
    Task GetAccessToTrackAsync();
    Task ShowInterstitialAdAsync();
}