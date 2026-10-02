namespace Principles.Core.Services;

public interface IPreferencesService
{
    void SetForever( string key, string value );
    void SetForever( string key, bool value );
    string GetStoredValue( string key );
    bool GetStoredValueOrDefault( string key, bool defaultValue = false );
    void Remove( string key );
}
