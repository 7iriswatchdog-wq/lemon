using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AML.Web.Models;
using AML.Core.ServiceContract;
using AML.Core.ServiceContract.User;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using AML.Web.CustomFilters;
using AML.Core.ServiceContract.Report;
using AML.Web.Helpers;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Http;
using AML.Core.ServiceContract.UserAccess;
using AML.ViewModel.ViewModels.UserAccess;
using Newtonsoft.Json;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.DataTable;
using System.Reflection;
using AML.Core.ServiceContract.CustomerCase;
using AML.Web.Helper;
using NToastNotify;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.TransactionMonitor;
using AML.Core.ServiceContract.TransactionMonitor;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Report;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Charts;
using AML.Core.DataContract.Enum;
using AML.Core.ServiceContract.Common;

namespace AML.Web.Controllers
{
    [SessionAuthorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;
        private IUserService _user;
        private IReportService _reportService;
        private IUserGroupRightService _userGroupRightService;
        private ITransactionMonitorService _transacitonMonitorService;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private ICustomerCaseService _customerCaseService;
        private ICommonService _commonService;
        public HomeController(IMapper mapper, IHttpClientHandler clientHandler, ITransactionMonitorService transacitonMonitorService,ICustomerCaseService customerCaseService, IConfiguration configuration, IUserService user, IReportService reportService, IUserGroupRightService userGroupRightService, ICommonService commonService)
        {
            _mapper = mapper;
            _configuration = configuration;
            _user = user;
            _reportService = reportService;
            _userGroupRightService = userGroupRightService;
            _clientHandler = clientHandler;
            _customerCaseService = customerCaseService;
            _transacitonMonitorService = transacitonMonitorService;
            _commonService = commonService;
        }

        public IActionResult Index()
        {
            int userId = Convert.ToInt32(HttpContext.Session.GetString("SessUserId"));
            int clientId = Convert.ToInt32(HttpContext.Session.GetString("SessClientId"));
            var result = _userGroupRightService.getClientMenuByClientId(clientId).Result;
            //var clientId = Convert.ToInt32(HttpContext.Session.GetString("SessClientId"));
            //for progress bar
            var IndividualScreeningCount = _reportService.GetCustomerTypeCount("I", clientId);
            ViewBag.individualcustomertype = IndividualScreeningCount;

            var CorporateScreeningCount = _reportService.GetCustomerTypeCount("C", clientId);
            ViewBag.corporatecustomertype = CorporateScreeningCount;
          
            var indvidualcount = _reportService.GetRiskCount(1, clientId);
            ViewBag.indvidualval = indvidualcount[0].total_count;
          

			var corporatecount = _reportService.GetRiskCount(2, clientId);
            ViewBag.corporateval = corporatecount[0].total_count;
            var bankcount = _reportService.GetRiskCount(3, clientId);
            ViewBag.bankval = bankcount[0].total_count;
            var vendorcount = _reportService.GetRiskCount(4, clientId);
            ViewBag.vendorval = vendorcount[0].total_count;

            ViewBag.totalhighrisk = indvidualcount[0].high_risk_count + corporatecount[0].high_risk_count;
            ViewBag.totallowrisk= indvidualcount[0].low_risk_count + corporatecount[0].low_risk_count;
            ViewBag.totalmediumrisk = indvidualcount[0].medium_risk_count + corporatecount[0].medium_risk_count;
            //for chartdata

            var Apprcount = _reportService.GetCustomerCaseCount(2, clientId);
            ViewBag.approvecount = Apprcount;
            var Pendingcount = _reportService.GetCustomerCaseCount(0, clientId);
            ViewBag.pendingvalcount = Pendingcount;
            var Rejectedcount = _reportService.GetCustomerCaseCount(3, clientId);
            ViewBag.rejectvalcount = Rejectedcount;

            var approvedcount = _reportService.GetapprovedCaseCount(2, clientId);

            var autoapproved= _reportService.GetCustomerCaseCount(5, clientId);
            ViewBag.autoapproved = autoapproved;
            ViewBag.ManuallyApproved = approvedcount;
            ViewBag.PotentialMatch = Pendingcount;

            ViewBag.Toatalapprovedcount = autoapproved + approvedcount;



            var totalcount=_reportService.GetCustomerCaseCount(10, clientId);

            ViewBag.totalcount = totalcount;

            var totalriskcount = indvidualcount[0].total_count + corporatecount[0].total_count;


            ViewBag.Unclassifiedriskcount = totalcount - totalriskcount;






            var userMenus = result.Select(x => new ClientRightsModel()
            {
                Menu_Id = x.Menu_Id
            }).ToList();
            //test 
            List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCasePreviousWeekReportList(new CaseReportRequestDTO()
            {

                StartDate = System.DateTime.Now.AddDays(-7).ToString(),
                EndDate = System.DateTime.Now.ToString(),
                Status = "0",
                ClientId = _clientHandler.GetClientId()


            })) ;
            //test
            
            //List<CaseReportListModel> date = _mapper.Map<List<CaseReportListModel>>(_reportService.GetLatestDate(clientId));

            //DateTime ldate = Convert.ToDateTime(date[0].CreatedOn);

            //ViewBag.date = ldate.ToShortDateString(); ;                 
            //ViewBag.count = abc.Count();

            var jsonModules = JsonConvert.SerializeObject(userMenus);
            HttpContext.Session.SetString("SessModules", jsonModules);
            return View();
        }

