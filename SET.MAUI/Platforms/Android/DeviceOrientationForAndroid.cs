using Android.Content.Res;

namespace SET.MAUI;

public class DeviceOrientationForAndroid : IDeviceOrientation
{
    public DeviceOrientationType GetOrientation()
    {
        Orientation? orientation = Platform.CurrentActivity?.Resources?.Configuration?.Orientation;
        return orientation switch
        {
            Orientation.Landscape => DeviceOrientationType.Landscape,
            Orientation.Portrait => DeviceOrientationType.Portrait,
            _ => DeviceOrientationType.Undefined
        };
    }
}