﻿using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Volo.Abp.Collections;

namespace SharpAbp.Abp.Data.DbConnections
{
    public class AbpDataDbConnectionsOptions
    {
        public DbConnectionConfigurations DbConnections { get; }
        public HashSet<DatabaseProvider> DatabaseProviders { get; }
        public ITypeList<IDbConnectionCreator> Creators { get; }
        public AbpDataDbConnectionsOptions()
        {
            DbConnections = new DbConnectionConfigurations();
            DatabaseProviders = new HashSet<DatabaseProvider>();
            Creators = new TypeList<IDbConnectionCreator>();
        }


        public AbpDataDbConnectionsOptions Configure(IConfiguration configuration)
        {
            var dbConnectionConfigurations = configuration.GetSection("DbConnections")
                .Get<Dictionary<string, DbConnectionConfiguration>>();

            foreach (var dbConnectionConfigurationKv in dbConnectionConfigurations)
            {
                DbConnections.Configure(dbConnectionConfigurationKv.Key, c =>
                {
                    c.DatabaseProvider = dbConnectionConfigurationKv.Value.DatabaseProvider;
                    c.ConnectionString = dbConnectionConfigurationKv.Value.ConnectionString;
                });
            }

            var databaseProviders = configuration.GetSection("DatabaseProviders")?
                .Get<List<DatabaseProvider>>();
            if (databaseProviders != null)
            {
                foreach (var databaseProvider in databaseProviders)
                {
                    DatabaseProviders.AddIfNotContains(databaseProvider);
                }
            }

            return this;

        }
    }
}