        public IActionResult UnauthorizedAccess()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public ActionResult SetCulture(string culture)
        {
            // Validate input
            culture = CultureHelper.GetImplementedCulture(culture);
            // Save culture in a cookie
         
                Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );
            var returnUrl = Request.Headers["Referer"].ToString();
            if (returnUrl.Contains("?culture="))
            {
                var url = returnUrl.Substring(0, returnUrl.IndexOf("?culture="));
                return Redirect(url + "?culture=" + culture);
            }
            else
            {
                return Redirect(returnUrl);
            }
        }
        //[HttpPost("Home/CaseReportCustompagination")]
        //public  CaseReportCustompagination(DataTableModel model, string userID, string updatedByUserID, string startDate, string endDate, string status, string cust_type)
        //{
        //    startDate = System.DateTime.Now.AddDays(-7).ToString();
        //    endDate = System.DateTime.Now.ToString();
        //    status = "0";
        //    List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportList(new CaseReportRequestDTO()
        //    {
        //        User = userID,
        //        StartDate = startDate,
        //        EndDate = endDate,
        //        Status = status,
        //        Cust_type = cust_type,
        //        UpdatedByUserId = updatedByUserID,
        //        ClientId = _clientHandler.GetClientId()
        //    }));
     
        //    ViewBag.CaseCnt= abc.Count();
        //    return View("Index");
        //}

        ////[HttpPost("Home/GetCaseCount")]
        public IActionResult GetCustomerCaseCount(int caseStatus)
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetString("SessClientId"));
            var count = _reportService.GetCustomerCaseCount(caseStatus,clientId);
            return Json(new { data = count });
        }

        [HttpPost("Home/GetRiskCount")]
        public IActionResult GetRiskCount(int category)
        {
            var clientId = Convert.ToInt32(HttpContext.Session.GetString("SessClientId"));
            var count = _reportService.GetRiskCount(category, clientId);
            return Json(new { data = count });
        }

        [HttpGet ("/home/custompagination")]
        
        public JsonResult CustomPagination(DataTableModel model, String type)
        {
            var userId = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();
            List<CustomerCaseDTO> abc = new List<CustomerCaseDTO>();
            

            abc = _mapper.Map<List<CustomerCaseDTO>>(_customerCaseService.GetAllSanctionDashboard(clientId, type));
            int totalcount = abc.Count;
          
            int filteredcount = abc.Count;
           
            return Json(abc);
            
        }


        [HttpGet("/home/CaseManagement")]
        public JsonResult CaseManagement(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            dynamic list = _transacitonMonitorService.GetAllTMSCasefordashboard(clientId);
            List<TMSCaseDTO> abc = _mapper.Map<List<TMSCaseDTO>>(list);
            int totalcount = abc.Count;
            int filteredcount = abc.Count;
            return Json(abc);
        }
        public List<T> Sort<T>(List<T> input, string property, string dir)

        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty?.GetValue(p, null)).ToList();

        }

        public async Task<IActionResult> ApprovedScreeningApi(int clientid) 
        {
            var x =await _commonService.ScreenApprovedListByClientId(clientid);

            return View();
        
        }
    }
}
