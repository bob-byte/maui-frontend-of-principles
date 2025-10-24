using Android.App;
using Android.Content.PM;
using Android.Gms.Ads;
using Android.OS;

using Xamarin.Google.UserMesssagingPlatform;

using static Xamarin.Google.UserMesssagingPlatform.UserMessagingPlatform;

using IConsentInformation = Xamarin.Google.UserMesssagingPlatform.IConsentInformation;

namespace Principles;

[Activity( Label = "@string/app_name", Exported = true, Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density )]
public class MainActivity : MauiAppCompatActivity
{
}
