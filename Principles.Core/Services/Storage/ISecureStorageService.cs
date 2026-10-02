namespace Principles.Core.Services;

/// <summary>
/// Platform-agnostic secure key/value storage (e.g. MAUI SecureStorage).
/// </summary>
public interface ISecureStorageService
{
    Task<string?> GetAsync( string key );
    Task SetAsync( string key, string value );
}
