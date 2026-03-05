using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.Parameter;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Collections.Generic;
using System.Linq;

namespace AML.Web.Controllers.Parameter
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class ParameterMasterController : Controller
    {
        public IActionResult Index()

        {
            return View();
        }
        


    }

}
