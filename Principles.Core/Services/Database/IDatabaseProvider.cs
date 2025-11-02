namespace Principles.Core.Services;

public interface IDatabaseProvider
{
    SQLiteConnection SyncConnection { get; }
    SQLiteAsyncConnection AsyncConnection { get; }

    string GetDatabasePath();
}