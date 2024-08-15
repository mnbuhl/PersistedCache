﻿using System.Data.Common;
using Npgsql;
using PersistedCache.Sql;

namespace PersistedCache.PostgreSql;

public class PostgreSqlCacheDriver : ISqlCacheDriver
{
    private readonly SqlPersistedCacheOptions _options;

    public PostgreSqlCacheDriver(SqlPersistedCacheOptions options)
    {
        _options = options;
    }
    
    public string SetupStorageScript => 
        /*lang=PostgreSQL*/
        $"""
          DO
          $$
          BEGIN
          CREATE TABLE IF NOT EXISTS "{_options.TableName}" (
              "key" VARCHAR(255) NOT NULL PRIMARY KEY,
              "value" JSONB NOT NULL,
              "expiry" TIMESTAMP(6) WITH TIME ZONE NOT NULL
          );
          CREATE INDEX IF NOT EXISTS idx_key_expiry ON "{_options.TableName}" ("key", "expiry");
          CREATE INDEX IF NOT EXISTS idx_expiry ON "{_options.TableName}" ("expiry");
          END;
          $$
          """;

    public string GetScript =>
        /*lang=PostgreSQL*/
        $"""
         SELECT "value"
         FROM "{_options.TableName}"
         WHERE "key" = @Key
           AND "expiry" > @Expiry;
         """;

    public string SetScript =>
        /*lang=PostgreSQL*/
        $"""
         INSERT INTO "{_options.TableName}" ("key", "value", "expiry")
         VALUES (@Key, to_json(@Value), @Expiry)
         ON CONFLICT ("key") DO UPDATE 
         SET "value" = to_json(@Value), "expiry" = @Expiry;
         """;

    public string ForgetScript =>
        /*lang=PostgreSQL*/
        $"""
         DELETE FROM "{_options.TableName}"
         WHERE "key" = @Key;
         """;

    public string FlushScript => 
        /*lang=PostgreSQL*/ 
        $"""DELETE FROM "{_options.TableName}";""";

    public string FlushPatternScript =>
        /*lang=PostgreSQL*/
        $"""
         DELETE FROM "{_options.TableName}"
         WHERE "key" ILIKE @Pattern;
         """;

    public string PurgeScript => 
        /*lang=PostgreSQL*/
        $"""
         DELETE FROM "{_options.TableName}"
         WHERE "expiry" <= @Expiry;
         """;
    
    public char Wildcard => '%';
    
    public DbConnection CreateConnection()
    {
        return new NpgsqlConnection(_options.ConnectionString);
    }
}