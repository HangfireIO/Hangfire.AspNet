using System;
using System.Collections.Generic;
using System.Web.Hosting;
using Hangfire.MemoryStorage;

namespace Hangfire.IIS.Sample
{
    public class ApplicationPreload : IProcessHostPreloadClient
    {
        public static IEnumerable<IDisposable> GetHangfireConfiguration()
        {
            GlobalConfiguration.Configuration.UseMemoryStorage();

            yield return new BackgroundJobServer();
        }

        public void Preload(string[] parameters)
        {
            AspNetHangfireServer.Use(GetHangfireConfiguration);
        }
    }
}