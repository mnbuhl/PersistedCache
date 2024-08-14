using System;
using System.Data;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace PersistedCache
{
    public class PersistedCache : IPersistedCache
    {
        private readonly ICacheDriver _driver;
        private readonly PersistedCacheOptions _options;

        public PersistedCache(ICacheDriver driver, PersistedCacheOptions options)
        {
            _driver = driver;
            _options = options;

            if (options.CreateTableIfNotExists)
            {
                RunInTransaction((connection, transaction) =>
                {
                    connection.Execute(
                        _driver.SetupStorageScript,
                        transaction
                    );
                });

                options.CreateTableIfNotExists = false;
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = DateTimeOffset.UtcNow.Add(expiry)
            };

            RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.SetScript,
                    entry,
                    transaction
                );
            });
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = DateTimeOffset.UtcNow.Add(expiry)
            };

            await RunInTransactionAsync(async (connection, transaction) =>
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        _driver.SetScript,
                        entry,
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );
            }, cancellationToken);
        }

        public T Get<T>(string key)
        {
            return RunInTransaction((connection, transaction) =>
            {
                var res = connection.QueryFirstOrDefault<string>(
                    _driver.GetScript,
                    new { Key = key },
                    transaction
                );

                return !string.IsNullOrWhiteSpace(res)
                    ? JsonSerializer.Deserialize<T>(res, _options.JsonOptions)
                    : default;
            });
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return await RunInTransactionAsync(async (connection, transaction) =>
            {
                var res = await connection.QueryFirstOrDefaultAsync<string>(
                    new CommandDefinition(
                        _driver.GetScript,
                        new { Key = key },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );

                return !string.IsNullOrWhiteSpace(res)
                    ? JsonSerializer.Deserialize<T>(res, _options.JsonOptions)
                    : default;
            }, cancellationToken);
        }

        private void RunInTransaction(Action<IDbConnection, IDbTransaction> action)
        {
            var connection = _driver.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            action(connection, transaction);

            transaction.Commit();
        }

        private T RunInTransaction<T>(Func<IDbConnection, IDbTransaction, T> action)
        {
            var connection = _driver.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            var result = action(connection, transaction);

            transaction.Commit();

            return result;
        }

        private async Task RunInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> action,
            CancellationToken cancellationToken = default)
        {
            var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            await action(connection, transaction);

            await transaction.CommitAsync(cancellationToken);
        }

        private async Task<T> RunInTransactionAsync<T>(
            Func<IDbConnection, IDbTransaction, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
            
            var result = await action(connection, transaction);

            await transaction.CommitAsync(cancellationToken);

            return result;
        }
    }
}