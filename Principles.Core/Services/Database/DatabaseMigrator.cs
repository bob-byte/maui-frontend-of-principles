using System.Reflection;

namespace Principles.Core.Services;

public class DatabaseMigrator : IDatabaseMigrator
{
    public void Migrate()
    {
        IDatabaseConnectionProvider dbConnectionProvider = ServiceLocator.Current!.GetRequiredService<IDatabaseConnectionProvider>();
        SQLiteConnection sqlConnection = dbConnectionProvider.SyncConnection;
            
        ISettingsService settingsService = ServiceLocator.Current.GetRequiredService<ISettingsService>();
        if (settingsService.IsDebug)
        {
            // Content-based
            string migrationsFolderPath = Path.Combine( AppContext.BaseDirectory, "Migrations" );

            foreach (string file in Directory.GetFiles( migrationsFolderPath, "*.sql" ).OrderBy( f => f ))
            {
                string sql = File.ReadAllText( file );
                sqlConnection.Execute( sql );
            }
        }
        else
        {
            Assembly assembly = typeof( DatabaseMigrator ).Assembly;
            string path = Path.Combine( AppContext.BaseDirectory, "Migrations" );
            IOrderedEnumerable<string> resourceNames = assembly.GetManifestResourceNames()
                .Where( name => name.StartsWith( path ) && name.EndsWith( ".sql" ) )
                .OrderBy( name => name );

            foreach (string resourceName in resourceNames)
            {
                using Stream? stream = assembly.GetManifestResourceStream( resourceName );
                using var reader = new StreamReader( stream! );
                string sql = reader.ReadToEnd();
                sqlConnection.Execute( sql );
            }
        }
    }
}