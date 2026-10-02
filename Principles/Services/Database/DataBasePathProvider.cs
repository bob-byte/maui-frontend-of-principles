namespace Principles.Services;

public class DatabasePathProvider : IDatabasePathProvider
{
    public string GetDatabasePath()
    {
        string folderPath = FileSystem.AppDataDirectory;
        string dbPath = Path.Combine( folderPath, "principles_app.db3" );
        return dbPath;
    }
}
