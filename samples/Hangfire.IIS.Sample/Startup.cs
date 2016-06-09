using Owin;

namespace Hangfire.IIS.Sample
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // TODO: This is needed only for development purposes
            // TODO: Call this BEFORE dashboard
            app.UseHangfire(ApplicationPreload.GetHangfireConfiguration);
            app.UseHangfireDashboard();
        }
    }
}