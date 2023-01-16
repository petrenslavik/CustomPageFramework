using System;
using System.Text;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CustomPage.HangfireExtensions.ManagementPage
{
    public static class ManagementPageExtensions
    {
        public const string UrlPath = "/management";
        public static IApplicationBuilder UseManagementPage(this IApplicationBuilder app)
        {
            var tables = BuildTableConfigurations();

            DashboardRoutes.Routes.AddRazorPage(UrlPath, x => new Management(app.ApplicationServices, tables));
            DashboardRoutes.Routes.AddRazorPage($"{UrlPath}/(?<TableName>.*)", x => new Management(app.ApplicationServices, tables, x.Groups["TableName"].Value));
            DashboardRoutes.Routes.Add($"{UrlPath}Actions/Save", new SaveDispatcher(app.ApplicationServices, tables));
            NavigationMenu.Items.Add(page => new MenuItem("Management", page.Url.To(UrlPath))
            {
                Active = page.RequestPath.StartsWith(UrlPath)
            });

            GlobalConfigurationExtensions.AddDashboardRouteToEmbeddedResource($"{UrlPath}Resources/css/style.css", "text/css", "CustomPage.HangfireExtensions.ManagementPage.Resources.style.css");
            GlobalConfigurationExtensions.AddDashboardRouteToEmbeddedResource($"{UrlPath}Resources/js/app.js", "application/javascript", "CustomPage.HangfireExtensions.ManagementPage.Resources.app.js");
            GlobalConfigurationExtensions.AddDashboardRouteToEmbeddedResource($"{UrlPath}Resources/js/vue.global.js", "application/javascript", "CustomPage.HangfireExtensions.ManagementPage.Resources.vue.global.js");
            GlobalConfigurationExtensions.AddDashboardRouteToEmbeddedResource($"{UrlPath}Resources/js/axios.min.js", "application/javascript", "CustomPage.HangfireExtensions.ManagementPage.Resources.axios.min.js");
            return app;
        }

        private static TableConfiguration[] BuildTableConfigurations()
        {
            var serverSettingsFields = new[] { "SettingType", "SettingJson" };
            var serverSettings = new TableConfiguration()
            {
                Name = "ServerSettings",
                Fields = serverSettingsFields,
                InsertBuilder = (object row) =>
                {
                    var data = (JsonConvert.DeserializeObject(row as string) as JObject);
                    var sb = new StringBuilder();
                    
                    foreach (var field in serverSettingsFields)
                    {
                        sb.Append("'");
                        sb.Append(data[field].ToString().Replace("'","''"));
                        sb.Append("'");
                        sb.Append(",");
                    }

                    sb.Remove(sb.Length - 1, 1);
                    return sb.ToString();
                }
            };

            return new[]
            {
                serverSettings
            };
        }
    }
}
