namespace Principles.Core.Services;

public class DatabaseConnectionProvider :IDatabaseConnectionProvider
{
    private readonly IDatabasePathProvider m_databasePathProvider;

    public DatabaseConnectionProvider( IDatabasePathProvider databasePathProvider, IDatabaseKeyProvider keyProvider )
    {
        m_databasePathProvider = databasePathProvider;
        string dbPath = databasePathProvider.GetDatabasePath();
        SQLiteOpenFlags flags = SQLiteOpenFlags.ReadWrite |
                                SQLiteOpenFlags.Create |
                                SQLiteOpenFlags.SharedCache |
                                SQLiteOpenFlags.FullMutex;

        string key = keyProvider.GetDatabaseKey();

        SQLiteConnectionString connectionString = new( dbPath, flags, storeDateTimeAsTicks: false, key: key );
        SyncConnection = new SQLiteConnection( connectionString );
        SyncConnection.Execute( "PRAGMA foreign_keys = ON;" );

        AsyncConnection = new SQLiteAsyncConnection( connectionString );
        AsyncConnection.ExecuteAsync( "PRAGMA foreign_keys = ON;" ).GetAwaiter().GetResult();
    }

    public SQLiteConnection SyncConnection { get; }
    public SQLiteAsyncConnection AsyncConnection { get; }

    public string GetDatabasePath()
    {
        return m_databasePathProvider.GetDatabasePath();
    }
}
