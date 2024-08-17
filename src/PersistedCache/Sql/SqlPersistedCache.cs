using System.Text.Json;
using Dapper;

namespace PersistedCache.Sql;

public class SqlPersistedCache : IPersistedCache
{
    private readonly ISqlCacheDriver _driver;
    private readonly ISqlPersistedCacheOptions _options;
    private readonly SqlConnectionFactory _connectionFactory;

    public SqlPersistedCache(ISqlCacheDriver driver, ISqlPersistedCacheOptions options)
    {
        _driver = driver;
        _options = options;
        _connectionFactory = new SqlConnectionFactory(_driver);

        if (options.CreateTableIfNotExists)
        {
            _connectionFactory.RunInTransaction((connection, transaction) =>
            {
                connection.Execute(
                    new CommandDefinition(
                        commandText: _driver.SetupStorageScript,
                        transaction: transaction
                    )
                );
            });

            options.CreateTableIfNotExists = false;
        }
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value, Expire expiry)
    {
        ValidateKey(key);
        ValidateValue(value);

        var entry = new PersistedCacheEntry
        {
            Key = key,
            Value = JsonSerializer.Serialize(value, _options.JsonOptions),
            Expiry = expiry
        };

        _connectionFactory.RunInTransaction((connection, transaction) =>
        {
            connection.Execute(
                new CommandDefinition(
                    commandText: _driver.SetScript,
                    parameters: entry,
                    transaction: transaction
                )
            );
        });
    }

    /// <inheritdoc />
    public void SetForever<T>(string key, T value)
    {
        Set(key, value, Expire.Never);
    }

