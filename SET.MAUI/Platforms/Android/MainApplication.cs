using Android.App;
using Android.Runtime;

namespace SET.MAUI;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }
    protected override MauiApp CreateMauiApp()
    {
        DependencyService.RegisterSingleton<IDeviceOrientation>(new DeviceOrientationForAndroid());

        return MauiProgram.CreateMauiApp();
    }
}