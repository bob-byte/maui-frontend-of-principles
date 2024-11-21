using Foundation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Platforms.iOS
{
    class LockDeviceOrientationForIos : ILockDeviceOrientation
    {
        public void LockOrientation( DeviceOrientation orientation )
        {
            UIInterfaceOrientation uiOrientation = orientation == DeviceOrientation.Portrait
                ? UIInterfaceOrientation.Portrait
                : UIInterfaceOrientation.LandscapeLeft;

            UIDevice.CurrentDevice.SetValueForKey( new NSNumber( (int)uiOrientation ), new NSString( "orientation" ) );
        }

        public void UnlockOrientation()
        {
            UIDevice.CurrentDevice.SetValueForKey( new NSNumber( (int)UIInterfaceOrientation.Unknown ), new NSString( "orientation" ) );
        }
    }
}
