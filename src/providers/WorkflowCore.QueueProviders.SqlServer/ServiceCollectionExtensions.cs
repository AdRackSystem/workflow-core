#region using

using System;
using WorkflowCore.Interface;
using WorkflowCore.QueueProviders.SqlServer.Interfaces;
using WorkflowCore.QueueProviders.SqlServer.Services;

#endregion

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerBroker(this IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddWorkflowSqlServer(connectionString);
            serviceCollection.AddScoped<IQueueConfigProvider, QueueConfigProvider>();
            serviceCollection.AddScoped<ISqlCommandExecutor, SqlCommandExecutor>();
            serviceCollection.AddScoped<ISqlServerQueueProviderMigrator, SqlServerQueueProviderMigrator>();

            serviceCollection.AddScoped<IQueueProvider, SqlServerQueueProvider>();

            return serviceCollection;
        }
    }
}