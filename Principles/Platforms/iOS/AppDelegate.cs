using Foundation;

using MT.Admob.UMP;

using ObjCRuntime;

using Plugin.MauiMTAdmob;

using Principles.Platforms.iOS;

using DebugGeography = Plugin.MauiMTAdmob.Extra.DebugGeography;

namespace Principles;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
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

        CrossMauiMTAdmob.Current.Init(debugMode: true, geography: DebugGeography.DEBUG_GEOGRAPHY_REGULATED_US_STATE);
        
        return base.FinishedLaunching( application, launchOptions );
    }
}
