using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SET.Core.Services;

public class SimpleSettingsService : ISettingsService
{
    public SimpleSettingsService()
    {
        AuthAccessToken = "";
        UserId = "";
    }

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

    public string AuthAccessToken { get; set; }

    public string UserId { get; set; }
}
