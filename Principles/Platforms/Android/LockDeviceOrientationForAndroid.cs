using Android.Content.PM;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Principles.Platforms.Android
{
    class LockDeviceOrientationForAndroid : ILockDeviceOrientation
    {
        public void LockOrientation( DeviceOrientation orientation )
        {
            global::Android.App.Activity? activity = Platform.CurrentActivity;
            activity.RequestedOrientation = orientation == DeviceOrientation.Portrait
                ? ScreenOrientation.Portrait
                : ScreenOrientation.Landscape;
        }

        public void UnlockOrientation()
        {
            global::Android.App.Activity? activity = Platform.CurrentActivity;
            activity.RequestedOrientation = ScreenOrientation.Unspecified;
        }
    }
}
