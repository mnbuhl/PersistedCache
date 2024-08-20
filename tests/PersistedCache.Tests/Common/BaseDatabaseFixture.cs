using System;
using System.Threading.Tasks;
using Dapper;
using DotNet.Testcontainers.Containers;
using PersistedCache.Sql;
using Xunit;

namespace PersistedCache.Tests.Common;

public abstract class BaseDatabaseFixture<TDriver> : BaseFixture, IAsyncLifetime
    where TDriver : class, ISqlCacheDriver, IDriver
{
    public override IPersistedCache PersistedCache { get; protected set; }

    protected DockerContainer Container;
    protected ISqlCacheDriver Driver => _driver;
    protected string ConnectionString;

    private ISqlCacheDriver _driver;

    public async Task InitializeAsync()
    {
        if (Container != null)
        {
            await Container.StartAsync();
            ConnectionString = (Container as IDatabaseContainer).GetConnectionString();
        }
            
        var options = GetOptions(ConnectionString);
        var driver = (TDriver)Activator.CreateInstance(typeof(TDriver), options);

        SetupStorage(driver);

        _driver = driver;
        PersistedCache = new SqlPersistedCache<TDriver>(driver, options);
    }

    public async Task DisposeAsync()
    {
        if (Container != null)
        {
            await Container.DisposeAsync();
        }
    }

    private static void SetupStorage(ISqlCacheDriver driver)
    {
        var connectionFactory = new SqlConnectionFactory(driver);

        connectionFactory.RunInTransaction((connection, transaction) =>
        {
            connection.Execute(driver.SetupStorageScript, transaction: transaction);
        });
    }

    private static SqlPersistedCacheOptions GetOptions(string connectionString)
    {
        SqlPersistedCacheOptions options;

        switch (typeof(TDriver))
        {
            case { } type when type == typeof(SqlServerDriver):
                options = new SqlServerPersistedCacheOptions(connectionString);
                break;
            case { } type when type == typeof(MySqlDriver):
                options = new MySqlPersistedCacheOptions(connectionString);
                break;
            case { } type when type == typeof(PostgreSqlDriver):
                options = new PostgreSqlPersistedCacheOptions(connectionString);
                break;
            case { } type when type == typeof(SqliteDriver):
                options = new SqlitePersistedCacheOptions(connectionString);
                break;
            default:
                throw new ArgumentException("Invalid driver type.");
        }

        options.TableName = TestConstants.TableName;
        options.CreateTableIfNotExists = false;

        return options;
    }
}