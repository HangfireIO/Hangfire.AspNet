using System.Web.Mvc;
using System.Web.Routing;
using Serilog;

namespace Hangfire.AspNet.Sample
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_End()
        {
            Log.Logger.Information("Application_End method called.");
        }
    }
}
