using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

namespace PersistedCache.Sql
{
    public class SqlPersistedCache : IPersistedCache
    {
        private readonly ISqlCacheDriver _driver;
        private readonly SqlPersistedCacheOptions _options;
        private readonly SqlConnectionFactory _connectionFactory;

        public SqlPersistedCache(ISqlCacheDriver driver, SqlPersistedCacheOptions options)
        {
            _driver = driver;
            _options = options;
            _connectionFactory = new SqlConnectionFactory(_driver);

            if (options.CreateTableIfNotExists)
            {
                _connectionFactory.RunInTransaction((connection, transaction) =>
                {
                    connection.Execute(
                        _driver.SetupStorageScript,
                        transaction: transaction
                    );
                });

                options.CreateTableIfNotExists = false;
            }
        }

        /// <inheritdoc />
        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = GetExpiryDate(expiry)
            };

            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.SetScript,
                    entry,
                    transaction
                );
            });
        }

        /// <inheritdoc />
        public void SetForever<T>(string key, T value)
        {
            Set(key, value, TimeSpan.MaxValue);
        }

        /// <inheritdoc />
        public async Task SetAsync<T>(string key, T value, TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = GetExpiryDate(expiry)
            };

            await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
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

        /// <inheritdoc />
        public Task SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            return SetAsync(key, value, TimeSpan.MaxValue, cancellationToken);
        }

        /// <inheritdoc />
        public T Get<T>(string key)
        {
            return _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                var res = connection.QueryFirstOrDefault<string>(
                    _driver.GetScript,
                    new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction
                );

                return !string.IsNullOrWhiteSpace(res)
                    ? JsonSerializer.Deserialize<T>(res, _options.JsonOptions)
                    : default;
            });
        }

        /// <inheritdoc />
        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
            {
                var res = await connection.QueryFirstOrDefaultAsync<string>(
                    new CommandDefinition(
                        _driver.GetScript,
                        new { Key = key, Expiry = DateTimeOffset.UtcNow },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );

                return !string.IsNullOrWhiteSpace(res)
                    ? JsonSerializer.Deserialize<T>(res, _options.JsonOptions)
                    : default;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan expiry)
        {
            return _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                var value = connection.QueryFirstOrDefault<string>(
                    _driver.GetScript,
                    new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction
                );

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return JsonSerializer.Deserialize<T>(value, _options.JsonOptions);
                }

                var result = valueFactory();

                var entry = new PersistedCacheEntry
                {
                    Key = key,
                    Value = JsonSerializer.Serialize(result, _options.JsonOptions),
                    Expiry = GetExpiryDate(expiry)
                };

                connection.Execute(
                    _driver.SetScript,
                    entry,
                    transaction
                );

                return result;
            });
        }

        /// <inheritdoc />
        public T GetOrSetForever<T>(string key, Func<T> valueFactory)
        {
            return GetOrSet(key, valueFactory, TimeSpan.MaxValue);
        }

        /// <inheritdoc />
        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            return await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
            {
                var value = await connection.QueryFirstOrDefaultAsync<string>(
                    new CommandDefinition(
                        _driver.GetScript,
                        new { Key = key, Expiry = DateTimeOffset.UtcNow },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );

                if (!string.IsNullOrWhiteSpace(value))
                {
                    return JsonSerializer.Deserialize<T>(value, _options.JsonOptions);
                }

                var result = await valueFactory();

                var entry = new PersistedCacheEntry
                {
                    Key = key,
                    Value = JsonSerializer.Serialize(result, _options.JsonOptions),
                    Expiry = GetExpiryDate(expiry)
                };

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        _driver.SetScript,
                        entry,
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );

                return result;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory, CancellationToken cancellationToken = default)
        {
            return GetOrSetAsync(key, valueFactory, TimeSpan.MaxValue, cancellationToken);
        }

        /// <inheritdoc />
        public void Forget(string key)
        {
            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.ForgetScript,
                    new { Key = key },
                    transaction
                );
            });
        }

        /// <inheritdoc />
        public Task ForgetAsync(string key, CancellationToken cancellationToken = default)
        {
            return _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        _driver.ForgetScript,
                        new { Key = key },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );
            }, cancellationToken);
        }

        /// <inheritdoc />
        public T Pull<T>(string key)
        {
            return _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                var value = connection.QueryFirstOrDefault<string>(
                    new CommandDefinition(
                        _driver.GetScript,
                        new { Key = key, Expiry = DateTimeOffset.UtcNow },
                        transaction
                    )
                );

                connection.Execute(
                    _driver.ForgetScript,
                    new { Key = key },
                    transaction
                );

                return !string.IsNullOrWhiteSpace(value)
                    ? JsonSerializer.Deserialize<T>(value, _options.JsonOptions)
                    : default;
            });
        }

        /// <inheritdoc />
        public async Task<T> PullAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
            {
                var value = await connection.QueryFirstOrDefaultAsync<string>(
                    new CommandDefinition(
                        _driver.GetScript,
                        new { Key = key, Expiry = DateTimeOffset.UtcNow },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );

                await connection.ExecuteAsync(
                    new CommandDefinition(
                        _driver.ForgetScript,
                        new { Key = key },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );

                return !string.IsNullOrWhiteSpace(value)
                    ? JsonSerializer.Deserialize<T>(value, _options.JsonOptions)
                    : default;
            }, cancellationToken);
        }

        /// <inheritdoc />
        public void Flush()
        {
            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.FlushScript,
                    transaction
                );
            });
        }

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        _driver.FlushScript,
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );
            }, cancellationToken);
        }

        /// <inheritdoc />
        public void Flush(string pattern)
        {
            pattern = ValidatePattern(pattern);
            
            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.FlushPatternScript,
                    new { Pattern = pattern },
                    transaction
                );
            });
        }

        /// <inheritdoc />
        public Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
        {
            pattern = ValidatePattern(pattern);
            
            return _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        _driver.FlushPatternScript,
                        new { Pattern = pattern },
                        transaction,
                        cancellationToken: cancellationToken
                    )
                );
            }, cancellationToken);
        }

        /// <inheritdoc />
        public void Purge()
        {
            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.PurgeScript,
                    new { Expiry = DateTimeOffset.UtcNow },
                    transaction
                );
            });
        }

        private string ValidatePattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern cannot be empty", nameof(pattern));
            }
            
            if (!pattern.StartsWith("*") && !pattern.EndsWith("*"))
            {
                throw new ArgumentException("Pattern must begin or end with a '*'", nameof(pattern));
            }
            
            return pattern.Replace('*', _driver.Wildcard);
        }
        
        private static DateTimeOffset GetExpiryDate(TimeSpan expiry)
        {
            return expiry == TimeSpan.MaxValue
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.UtcNow.Add(expiry);
        }
    }
}