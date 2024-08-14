using Dapper;
using PersistedCache.MySql;
using PersistedCache.Sql;
using Testcontainers.MySql;

namespace PersistedCache.Tests.Fixtures;

[CollectionDefinition(nameof(MySqlFixture))]
public class MySqlFixture : ICollectionFixture<MySqlFixture>, IAsyncLifetime
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithDatabase("PersistedCache")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    public IPersistedCache PersistedCache { get; private set; } = null!;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new SqlPersistedCacheOptions(_container.GetConnectionString())
        {
            CreateTableIfNotExists = false,
            TableName = "persisted_cache",
        };
        
        var driver = new MySqlPersistedSqlCacheDriver(options);
        
        SetupStorage(driver);
        
        PersistedCache = new SqlPersistedCache(driver, options);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
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