using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.UserAccess;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.Helpers
{
    public static class NavigationIndicatorHelper
    {
        public static string MakeActiveClass(this IUrlHelper urlHelper, string controller, string action)
        {
            try
            {
                string result = "active";
                string controllerName = urlHelper.ActionContext.RouteData.Values["controller"].ToString();
                string methodName = urlHelper.ActionContext.RouteData.Values["action"].ToString();
                if (string.IsNullOrEmpty(controllerName)) return null;
                if (controllerName.Equals(controller, StringComparison.OrdinalIgnoreCase))
                {
                    if (methodName.Equals(action, StringComparison.OrdinalIgnoreCase))
                    {
                        return result;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static string DisplayNavLink(this IUrlHelper urlHelper,int menuId, string modules )
        {
            var isMenu = 0;
            List<ClientRightsModel> userMenus = JsonConvert.DeserializeObject<List<ClientRightsModel>>(modules);
            string result = "navshow";
            foreach(var menu in userMenus)
            {
                if(menu.Menu_Id == menuId)
                {
                    isMenu++;
                }
            }
            if(isMenu > 0)
            {
                result = "navshow";
            }
            else
            {
                result = "navhide";
            }
            return result;
        }
    }
}
