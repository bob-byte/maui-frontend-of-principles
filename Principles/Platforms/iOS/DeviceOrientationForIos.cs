
namespace Principles;

public class DeviceOrientationForIos : IDeviceOrientation
{
    public DeviceOrientationType GetOrientation()
    {
        DisplayOrientation orientation = DeviceDisplay.Current.MainDisplayInfo.Orientation;
        return orientation switch
        {
            DisplayOrientation.Landscape => DeviceOrientationType.Landscape,
            DisplayOrientation.Portrait => DeviceOrientationType.Portrait,
            _ => DeviceOrientationType.Undefined
        };
    }
}