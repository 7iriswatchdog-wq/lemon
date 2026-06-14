using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using AML.Web.Helper;
using Microsoft.AspNetCore.Http;
using AML.Core.Common.StaticResource;
using System.Linq;
using Microsoft.AspNetCore.Authorization;

namespace AML.Web.CustomFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class SessionAuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is AllowAnonymousAttribute)) return;
            if (
                filterContext.HttpContext.Session.GetString(StaticResource.sessUserId).IsNotNullOrEmpty() 
                && filterContext.HttpContext.Session.GetString(StaticResource.sessRoleId).IsNotNullOrEmpty() 
                && filterContext.HttpContext.Session.GetString(StaticResource.sessUserId).ParseInt() > 0 
                && filterContext.HttpContext.Session.GetString(StaticResource.sessRoleId).ParseInt() > 0)
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {

                ContextResult(filterContext, "Login", "UserAccess");
            }
        }


        private static void ContextResult(ActionExecutingContext filterContext, string actionName, string controllerName)
        {
            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
            {
                area = "",
                controller = controllerName,
                action = actionName
            }));
        }
    }
}
