using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Threading.Tasks;
using CustomPage.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CustomPage.HangfireExtensions.ManagementPage
{
    public class SaveDispatcher : IDashboardDispatcher
    {
        private readonly IServiceProvider _applicationServices;
        private readonly IEnumerable<TableConfiguration> _tableConfigurations;

        public SaveDispatcher(IServiceProvider applicationServices, IEnumerable<TableConfiguration> tableConfigurations)
        {
            _applicationServices = applicationServices;
            _tableConfigurations = tableConfigurations;
        }

        public Task Dispatch([NotNull] DashboardContext context)
        {
            var data = context.Request.GetFormValuesAsync("data").Result;
            var tableName = context.Request.GetFormValuesAsync("tableName").Result.SingleOrDefault();
            
            if (tableName == null || data == null)
            {
                return Task.CompletedTask;
            }

            var tableConfig = _tableConfigurations.FirstOrDefault(x => x.Name == tableName);

            if (tableConfig == null)
            {
                return Task.CompletedTask;
            }

            using (var scope = _applicationServices.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<ApplicationDbContext>().Database;
                database.OpenConnection();
                var dbConnection = database.GetDbConnection();
                var transaction = dbConnection.BeginTransaction();
                try
                {

                    using (var command = dbConnection.CreateCommand())
                    {
                        command.Transaction = transaction;
                        command.CommandText = $"Delete from {tableName}";
                        command.ExecuteNonQuery();
                    }

                    foreach (var row in data)
                    {
                        var insertStatement = tableConfig.InsertBuilder(row);
                        using (var command = dbConnection.CreateCommand())
                        {
                            command.Transaction = transaction;
                            command.CommandText = $"Insert into {tableName} Values ({insertStatement})";
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch(Exception ex)
                {
                    transaction.Rollback();
                }
            }

            return Task.CompletedTask;
        }
    }
}
