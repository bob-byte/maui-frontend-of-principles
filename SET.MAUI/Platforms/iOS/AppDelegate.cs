using Foundation;

using ObjCRuntime;

namespace SET.MAUI;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp()
    {
        DependencyService.RegisterSingleton<IDeviceOrientation>( new DeviceOrientationForIos() );
        return MauiProgram.CreateMauiApp();
    }

    public override bool FinishedLaunching( UIApplication application, NSDictionary launchOptions )
    {
        Runtime.MarshalManagedException += ( _, e ) => e.ExceptionMode = MarshalManagedExceptionMode.UnwindNativeCode;
        Runtime.MarshalObjectiveCException += ( _, e ) => e.ExceptionMode = MarshalObjectiveCExceptionMode.UnwindManagedCode;
        return base.FinishedLaunching( application, launchOptions );
    }
}
