using System.Text.Json;

namespace PersistedCache;

public abstract class PersistedCacheOptions
{
    private TimeSpan _purgeInterval = TimeSpan.FromHours(24);

    
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
        set => _purgeInterval = value <= TimeSpan.Zero 
            ? throw new ArgumentException("Purge interval must be greater than zero.")
            : value;
    }
    
    /// <summary>
    /// The options to use for JSON serialization.
    /// </summary>
    public JsonSerializerOptions JsonOptions { get; set; } = new JsonSerializerOptions();
}