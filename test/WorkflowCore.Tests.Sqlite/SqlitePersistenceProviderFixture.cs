﻿using System;
using Newtonsoft.Json;
using WorkflowCore.Interface;
using WorkflowCore.Persistence.EntityFramework;
using WorkflowCore.Persistence.EntityFramework.Services;
using WorkflowCore.Persistence.Sqlite;
using WorkflowCore.UnitTests;
using Xunit;

namespace WorkflowCore.Tests.Sqlite
{
    [Collection("Sqlite collection")]
    public class SqlitePersistenceProviderFixture : BasePersistenceFixture
    {
        string _connectionString;

        public SqlitePersistenceProviderFixture(SqliteSetup setup)
        {
            _connectionString = setup.ConnectionString;
        }

        protected override IPersistenceProvider Subject
        {
            get
            {                
                var db = new EntityFrameworkPersistenceProvider(new SqliteContextFactory(_connectionString), new ModelConverterService(new JsonSerializerSettings()),true, false);
                db.EnsureStoreExists();
                return db;
            }
        }
    }
}
