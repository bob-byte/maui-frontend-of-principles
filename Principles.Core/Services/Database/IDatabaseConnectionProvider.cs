namespace Principles.Core.Services;

public interface IDatabaseConnectionProvider
{
    SQLiteConnection SyncConnection { get; }
    SQLiteAsyncConnection AsyncConnection { get; }
}
