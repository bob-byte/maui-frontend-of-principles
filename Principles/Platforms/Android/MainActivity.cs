using Android.App;
using Android.Content.PM;
using Android.OS;

using AndroidX.AppCompat.App;

namespace Principles;

[Activity( Label = "@string/app_name", Exported = true, Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density )]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate( Bundle? savedInstanceState )
    {
        AppCompatDelegate.DefaultNightMode = AppCompatDelegate.ModeNightNo;

        base.OnCreate(savedInstanceState);
    }
}
