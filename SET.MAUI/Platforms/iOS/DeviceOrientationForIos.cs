[assembly: Microsoft.Maui.Controls.Dependency( typeof( IDeviceOrientation ) )]

namespace SET.MAUI
{
    public class DeviceOrientationForIos : IDeviceOrientation
    {
        public DeviceOrientationType GetOrientation()
        {
            UIInterfaceOrientation orientation = UIApplication.SharedApplication.StatusBarOrientation;
            return orientation switch
            {
                UIInterfaceOrientation.LandscapeLeft => DeviceOrientationType.Landscape,
                UIInterfaceOrientation.LandscapeRight => DeviceOrientationType.Landscape,
                UIInterfaceOrientation.Portrait => DeviceOrientationType.Portrait,
                UIInterfaceOrientation.PortraitUpsideDown => DeviceOrientationType.Portrait,
                _ => DeviceOrientationType.Undefined
            };
        }
    }
}