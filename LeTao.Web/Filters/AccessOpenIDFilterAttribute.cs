using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Configuration;
using System.Web.Routing;

namespace LeTao.Web.Filters
{
    public class AccessOpenIDFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (!string.IsNullOrEmpty(SessionManager.OpenID))
            {
                //留着待扩展
            }
            else
            {
                //异步提交判断是否需要登录
                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    filterContext.Result = new JsonResult() { Data = new { Flag = 888, Message = string.Format(WebConfigurationManager.AppSettings["UserLoginPage"].Replace("$", "&"), "returnUrl=" + filterContext.HttpContext.Request.ServerVariables["http_referer"].Replace("&", "$")) } };
                }
                else//页面加载时无登录直接跳转
                {
                    byte[] bytes = Encoding.Default.GetBytes(filterContext.HttpContext.Request.Url.ToString());
                    string returnUrl = Convert.ToBase64String(bytes);
                    //VoBao.Base.Common.File_Ext.CreateFile(@"/log/1.txt", "returnUrl:" + returnUrl);
                    filterContext.Result = new RedirectResult(string.Format(WebConfigurationManager.AppSettings["UserLoginPage"].Replace("$", "&"), "ReturnUrl=" + returnUrl));
                }
            }
        }
    }
}
