using System.Data.Common;
using MySql.Data.MySqlClient;
using PersistedCache.Sql;

namespace PersistedCache.MySql
{
    public class MySqlPersistedSqlCacheDriver : ISqlCacheDriver
    {
        private readonly SqlPersistedCacheOptions _options;

        public MySqlPersistedSqlCacheDriver(SqlPersistedCacheOptions options)
        {
            _options = options;
        }

        public string SetupStorageScript => $@"
            CREATE TABLE IF NOT EXISTS {_options.TableName} (
                `key` VARCHAR(255) NOT NULL PRIMARY KEY,
                `value` JSON NOT NULL,
                `expiry` DATETIME(6) NOT NULL,
                INDEX `idx_key_expiry` (`key`, `expiry`),
                INDEX `idx_expiry` (`expiry`)
            ) ENGINE=InnoDB
              DEFAULT CHARSET=utf8
              COLLATE=utf8_general_ci;";

        public string GetScript => $@"
            SELECT `value`
            FROM {_options.TableName}
            WHERE `key` = @Key
              AND `expiry` > @Expiry;";
        
        public string SetScript => $@"
            INSERT INTO {_options.TableName} (`key`, `value`, `expiry`)
            VALUES (@Key, @Value, @Expiry)
            ON DUPLICATE KEY UPDATE `value` = @value, `expiry` = @expiry;";
        
        public string ForgetScript => $@"
            DELETE FROM {_options.TableName}
            WHERE `key` = @Key;";

        public string FlushScript => $@"
            DELETE FROM {_options.TableName};";

        public string FlushPatternScript => $@"
            DELETE FROM {_options.TableName}
            WHERE `key` LIKE @Pattern;";

        public string PurgeScript => $@"
            DELETE FROM {_options.TableName}
            WHERE `expiry` <= @Expiry;";

        public char Wildcard => '%';

        public DbConnection CreateConnection()
        {
            return new MySqlConnection(_options.ConnectionString);
        }
    }
}