using System;

namespace Principles.Services;

public class DatabaseKeyProvider : IDatabaseKeyProvider
{
    public string GetDatabaseKey()
    {
        const string KEY_NAME = "db_encryption_key";
        string? key = SecureStorage.Default.GetAsync( KEY_NAME ).Result;
        if (string.IsNullOrWhiteSpace( key ))
        {
            key = Guid.NewGuid().ToString( format: "N" );
            SecureStorage.Default.SetAsync( KEY_NAME, key );
        }

        return key;
    }
}
