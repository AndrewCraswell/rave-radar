using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Facebook;
using Facebook.Web;
using System.Configuration;

namespace RaveRadar
{
    // TODO: if cancelled we need to know state
    public class FacebookAuthenticationHelper
    {
        public static FacebookWebClient _fb = new FacebookWebClient(FacebookWebContext.Current);
        private static FacebookOAuthResult OAuthResult;
        private static FacebookOAuthClient OAuth = new FacebookOAuthClient();
        private static IDictionary<string, object> parametersDictionary = new Dictionary<string, object>();

        public static void Authenticate(Uri redirectUri, string permissions)
        {
            if (FacebookOAuthResult.TryParse(HttpContext.Current.Request.Url, out OAuthResult))
            {
                
                if (OAuthResult.IsSuccess)
                {
                    parametersDictionary.Clear();
                    JsonObject token = (JsonObject)OAuth.ExchangeCodeForAccessToken(OAuthResult.Code, parametersDictionary);
                    dynamic theToken = token;

                    _fb.AccessToken = theToken.access_token;
                }
                else
                {
                    // There was a problem while authenticating
                    // User may have clicked cancel
                }
                 
            }
            else if (!IsTokenized())
            {
                // Set OAuth details
                SetAppDetails(redirectUri);

                parametersDictionary.Clear();
                //parametersDictionary.Add("display", "popup");
                parametersDictionary.Add("scope", permissions);
                
                Uri loginUrl = OAuth.GetLoginUrl(parametersDictionary);
                HttpContext.Current.Response.Redirect(loginUrl.AbsoluteUri);
            }            
        }

        public static string GetAppID()
        {
            return ConfigurationManager.AppSettings["raveRadarAppID"].ToString();
        }
        public static string GetAppSecret()
        {
            return ConfigurationManager.AppSettings["raveRadarAppSecret"].ToString();
        }
        public static string GetAccessToken()
        {
            return _fb.AccessToken;
        }
        public static bool IsTokenized()
        {
            return !String.IsNullOrEmpty(_fb.AccessToken);
        }

        private static void SetAppDetails(Uri redirectUri)
        {
            OAuth.RedirectUri = redirectUri;
            SetAppDetails();
        }
        private static void SetAppDetails()
        {
            OAuth.AppId = GetAppID();
            OAuth.AppSecret = GetAppSecret();
        }
    }
}