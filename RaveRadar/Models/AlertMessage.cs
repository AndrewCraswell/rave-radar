using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RaveRadar.Models
{
    public enum AlertType
    {
        Block = 0,
        Info = 1,
        Success = 2,
        Error = 3
    }

    public class AlertMessage
    {
        public string Header { get; set; }

        public string Body { get; set; }

        public string CssClass { get; set; }

        public bool EnableClose { get; set; }

        public int TimeoutDelay { get; set; }

        #region Constructors
        public AlertMessage()
        {
        }

        public AlertMessage(string header, string body, bool enableClose = true, int timeoutDelay = 0)
        {
            Header = header;
            Body = body;
            CssClass = "alert-info";
            EnableClose = enableClose;
        }

        public AlertMessage(string header, string body, string cssClass, bool enableClose = true)
            : this(header, body, enableClose)
        {
            CssClass = cssClass;
        }

        public AlertMessage(string header, string body, AlertType type, bool enableClose = true)
            : this(header, body, enableClose)
        {
            switch (type)
            {
                case AlertType.Block:
                    CssClass = "alert-block";
                    break;
                case AlertType.Info:
                    CssClass = "alert-info";
                    break;
                case AlertType.Success:
                    CssClass = "alert-success";
                    break;
                case AlertType.Error:
                    CssClass = "alert-error";
                    break;
            }

        }
        #endregion
    }
}