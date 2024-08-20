# Persisted Cache

Persisted Cache is a simple caching library that allows you to turn any stateful resource into a key-value store. 
It is designed to be simple and easy to use, meanwhile it spares you the hassle and costs of managing a separate cache server.

### Why would you use this?

* Your team doesn't want to manage a separate cache server.
* You need a distributed cache that can be shared across multiple instances of your application.
* You need a cache that can be persisted to disk.
* You need a cache that can be shared across multiple applications.

### How to use it?

Install the specific package for the resource you want to use. For example, if you want to use MySQL as the cache store, you would install the `PersistedCache.MySql` package. (Installing the base package is not necessary)

```bash
dotnet add package PersistedCache.MySql
```
Or simply add it from the NuGet package gallery.


Currently supported resources are:
* [x] `MySQL` - [PersistedCache.MySql](https://www.nuget.org/packages/PersistedCache.MySql)
* [x] `PostgreSQL` - [PersistedCache.PostgreSql](https://www.nuget.org/packages/PersistedCache.PostgreSql)
* [x] `SQL Server` - [PersistedCache.SqlServer](https://www.nuget.org/packages/PersistedCache.SqlServer)
* [x] `File System` - [PersistedCache.FileSystem](https://www.nuget.org/packages/PersistedCache.FileSystem)
* [x] `SQLite` - [PersistedCache.Sqlite](https://www.nuget.org/packages/PersistedCache.Sqlite)
* [x] `MongoDB` - [PersistedCache.MongoDb](https://www.nuget.org/packages/PersistedCache.MongoDb)
* [ ] `Version 1.0` milestone

The reason that the version does not match is due to semantic versioning. 
The base package is versioned independently of the resource packages. All packages are on the latest version, regardless of the version number.

### Add the service to your DI container

```csharp
services.AddMySqlPersistedCache("Your connection string here", options =>
{
    // The name of the table to use for the cache
    options.TableName = "persisted_cache";
    
    // Can be set to false after the table is created (or if you want to manage the table yourself)
    options.CreateTableIfNotExists = true; 
    
    // Purges expired entries based on configured expiry interval
    options.PurgeExpiredEntries = true;
    
    // The interval at which the cache purges expired entries
    options.PurgeInterval = TimeSpan.FromHours(24);
    
    // If you need to serialize/deserialize objects differently
    options.JsonOptions = new JsonSerializerOptions()
});

```
All the options shown above are optional and have default values, only the connection string is required.

### Use the cache

```csharp
public class MyService(IPersistedCache cache)
{    
    // Set a value in the cache
    public void SetSomething()
    {
        cache.Set("my-key", "some value", Expire.InMinutes(5));
    }

    // Get a value from the cache
    public string? GetSomething()
    {
        return cache.Get<string>("my-key");
    }
    
    // Get a value from the cache or set it if it doesn't exist
    public void GetOrSetSomething()
    {
        var value = cache.GetOrSet("my-key", () => new RandomObject(), Expire.InMinutes(5));
    }
    
    // Forget a value from the cache
    public void ForgetSomething()
    {
        cache.Forget("my-key");
    }
    
    // Flush values matching a pattern from the cache
    public void FlushPattern()
    {
        cache.Flush("my-*");
    }
    
    // Flush all values from the cache
    public void FlushAll()
    {
        cache.Flush();
    }
    
    // Get a value from the cache and remove it
    public RandomObject? PullSomething()
    {
        return cache.Pull<RandomObject>("my-key");
    }
    
    // Purge the cache of expired entries
    public void PurgeCache()
    {
        cache.Purge();
    }
    
    // Set a value in the cache asynchronously
    public async Task SetSomethingAsync()
    {
        await cache.SetForeverAsync("my-async-key", new RandomObject());
    }
    
    // Get a value from the cache asynchronously
    public async Task<RandomObject?> GetSomethingAsync()
    {
        return await cache.GetAsync<RandomObject>("my-async-key");
    }
    
    // Get a value from the cache or set it if it doesn't exist asynchronously
    public async Task<RandomObject?> GetOrSetSomethingAsync()
    {
        return await cache.GetOrSetAsync("my-async-key", async () => await GetRandomObjectAsync());
    }
}
```

Be aware when using value types, as the cache will return the default value if the key does not exist instead of null, unless you use a nullable value in the generic type.

### Use more than one persisted cache

If you need to use more than one Persisted Cache, you can use the driver based injection.

```csharp
services.AddMySqlPersistedCache("Your connection string here");
services.AddFileSystemPersistedCache("Your path here");
```

Then you can inject the cache you need in your service.

```csharp
public class MyService(
    IPersistedCache<MySqlDriver> mySqlCache,
    IPersistedCache<FileSystemDriver> fileSystemCache
)  {
    public void SetSomething()
    {
        mySqlCache.Set("my-key", "some value", Expire.InMinutes(5));
        fileSystemCache.Set("my-key", "some value", Expire.InMinutes(5));
    }
}
```

The first cache registered will be the default cache, so you can use the `IPersistedCache` interface without specifying the driver.

### Methods

| Method                                                                                                                   | Description                                                                     |
|--------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `Set<T>(string key, T value, Expire expiry)`                                                                             | Set a value in the cache with an expiry time                                    |
| `SetForever<T>(string key, T value)`                                                                                     | Set a value in the cache forever                                                |
| `SetAsync<T>(string key, T value, Expire expiry, CancellationToken cancellationToken = default)`                         | Set a value in the cache with an expiry time asynchronously                     |
| `SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default)`                                 | Set a value in the cache forever asynchronously                                 |
| `Get<T>(string key)`                                                                                                     | Get a value from the cache                                                      |
| `GetAsync<T>(string key, CancellationToken cancellationToken = default)`                                                 | Get a value from the cache asynchronously                                       |
| `GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)`                                                           | Get a value from the cache or set it if it doesn't exist                        |
| `GetOrSetForever<T>(string key, Func<T> valueFactory)`                                                                   | Get a value from the cache or set it if it doesn't exist forever                |
| `GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry, CancellationToken cancellationToken = default)` | Get a value from the cache or set it if it doesn't exist asynchronously         |
| `GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory, CancellationToken cancellationToken = default)`         | Get a value from the cache or set it if it doesn't exist forever asynchronously |
| `Forget(string key)`                                                                                                     | Forget a value from the cache                                                   |
| `ForgetAsync(string key, CancellationToken cancellationToken = default)`                                                 | Forget a value from the cache asynchronously                                    |
| `Pull<T>(string key)`                                                                                                    | Get a value from the cache and remove it                                        |
| `PullAsync<T>(string key, CancellationToken cancellationToken = default)`                                                | Get a value from the cache and remove it asynchronously                         |
| `Flush()`                                                                                                                | Flush all values from the cache                                                 |
| `FlushAsync(CancellationToken cancellationToken = default)`                                                              | Flush all values from the cache asynchronously                                  |
| `Flush(string pattern)`                                                                                                  | Flush values from the cache by pattern                                          |
| `FlushAsync(string pattern, CancellationToken cancellationToken = default)`                                              | Flush values from the cache by pattern asynchronously                           |
| `Purge()`                                                                                                                | Purge the cache of expired entries                                              |


### Want to contribute?

If you want to contribute to this project, please feel free to open an issue or a pull request.

Want to make your own adapter? Add the `PersistedCache` package to your project and implement the `IPersistedCache` interface.