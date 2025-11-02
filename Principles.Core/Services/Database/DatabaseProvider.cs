namespace Principles.Core.Services;

public class DatabaseProvider : IDatabaseProvider
{
    private readonly IDatabasePathProvider m_databasePathProvider;

    public DatabaseProvider( IDatabasePathProvider databasePathProvider )
    {
        string dbPath = databasePathProvider.GetDatabasePath();
        SQLiteOpenFlags flags = SQLiteOpenFlags.ReadWrite |
                                SQLiteOpenFlags.Create |
                                SQLiteOpenFlags.SharedCache |
                                SQLiteOpenFlags.FullMutex;

        string key = GetDatabaseKey();

        SQLiteConnectionString connectionString = new( dbPath, flags, storeDateTimeAsTicks: true, key: key );
        SyncConnection = new SQLiteConnection( connectionString );
        AsyncConnection =
            new SQLiteAsyncConnection( connectionString );
    }

    public SQLiteConnection SyncConnection { get; }
    public SQLiteAsyncConnection AsyncConnection { get; }

    public string GetDatabasePath()
    {
        return m_databasePathProvider.GetDatabasePath();
    }

    private string GetDatabaseKey()
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