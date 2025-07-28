#if __ANDROID__
using Android.Gms;
using Android.Gms.Ads;

using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

using Plugin.MauiMTAdmob;

#elif __IOS__
using AppTrackingTransparency;
using Foundation;

using Google.MobileAds;
#endif


namespace Principles.Services;
public class AdvertisementService : IAdvertisementService
{
    public async Task GetAccessToTrackAsync()
    {
#if __ANDROID__
        await Task.CompletedTask;
#elif __IOS__
        if (ATTrackingManager.TrackingAuthorizationStatus == ATTrackingManagerAuthorizationStatus.NotDetermined)
        {
            await ATTrackingManager.RequestTrackingAuthorizationAsync();
        }
#endif
    }
}
