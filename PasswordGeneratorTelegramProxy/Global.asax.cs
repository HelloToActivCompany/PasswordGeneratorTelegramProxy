using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Collections.Concurrent;
using PasswordGeneratorTelegramProxy.Models;
using System.Configuration;

namespace PasswordGeneratorTelegramProxy
{
    public class WebApiApplication : System.Web.HttpApplication
    {      
        public static string BotToken { get; set; }
        public static string PasswordGeneratorUrl { get; set; }
        public static ConcurrentDictionary<long, UserConnection> UserIdConnectionMap { get; set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            BotToken = ConfigurationManager.AppSettings["TelegramBotToken"];
            PasswordGeneratorUrl = ConfigurationManager.AppSettings["PasswordGenerator"];

            UserIdConnectionMap = new ConcurrentDictionary<long, UserConnection>();
        }
    }
}
