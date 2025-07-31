using Foundation;

using ObjCRuntime;

using Principles.Platforms.iOS;

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
        return base.FinishedLaunching( application, launchOptions );
    }
}
