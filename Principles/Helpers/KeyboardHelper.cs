
#if ANDROID
using Android.Views.InputMethods;
using Android.Content;

using Activity = Android.App.Activity;
using View = Android.Views.View;

using Android.OS;
#endif

#if IOS
using UIKit;
#endif

namespace Principles.Helpers;

public static class KeyboardHelper
{
    public static void HideKeyboard()
    {
#if ANDROID
        Activity? activity = Platform.CurrentActivity;
        var inputMethodManager = activity?.GetSystemService(Context.InputMethodService) as InputMethodManager;

        View? currentFocus = activity?.CurrentFocus;
        if (currentFocus != null && inputMethodManager != null)
        {
            inputMethodManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            currentFocus.ClearFocus(); // optional: also clears focus
        }
#elif IOS
        UIWindow? keyWindow = UIApplication.SharedApplication.KeyWindow;
        if (keyWindow is not null)
        {
            keyWindow?.EndEditing( force: true );
        }
#endif
    }
}

