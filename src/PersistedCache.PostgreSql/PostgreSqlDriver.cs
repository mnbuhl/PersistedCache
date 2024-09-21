using System.Data.Common;
using Npgsql;
using PersistedCache.Sql;

namespace PersistedCache;

public class PostgreSqlDriver : ISqlCacheDriver
{
    private readonly PostgreSqlPersistedCacheOptions _options;

    public PostgreSqlDriver(PostgreSqlPersistedCacheOptions options)
    {
        _options = options;
    }
    
    public string SetupStorageScript => 
        /*lang=PostgreSQL*/
        $"""
          DO
          $$
          BEGIN
          CREATE TABLE IF NOT EXISTS "{_options.Schema}"."{_options.TableName}" (
              "key" VARCHAR(255) NOT NULL PRIMARY KEY,
              "value" JSONB NOT NULL,
              "expiry" TIMESTAMP(6) WITH TIME ZONE NOT NULL
          );
          CREATE INDEX IF NOT EXISTS idx_key_expiry ON "{_options.Schema}"."{_options.TableName}" ("key", "expiry");
          CREATE INDEX IF NOT EXISTS idx_expiry ON "{_options.Schema}"."{_options.TableName}" ("expiry");
          END;
          $$
          """;

    public string GetScript =>
        /*lang=PostgreSQL*/
        $"""
         SELECT "value"
         FROM "{_options.Schema}"."{_options.TableName}"
         WHERE "key" = @Key
           AND "expiry" > @Expiry;
         """;

    public string SetScript =>
        /*lang=PostgreSQL*/
        $"""
         INSERT INTO "{_options.Schema}"."{_options.TableName}" ("key", "value", "expiry")
         VALUES (@Key, cast(@Value as jsonb), @Expiry)
         ON CONFLICT ("key") DO UPDATE 
         SET "value" = cast(@Value as jsonb), "expiry" = @Expiry;
         """;

    public string QueryScript =>
        /*lang=PostgreSQL*/
        $"""
         SELECT "value"
         FROM "{_options.Schema}"."{_options.TableName}"
         WHERE "key" ILIKE @Pattern
           AND "expiry" > @Expiry;
         """;

    public string HasScript =>
        /*lang=PostgreSQL*/
        $"""
         SELECT COUNT(*)
         FROM "{_options.Schema}"."{_options.TableName}"
         WHERE "key" = @Key
           AND "expiry" > @Expiry;
         """;

    public string ForgetScript =>
        /*lang=PostgreSQL*/
        $"""
         DELETE FROM "{_options.Schema}"."{_options.TableName}"
         WHERE "key" = @Key;
         """;

    public string FlushScript => 
        /*lang=PostgreSQL*/ 
        $"""DELETE FROM "{_options.Schema}"."{_options.TableName}";""";

    public string FlushPatternScript =>
        /*lang=PostgreSQL*/
        $"""
         DELETE FROM "{_options.Schema}"."{_options.TableName}"
         WHERE "key" ILIKE @Pattern;
         """;

    public string PurgeScript => 
        /*lang=PostgreSQL*/
        $"""
         DELETE FROM "{_options.Schema}"."{_options.TableName}"
         WHERE "expiry" <= @Expiry;
         """;
    
    public char MultipleCharWildcard => '%';
    public char SingleCharWildcard => '_';

    public DbConnection CreateConnection()
    {
        return new NpgsqlConnection(_options.ConnectionString);
    }
}