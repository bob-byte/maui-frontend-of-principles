using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SET.MAUI;

[Activity(Label = "@string/app_name", Exported = true, Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
[IntentFilter( new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "com.set.principles"
)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnNewIntent( Intent? intent )
    {
        base.OnNewIntent( intent );

        string? rawUri = intent?.Data?.ToString();
        if (rawUri is not null)
        {
            ILaunchUriHelper launchUriHelper = ServiceLocator.Current!.GetRequiredService<ILaunchUriHelper>();
            launchUriHelper.TryHandle( new Uri( rawUri ) );
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        if(Intent is not null && ServiceLocator.Current is not null)
        {
            WebAuthenticator.Default.OnResume( Intent );

            if(Intent.Data is null)
            {
                ILaunchUriHelper launchUriHelper = ServiceLocator.Current.GetRequiredService<ILaunchUriHelper>();
                launchUriHelper.Reset();
            }
        }
    }
}
