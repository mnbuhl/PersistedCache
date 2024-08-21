using System.Text.Json;

namespace PersistedCache;

internal class FileSystemPersistedCache : IPersistedCache<FileSystemDriver>
{
    private readonly FileSystemPersistedCacheOptions _options;

    public FileSystemPersistedCache(FileSystemPersistedCacheOptions options)
    {
        _options = options;

        EnsureDirectoryExists();
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, Expire expiry)
    {
        ValidateKey(key);
        Validators.ValidateValue(value);
        
        var filePath = GetFilePath(key);

        var cacheEntry = new PersistedCacheEntry<T>
        {
            Key = key,
            Value = value,
            Expiry = expiry
        };

        WriteToFile(filePath, cacheEntry);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, Expire expiry, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        Validators.ValidateValue(value);
        
        var filePath = GetFilePath(key);

        var cacheEntry = new PersistedCacheEntry<T>
        {
            Key = key,
            Value = value,
            Expiry = expiry
        };

        await WriteToFileAsync(filePath, cacheEntry, cancellationToken);
    }

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);

        var cacheEntry = ReadFromFile<T>(filePath);

        if (cacheEntry == null)
        {
            return default;
        }

        if (cacheEntry.IsExpired)
        {
            Task.Run(() => Forget(key));
            return default;
        }

        return cacheEntry.Value;
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);

        var cacheEntry = await ReadFromFileAsync<T>(filePath, cancellationToken: cancellationToken);

        if (cacheEntry == null)
        {
            return default;
        }

        if (cacheEntry.IsExpired)
        {
            await Task.Run(() => Forget(key), cancellationToken);
            return default;
        }

        return cacheEntry.Value;
    }

    /// <inheritdoc />
    public T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)
    {
        ValidateKey(key);
        var value = Get<T>(key);

        if (value != null)
        {
            return value;
        }

        value = valueFactory();

        Validators.ValidateValue(value);
        Set(key, value, expiry);

        return value;
    }

    /// <inheritdoc />
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var value = await GetAsync<T>(key, cancellationToken);

        if (value != null)
        {
            return value;
        }

        value = await valueFactory();

        Validators.ValidateValue(value);
        await SetAsync(key, value, expiry, cancellationToken);

        return value;
    }

    public bool Has(string key)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);
        var cacheEntry = ReadFromFile<object>(filePath);
        
        return cacheEntry is { IsExpired: false };
    }

    public async Task<bool> HasAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);
        var cacheEntry = await ReadFromFileAsync<object>(filePath, cancellationToken: cancellationToken);
        
        return cacheEntry is { IsExpired: false };
    }

    /// <inheritdoc />
    public void Forget(string key)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    /// <inheritdoc />
    public Task ForgetAsync(string key, CancellationToken cancellationToken = default)
    {
        Forget(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public T? Pull<T>(string key)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);
        
        var cacheEntry = ReadFromFile<T>(filePath, deleteOnClose: true);
        
        if (cacheEntry == null || cacheEntry.IsExpired)
        {
            return default;
        }
        
        return cacheEntry.Value;
    }

    /// <inheritdoc />
    public async Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var filePath = GetFilePath(key);
        
        var cacheEntry = await ReadFromFileAsync<T>(filePath, deleteOnClose: true, cancellationToken);
        
        if (cacheEntry == null || cacheEntry.IsExpired)
        {
            return default;
        }
        
        return cacheEntry.Value;
    }

    /// <inheritdoc />
    public void Flush()
    {
        var directory = new DirectoryInfo(_options.CacheFolderName);
        
        foreach (var file in directory.EnumerateFiles())
        {
            file.Delete();
        }
    }

    /// <inheritdoc />
    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        Flush();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Flush(string pattern)
    {
        Validators.ValidatePattern(pattern, new PatternValidatorOptions { SupportedWildcards = ["*", "?"] });
        
        var directory = new DirectoryInfo(_options.CacheFolderName);
        
        foreach (var file in directory.EnumerateFiles(pattern))
        {
            file.Delete();
        }
    }

    /// <inheritdoc />
    public Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
    {
        Flush(pattern);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public void Purge()
    {
        var directory = new DirectoryInfo(_options.CacheFolderName);
        
        foreach (var file in directory.EnumerateFiles())
        {
            var cacheEntry = ReadFromFile<object>(file.FullName);

            if (cacheEntry == null || cacheEntry.IsExpired)
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
        Validators.ValidateKey(key, new KeyValidatorOptions
        {
            InvalidChars = Path.GetInvalidFileNameChars(),
            MaxLength = 255
        });
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

    private async Task<PersistedCacheEntry<T>?> ReadFromFileAsync<T>(string filePath, bool deleteOnClose = false,
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
        return await JsonSerializer.DeserializeAsync<PersistedCacheEntry<T>>(fileStream, _options.JsonOptions, cancellationToken);
    }

    private PersistedCacheEntry<T>? ReadFromFile<T>(string filePath, bool deleteOnClose = false)
    {
        if (!File.Exists(filePath))
        {
            return default;
        }
        
        var fileOptions = deleteOnClose 
            ? FileOptions.DeleteOnClose | FileOptions.Asynchronous 
            : FileOptions.Asynchronous;

        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, fileOptions);
        return JsonSerializer.Deserialize<PersistedCacheEntry<T>>(fileStream, _options.JsonOptions);
    }

    private string GetFilePath(string key)
    {
        ValidateKey(key);

        return Path.Combine(_options.CacheFolderName, $"{key}.json");
    }
}