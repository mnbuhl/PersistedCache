using System;
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
        private readonly ConnectionFactory _connectionFactory;

        public PersistedCache(ICacheDriver driver, PersistedCacheOptions options)
        {
            _driver = driver;
            _options = options;
            _connectionFactory = new ConnectionFactory(_driver);

            if (options.CreateTableIfNotExists)
            {
                _connectionFactory.RunInConnection((connection) =>
                {
                    connection.Execute(
                        _driver.SetupStorageScript
                    );
                });

                options.CreateTableIfNotExists = false;
            }
        }

        public void Set<T>(string key, T value, TimeSpan expiry)
        {
            var expiryDate = expiry == TimeSpan.MaxValue
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.UtcNow.Add(expiry);
            
            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = expiryDate
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

        public void SetForever<T>(string key, T value)
        {
            Set(key, value, TimeSpan.MaxValue);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            var expiryDate = expiry == TimeSpan.MaxValue
                ? DateTimeOffset.MaxValue
                : DateTimeOffset.UtcNow.Add(expiry);
            
            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(value, _options.JsonOptions),
                Expiry = expiryDate
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

        public Task SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default)
        {
            return SetAsync(key, value, TimeSpan.MaxValue, cancellationToken);
        }

        public T Get<T>(string key)
        {
            return _connectionFactory.RunInConnection(connection =>
            {
                var res = connection.QueryFirstOrDefault<string>(
                    _driver.GetScript,
                    new { Key = key }
                );

                return !string.IsNullOrWhiteSpace(res)
                    ? JsonSerializer.Deserialize<T>(res, _options.JsonOptions)
                    : default;
            });
        }

        public async Task<T> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            return await _connectionFactory.RunInConnectionAsync(async connection =>
            {
                var res = await connection.QueryFirstOrDefaultAsync<string>(
                    new CommandDefinition(
                        _driver.GetScript,
                        new { Key = key },
                        cancellationToken: cancellationToken
                    )
                );

                return !string.IsNullOrWhiteSpace(res)
                    ? JsonSerializer.Deserialize<T>(res, _options.JsonOptions)
                    : default;
            }, cancellationToken);
        }

        public T GetOrSet<T>(string key, Func<T> valueFactory, TimeSpan expiry)
        {
            var value = Get<T>(key);

            if (value != null)
            {
                return value;
            }

            value = valueFactory();
            Set(key, value, expiry);

            return value;
        }

        public T GetOrSetForever<T>(string key, Func<T> valueFactory)
        {
            return GetOrSet(key, valueFactory, TimeSpan.MaxValue);
        }

        public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, TimeSpan expiry,
            CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);

            if (value != null)
            {
                return value;
            }

            value = await valueFactory();
            await SetAsync(key, value, expiry, cancellationToken);

            return value;
        }

        public Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory, CancellationToken cancellationToken = default)
        {
            return GetOrSetAsync(key, valueFactory, TimeSpan.MaxValue, cancellationToken);
        }

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

        public T Pull<T>(string key)
        {
            var value = Get<T>(key);
            Forget(key);

            return value;
        }

        public async Task<T> PullAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            var value = await GetAsync<T>(key, cancellationToken);
            await ForgetAsync(key, cancellationToken);
            
            return value;
        }

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

        public void Flush(string pattern)
        {
            ValidatePattern(pattern);
            
            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    _driver.FlushPatternScript,
                    new { Pattern = pattern },
                    transaction
                );
            });
        }

        public Task FlushAsync(string pattern, CancellationToken cancellationToken = default)
        {
            ValidatePattern(pattern);
            
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
        
        private static void ValidatePattern(string pattern)
        {
            if (string.IsNullOrWhiteSpace(pattern))
            {
                throw new ArgumentException("Pattern cannot be empty", nameof(pattern));
            }
            
            if (!pattern.Contains("*"))
            {
                throw new ArgumentException("Pattern must begin or end with a '*'", nameof(pattern));
            }
        }
    }
}