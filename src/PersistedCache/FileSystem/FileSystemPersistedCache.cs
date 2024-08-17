using System.Text.Json;

namespace PersistedCache.FileSystem;

public class FileSystemPersistedCache : IPersistedCache<FileSystem>
{
    private readonly FileSystemPersistedCacheOptions _options;

    public FileSystemPersistedCache(FileSystemPersistedCacheOptions options)
    {
        _options = options;
        
        EnsureDirectoryExists();
    }

    public void Set<T>(string key, T value, Expire expiry)
    {
        var filePath = GetFilePath(key);

        var cacheEntry = new FileSystemCacheEntry<T>
        {
            Key = key,
            Value = value,
            Expiry = expiry
        };
        
        WriteToFile(filePath, cacheEntry);
    }

    public void SetForever<T>(string key, T value)
    {
        Set(key, value, Expire.Never);
    }

    public async Task SetAsync<T>(string key, T value, Expire expiry, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);

        var cacheEntry = new FileSystemCacheEntry<T>
        {
            Key = key,
            Value = value,
            Expiry = expiry
        };

        await WriteToFileAsync(filePath, cacheEntry, cancellationToken);
    }

    public async Task SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        await SetAsync(key, value, Expire.Never, cancellationToken);
    }

    public T? Get<T>(string key)
    {
        var filePath = GetFilePath(key);

        var cacheEntry = ReadFromFile<FileSystemCacheEntry<T>>(filePath);

        if (cacheEntry == null || cacheEntry.Expiry.IsExpired)
        {
            return default;
        }

        return cacheEntry.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);

        var cacheEntry = await ReadFromFileAsync<FileSystemCacheEntry<T>>(filePath, cancellationToken);

        if (cacheEntry == null || cacheEntry.Expiry.IsExpired)
        {
            return default;
        }

        return cacheEntry.Value;
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)
    {
        throw new NotImplementedException();
    }

    public T GetOrSetForever<T>(string key, Func<T> valueFactory)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Forget(string key)
    {
        throw new NotImplementedException();
    }

    public Task ForgetAsync(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public T? Pull<T>(string key)
    {
        throw new NotImplementedException();
    }

    public Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Flush()
    {
        throw new NotImplementedException();
    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Flush(string pattern)
    {
        throw new NotImplementedException();
    }

    public Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public void Purge()
    {
        throw new NotImplementedException();
    }
    
    private void EnsureDirectoryExists()
    {
        if (!Directory.Exists(_options.CacheFolderName))
        {
            Directory.CreateDirectory(_options.CacheFolderName);
        }
    }
    
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("The key must not be null or empty.", nameof(key));
        }
        
        var invalidChars = Path.GetInvalidFileNameChars();
        
        if (key.Any(invalidChars.Contains))
        {
            throw new ArgumentException("The key contains invalid characters.", nameof(key));
        }
        
        if (key.Length > 255)
        {
            throw new ArgumentException("The key must not be longer than 255 characters.", nameof(key));
        }
        
        if (key.Contains('/') || key.Contains('\\'))
        {
            throw new ArgumentException("The key must not contain path separators.", nameof(key));
        }
    }
    
    private async Task WriteToFileAsync<T>(string filePath, T value, CancellationToken cancellationToken)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await JsonSerializer.SerializeAsync(fileStream, value, _options.JsonOptions, cancellationToken);
        await fileStream.FlushAsync(cancellationToken);
    }
    
    private void WriteToFile<T>(string filePath, T value)
    {
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, false);
        JsonSerializer.Serialize(fileStream, value, _options.JsonOptions);
        fileStream.Flush();
    }
    
    private async Task<T?> ReadFromFileAsync<T>(string filePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        return await JsonSerializer.DeserializeAsync<T>(fileStream, _options.JsonOptions, cancellationToken);
    }
    
    private T? ReadFromFile<T>(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, false);
        return JsonSerializer.Deserialize<T>(fileStream, _options.JsonOptions);
    }
    
    private string GetFilePath(string key)
    {
        ValidateKey(key);
        
        return Path.Combine(_options.CacheFolderName, $"{key}.json");
    }
}