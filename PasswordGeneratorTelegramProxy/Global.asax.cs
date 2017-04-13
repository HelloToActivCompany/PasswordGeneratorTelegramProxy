using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Collections.Concurrent;
using PasswordGeneratorTelegramProxy.Models;

namespace PasswordGeneratorTelegramProxy
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static ConcurrentDictionary<long, string> UserIdConnectionIdMap { get; set; }
        public static ConcurrentDictionary<long, UserConnection> UserIdConnectionMap { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            UserIdConnectionIdMap = new ConcurrentDictionary<long, string>();
            UserIdConnectionMap = new ConcurrentDictionary<long, UserConnection>();
        }
    }
}
