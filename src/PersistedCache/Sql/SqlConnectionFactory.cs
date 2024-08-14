using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace PersistedCache.Sql
{
    internal class SqlConnectionFactory
    {
        private readonly ISqlCacheDriver _driver;

        public SqlConnectionFactory(ISqlCacheDriver driver)
        {
            _driver = driver;
        }

        public void RunInTransaction(Action<IDbConnection, IDbTransaction> action)
        {
            var connection = _driver.CreateConnection();
            connection.Open();

            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                action(connection, transaction);
                transaction.Commit();
            }
        }

        public T RunInTransaction<T>(Func<IDbConnection, IDbTransaction, T> action)
        {
            var connection = _driver.CreateConnection();
            connection.Open();

            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var result = action(connection, transaction);

                transaction.Commit();

                return result;
            }
        }

        public async Task RunInTransactionAsync(Func<IDbConnection, IDbTransaction, Task> action,
            CancellationToken cancellationToken = default)
        {
            var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                await action(connection, transaction);
                transaction.Commit();
            }
        }

        public async Task<T> RunInTransactionAsync<T>(
            Func<IDbConnection, IDbTransaction, Task<T>> action,
            CancellationToken cancellationToken = default)
        {
            var connection = _driver.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            using (var transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted))
            {
                var result = await action(connection, transaction);
                transaction.Commit();

                return result;
            }
        }
    }
}