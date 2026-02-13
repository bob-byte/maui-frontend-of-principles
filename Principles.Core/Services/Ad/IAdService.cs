namespace Principles.Core.Services;
public interface IAdService
{
    bool IsConsentConfigured { get; }
    event EventHandler ConsentIsConfigured;
    Task GetAccessToTrackAsync();
    void LoadInterstitialAd();
    Task IfRequiredShowInterstitialAdAsync();
    Task ShowInterstitialAdAsync();
    void CleanupAds();
}