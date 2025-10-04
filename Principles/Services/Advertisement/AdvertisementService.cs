
using Plugin.MauiMTAdmob;
#if __ANDROID__
using Android.Gms;
using Android.Gms.Ads;

using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;

#elif __IOS__
using AppTrackingTransparency;
using Foundation;
#endif


namespace Principles.Services;
public class AdvertisementService : IAdvertisementService
{
    public async Task GetAccessToTrackAsync()
    {
#if __ANDROID__
        CrossMauiMTAdmob.Current.UserPersonalizedAds = true;
        await Task.CompletedTask;
#elif __IOS__
        if (ATTrackingManager.TrackingAuthorizationStatus == ATTrackingManagerAuthorizationStatus.NotDetermined)
        {
            IDialogService dialogService = ServiceLocator.Current!.GetRequiredService<IDialogService>();
            await dialogService.ShowAlertAsync(
                LocStrings.DescriptionWhyWeAppendedAds,
                LocStrings.Message, 
                LocStrings.OK 
            );
            
            ATTrackingManagerAuthorizationStatus result = await ATTrackingManager.RequestTrackingAuthorizationAsync();
            if (result == ATTrackingManagerAuthorizationStatus.Authorized)
            {
                CrossMauiMTAdmob.Current.UserPersonalizedAds = true;
            }
            else
            {
                // Even if user denies tracking, we can still show non-personalized ads
                CrossMauiMTAdmob.Current.UserPersonalizedAds = false;
            }
        }
        else if (ATTrackingManager.TrackingAuthorizationStatus == ATTrackingManagerAuthorizationStatus.Authorized)
        {
            CrossMauiMTAdmob.Current.UserPersonalizedAds = true;
        }
        else
        {
            // For denied or restricted status, show non-personalized ads
            CrossMauiMTAdmob.Current.UserPersonalizedAds = false;
        }
#endif
    }
}
