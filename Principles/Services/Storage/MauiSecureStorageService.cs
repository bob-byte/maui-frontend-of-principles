namespace Principles.Services;

public class MauiSecureStorageService : ISecureStorageService
{
    public Task<string?> GetAsync( string key )
    {
        return SecureStorage.Default.GetAsync( key );
    }

    public Task SetAsync( string key, string value )
    {
        return SecureStorage.Default.SetAsync( key, value );
    }
}
