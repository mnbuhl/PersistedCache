using System;

namespace PersistedCache
{
    public abstract class PersistedCacheOptions
    {
        private string _tableName = "persisted_cache";
        private TimeSpan _purgeInterval = TimeSpan.FromHours(24);

        protected PersistedCacheOptions(string connectionString)
        {
            ConnectionString = !string.IsNullOrWhiteSpace(connectionString) 
                ? connectionString 
                : throw new ArgumentException("Connection string cannot be null or empty.");
        }
        
        public virtual string ConnectionString { get; }
        public virtual bool PurgeExpiredEntries { get; set; } = true;

        public virtual string TableName
        {
            get => _tableName; 
            set => _tableName = !string.IsNullOrWhiteSpace(value) 
                ? value 
                : throw new ArgumentException("Table name cannot be null or empty.");
        }

        public virtual TimeSpan PurgeInterval
        {
            get => _purgeInterval; 
            set => _purgeInterval = value < TimeSpan.Zero 
                ? throw new ArgumentException("Purge interval must be positive.") 
                : value;
        }
    }
}