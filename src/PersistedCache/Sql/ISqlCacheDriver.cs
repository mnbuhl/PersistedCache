using System.Data.Common;

namespace PersistedCache.Sql;

public interface ISqlCacheDriver
{
    /// <summary>
    /// The script to setup the storage (i.e. create the table)
    /// </summary>
    string SetupStorageScript { get; }
        
    /// <summary>
    /// The script to get a value from the storage
    /// </summary>
    string GetScript { get; }
        
    /// <summary>
    /// The script to set a value in the storage
    /// </summary>
    string SetScript { get; }
        
    /// <summary>
    /// The script to forget a value in the storage
    /// </summary>
    string ForgetScript { get; }
        
    /// <summary>
    /// The script to flush the storage
    /// </summary>
    string FlushScript { get; }
        
    /// <summary>
    /// The script to flush the storage with a pattern
    /// </summary>
    string FlushPatternScript { get; }
        
    /// <summary>
    /// The script to purge the storage for expired entries
    /// </summary>
    string PurgeScript { get; }
        
    /// <summary>
    /// The wildcard character for the storage
    /// </summary>
    char Wildcard { get; }
        
    /// <summary>
    /// Create a connection to the storage
    /// </summary>
    /// <returns></returns>
    DbConnection CreateConnection();
}