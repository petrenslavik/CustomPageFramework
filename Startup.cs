using CustomPageFramework.HangfireExtensions;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Security;
using Owin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.Dashboard.Owin;
using Hangfire.SqlServer;

[assembly: OwinStartup(typeof(CustomPageFramework.Startup))]

namespace CustomPageFramework
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var options = new SqlServerStorageOptions { PrepareSchemaIfNecessary = true };

            string connectionString = ConfigurationManager
                .ConnectionStrings["DefaultConnection"]?
                .ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return;
            }

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString, options);
            app.UseHangfireCustomPages();
            var routes = DashboardRoutes.Routes;
            //app.UseHangfireDashboard("/hangfire");
            SignatureConversions.AddConversions(app);
            app.Map("/hangfire",
                subApp => subApp.UseOwin().UseHangfireDashboard(new DashboardOptions(), JobStorage.Current,
                    DashboardRoutes.Routes, null));
            app.UseHangfireServer();
        }
    }

    public static class Helper
    {
        public static
            Action<Func<IDictionary<string, object>,
                Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>>>
            UseHangfireDashboard(
                [NotNull]
                this Action<Func<IDictionary<string, object>, Func<Func<IDictionary<string, object>, Task>,
                    Func<IDictionary<string, object>, Task>>>> builder,
                [NotNull] DashboardOptions options,
                [NotNull] JobStorage storage,
                [NotNull] RouteCollection routes,
                [CanBeNull] IOwinDashboardAntiforgery antiforgery)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));
            builder(
                _ =>
                    Helper.UseHangfireDashboard(options, storage, routes, antiforgery));
            return builder;
        }

        public static Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>
            UseHangfireDashboard(
                [NotNull] DashboardOptions options,
                [NotNull] JobStorage storage,
                [NotNull] RouteCollection routes,
                [CanBeNull] IOwinDashboardAntiforgery antiforgery)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));
            if (storage == null)
                throw new ArgumentNullException(nameof(storage));
            if (routes == null)
                throw new ArgumentNullException(nameof(routes));
            return next =>
                async env =>
                {
                    OwinContext owinContext = new OwinContext(env);
                    OwinDashboardContext context = new OwinDashboardContext(storage, options, env);
                    if (!options.IgnoreAntiforgeryToken && antiforgery != null)
                    {
                        context.AntiforgeryHeader = antiforgery.HeaderName;
                        context.AntiforgeryToken = antiforgery.GetToken(env);
                    }

                    if (options.AuthorizationFilters != null)
                    {
                        if (options.AuthorizationFilters.Any<IAuthorizationFilter>(
                                (Func<IAuthorizationFilter, bool>)(filter =>
                                    !filter.Authorize(owinContext.Environment))))
                        {
                            owinContext.Response.StatusCode =
                                Helper.GetUnauthorizedStatusCode((IOwinContext)owinContext);
                            context = (OwinDashboardContext)null;
                            return;
                        }
                    }
                    else
                    {
                        foreach (IDashboardAuthorizationFilter authorizationFilter in options.Authorization)
                        {
                            if (!authorizationFilter.Authorize((DashboardContext)context))
                            {
                                owinContext.Response.StatusCode =
                                    Helper.GetUnauthorizedStatusCode((IOwinContext)owinContext);
                                context = (OwinDashboardContext)null;
                                return;
                            }
                        }

                        foreach (IDashboardAsyncAuthorizationFilter authorizationFilter in options.AsyncAuthorization)
                        {
                            if (!await authorizationFilter.AuthorizeAsync((DashboardContext)context))
                            {
                                owinContext.Response.StatusCode =
                                    Helper.GetUnauthorizedStatusCode((IOwinContext)owinContext);
                                context = (OwinDashboardContext)null;
                                return;
                            }
                        }
                    }

                    if (!options.IgnoreAntiforgeryToken && antiforgery != null && !antiforgery.ValidateRequest(env))
                    {
                        owinContext.Response.StatusCode = 403;
                        context = (OwinDashboardContext)null;
                    }
                    else
                    {
                        Tuple<IDashboardDispatcher, Match> dispatcher =
                            routes.FindDispatcher(owinContext.Request.Path.Value);
                        if (dispatcher == null)
                        {
                            await next(env);
                            context = (OwinDashboardContext)null;
                        }
                        else
                        {
                            context.UriMatch = dispatcher.Item2;
                            await dispatcher.Item1.Dispatch((DashboardContext)context);
                            context = (OwinDashboardContext)null;
                        }
                    }
                };
        }

        public static Action<Func<IDictionary<string, object>,
            Func<Func<IDictionary<string, object>, Task>, Func<IDictionary<string, object>, Task>>>> UseOwin(
            this IAppBuilder builder)
        {
            return middleware => builder.Use(middleware(builder.Properties));
        }

        private static int GetUnauthorizedStatusCode(IOwinContext owinContext)
        {
            IAuthenticationManager authentication = owinContext.Authentication;
            int num;
            if (authentication == null)
            {
                num = 0;
            }
            else
            {
                bool? isAuthenticated = authentication.User?.Identity?.IsAuthenticated;
                bool flag = true;
                num = isAuthenticated.GetValueOrDefault() == flag & isAuthenticated.HasValue ? 1 : 0;
            }

            return num == 0 ? 401 : 403;
        }
    }
}
