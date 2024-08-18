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

        var cacheEntry = ReadFromFile<T>(filePath);

        if (cacheEntry == null)
        {
            return default;
        }

        if (cacheEntry.Expiry.IsExpired)
        {
            Task.Run(() => Forget(key));
            return default;
        }

        return cacheEntry.Value;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);

        var cacheEntry = await ReadFromFileAsync<T>(filePath, cancellationToken: cancellationToken);

        if (cacheEntry == null)
        {
            return default;
        }

        if (cacheEntry.Expiry.IsExpired)
        {
            await Task.Run(() => Forget(key), cancellationToken);
            return default;
        }

        return cacheEntry.Value;
    }

    public T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)
    {
        var value = Get<T>(key);

        if (value != null)
        {
            return value;
        }

        value = valueFactory();

        Set(key, value, expiry);

        return value;
    }

    public T GetOrSetForever<T>(string key, Func<T> valueFactory)
    {
        return GetOrSet(key, valueFactory, Expire.Never);
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        var value = await GetAsync<T>(key, cancellationToken);

        if (value != null)
        {
            return value;
        }

        value = await valueFactory();

        await SetAsync(key, value, expiry, cancellationToken);

        return value;
    }

    public async Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory,
        CancellationToken cancellationToken = default)
    {
        return await GetOrSetAsync(key, valueFactory, Expire.Never, cancellationToken);
    }

    public void Forget(string key)
    {
        var filePath = GetFilePath(key);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    public Task ForgetAsync(string key, CancellationToken cancellationToken = default)
    {
        Forget(key);
        return Task.CompletedTask;
    }

    public T? Pull<T>(string key)
    {
        var filePath = GetFilePath(key);
        
        var cacheEntry = ReadFromFile<T>(filePath, deleteOnClose: true);
        
        if (cacheEntry == null || cacheEntry.Expiry.IsExpired)
        {
            return default;
        }
        
        return cacheEntry.Value;
    }

    public async Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var filePath = GetFilePath(key);
        
        var cacheEntry = await ReadFromFileAsync<T>(filePath, deleteOnClose: true, cancellationToken);
        
        if (cacheEntry == null || cacheEntry.Expiry.IsExpired)
        {
            return default;
        }
        
        return cacheEntry.Value;
    }

    public void Flush()
    {
        var directory = new DirectoryInfo(_options.CacheFolderName);
        
        foreach (var file in directory.EnumerateFiles())
        {
            file.Delete();
        }
    }

    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        Flush();
        return Task.CompletedTask;
    }

    public void Flush(string pattern)
    {
        var directory = new DirectoryInfo(_options.CacheFolderName);
        
        foreach (var file in directory.EnumerateFiles(pattern))
        {
            file.Delete();
        }
    }

    public Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
    {
        Flush(pattern);
        return Task.CompletedTask;
    }

    public void Purge()
    {
        var directory = new DirectoryInfo(_options.CacheFolderName);
        
        foreach (var file in directory.EnumerateFiles())
        {
            var cacheEntry = ReadFromFile<FileSystemCacheEntry<string>>(file.FullName);

            if (cacheEntry == null || cacheEntry.Expiry.IsExpired)
            {
                file.Delete();
            }
        }
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
        using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        JsonSerializer.Serialize(fileStream, value, _options.JsonOptions);
        fileStream.Flush();
    }

    private async Task<FileSystemCacheEntry<T>?> ReadFromFileAsync<T>(string filePath, bool deleteOnClose = false,
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }

        var fileOptions = deleteOnClose 
            ? FileOptions.DeleteOnClose | FileOptions.Asynchronous 
            : FileOptions.Asynchronous;

        using var fileStream =
            new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, fileOptions);
        return await JsonSerializer.DeserializeAsync<FileSystemCacheEntry<T>>(fileStream, _options.JsonOptions, cancellationToken);
    }

    private FileSystemCacheEntry<T>? ReadFromFile<T>(string filePath, bool deleteOnClose = false)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        var fileOptions = deleteOnClose 
            ? FileOptions.DeleteOnClose | FileOptions.Asynchronous 
            : FileOptions.Asynchronous;

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, fileOptions);
        return JsonSerializer.Deserialize<FileSystemCacheEntry<T>>(fileStream, _options.JsonOptions);
    }

    private string GetFilePath(string key)
    {
        ValidateKey(key);

        return Path.Combine(_options.CacheFolderName, $"{key}.json");
    }
}