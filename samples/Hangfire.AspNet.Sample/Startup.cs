using System;
using System.Collections.Generic;
using Owin;

namespace Hangfire.AspNet.Sample
{
    public class Startup
    {
        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            GlobalConfiguration.Configuration
                .UseSqlServerStorage(@"Server=.\sqlexpress;Database=Hangfire.AspNet;Integrated Security=SSPI;");

            yield return new BackgroundJobServer();
        }

        public void Configuration(IAppBuilder app)
        {
            app.UseHangfireAspNet(GetHangfireConfiguration);
            app.UseHangfireDashboard("");
        }
    }
}