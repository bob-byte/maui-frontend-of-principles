namespace Principles.Core.Services;

public interface ICachingService
{
    void SetForever( string key, string value );
    string GetStoredValue( string key );
    void Remove( string key );
}