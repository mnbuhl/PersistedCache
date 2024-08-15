using Dapper;
using DotNet.Testcontainers.Containers;
using PersistedCache.Sql;

namespace PersistedCache.Tests.Common;

public abstract class BaseDatabaseFixture<TDriver> : IAsyncLifetime where TDriver : ISqlCacheDriver
{
    public IPersistedCache PersistedCache { get; private set; } = null!;

    protected DockerContainer Container = null!;

    private ISqlCacheDriver _driver = null!;
    
    public async Task InitializeAsync()
    {
        await Container.StartAsync();
        
        var options = new SqlPersistedCacheOptions((Container as IDatabaseContainer)!.GetConnectionString())
        {
            CreateTableIfNotExists = false,
            TableName = TestConstants.TableName,
        };

        var driver = (TDriver)Activator.CreateInstance(typeof(TDriver), options)!;
        
        SetupStorage(driver);
        
        _driver = driver;
        PersistedCache = new SqlPersistedCache(driver, options);
    }

    public async Task DisposeAsync()
    {
        await Container.DisposeAsync();
    }
    
    public IEnumerable<dynamic> ExecuteSql(string sql)
    {
        using var connection = _driver.CreateConnection();
        var result = connection.Query(sql);
        return result;
    }
    
    private static void SetupStorage(ISqlCacheDriver driver)
    {
        var connectionFactory = new SqlConnectionFactory(driver);

        connectionFactory.RunInTransaction((connection, transaction) =>
        {
            connection.Execute(driver.SetupStorageScript, transaction: transaction);
        });
    }
}