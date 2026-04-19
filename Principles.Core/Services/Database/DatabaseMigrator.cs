using System.Reflection;

namespace Principles.Core.Services;

public class DatabaseMigrator : IDatabaseMigrator
{
    public void Migrate()
    {
        IDatabaseConnectionProvider dbConnectionProvider = ServiceLocator.Current!.GetRequiredService<IDatabaseConnectionProvider>();
        SQLiteConnection sqlConnection = dbConnectionProvider.SyncConnection;

        if (!TryMigrateFromEmbeddedResources(sqlConnection))
        {
            TryMigrateFromFileSystem( sqlConnection );
        }
    }

    private static bool TryMigrateFromEmbeddedResources( SQLiteConnection sqlConnection )
    {
        Assembly assembly = typeof( DatabaseMigrator ).Assembly;
        string[] resourceNames = assembly.GetManifestResourceNames()
            .Where( name => name.Contains( ".Migrations." ) && name.EndsWith( ".sql", StringComparison.OrdinalIgnoreCase ) )
            .OrderBy( name => name )
            .ToArray();

        if (resourceNames.Length == 0)
        {
            return false;
        }

        foreach (string resourceName in resourceNames)
        {
            using Stream? stream = assembly.GetManifestResourceStream( resourceName );
            if (stream is null)
            {
                continue;
            }

            using StreamReader reader = new( stream );
            string sql = reader.ReadToEnd();
            ExecuteSqlScript( sqlConnection, sql );
        }

        return true;
    }

    private static void TryMigrateFromFileSystem( SQLiteConnection sqlConnection )
    {
        string migrationsFolderPath = Path.Combine( AppContext.BaseDirectory, "Migrations" );
        if (!Directory.Exists( migrationsFolderPath ))
        {
            throw new DirectoryNotFoundException( $"Migrations folder was not found: {migrationsFolderPath}" );
        }

        foreach (string file in Directory.GetFiles( migrationsFolderPath, "*.sql" ).OrderBy( f => f ))
        {
            string sql = File.ReadAllText( file );
            ExecuteSqlScript( sqlConnection, sql );
        }
    }

    private static void ExecuteSqlScript( SQLiteConnection sqlConnection, string sqlScript )
    {
        string[] statements = sqlScript
            .Split( ';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );

        foreach (string statement in statements)
        {
            if (!string.IsNullOrWhiteSpace( statement ))
            {
                sqlConnection.Execute( statement );
            }
        }
    }
}
