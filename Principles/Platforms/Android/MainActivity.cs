using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;

using Plugin.MauiMTAdmob;

using Xamarin.Google.UserMesssagingPlatform;

using static Xamarin.Google.UserMesssagingPlatform.UserMessagingPlatform;

using IConsentInformation = Xamarin.Google.UserMesssagingPlatform.IConsentInformation;

namespace Principles;

[Activity( Label = "@string/app_name", Exported = true, Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density )]
public class MainActivity : MauiAppCompatActivity
{
    
    protected override void OnCreate( Bundle savedInstanceState )
    {
        base.OnCreate( savedInstanceState );

        MobileAds.Initialize( this );
        RequestConsent();
    }

    private void RequestConsent()
    {
        ConsentRequestParameters parameters = new ConsentRequestParameters.Builder()
            .SetTagForUnderAgeOfConsent( false )
            .Build();
    
        IConsentInformation consentInformation = UserMessagingPlatform.GetConsentInformation( this );
    
        consentInformation.RequestConsentInfoUpdate(
            this,
            parameters,
            new ConsentInfoUpdateSuccessListener( this ),
            new ConsentInfoUpdateFailureListener()
        );
    }
    
    private class ConsentInfoUpdateSuccessListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateSuccessListener
    {
        private readonly MainActivity m_activity;
        
        public ConsentInfoUpdateSuccessListener( MainActivity activity )
        {
            m_activity = activity;
        }
    
        public void OnConsentInfoUpdateSuccess()
        {
            IConsentInformation consentInformation = UserMessagingPlatform.GetConsentInformation( m_activity );
            
            ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
            if (settingsService.IsDebug)
            {
                ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
                loggingService.LogInfo( $"[UMP] InfoUpdateSuccess → Status={consentInformation.ConsentStatus}, FormAvailable={consentInformation.IsConsentFormAvailable}" );
            }
            
            if (consentInformation is { ConsentStatus: ConsentInformationConsentStatus.Required, IsConsentFormAvailable: true })
            {
                Task.Run( async () =>
                {
                    IDialogService dialogService = ServiceLocator.Current.GetRequiredService<IDialogService>();
                    
                    await MainThread.InvokeOnMainThreadAsync( async () =>
                    {
                        await dialogService.ShowAlertAsync(
                            LocStrings.DescriptionWhyWeAppendedAds,
                            LocStrings.Message,
                            LocStrings.OK
                        );
    
                        UserMessagingPlatform.LoadConsentForm(
                            m_activity,
                            new ConsentFormLoadSuccessListener( m_activity ),
                            new ConsentFormLoadFailureListener()
                        );
                    } );
                } );
            }
            else
            {
                m_activity.SetupAds();
            }
        }
    }
    
    private class ConsentInfoUpdateFailureListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateFailureListener
    {
        public void OnConsentInfoUpdateFailure( FormError error )
        {
            ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
            loggingService.LogError( $"[UMP] InfoUpdate FAILED → {error.Message}" );
        }
    }
    
    private class ConsentFormLoadSuccessListener : Java.Lang.Object, IOnConsentFormLoadSuccessListener
    {
        private readonly MainActivity activity;
        public ConsentFormLoadSuccessListener( MainActivity activity ) => this.activity = activity;
    
        public void OnConsentFormLoadSuccess( IConsentForm form )
        {
            ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
            if (settingsService.IsDebug)
            {
                ILoggingService loggingService = ServiceLocator.Current.GetRequiredService<ILoggingService>();
                loggingService.LogInfo( "[UMP] Consent form loaded → showing..." );
            }
            
            form.Show( activity, new ConsentFormDismissedListener( activity ) );
        }
    }
    
    private class ConsentFormLoadFailureListener : Java.Lang.Object, IOnConsentFormLoadFailureListener
    {
        public void OnConsentFormLoadFailure( FormError error )
        {
            ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
            loggingService.LogInfo( $"[UMP] Form load FAILED → {error.Message}" );
        }
    }
    
    private class ConsentFormDismissedListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
    {
        private readonly MainActivity activity;
        public ConsentFormDismissedListener( MainActivity activity ) => this.activity = activity;
    
        public void OnConsentFormDismissed( FormError error )
        {
            ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
    
            if (string.IsNullOrWhiteSpace(error?.Message))
            {
                ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
                if (settingsService.IsDebug)
                {
                    loggingService.LogInfo( "[UMP] Form dismissed successfully." );
                }
            }
            else
            {
                loggingService.LogError( $"[UMP] Form dismissed with error: {error.Message}" );
            }
    
            activity.SetupAds();
        }
    }
    private void SetupAds()
    {
        IConsentInformation ci = UserMessagingPlatform.GetConsentInformation( this );
    
        bool nonPersonalized = ci.ConsentStatus != ConsentInformationConsentStatus.Obtained;
    
        var extras = new Bundle();
        if (nonPersonalized)
        {
            extras.PutString( "npa", "1" );
        }
    
        ISettingsService settingsService = ServiceLocator.Current!.GetRequiredService<ISettingsService>();
        if (settingsService.IsDebug)
        {
            ILoggingService loggingService = ServiceLocator.Current!.GetRequiredService<ILoggingService>();
            loggingService.LogInfo( $"[ADS] Initializing ads. NonPersonalized={nonPersonalized}" );
        }
    }
}
