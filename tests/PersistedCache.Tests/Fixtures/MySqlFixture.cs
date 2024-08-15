using Dapper;
using PersistedCache.MySql;
using PersistedCache.Sql;
using Testcontainers.MySql;

namespace PersistedCache.Tests.Fixtures;

[CollectionDefinition(nameof(MySqlFixture))]
public class MySqlFixture : ICollectionFixture<MySqlFixture>, IAsyncLifetime
{
    public IPersistedCache PersistedCache { get; private set; } = null!;
    
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithDatabase("PersistedCache")
        .WithUsername("root")
        .WithPassword("root")
        .Build();

    private ISqlCacheDriver _driver = null!;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new SqlPersistedCacheOptions(_container.GetConnectionString())
        {
            CreateTableIfNotExists = false,
            TableName = TestConstants.TableName,
        };
        
        var driver = new MySqlPersistedSqlCacheDriver(options);
        
        SetupStorage(driver);
        
        _driver = driver;
        PersistedCache = new SqlPersistedCache(driver, options);
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
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