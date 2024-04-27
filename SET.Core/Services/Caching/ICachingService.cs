namespace SET.Core.Services;

public interface ICachingService
{
    void SetForever( string key, string value );
    string StoredValue( string key );
    void Remove( string key );
}