using System;
using System.Text.Json;

namespace PersistedCache.Sql
{
    public class SqlPersistedCacheOptions
    {
        private TimeSpan _purgeInterval = TimeSpan.FromHours(24);
        private string _tableName = "persisted_cache";

        public SqlPersistedCacheOptions(string connectionString)
        {
            ConnectionString = !string.IsNullOrWhiteSpace(connectionString) 
                ? connectionString 
                : throw new ArgumentException("Connection string cannot be null or empty.");
        }
        
        /// <summary>
        /// The name of the table to use for the cache.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when the table name is null or empty.</exception>
        public string TableName
        {
            get => _tableName; 
            set => _tableName = !string.IsNullOrWhiteSpace(value) 
                ? value 
                : throw new ArgumentException("Table name cannot be null or empty.");
        }
        
        /// <summary>
        /// Whether to create the table if it does not exist.
        /// </summary>
        public bool CreateTableIfNotExists { get; set; } = true;
        
        /// <summary>
        /// The connection string to use for the cache.
        /// </summary>
        public string ConnectionString { get; }
        
        /// <summary>
        /// Whether to purge expired entries.
        /// </summary>
        public bool PurgeExpiredEntries { get; set; } = true;
        
        /// <summary>
        /// The interval at which to purge expired entries.
        /// </summary>
        public TimeSpan PurgeInterval
        {
            get => _purgeInterval; 
            set => _purgeInterval = value < TimeSpan.Zero 
                ? throw new ArgumentException("Purge interval must be positive.") 
                : value;
        }
        
        /// <summary>
        /// The options to use for JSON serialization.
        /// </summary>
        public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions();

        
    }
}