namespace Principles.Core;

public class AppDatabase : SQLiteConnection
{
    public AppDatabase(string dbPath) : base(dbPath)
    {
        CreateTable<SyncQueueItem>();
    }

    public TableQuery<SyncQueueItem> SyncQueue => Table<SyncQueueItem>();
}