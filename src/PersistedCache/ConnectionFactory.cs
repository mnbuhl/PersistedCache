using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PersistedCache
{
    internal class ConnectionFactory
    {
        private readonly ICacheDriver _driver;

        public ConnectionFactory(ICacheDriver driver)
        {
            _driver = driver;
        }
        
        public void RunInConnection(Action<IDbConnection> action)
        {
            using var connection = _driver.CreateConnection();
            connection.Open();

            action(connection);
        }
        
        public T RunInConnection<T>(Func<IDbConnection, T> action)
        {
            using var connection = _driver.CreateConnection();
            connection.Open();

            return action(connection);
        }
        
        public async Task RunInConnectionAsync(Func<IDbConnection, Task> action,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await action(connection);
        }
        
        public async Task<T> RunInConnectionAsync<T>(
            Func<IDbConnection, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            await using var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            return await action(connection);
        }

        public void RunInTransaction(Action<IDbConnection, IDbTransaction> action)
        {
            var connection = _driver.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            action(connection, transaction);

            transaction.Commit();
        }

        public T RunInTransaction<T>(Func<IDbConnection, IDbTransaction, T> action)
        {
            var connection = _driver.CreateConnection();
            connection.Open();

            using var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            var result = action(connection, transaction);

            transaction.Commit();

            return result;
        }

        public async Task RunInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> action,
            CancellationToken cancellationToken = default)
        {
            var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            await using var transaction =
                await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            await action(connection, transaction);

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task<T> RunInTransactionAsync<T>(
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