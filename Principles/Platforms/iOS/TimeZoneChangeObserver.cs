using System;

using Foundation;

namespace Principles;

public class TimeZoneChangeObserver
{
    private NSObject? m_notificationObserver;

    public void StartObservingTimeZoneChanges(Action<NSNotification> handleFunc)
    {
        m_notificationObserver = NSNotificationCenter.DefaultCenter.AddObserver( (NSString)"NSSystemTimeZoneDidChangeNotification", handleFunc );
    }

    public void StopObservingTimeZoneChanges()
    {
        if (m_notificationObserver != null)
        {
            NSNotificationCenter.DefaultCenter.RemoveObserver( m_notificationObserver );
            m_notificationObserver.Dispose();
            m_notificationObserver = null;
        }
    }
}

