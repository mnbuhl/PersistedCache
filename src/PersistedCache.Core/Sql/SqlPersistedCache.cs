using System.Text.Json;
using Dapper;

namespace PersistedCache.Sql;

public class SqlPersistedCache<TDriver> : IPersistedCache<TDriver> where TDriver : class, IDriver
{
    private readonly ISqlCacheDriver _driver;
    private readonly SqlPersistedCacheOptions _options;
    private readonly SqlConnectionFactory _connectionFactory;

    public SqlPersistedCache(ISqlCacheDriver driver, SqlPersistedCacheOptions options)
    {
        _driver = driver;
        _options = options;
        _connectionFactory = new SqlConnectionFactory(_driver);
        
        SqlMapper.AddTypeHandler(new ExpireTypeHandler());

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
        Validators.ValidateKey(key);
        Validators.ValidateValue(value);

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
    public async Task SetAsync<T>(string key, T value, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        Validators.ValidateKey(key);
        Validators.ValidateValue(value);

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
    public T? Get<T>(string key)
    {
        Validators.ValidateKey(key);
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
        Validators.ValidateKey(key);
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
        Validators.ValidateKey(key);
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

            Validators.ValidateValue(result);

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
    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> valueFactory, Expire expiry,
        CancellationToken cancellationToken = default)
    {
        Validators.ValidateKey(key);
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

            Validators.ValidateValue(result);

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
    public void Forget(string key)
    {
        Validators.ValidateKey(key);
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
        Validators.ValidateKey(key);
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
        Validators.ValidateKey(key);
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
        Validators.ValidateKey(key);
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
        Validators.ValidatePattern(pattern);
        pattern = pattern.Replace('*', _driver.Wildcard);

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
        Validators.ValidatePattern(pattern, new PatternValidatorOptions());
        pattern = pattern.Replace('*', _driver.Wildcard);

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
}