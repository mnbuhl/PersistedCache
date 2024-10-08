﻿using System.Collections.Generic;
using Dapper;
using PersistedCache.Tests.Common;
using Testcontainers.MsSql;
using Xunit;

namespace PersistedCache.Tests.Fixtures;

[CollectionDefinition(nameof(SqlServerFixture))]
public class SqlServerFixture : BaseDatabaseFixture<SqlServerDriver>, ICollectionFixture<SqlServerFixture>
{
    public SqlServerFixture() : base(
        new MsSqlBuilder()
            .WithPassword("Password123!")
            .Build())
    {
    }

    public override IEnumerable<CacheEntry> GetCacheEntries()
    {
        using var connection = Driver.CreateConnection();
        return connection.Query<CacheEntry>($"SELECT * FROM [{TestConstants.TableName}]");
    }

    public override CacheEntry? GetCacheEntry(string key)
    {
        using var connection = Driver.CreateConnection();
        return connection.QueryFirstOrDefault<CacheEntry>(
            $"SELECT * FROM [{TestConstants.TableName}] WHERE [key] = @Key",
            new { Key = key });
    }
}