    /// <inheritdoc />
    public async Task SetAsync<T>(string key, T value, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        ValidateValue(value);

        var entry = new PersistedCacheEntry
        {
            Key = key,
            Value = JsonSerializer.Serialize(value, _options.JsonOptions),
            Expiry = expiry
        };

        await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    commandText: _driver.SetScript,
                    parameters: entry,
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );
        }, cancellationToken);
    }

    /// <inheritdoc />
    public Task SetForeverAsync<T>(string key, T value, CancellationToken cancellationToken = default)
    {
        return SetAsync(key, value, Expire.Never, cancellationToken);
    }

    /// <inheritdoc />
    public T? Get<T>(string key)
    {
        ValidateKey(key);
        return _connectionFactory.RunInTransaction((connection, transaction) =>
        {
            var res = connection.QueryFirstOrDefault<string>(
                new CommandDefinition(
                    commandText: _driver.GetScript,
                    parameters: new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction
                )
            );

            return !string.IsNullOrWhiteSpace(res)
                ? JsonSerializer.Deserialize<T>(res!, _options.JsonOptions)
                : default;
        });
    }

    /// <inheritdoc />
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        return await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
        {
            var res = await connection.QueryFirstOrDefaultAsync<string>(
                new CommandDefinition(
                    commandText: _driver.GetScript,
                    parameters: new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );

            return !string.IsNullOrWhiteSpace(res)
                ? JsonSerializer.Deserialize<T>(res!, _options.JsonOptions)
                : default;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public T GetOrSet<T>(string key, Func<T> valueFactory, Expire expiry)
    {
        ValidateKey(key);
        return _connectionFactory.RunInTransaction((connection, transaction) =>
        {
            var value = connection.QueryFirstOrDefault<string>(
                new CommandDefinition(
                    commandText: _driver.GetScript,
                    parameters: new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction
                )
            );

            if (!string.IsNullOrWhiteSpace(value))
            {
                return JsonSerializer.Deserialize<T>(value!, _options.JsonOptions);
            }

            var result = valueFactory();

            ValidateValue(result);

            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(result, _options.JsonOptions),
                Expiry = expiry
            };

            connection.Execute(
                new CommandDefinition(
                    commandText: _driver.SetScript,
                    parameters: entry,
                    transaction: transaction
                )
            );

            return result;
        })!;
    }

    /// <inheritdoc />
    public T GetOrSetForever<T>(string key, Func<T> valueFactory)
    {
        return GetOrSet(key, valueFactory, Expire.Never);
    }

    /// <inheritdoc />
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        var result = await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
        {
            var value = await connection.QueryFirstOrDefaultAsync<string>(
                new CommandDefinition(
                    commandText: _driver.GetScript,
                    parameters: new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );

            if (!string.IsNullOrWhiteSpace(value))
            {
                return JsonSerializer.Deserialize<T>(value!, _options.JsonOptions);
            }

            var result = await valueFactory();

            ValidateValue(result);

            var entry = new PersistedCacheEntry
            {
                Key = key,
                Value = JsonSerializer.Serialize(result, _options.JsonOptions),
                Expiry = expiry
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    commandText: _driver.SetScript,
                    parameters: entry,
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );

            return result;
        }, cancellationToken);

        return result!;
    }

    /// <inheritdoc />
    public Task<T> GetOrSetForeverAsync<T>(string key, Func<Task<T>> valueFactory,
        CancellationToken cancellationToken = default)
    {
        return GetOrSetAsync(key, valueFactory, Expire.Never, cancellationToken);
    }

    /// <inheritdoc />
    public void Forget(string key)
    {
        ValidateKey(key);
        _connectionFactory.RunInTransaction((connection, transaction) =>
        {
            connection.Execute(
                new CommandDefinition(
                    commandText: _driver.ForgetScript,
                    parameters: new { Key = key },
                    transaction: transaction
                )
            );
        });
    }

    /// <inheritdoc />
    public Task ForgetAsync(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        return _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    commandText: _driver.ForgetScript,
                    parameters: new { Key = key },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );
        }, cancellationToken);
    }

    /// <inheritdoc />
    public T? Pull<T>(string key)
    {
        ValidateKey(key);
        return _connectionFactory.RunInTransaction((connection, transaction) =>
        {
            var value = connection.QueryFirstOrDefault<string>(
                new CommandDefinition(
                    commandText: _driver.GetScript,
                    parameters: new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction
                )
            );

            connection.Execute(
                new CommandDefinition(
                    commandText: _driver.ForgetScript,
                    parameters: new { Key = key },
                    transaction: transaction
                )
            );

            return !string.IsNullOrWhiteSpace(value)
                ? JsonSerializer.Deserialize<T>(value!, _options.JsonOptions)
                : default;
        });
    }

    /// <inheritdoc />
    public async Task<T?> PullAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        ValidateKey(key);
        return await _connectionFactory.RunInTransactionAsync(async (connection, transaction) =>
        {
            var value = await connection.QueryFirstOrDefaultAsync<string>(
                new CommandDefinition(
                    commandText: _driver.GetScript,
                    parameters: new { Key = key, Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );

            await connection.ExecuteAsync(
                new CommandDefinition(
                    commandText: _driver.ForgetScript,
                    parameters: new { Key = key },
                    transaction: transaction,
                    cancellationToken: cancellationToken
                )
            );

            return !string.IsNullOrWhiteSpace(value)
                ? JsonSerializer.Deserialize<T>(value!, _options.JsonOptions)
                : default;
        }, cancellationToken);
    }

    /// <inheritdoc />
    public void Flush()
    {
        _connectionFactory.RunInTransaction((connection, transaction) =>
        {
            connection.Execute(
                new CommandDefinition(
                    commandText: _driver.FlushScript,
                    transaction: transaction
                )
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
                    commandText: _driver.FlushScript,
                    transaction: transaction,
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
                new CommandDefinition(
                    commandText: _driver.FlushPatternScript,
                    parameters: new { Pattern = pattern },
                    transaction: transaction
                )
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
                    commandText: _driver.FlushPatternScript,
                    parameters: new { Pattern = pattern },
                    transaction: transaction,
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
                new CommandDefinition(
                    commandText: _driver.PurgeScript,
                    parameters: new { Expiry = DateTimeOffset.UtcNow },
                    transaction: transaction
                )
            );
        });
    }

    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Key cannot be empty", nameof(key));
        }

        if (key.Length > 255)
        {
            throw new ArgumentException("Key length cannot exceed 255 characters", nameof(key));
        }
    }

    private static void ValidateValue<T>(T value)
    {
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value), "Value cannot be null");
        }
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
}