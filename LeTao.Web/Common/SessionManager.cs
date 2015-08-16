using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.SessionState;

namespace System.Web.Mvc
{
    
    [Serializable]
    public class SessionManager
    {
        const string SESSION_KEY_OpenID = "SESSION_KEY_OpenID";
        const string SESSION_KEY_AccessToken = "SESSION_KEY_AccessToken";
        const string SESSION_KEY_Ticket = "SESSION_KEY_Ticket";
        const string SESSION_KEY_TokenExpireTime = "SESSION_KEY_TokenExpireTime";

        public static string OpenID
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["SESSION_KEY_OpenID"] == null)
                {
                    return string.Empty;
                }
                return HttpContext.Current.Session["SESSION_KEY_OpenID"].ToString();
            }
            set
            {
                HttpContext.Current.Session["SESSION_KEY_OpenID"] = value;
            }
        }

        public static string AccessToken
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["SESSION_KEY_AccessToken"] == null)
                {
                    return string.Empty;
                }
                return HttpContext.Current.Session["SESSION_KEY_AccessToken"].ToString();
            }
            set
            {
                HttpContext.Current.Session["SESSION_KEY_AccessToken"] = value;
            }
        }

        public static string Ticket
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["SESSION_KEY_Ticket"] == null)
                {
                    return string.Empty;
                }
                return HttpContext.Current.Session["SESSION_KEY_Ticket"].ToString();
            }
            set
            {
                HttpContext.Current.Session["SESSION_KEY_Ticket"] = value;
            }
        }

        public static string  TokenExpireTime
        {
            get
            {
                if (HttpContext.Current.Session == null || HttpContext.Current.Session["SESSION_KEY_TokenExpireTime"] == null)
                {
                    return string.Empty;
                }
                return HttpContext.Current.Session["SESSION_KEY_TokenExpireTime"].ToString();
            }
            set
            {
                HttpContext.Current.Session["SESSION_KEY_TokenExpireTime"] = value;
            }
        }

    }
}
