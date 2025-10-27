
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
    private readonly ILoggingService m_loggingService;

    private IInterstitialAd? m_interstitialAd;

    public AdService( IServiceProvider serviceProvider )
    {
        m_loggingService = serviceProvider.GetRequiredService<ILoggingService>();
    }

    public event EventHandler ConsentIsConfigured;

    public bool IsConsentConfigured { get; private set; }
    
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
        TaskCompletionSource taskCompletionSource = new();
        adConsentService.OnConsentFormDismissed += ( _, _ ) => 
        {
            taskCompletionSource.SetResult();
        };
        adConsentService.OnConsentFormError += ( _, error ) =>
        {
            taskCompletionSource.SetResult();
            m_loggingService.LogError( $"Consent form error: {error.Message}" );
        };
        adConsentService.OnConsentInfoFailedToUpdate += ( _, error ) =>
        {
            taskCompletionSource.SetResult();
            m_loggingService.LogError( $"Consent info failed to update: {error.Message}" );
        };
        adConsentService.OnConsentInfoUpdated += ( _, _ ) => 
        {
            taskCompletionSource.SetResult();
        };
        adConsentService.LoadAndShowConsentFormIfRequired();

        await taskCompletionSource.Task;

        IsConsentConfigured = true;
        ConsentIsConfigured?.Invoke( this, new EventArgs() );

        IInterstitialAdService adService = ServiceLocator.Current!.GetRequiredService<IInterstitialAdService>();
        
        m_interstitialAd = adService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
        m_interstitialAd.OnAdFailedToLoad += ( _, error ) =>
        {
            m_loggingService.LogError( $"Failed to load interstitial ad: {error.Message}" );
        };

        m_interstitialAd.Load();
    }

    public void LoadInterstitialAd()
    {
        if (m_interstitialAd is null)
        {
            IInterstitialAdService interstitialAdService = ServiceLocator.Current!.GetRequiredService<IInterstitialAdService>();
            m_interstitialAd = interstitialAdService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
        }
        
        m_interstitialAd.Load();
    }

    public async Task IfRequiredShowInterstitialAdAsync()
    {
#if !DEBUG
        int tapsToShowAds = Preferences.Get( CacheKeys.TAPS_TO_SHOW_ADS, 0 ) + 1;
        Preferences.Set( CacheKeys.TAPS_TO_SHOW_ADS, tapsToShowAds );
        if (tapsToShowAds >= Constants.Constants.MAX_TAPS_TO_SHOW_ADS)
        {
            Preferences.Set( CacheKeys.TAPS_TO_SHOW_ADS, 0 );
            await ShowInterstitialAdAsync().DefaultConfigureAwait();
        }
#endif
    }

    public async Task ShowInterstitialAdAsync()
    {
        IInterstitialAdService adService = ServiceLocator.Current!.GetRequiredService<IInterstitialAdService>();
        
        if (m_interstitialAd is not null && m_interstitialAd.IsLoaded)
        {
            TaskCompletionSource taskCompletionSource = new();
            
            IInterstitialAd interstitialAd = m_interstitialAd;
            interstitialAd.OnAdFailedToShow += ( _, error ) =>
            {
                m_loggingService.LogError( $"Failed to show interstitial ad: {error.Message}" );
                
                taskCompletionSource.SetResult();
            };
            interstitialAd.OnAdShowed += ( _, _ ) =>
            {
                taskCompletionSource.SetResult();
                m_interstitialAd = adService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
                m_interstitialAd.Load();
            };
            
            interstitialAd.Show();
            
            await taskCompletionSource.Task;
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
                m_loggingService.LogError( $"Failed to load interstitial ad: {error.Message}" );
                
                taskCompletionSource.SetResult();
            };
            
            m_interstitialAd.OnAdFailedToShow += ( _, error ) =>
            {
                m_loggingService.LogError( $"Failed to show interstitial ad: {error.Message}" );
                
                taskCompletionSource.SetResult();
            };
            m_interstitialAd.OnAdShowed += ( _, _ ) =>
            {
                taskCompletionSource.SetResult();
                
                m_interstitialAd = adService.CreateAd( AdConfig.DefaultInterstitialAdUnitId );
                m_interstitialAd.Load();
            };
            
            m_interstitialAd.Load();

            await taskCompletionSource.Task;
        }
    }

    public void CleanupAds()
    {
        m_interstitialAd = null;
    }
}
