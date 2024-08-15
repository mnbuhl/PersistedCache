﻿using System;
using Microsoft.Extensions.DependencyInjection;
using PersistedCache.Sql;

namespace PersistedCache.PostgreSql
{
    public static class PostgreSqlPersistedCacheExtensions
    {
        /// <summary>
        /// Adds a MySql persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the MySql database.</param>
        public static IServiceCollection AddPostgreSqlPersistedCache(this IServiceCollection services,
            string connectionString)
        {
            return services.AddPostgreSqlPersistedCache(new SqlPersistedCacheOptions(connectionString));
        }

        /// <summary>
        /// Adds a MySql persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="connectionString">Connection string to the MySql database.</param>
        /// <param name="configure">Action to configure the cache options.</param>
        public static IServiceCollection AddPostgreSqlPersistedCache(this IServiceCollection services,
            string connectionString, Action<SqlPersistedCacheOptions> configure)
        {
            var options = new SqlPersistedCacheOptions(connectionString);
            configure(options);

            return services.AddPostgreSqlPersistedCache(options);
        }

        /// <summary>
        /// Adds a MySql persisted cache to the service collection.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="options">Options for the cache.</param>
        public static IServiceCollection AddPostgreSqlPersistedCache(this IServiceCollection services,
            SqlPersistedCacheOptions options)
        {
            services.AddSingleton(options);
            services.AddSingleton<ISqlCacheDriver, PostgreSqlPersistedCacheDriver>();
            services.AddSingleton<IPersistedCache, SqlPersistedCache>();

            if (options.PurgeExpiredEntries)
            {
                services.AddHostedService<SqlPurgeCacheBackgroundJob>();
            }

            return services;
        }
    }
}