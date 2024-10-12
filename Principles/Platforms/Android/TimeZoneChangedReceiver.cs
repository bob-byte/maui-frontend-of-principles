using Android.App;
using Android.Content;

namespace Principles;

[BroadcastReceiver(Enabled = true, Exported = true)]
[IntentFilter(new[] { Intent.ActionTimezoneChanged })]
public class TimeZoneChangedReceiver : BroadcastReceiver
{
    private readonly Action<Context?, Intent?>? m_handleFunc;

    public TimeZoneChangedReceiver( Action<Context?, Intent?> handleFunc )
    {
        m_handleFunc = handleFunc;
    }

    public TimeZoneChangedReceiver()
    {

    }

    public override void OnReceive( Context? context, Intent? intent )
    {
        m_handleFunc?.Invoke( context, intent );
    }
}

