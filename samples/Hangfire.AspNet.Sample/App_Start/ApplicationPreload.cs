using System;
using System.Collections.Generic;
using System.Web.Hosting;
using Hangfire.MemoryStorage;

namespace Hangfire.AspNet.Sample
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
            HangfireAspNet.Use(GetHangfireConfiguration);
        }
    }
}