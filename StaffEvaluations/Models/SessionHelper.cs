using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Web.Mvc;


namespace Mayur.Web.Attributes
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
        public TempDataDictionary TempData { get; set; }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            if (HttpContext.Current.Session["Masquerade"] == null && HttpContext.Current.Response.Cookies["Masquerading"]["Masquerade"] == "true")
            {
                filterContext.HttpContext.Session["SessionTimeout"] = true;
                filterContext.Result = new RedirectResult("~/Home/Index");

                HttpContext.Current.Response.Cookies["Masquerading"]["Masquerade"] = "false";
                
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}