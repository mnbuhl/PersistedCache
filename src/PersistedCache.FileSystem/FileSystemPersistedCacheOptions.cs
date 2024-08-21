namespace PersistedCache;

public class FileSystemPersistedCacheOptions : PersistedCacheOptions
{
    public FileSystemPersistedCacheOptions(string path)
    {
        ValidatePath(path);
        CacheFolderName = path;
    }
    
    public string CacheFolderName { get; }

    private static void ValidatePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("The path must not be null or empty.", nameof(path));
        }

        var invalidChars = Path.GetInvalidPathChars();
        
        if (path.Any(invalidChars.Contains))
        {
            throw new ArgumentException("The path contains invalid characters.", nameof(path));
        }
    }
}