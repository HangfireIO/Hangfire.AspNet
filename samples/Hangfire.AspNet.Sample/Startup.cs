using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Web.Hosting;
using Owin;
using Serilog;

namespace Hangfire.AspNet.Sample
{
    public class Startup : IRegisteredObject
    {
        public Startup()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            GlobalConfiguration.Configuration
                .UseSerilogLogProvider()
                .UseSqlServerStorage(@"Server=.\sqlexpress;Database=Hangfire.AspNet;Integrated Security=SSPI;");

            yield return new BackgroundJobServer(
                new BackgroundJobServerOptions { ServerName = $"{Environment.MachineName}:{Process.GetCurrentProcess().Id}:{AppDomain.CurrentDomain.Id}" });
        }

        public void Configuration(IAppBuilder app)
        {  
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File($@"C:\Temp\Hangfire.AspNet.Sample.{AppDomain.CurrentDomain.Id}.log")
                .CreateLogger();

            Log.Logger.Information("Application started.");

            app.UseHangfireAspNet(GetHangfireConfiguration);
            app.UseHangfireDashboard("");
        }

        public void Stop(bool immediate)
        {
            Thread.Sleep(TimeSpan.FromSeconds(30));
            HostingEnvironment.UnregisterObject(this);
        }
    }
}