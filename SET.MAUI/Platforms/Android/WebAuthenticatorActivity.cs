using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SET.MAUI;

[Activity( NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true )]
[IntentFilter( new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "com.set.principles"
)]
public class WebAuthenticatorActivity : WebAuthenticatorCallbackActivity
{

}
