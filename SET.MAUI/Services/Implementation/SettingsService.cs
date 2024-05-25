using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.MAUI.Services;

public class SettingsService : ISettingsService
{
    public bool IsDebug
    {
        get
        {
            bool result;
#if DEBUG
            result = true;
#else
            result = false;
#endif
            return result;
        }
    }

    public string AuthAccessToken
    {
        get => Preferences.Get( key: "access_token", defaultValue: "" )!;
        set => Preferences.Set( key: "access_token", value );
    }

    public string UserId
    {
        get => Preferences.Get( key: "user_id", defaultValue: "0" )!;
        set => Preferences.Set( key: "user_id", value );
    }
}
