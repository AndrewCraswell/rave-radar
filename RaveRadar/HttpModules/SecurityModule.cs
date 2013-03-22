using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Web;

namespace RaveRadar.HttpModules
{
    public class SecurityModule : IHttpModule
    {
        public void Dispose()
        {
            //intentionally do nothing
        }

        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += new EventHandler(context_PreSendRequestHeaders);
        }

        private void context_PreSendRequestHeaders(object sender, EventArgs e)
        {
            var context = ((HttpApplication)sender).Context;
            context.Response.Headers.Remove("Server");

            // ensure that if GZip/Deflate Encoding is applied that headers are set
            // also works when error occurs if filters are still active
            HttpResponse response = HttpContext.Current.Response;
            if (response.Filter is GZipStream && response.Headers["Content-encoding"] != "gzip")
            {
                response.AppendHeader("Content-encoding", "gzip");
            }
            else if (response.Filter is DeflateStream && response.Headers["Content-encoding"] != "deflate")
            {
                response.AppendHeader("Content-encoding", "deflate");
            }
        }
    }
}