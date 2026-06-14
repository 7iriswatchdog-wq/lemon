using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using System;
using AML.Web.Helper;
using Microsoft.AspNetCore.Http;
using AML.Core.Common.StaticResource;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using AML.Core.ServiceContract.UserAccess;

namespace AML.Web.CustomFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class PageAuthorizeAttribute : ActionFilterAttribute
    {
        IUserGroupRightService _userGroupRightService;
        public PageAuthorizeAttribute(IUserGroupRightService userGroupRightService)
        {
            _userGroupRightService = userGroupRightService;
        }
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.ActionDescriptor.FilterDescriptors.Any(x => x.Filter is AllowAnonymousAttribute))
                return;

            string controllerName = filterContext.RouteData.Values["controller"].ToString().ToLower();
            string actionName = filterContext.RouteData.Values["action"].ToString().ToLower();
            int userId = filterContext.HttpContext.Session.GetString(StaticResource.sessUserId).ParseInt();
            int userGroupId = filterContext.HttpContext.Session.GetString(StaticResource.sessRoleId).ParseInt();
            //if(CheckUserAutherization(controllerName, actionName, userId, userGroupId))
            //{
            //    base.OnActionExecuting(filterContext);
            //}
            string sessionId = filterContext.HttpContext.Session.GetString("SessID");
            if (CheckUserAutherization(controllerName, actionName, userId, userGroupId, sessionId))
            {
                base.OnActionExecuting(filterContext);
            }
            else
            {
                ContextResult(filterContext, "UnauthorizedAccess", "Home");
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
        //private bool CheckUserAutherization(string _controllerName, string _actionName, int _userId, int _roleId)
        private bool CheckUserAutherization(string _controllerName, string _actionName, int _userId, int _roleId, string sessionId)
        {
            //ServiceResponse<bool> serviceResponse = _userGroupRightService.CheckUserRightExixts(_controllerName, _actionName, _userId, _roleId);
            ServiceResponse<bool> serviceResponse = _userGroupRightService.CheckUserRightExixts(_controllerName, _actionName, _userId, _roleId, sessionId);
            if (serviceResponse.Status == StaticResource.SuccessStatusCode)
            {
                return serviceResponse.Result;
            }
            else
            {
                return false;
            }
        }
    }
}
