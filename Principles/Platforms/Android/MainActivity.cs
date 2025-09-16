using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;

using Xamarin.Google.UserMesssagingPlatform;

using static Xamarin.Google.UserMesssagingPlatform.UserMessagingPlatform;

using Activity = Android.App.Activity;

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
        var parameters = new ConsentRequestParameters.Builder()
            .SetTagForUnderAgeOfConsent( false )
            .Build();

        var consentInformation = UserMessagingPlatform.GetConsentInformation( this );

        consentInformation.RequestConsentInfoUpdate(
            this,
            parameters,
            new ConsentInfoUpdateSuccessListener( this ),
            new ConsentInfoUpdateFailureListener()
        );
    }

    private class ConsentInfoUpdateSuccessListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateSuccessListener
    {
        private readonly MainActivity activity;
        public ConsentInfoUpdateSuccessListener( MainActivity activity ) => this.activity = activity;

        public void OnConsentInfoUpdateSuccess()
        {
            var ci = UserMessagingPlatform.GetConsentInformation( activity );
            System.Diagnostics.Debug.WriteLine( $"[UMP] InfoUpdateSuccess → Status={ci.ConsentStatus}, FormAvailable={ci.IsConsentFormAvailable}" );

            if (ci.ConsentStatus == ConsentInformationConsentStatus.Required && ci.IsConsentFormAvailable)
            {
                UserMessagingPlatform.LoadConsentForm(
                    activity,
                    new ConsentFormLoadSuccessListener( activity ),
                    new ConsentFormLoadFailureListener()
                );
            }
            else
            {
                activity.SetupAds();
            }
        }
    }

    private class ConsentInfoUpdateFailureListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateFailureListener
    {
        public void OnConsentInfoUpdateFailure( FormError error )
        {
            System.Diagnostics.Debug.WriteLine( $"[UMP] InfoUpdate FAILED → {error.Message}" );
        }
    }

    private class ConsentFormLoadSuccessListener : Java.Lang.Object, IOnConsentFormLoadSuccessListener
    {
        private readonly MainActivity activity;
        public ConsentFormLoadSuccessListener( MainActivity activity ) => this.activity = activity;

        public void OnConsentFormLoadSuccess( IConsentForm form )
        {
            System.Diagnostics.Debug.WriteLine( "[UMP] Consent form loaded → showing..." );
            form.Show( activity, new ConsentFormDismissedListener( activity ) );
        }
    }

    private class ConsentFormLoadFailureListener : Java.Lang.Object, IOnConsentFormLoadFailureListener
    {
        public void OnConsentFormLoadFailure( FormError error )
        {
            System.Diagnostics.Debug.WriteLine( $"[UMP] Form load FAILED → {error.Message}" );
        }
    }

    private class ConsentFormDismissedListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
    {
        private readonly MainActivity activity;
        public ConsentFormDismissedListener( MainActivity activity ) => this.activity = activity;

        public void OnConsentFormDismissed( FormError error )
        {
            if (error != null)
                System.Diagnostics.Debug.WriteLine( $"[UMP] Form dismissed with error: {error.Message}" );
            else
                System.Diagnostics.Debug.WriteLine( "[UMP] Form dismissed successfully." );

            activity.SetupAds();
        }
    }
    private void SetupAds()
    {
        var ci = UserMessagingPlatform.GetConsentInformation( this );

        bool nonPersonalized = ci.ConsentStatus != ConsentInformationConsentStatus.Obtained;

        var extras = new Bundle();
        if (nonPersonalized)
            extras.PutString( "npa", "1" ); 

        System.Diagnostics.Debug.WriteLine( $"[ADS] Initializing ads. NonPersonalized={nonPersonalized}" );

    }
}
