using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AML.Web.CustomFilters;
using Microsoft.AspNetCore.Mvc;

namespace AML.Web.Controllers.Dashboard
{
    [SessionAuthorize]
    //[ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }
    }
}
