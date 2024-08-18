﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using DotNet.Testcontainers.Containers;
using PersistedCache.MySql;
using PersistedCache.PostgreSql;
using PersistedCache.Sql;
using PersistedCache.SqlServer;
using Xunit;

namespace PersistedCache.Tests.Common
{
    public abstract class BaseDatabaseFixture<TDriver> : IAsyncLifetime where TDriver : ISqlCacheDriver
    {
        public IPersistedCache PersistedCache { get; private set; }

        protected DockerContainer Container;
        protected ISqlCacheDriver Driver => _driver;

        private ISqlCacheDriver _driver;
    
        public async Task InitializeAsync()
        {
            await Container.StartAsync();
            
            var connectionString = (Container as IDatabaseContainer).GetConnectionString();
            var options = GetOptions(connectionString);
            var driver = (TDriver)Activator.CreateInstance(typeof(TDriver), options);
        
            SetupStorage(driver);
        
            _driver = driver;
            PersistedCache = new SqlPersistedCache(driver, options);
        }

        public async Task DisposeAsync()
        {
            await Container.DisposeAsync();
        }

        // @TODO: Change object to PersistedCacheEntry
        public abstract IEnumerable<object> GetCacheEntries();
        
        // @TODO: Change object to PersistedCacheEntry
        public abstract object GetCacheEntry(string key);
    
        private static void SetupStorage(ISqlCacheDriver driver)
        {
            var connectionFactory = new SqlConnectionFactory(driver);

            connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(driver.SetupStorageScript, transaction: transaction);
            });
        }
    
        private static ISqlPersistedCacheOptions GetOptions(string connectionString)
        {
            ISqlPersistedCacheOptions options = new SqlPersistedCacheOptions(connectionString);

            switch (typeof(TDriver))
            {
                case Type type when type == typeof(SqlServerCacheDriver):
                    options = new SqlServerPersistedCacheOptions(connectionString);
                    break;
                case Type type when type == typeof(MySqlCacheDriver):
                    options = new SqlPersistedCacheOptions(connectionString);
                    break;
                case Type type when type == typeof(PostgreSqlCacheDriver):
                    options = new PostgreSqlPersistedCacheOptions(connectionString);
                    break;
            }
        
            options.TableName = TestConstants.TableName;
            options.CreateTableIfNotExists = false;

            return options;
        }
    }
}