using System;
using System.Collections.Generic;
using Hangfire.MemoryStorage;
using Owin;

namespace Hangfire.AspNet.Sample
{
    public class Startup
    {
        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();
            yield return new BackgroundJobServer();
        }

        public void Configuration(IAppBuilder app)
        {
            // TODO: This is needed only for development purposes
            // TODO: Call this BEFORE dashboard
            app.UseHangfireAspNet(GetHangfireConfiguration);
            app.UseHangfireDashboard();
        }
    }
}