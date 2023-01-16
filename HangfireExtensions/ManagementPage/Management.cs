using CustomPage.Database;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Linq;
using CustomPage.Database.DbModels;

namespace CustomPage.HangfireExtensions.ManagementPage
{
    internal partial class Management
    {
        public Management(IServiceProvider applicationServices, IEnumerable<TableConfiguration> tableConfigurations, string tableName = null)
        {
            Services = applicationServices;
            AvailableTables = tableConfigurations;

            CurrentTable = string.IsNullOrWhiteSpace(tableName) ? AvailableTables.ElementAt(0) : AvailableTables.FirstOrDefault(x => x.Name == tableName);
        }

        public IServiceProvider Services { get; }

        public TableConfiguration CurrentTable { get; private set; }

        public IEnumerable<TableConfiguration> AvailableTables { get; private set; }

        private List<object> ReadData(TableConfiguration tableConfiguration)
        {
            var data = new List<object>();
            using (var scope = Services.CreateScope())
            {
                var database = scope.ServiceProvider.GetService<ApplicationDbContext>().Database;

                using (var command = database.GetDbConnection().CreateCommand())
                {
                    command.CommandText = $"Select * from dbo.{tableConfiguration.Name}";
                    database.OpenConnection();
                    using (var result = command.ExecuteReader())
                    {
                        if (!result.HasRows)
                        {
                            return data;
                        }

                        while (result.Read())
                        {
                            data.Add(tableConfiguration.Fields.ToDictionary(field => field, field => result[field]));
                        }
                    }
                }
            }

            return data;
        }
    }
}