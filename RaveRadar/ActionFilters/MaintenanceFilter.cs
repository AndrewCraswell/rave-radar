using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaveRadar.ActionFilters
{
    public class MaintenanceFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (String.Compare(ConfigurationManager.AppSettings["MaintenanceMode"], "True", true) == 0)
            {
                filterContext.RouteData.Values["controller"] = "Splash";
                filterContext.RouteData.Values["action"] = "Maintenance";
            }
        }
    }
}
