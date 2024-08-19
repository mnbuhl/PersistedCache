using System.Data.Common;
using Microsoft.Data.SqlClient;
using PersistedCache.Sql;

namespace PersistedCache.SqlServer;

public class SqlServerDriver : ISqlCacheDriver
{
    private readonly SqlServerPersistedCacheOptions _options;

    public SqlServerDriver(SqlServerPersistedCacheOptions options)
    {
        _options = options;
    }

    public string SetupStorageScript => 
        /*lang=TSQL*/
        $"""
         IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = '{_options.TableName}' AND schema_id = SCHEMA_ID('{_options.Schema}'))
         BEGIN
             CREATE TABLE [{_options.Schema}].[{_options.TableName}] (
                 [key] NVARCHAR(255) NOT NULL PRIMARY KEY,
                 [value] NVARCHAR(MAX) NOT NULL,  -- JSON is stored as NVARCHAR in SQL Server
                 [expiry] DATETIME2(6) NOT NULL,
                 INDEX idx_key_expiry ([key], [expiry]),
                 INDEX idx_expiry ([expiry])
             );
         END;
         """;
    
    public string GetScript =>
        /*lang=TSQL*/
        $"""
         SELECT [value]
         FROM [{_options.Schema}].[{_options.TableName}]
         WHERE [key] = @Key
           AND [expiry] > @Expiry;
         """;
    
    public string SetScript =>
        /*lang=TSQL*/
        $"""
         MERGE [{_options.Schema}].[{_options.TableName}] AS target
         USING (SELECT @Key AS [key], @Value AS [value], @Expiry AS [expiry]) AS source
         ON target.[key] = source.[key]
         WHEN MATCHED THEN 
             UPDATE SET [value] = source.[value], [expiry] = source.[expiry]
         WHEN NOT MATCHED THEN
             INSERT ([key], [value], [expiry])
             VALUES (source.[key], source.[value], source.[expiry]);
         """;
    
    public string ForgetScript =>
        /*lang=TSQL*/
        $"""
         DELETE FROM [{_options.Schema}].[{_options.TableName}]
         WHERE [key] = @Key;
         """;
    
    public string FlushScript => 
        /*lang=TSQL*/
        $"""DELETE FROM [{_options.Schema}].[{_options.TableName}];""";
    
    public string FlushPatternScript =>
        /*lang=TSQL*/
        $"""
         DELETE FROM [{_options.Schema}].[{_options.TableName}]
         WHERE [key] LIKE @Pattern ESCAPE '\';
         """;
    
    public string PurgeScript => 
        /*lang=TSQL*/
        $"""
         DELETE FROM [{_options.Schema}].[{_options.TableName}]
         WHERE [expiry] <= @Expiry;
         """;
    
    public char Wildcard => '%';
    
    public DbConnection CreateConnection()
    {
        return new SqlConnection(_options.ConnectionString);
    }
}