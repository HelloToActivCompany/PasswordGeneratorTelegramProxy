using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace PasswordGeneratorTelegramProxy
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "api/{controller}",
                defaults: new {controller = "Telegram"}
                //url: "api/{controller}/{action}",
                //defaults: new { controller = "Telegram", action = "Heartbeat"}
            );
        }
    }
}
