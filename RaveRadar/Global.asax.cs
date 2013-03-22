using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.WebPages;

namespace RaveRadar
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            MvcHandler.DisableMvcResponseHeader = true;
            
            AreaRegistration.RegisterAllAreas();

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);


        }

        public override string GetVaryByCustomString(HttpContext context, string custom)
        {
            // Override Caching for compression
            if (custom == "GZIP")
            {
                string acceptEncoding = HttpContext.Current.Response.Headers["Content-Encoding"];

                if (string.IsNullOrEmpty(acceptEncoding))
                    return "";
                else if (acceptEncoding.Contains("gzip"))
                    return "GZIP";
                else if (acceptEncoding.Contains("deflate"))
                    return "DEFLATE";

                return "";
            }

            return base.GetVaryByCustomString(context, custom);
        }
    }
}