using System;
namespace SET.Core.Services;

//TODO: use Redis DB to store data
public class CachingService : ICachingService
{
    public string StoredValue( string key )
    {
        string result = Preferences.Get( key, defaultValue: string.Empty )!;
        return result;
    }

    public void SetForever( string key, string value )
    {
        Preferences.Set( key, value );
    }

    public void Remove( string key )
    {
        Preferences.Remove( key );
    }
}

