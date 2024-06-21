#if ANDROID
using Android.Views.InputMethods;
using Android.Content;
using Android.OS;
#endif

#if IOS
using UIKit;
#endif

namespace SET.MAUI.Helpers;

public static class KeyboardHelper
{
    public static void HideKeyboard()
    {
#if ANDROID
        var inputMethodManager = Platform.CurrentActivity?.GetSystemService( Context.InputMethodService ) as InputMethodManager;

        IBinder? windowToken = Platform.CurrentActivity?.CurrentFocus?.WindowToken;
        inputMethodManager?.HideSoftInputFromWindow( windowToken, HideSoftInputFlags.None );
#elif IOS
        UIWindow? keyWindow = UIApplication.SharedApplication.KeyWindow;
        if (keyWindow is not null)
        {
            keyWindow?.EndEditing( force: true );
        }
#endif
    }
}

