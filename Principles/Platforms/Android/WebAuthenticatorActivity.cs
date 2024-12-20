using Android.App;
using Android.Content;
using Android.Content.PM;

namespace Principles;

[Activity( NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true )]
[IntentFilter( new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "com.set.principles" // Для Google OAuth
)]
// [IntentFilter( new[] { Intent.ActionView },
//     Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
//     DataScheme = "https",            // Для Apple OAuth
//     DataHost = "principles.top",     // Ваш домен
//     DataPathPrefix = "/api/auth/apple" // Шлях, що вказаний у callbackUrl
// )]
public class WebAuthenticatorActivity : WebAuthenticatorCallbackActivity
{
    
}
