using System;
using System.Text.Json;

namespace PersistedCache
{
    public class PersistedCacheOptions
    {
        private string _tableName = "persisted_cache";
        private TimeSpan _purgeInterval = TimeSpan.FromHours(24);

        public PersistedCacheOptions(string connectionString)
        {
            ConnectionString = !string.IsNullOrWhiteSpace(connectionString) 
                ? connectionString 
                : throw new ArgumentException("Connection string cannot be null or empty.");
        }
        
        public string ConnectionString { get; }
        public bool PurgeExpiredEntries { get; set; } = true;
        public bool CreateTableIfNotExists { get; set; } = true;
        public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions();

        public string TableName
        {
            get => _tableName; 
            set => _tableName = !string.IsNullOrWhiteSpace(value) 
                ? value 
                : throw new ArgumentException("Table name cannot be null or empty.");
        }

        public TimeSpan PurgeInterval
        {
            get => _purgeInterval; 
            set => _purgeInterval = value < TimeSpan.Zero 
                ? throw new ArgumentException("Purge interval must be positive.") 
                : value;
        }
    }
}