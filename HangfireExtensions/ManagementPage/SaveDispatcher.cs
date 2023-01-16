using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CustomPageFramework.Database;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.Owin;

namespace CustomPageFramework.HangfireExtensions.ManagementPage
{
    public class SaveDispatcher : IDashboardDispatcher
    {
        private readonly IEnumerable<TableConfiguration> _tableConfigurations;

        public SaveDispatcher(IEnumerable<TableConfiguration> tableConfigurations)
        {
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

            var database =  new ApplicationDbContext().Database;
            database.Connection.Open();
            var dbConnection = database.Connection;
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

            return Task.CompletedTask;
        }
    }
}
