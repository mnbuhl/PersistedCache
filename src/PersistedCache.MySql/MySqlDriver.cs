using System.Data.Common;
using MySql.Data.MySqlClient;
using PersistedCache.Sql;

namespace PersistedCache
{
    public class MySqlDriver : ISqlCacheDriver
    {
        private readonly MySqlPersistedCacheOptions _options;

        public MySqlDriver(MySqlPersistedCacheOptions options)
        {
            _options = options;
        }

        public string SetupStorageScript => 
            /*lang=MySQL*/
            $"""
             CREATE TABLE IF NOT EXISTS `{_options.TableName}` (
                 `key` VARCHAR(255) NOT NULL PRIMARY KEY,
                 `value` JSON NOT NULL,
                 `expiry` DATETIME(6) NOT NULL,
                 INDEX `idx_key_expiry` (`key`, `expiry`),
                 INDEX `idx_expiry` (`expiry`)
             ) ENGINE=InnoDB
               DEFAULT CHARSET=utf8
               COLLATE=utf8_general_ci;
             """;

        public string GetScript =>
            /*lang=MySQL*/
            $"""
            SELECT `value`
            FROM `{_options.TableName}`
            WHERE `key` = @Key
              AND `expiry` > @Expiry;
            """;
        
        public string SetScript =>
            /*lang=MySQL*/
            $"""
            INSERT INTO `{_options.TableName}` (`key`, `value`, `expiry`)
            VALUES (@Key, @Value, @Expiry)
            ON DUPLICATE KEY UPDATE `value` = @value, `expiry` = @expiry;
            """;

        public string HasScript =>
            /*lang=MySQL*/
            $"""
            SELECT COUNT(*)
            FROM `{_options.TableName}`
            WHERE `key` = @Key
              AND `expiry` > @Expiry;
            """;

        public string ForgetScript =>
            /*lang=MySQL*/
            $"""
            DELETE FROM `{_options.TableName}`
            WHERE `key` = @Key;
            """;

        public string FlushScript => 
            /*lang=MySQL*/ 
            $"DELETE FROM {_options.TableName};";

        public string FlushPatternScript =>
            /*lang=MySQL*/
            $"""
             DELETE FROM `{_options.TableName}`
             WHERE `key` LIKE @Pattern;
             """;

        public string PurgeScript => 
            /*lang=MySQL*/
            $"""
             DELETE FROM `{_options.TableName}`
             WHERE `expiry` <= @Expiry;
             """;

        public char Wildcard => '%';

        public DbConnection CreateConnection()
        {
            return new MySqlConnection(_options.ConnectionString);
        }
    }
}