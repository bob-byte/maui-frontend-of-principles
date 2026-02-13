namespace Principles.Services;

//TODO: use Redis DB to store data
public class CachingService : ICachingService
{
    public string GetStoredValue( string key )
    {
        string result = Preferences.Get( key, defaultValue: string.Empty )!;
        return result;
    }

    public bool GetStoredValueOrDefault( string key, bool defaultValue = false )
    {
        return Preferences.Get( key, defaultValue );
    }

    public void SetForever( string key, string value )
    {
        Preferences.Set( key, value );
    }
    
    public void SetForever( string key, bool value )
    {
        Preferences.Set( key, value );
    }

    public void Remove( string key )
    {
        Preferences.Remove( key );
    }
}