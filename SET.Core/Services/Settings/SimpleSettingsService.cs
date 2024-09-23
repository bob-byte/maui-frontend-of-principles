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

    public string AuthAccessToken { get; private set; }

    public string UserId { get; set; }

    public double NormalPageWidth { get; set; }

    public Task<string> GetAuthAccessTokenAsync()
    {
        return Task.FromResult(AuthAccessToken);
    }

    public Task SetAuthAccessTokenAsync( string value )
    {
        AuthAccessToken = value;
        return Task.CompletedTask;
    }
}
