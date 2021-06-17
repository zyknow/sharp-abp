﻿using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Data;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SharpAbp.Abp.Data.DbConnections
{
    public class DbConnectionCreateService : IDbConnectionCreateService, ITransientDependency
    {
        protected AbpDataDbConnectionsOptions Options { get; }
        protected IServiceProvider ServiceProvider { get; }
        protected IDbConnectionInfoResolver DbConnectionInfoResolver { get; }
        public DbConnectionCreateService(
            IOptions<AbpDataDbConnectionsOptions> options,
            IServiceProvider serviceProvider,
            IDbConnectionInfoResolver dbConnectionInfoResolver)
        {
            Options = options.Value;
            ServiceProvider = serviceProvider;
            DbConnectionInfoResolver = dbConnectionInfoResolver;
        }

        /// <summary>
        /// Create DbConnection
        /// </summary>
        /// <param name="dbConnectionName"></param>
        /// <returns></returns>
        [NotNull]
        public virtual async Task<IDbConnection> CreateAsync([NotNull] string dbConnectionName)
        {
            Check.NotNullOrWhiteSpace(dbConnectionName, nameof(dbConnectionName));
            var dbConnectionInfo = await DbConnectionInfoResolver.ResolveAsync(dbConnectionName);
            if (dbConnectionInfo == null)
            {
                throw new AbpException($"Could not find DbConnectionInfo by dbConnectionName '{dbConnectionName}'.");
            }
            if (!Options.DatabaseProviders.Contains(dbConnectionInfo.DatabaseProvider))
            {
                throw new AbpException($"Database '{dbConnectionInfo.DatabaseProvider}' not support.");
            }

            using var scope = ServiceProvider.CreateScope();
            var creators = scope.ServiceProvider.GetServices<IDbConnectionCreator>();
            if (creators != null)
            {
                foreach (var creator in creators)
                {
                    if (creator.IsMatch(dbConnectionName, dbConnectionInfo))
                    {
                        return await creator.CreateAsync(dbConnectionName, dbConnectionInfo);
                    }
                }
            }

            //InternalDbConnectionCreator
            var internalCreators = scope.ServiceProvider.GetServices<IInternalDbConnectionCreator>();
            foreach (var internalCreator in internalCreators)
            {
                if (internalCreator.DatabaseProvider == dbConnectionInfo.DatabaseProvider)
                {
                    return internalCreator.Create(dbConnectionInfo);
                }
            }

            throw new AbpException("Could not find any creator to create DbConnection.");
        }
    }
}
