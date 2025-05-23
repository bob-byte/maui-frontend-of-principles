using Android.App;
using Android.Content.Res;
using Android.Runtime;

using Principles.Platforms.Android;

namespace Principles;

#if LOCALDEBUG
[Application( UsesCleartextTraffic = true )]
#else
[Application]
#endif
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping( nameof( Editor ), ( handler, view ) =>
        {
            if (view is Editor)
            {
                handler.PlatformView.BackgroundTintList = ColorStateList.ValueOf( Android.Graphics.Color.Transparent );
            }
        } );
    }
    protected override MauiApp CreateMauiApp()
    {
        DependencyService.RegisterSingleton<IDeviceOrientation>(new DeviceOrientationForAndroid());
        DependencyService.RegisterSingleton<ILockDeviceOrientation>( new LockDeviceOrientationForAndroid() );

        return MauiProgram.CreateMauiApp();
    }
}