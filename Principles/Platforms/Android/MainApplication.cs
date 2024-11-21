using Android.App;
using Android.Runtime;

using Principles.Platforms.Android;

namespace Principles;

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
        DependencyService.RegisterSingleton<ILockDeviceOrientation>( new LockDeviceOrientationForAndroid() );

        return MauiProgram.CreateMauiApp();
    }
}