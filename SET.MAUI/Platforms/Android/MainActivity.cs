using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SET.MAUI;



[Activity(Label = "@string/app_name", Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter( new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "com.set.principles"
)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate( Bundle savedInstanceState )
    {
        base.OnCreate( savedInstanceState );
      
    }
    protected override void OnActivityResult( int requestCode, Result resultCode, Intent data )
    {
        base.OnActivityResult( requestCode, resultCode, data );
        //AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs( requestCode, resultCode, data );
    }

    protected override void OnNewIntent( Intent? intent )
    {
        base.OnNewIntent( intent );
        string? rawUri = intent?.Data?.ToString();
        if (rawUri == null) return;
        LaunchUriHelper.TryHandle( new Uri( rawUri ) );
    }
}