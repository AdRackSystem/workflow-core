using System;
using System.Linq;
using Microsoft.Extensions.Logging;
using WorkflowCore.Interface;
using WorkflowCore.LockProviders.SqlServer;
using WorkflowCore.Models;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlServerLocking(this IServiceCollection serviceCollection, string connectionString)
        {
            return serviceCollection.AddScoped<IDistributedLockProvider, SqlLockProvider>();
        }
    }
}
