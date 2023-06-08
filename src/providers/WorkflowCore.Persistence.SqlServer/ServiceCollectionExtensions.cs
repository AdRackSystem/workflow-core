using System;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using WorkflowCore.Persistence.EntityFramework.Services;
using WorkflowCore.Persistence.SqlServer;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddWorkflowSqlServer(this IServiceCollection serviceCollection, string connectionString)
        {
            serviceCollection.AddDbContext<WorkflowDbContext, SqlServerContext>(c => c.UseSqlServer(connectionString));
            serviceCollection.AddScoped<IWorkflowPurger, WorkflowPurger>();

            return serviceCollection;
        }
    }
}
