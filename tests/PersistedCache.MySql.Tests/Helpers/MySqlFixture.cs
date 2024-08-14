﻿using Dapper;
using PersistedCache.Sql;
using Testcontainers.MySql;

namespace PersistedCache.MySql.Tests.Helpers;

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

    private static void SetupStorage(ISqlCacheDriver driver)
    {
        var connectionFactory = new SqlConnectionFactory(driver);

        connectionFactory.RunInTransaction((connection, transaction) =>
        {
            connection.Execute(driver.SetupStorageScript, transaction: transaction);
        });
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}