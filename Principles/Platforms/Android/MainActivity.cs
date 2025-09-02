using Android.App;
using Android.Content.PM;
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

        var androidId = Android.Provider.Settings.Secure.GetString(
            ContentResolver,
            Android.Provider.Settings.Secure.AndroidId );
        var md5 = System.Security.Cryptography.MD5.Create();
        var bytes = md5.ComputeHash( System.Text.Encoding.UTF8.GetBytes( androidId ) );
        var hashedId = string.Concat( bytes.Select( b => b.ToString( "X2" ) ) );
        System.Diagnostics.Debug.WriteLine( $"Test Device ID: {hashedId}" );

        RequestConsent();
    }


    private void RequestConsent()
    {
        var debugSettings = new ConsentDebugSettings
            .Builder( this )
            .SetDebugGeography( ConsentDebugSettings.DebugGeography.DebugGeographyEea )
            .AddTestDeviceHashedId( "2CCF7401618406FDEF0E2940789EC74C" )
            .Build();

        var parameters = new ConsentRequestParameters.Builder()
            .SetTagForUnderAgeOfConsent( false )
            .SetConsentDebugSettings( debugSettings )
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
        public ConsentInfoUpdateSuccessListener( MainActivity activity )
        {
            this.activity = activity;
        }
        public void OnConsentInfoUpdateSuccess()
        {
            if (UserMessagingPlatform.GetConsentInformation( activity ).IsConsentFormAvailable)
            {
                UserMessagingPlatform.LoadConsentForm(
                    activity,
                    new ConsentFormLoadSuccessListener( activity ),
                    new ConsentFormLoadFailureListener()
                );
            }
        }
    }

    private class ConsentInfoUpdateFailureListener : Java.Lang.Object, IConsentInformationOnConsentInfoUpdateFailureListener
    {
        public void OnConsentInfoUpdateFailure( FormError error )
        {
            System.Diagnostics.Debug.WriteLine( $"Consent update failed: {error.Message}" );
        }
    }

    private class ConsentFormLoadSuccessListener : Java.Lang.Object, IOnConsentFormLoadSuccessListener
    {
        private readonly MainActivity activity;
        public ConsentFormLoadSuccessListener( MainActivity activity )
        {
            this.activity = activity;
        }

        public void OnConsentFormLoadSuccess( IConsentForm p0 )
        {
            p0.Show( activity, new ConsentFormDismissedListener() );
        }
    }

    private class ConsentFormLoadFailureListener : Java.Lang.Object, UserMessagingPlatform.IOnConsentFormLoadFailureListener
    {
        public void OnConsentFormLoadFailure( FormError error )
        {
            System.Diagnostics.Debug.WriteLine( $"Consent form load failed: {error.Message}" );
        }
    }

    private class ConsentFormDismissedListener : Java.Lang.Object, IConsentFormOnConsentFormDismissedListener
    {
        public void OnConsentFormDismissed( FormError error )
        {
            if (error != null)
                System.Diagnostics.Debug.WriteLine( $"Consent form dismissed with error: {error.Message}" );
            else
                System.Diagnostics.Debug.WriteLine( "Consent form dismissed successfully." );
        }
    }
}
