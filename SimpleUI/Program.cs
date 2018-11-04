using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Events;
using System;
using Serilog.Enrichers.AspnetcoreHttpcontext;
using Microsoft.AspNetCore.Http;
using SimpleUI.Models;
using System.Linq;
using Serilog.Core;
using System.Reflection;
using Serilog.Formatting.Compact;

namespace SimpleUI
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var name = Assembly.GetExecutingAssembly().GetName();
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                //.Enrich.FromLogContext()
                //.Enrich.WithMachineName()
                //.Enrich.WithProperty("Assembly", $"{name.Name}")
                //.Enrich.WithProperty("Version", $"{name.Version}")
                .WriteTo.File(new RenderedCompactJsonFormatter(), @"C:\users\edahl\Source\Logs\SimpleUi.json")
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateWebHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
                //.UseSerilog((provider, ContextBoundObject, loggerConfig) =>
                //{
                //    loggerConfig
                //        .MinimumLevel.Debug()
                //        .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                //        .Enrich.WithAspnetcoreHttpcontext(provider, false, AddCustomContextInfo)
                //        .Enrich.FromLogContext()
                //        .WriteTo.Console(
                //            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{HttpContext} {NewLine}{UserInfo}");
                //}, true);

        public static void AddCustomContextInfo(IHttpContextAccessor ctx, LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            HttpContext context = ctx.HttpContext;
            if (context == null)
            {
                return;
            }
            var userInfo = context.Items[$"serilog-enrichers-aspnetcore-userinfo"] as UserInfo;
            if (userInfo == null)
            {
                var user = context.User.Identity;
                if (user == null || !user.IsAuthenticated) return;
                userInfo = new UserInfo
                {
                    Name = user.Name,
                    Claims = context.User.Claims.ToDictionary(x => x.Type, y => y.Value)
                };
                context.Items[$"serilog-enrichers-aspnetcore-userinfo"] = userInfo;
            }

            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserInfo", userInfo, true));
        }
    }
}
