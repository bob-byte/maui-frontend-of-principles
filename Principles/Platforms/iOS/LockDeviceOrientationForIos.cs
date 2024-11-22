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
            UIInterfaceOrientation orientationMask = orientation switch
            {
                DeviceOrientation.Portrait => UIInterfaceOrientation.Portrait,
                DeviceOrientation.Landscape => UIInterfaceOrientation.LandscapeLeft,
                _ => UIInterfaceOrientation.Unknown
            };
            
            //TODO: fix it, because it doesn't work
            UIDevice.CurrentDevice.SetValueForKey(
                NSNumber.FromNInt((int)(orientationMask)),
                new NSString("orientation")
            );
        }

        public void UnlockOrientation()
        {
            UIDevice.CurrentDevice.SetValueForKey( NSNumber.FromNInt((int)(UIInterfaceOrientation.Unknown)), new NSString( "orientation" ) );
        }
    }
}
