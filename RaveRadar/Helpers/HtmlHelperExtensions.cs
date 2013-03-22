using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RaveRadar.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static string AppVersion(this HtmlHelper helper)
        {
            var appInstance = helper.ViewContext.HttpContext.ApplicationInstance;
            var assemblyVersion = appInstance.GetType().BaseType.Assembly.GetName().Version;
            return assemblyVersion.ToString();
        }

        public static string GetFacebookProfileUrl(this HtmlHelper helper, string profileId)
        {
            return String.Concat("http://www.facebook.com/", profileId);
        }
    }
}