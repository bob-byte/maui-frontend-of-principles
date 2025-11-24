namespace Principles.Core.Services;

public class DatabaseConnectionProvider :IDatabaseConnectionProvider
{
    private readonly IDatabasePathProvider m_databasePathProvider;

    public DatabaseConnectionProvider( IDatabasePathProvider databasePathProvider, IDatabaseKeyProvider keyProvider )
    {
        string dbPath = databasePathProvider.GetDatabasePath();
        SQLiteOpenFlags flags = SQLiteOpenFlags.ReadWrite |
                                SQLiteOpenFlags.Create |
                                SQLiteOpenFlags.SharedCache |
                                SQLiteOpenFlags.FullMutex;
                                
        string key = keyProvider.GetDatabaseKey();

        SQLiteConnectionString connectionString = new( dbPath, flags, storeDateTimeAsTicks: false, key: key );
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
}