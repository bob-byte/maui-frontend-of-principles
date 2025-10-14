namespace Principles.Core.Services;
public interface IAdService
{
    Task GetAccessToTrackAsync();
    void LoadInterstitialAd();
    Task ShowInterstitialAdAsync();
    void CleanupAds();
}