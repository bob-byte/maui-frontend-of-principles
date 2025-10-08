
using Plugin.AdMob;
using Plugin.AdMob.Configuration;
using Plugin.AdMob.Services;

#if IOS
using AppTrackingTransparency;
using Foundation;
#endif

namespace Principles.Services;

public class AdService : IAdService
{
    private IInterstitialAd? m_interstitialAd;
    
    public async Task GetAccessToTrackAsync()
    {
        IServiceLocator serviceLocator = ServiceLocator.Current!;

        if (VersionTracking.IsFirstLaunchEver)
        {
            IDialogService dialogService = serviceLocator.GetRequiredService<IDialogService>();
            await dialogService.ShowAlertAsync(
                LocStrings.DescriptionWhyWeAppendedAds,
                LocStrings.Message, 
                LocStrings.OK 
            );
        }
            
#if IOS
        if (ATTrackingManager.TrackingAuthorizationStatus == ATTrackingManagerAuthorizationStatus.NotDetermined)
        {
            await ATTrackingManager.RequestTrackingAuthorizationAsync();
        }
#endif
            
        IAdConsentService adConsentService = ServiceLocator.Current!.GetRequiredService<IAdConsentService>();
        adConsentService.LoadAndShowConsentFormIfRequired();
        
        IInterstitialAdService adService = ServiceLocator.Current!.GetRequiredService<IInterstitialAdService>();
        m_interstitialAd = adService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
        m_interstitialAd.Load();
    }

    public async Task ShowInterstitialAdAsync()
    {
        IInterstitialAdService adService = ServiceLocator.Current!.GetRequiredService<IInterstitialAdService>();
        
        if (m_interstitialAd is not null && m_interstitialAd.IsLoaded)
        {
            TaskCompletionSource taskCompletionSource = new();
            
            IInterstitialAd? interstitialAd = m_interstitialAd;
            interstitialAd.OnAdFailedToShow += ( _, error ) =>
            {
                ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
                loggingService.LogError( $"Failed to show interstitial ad: {error.Message}" );
                
                taskCompletionSource.SetResult();
            };
            interstitialAd.OnAdShowed += ( _, _ ) =>
            {
                taskCompletionSource.SetResult();
                m_interstitialAd = adService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
                m_interstitialAd.Load();
            };
            
            interstitialAd.Show();
        }
        else
        {
            TaskCompletionSource taskCompletionSource = new();
            m_interstitialAd = adService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
            m_interstitialAd.OnAdLoaded += ( _, _ ) =>
            {
                m_interstitialAd.Show();
            };
            m_interstitialAd.OnAdFailedToLoad += ( _, error ) =>
            {
                ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
                loggingService.LogError( $"Failed to load interstitial ad: {error.Message}" );
                
                taskCompletionSource.SetResult();
            };
            
            m_interstitialAd.Load();
            
            m_interstitialAd.OnAdFailedToShow += ( _, error ) =>
            {
                ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
                loggingService.LogError( $"Failed to show interstitial ad: {error.Message}" );
                
                taskCompletionSource.SetResult();
            };
            m_interstitialAd.OnAdShowed += ( _, _ ) =>
            {
                taskCompletionSource.SetResult();
            };

            await taskCompletionSource.Task;
        }
    }
}
