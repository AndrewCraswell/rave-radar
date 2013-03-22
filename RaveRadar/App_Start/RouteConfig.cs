using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace RaveRadar
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Beta", // Route name
                "Beta", // URL with parameters
                new { controller = "Splash", action = "Beta" } // Parameter defaults
            );

            routes.MapRoute(
                "Maintenance", // Route name
                "Maintenance", // URL with parameters
                new { controller = "Splash", action = "Maintenance" } // Parameter defaults
            );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}", // URL with parameters
                new { controller = "Map", action = "Index" } // Parameter defaults
            );
        }
    }
}