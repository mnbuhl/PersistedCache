using System.Data.Common;
using Microsoft.Data.Sqlite;
using PersistedCache.Sql;

namespace PersistedCache.Sqlite;

public class SqliteDriver : ISqlCacheDriver
{
    private readonly SqlitePersistedCacheOptions _options;

    public SqliteDriver(SqlitePersistedCacheOptions options)
    {
        _options = options;
    }

    public string SetupStorageScript => 
        /*lang=SQLite*/
        $"""
         CREATE TABLE IF NOT EXISTS "{_options.TableName}" (
             "key" TEXT NOT NULL PRIMARY KEY,
             "value" TEXT NOT NULL,  -- JSON is stored as TEXT in SQLite
             "expiry" TEXT NOT NULL
         );

         CREATE INDEX IF NOT EXISTS idx_key_expiry ON "{_options.TableName}" ("key", "expiry");
         CREATE INDEX IF NOT EXISTS idx_expiry ON "{_options.TableName}" ("expiry");
         """;
    
    public string GetScript =>
        /*lang=SQLite*/
        $"""
         SELECT "value"
         FROM "{_options.TableName}"
         WHERE "key" = @Key
           AND "expiry" > @Expiry;
         """;
    
    public string SetScript =>
        /*lang=SQLite*/
        $"""
         INSERT OR REPLACE INTO "{_options.TableName}" ("key", "value", "expiry")
         VALUES (@Key, @Value, @Expiry);
         """;
    
    public string ForgetScript =>
        /*lang=SQLite*/
        $"""
         DELETE FROM "{_options.TableName}"
         WHERE "key" = @Key;
         """;
    
    public string FlushScript => 
        /*lang=SQLite*/
        $"""
         DELETE FROM "{_options.TableName}";
         """;
    
    public string FlushPatternScript =>
        /*lang=SQLite*/
        $"""
         DELETE FROM "{_options.TableName}"
         WHERE "key" LIKE @Pattern ESCAPE '\';
         """;
    
    public string PurgeScript => 
        /*lang=SQLite*/
        $"""
         DELETE FROM "{_options.TableName}"
         WHERE "expiry" <= @Expiry;
         """;

    public char Wildcard => '%';
    
    public DbConnection CreateConnection()
    {
        return new SqliteConnection(_options.ConnectionString);
    }
}