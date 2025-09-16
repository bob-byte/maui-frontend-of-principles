using Foundation;

using Google.MobileAds;

using ObjCRuntime;

using Principles.Platforms.iOS;

using Google.UserMessagingPlatform;
namespace Principles;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    public AppDelegate() { }
    
    protected override MauiApp CreateMauiApp()
    {
        DependencyService.RegisterSingleton<IDeviceOrientation>( new DeviceOrientationForIos() );
        DependencyService.RegisterSingleton<ILockDeviceOrientation>( new LockDeviceOrientationForIos() );

        return MauiProgram.CreateMauiApp();
    }

    public override bool FinishedLaunching( UIApplication application, NSDictionary launchOptions )
    {
        Runtime.MarshalManagedException += ( _, e ) => e.ExceptionMode = MarshalManagedExceptionMode.UnwindNativeCode;
        Runtime.MarshalObjectiveCException += ( _, e ) => e.ExceptionMode = MarshalObjectiveCExceptionMode.UnwindManagedCode;
        MobileAds.SharedInstance.Start( CompletionHandler );
        RequestConsent();
        return base.FinishedLaunching( application, launchOptions );
    }

    private void CompletionHandler(InitializationStatus status)
    {
        
    }

    private void RequestConsent()
    {
        var parameters = new RequestParameters
        {
            TagForUnderAgeOfConsent = false
        };

        var consentInformation = ConsentInformation.SharedInstance;
        consentInformation.RequestConsentInfoUpdateWithParameters(
            parameters,
            ( NSError error ) =>
            {
                if (error != null)
                {
                    Console.WriteLine( $"[UMP iOS] RequestConsentInfoUpdate error: {error.LocalizedDescription}" );
                    return;
                }

                if (consentInformation.FormStatus == FormStatus.Available)
                {
                    ConsentForm.LoadWithCompletionHandler( ( form, loadError ) =>
                    {
                        if (loadError != null)
                        {
                            Console.WriteLine( $"[UMP iOS] Form load error: {loadError.LocalizedDescription}" );
                            return;
                        }
                        var rootVc = UIApplication.SharedApplication
                                    .ConnectedScenes
                                    .OfType<UIWindowScene>()
                                    .SelectMany( s => s.Windows )
                                    .FirstOrDefault( w => w.IsKeyWindow )?
                                    .RootViewController;

                        form?.PresentFromViewController(rootVc,
                            ( dismissError ) =>
                            {
                                if (dismissError != null)
                                    Console.WriteLine( $"[UMP iOS] Dismiss error: {dismissError.LocalizedDescription}" );
                                else
                                    Console.WriteLine( "[UMP iOS] Consent form dismissed successfully." );
                            } );
                    } );
                }
            } );
    }
}
