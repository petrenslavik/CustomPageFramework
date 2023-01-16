using System.Collections.Generic;
using System.Linq;
using CustomPageFramework.Database;

namespace CustomPageFramework.HangfireExtensions.ManagementPage
{
    internal partial class Management
    {
        public Management(IEnumerable<TableConfiguration> tableConfigurations, string tableName = null)
        {
            AvailableTables = tableConfigurations;

            CurrentTable = string.IsNullOrWhiteSpace(tableName) ? AvailableTables.ElementAt(0) : AvailableTables.FirstOrDefault(x => x.Name == tableName);
        }

        public TableConfiguration CurrentTable { get; private set; }

        public IEnumerable<TableConfiguration> AvailableTables { get; private set; }

        private List<object> ReadData(TableConfiguration tableConfiguration)
        {
            var data = new List<object>();

            var database = new ApplicationDbContext().Database;

            using (var command = database.Connection.CreateCommand())
            {
                command.CommandText = $"Select * from dbo.{tableConfiguration.Name}";
                database.Connection.Open();
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

            return data;
        }
    }
}