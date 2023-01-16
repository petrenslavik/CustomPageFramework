using System;
using CustomPageFramework.HangfireExtensions.ManagementPage;
using Hangfire.Dashboard;
using Owin;

namespace CustomPageFramework.HangfireExtensions
{
    public static class GlobalConfigurationExtensions
    {
        public static IAppBuilder UseHangfireCustomPages(this IAppBuilder app)
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