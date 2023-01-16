using CustomPage.HangfireExtensions.ManagementPage;
using Microsoft.AspNetCore.Builder;
using System;
using Hangfire.Dashboard;

namespace CustomPage.HangfireExtensions
{
    public static class GlobalConfigurationExtensions
    {
        public static IApplicationBuilder UseHangfireCustomPages(this IApplicationBuilder app)
        {
            if (app == null)
                throw new ArgumentNullException(nameof(app));

            app.UseManagementPage();

            return app;
        }

        public static void AddDashboardRouteToEmbeddedResource(string route, string contentType, string resourceName)
            => DashboardRoutes.Routes.Add(route, new ContentDispatcher(contentType, resourceName, TimeSpan.FromDays(1)));
    }
}