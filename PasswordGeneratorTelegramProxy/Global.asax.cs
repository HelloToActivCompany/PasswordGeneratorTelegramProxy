using System;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using PasswordGeneratorTelegramProxy.Models;
using System.Configuration;
using Microsoft.AspNet.SignalR.Client;

namespace PasswordGeneratorTelegramProxy
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static readonly Object Sync = new Object();

        public static Telegram.Bot.TelegramBotClient BotClient { get; private set; }
        public static IHubProxy PasswordGeneratorProxy { get; set; }
        public static string PasswordGeneratorUrl { get; set; }

        public static Cache Cache { get; private set; }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            var botToken = ConfigurationManager.AppSettings["TelegramBotToken"];
            BotClient = new Telegram.Bot.TelegramBotClient(botToken);
            
            PasswordGeneratorUrl = ConfigurationManager.AppSettings["PasswordGenerator"];

            Cache = new Cache(new TimeSpan(0, 5, 0), 100, 25, 100000, new SystemTimeManager());
        }
    }
}
