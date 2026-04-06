using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AML.ViewModel.ViewModels.User;
using AML.Core.ServiceContract.User;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using AML.Web.Helper;
using AML.DTO.DTO.User;
using AML.Core.ServiceContract.Department;
using AML.ViewModel.ViewModels.Department;
using AML.Core.ServiceContract.Designation;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.UserGroup;
using AML.ViewModel.ViewModels.Designation;
using AML.ViewModel.ViewModels.Branch;
using AML.ViewModel.ViewModels.UserGroup;
using Microsoft.AspNetCore.Mvc.Rendering;
using AML.ViewModel.ViewModels.VisaType;
using AML.Core.ServiceContract.VisaType;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.Country;
using AML.ViewModel.ViewModels.IdentityType;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.DataTable;
using AML.Core.ServiceContract.UserAccess;
using Microsoft.Extensions.Logging;
using AML.DTO.DTO.UserAccess;
using AML.ViewModel.ViewModels.UserAccess;
using AML.ViewModel.ViewModels.Common;
using NToastNotify;
using Microsoft.AspNetCore.Authorization;
using AML.Core.Common.StaticResource;
using AML.Web.CustomFilters;
using Microsoft.AspNetCore.Authentication;

using Microsoft.AspNetCore.Routing;
using AML.Core.ServiceContract.Common;
using System.Threading;
using Microsoft.AspNetCore.Http;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.DataContract.Enum;
using AML.DTO.DTO.Common;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using AML.ViewModel.ViewModels.TransactionMonitor;
using static AML.Core.Service.Common.CommonService;

namespace AML.Web.Controllers.User
{
    public class UserAccessController : Controller
    {
        private readonly ILogger<UserAccessController> _logger;
        private readonly IMapper _mapper;
        private IConfiguration _configuration;
        private AML.Core.ServiceContract.UserAccess.IAuthenticationService _authenticationService;
        private IModuleService _moduleService;
        private IFunctionalityService _functionalityService;
        private IUserGroupRightService _userGroupRightService;
        private IUserGroupService _userGroupService;
        private readonly IToastNotification _toastNotification;
        private readonly IHttpClientHandler _clientHandler;
        ICommonService _commonService;
        string c6BaseURL = string.Empty;
        private ICustomerCaseService _customerCaseService;
        private readonly AML.Web.Services.ChatHistoryService _chatHistoryService;

        const string sessUsername = "";
        const string sessId = "";
        const string sessClientId = "";
        IFileUploader _fileUploader;
        public UserAccessController(AML.Core.ServiceContract.UserAccess.IAuthenticationService authenticationService,
        IToastNotification toastNotification, IModuleService moduleService, ICommonService commonService,
        IFunctionalityService functionalityService, IUserGroupRightService userGroupRightService,
        IUserGroupService userGroupService, IMapper mapper, IHttpClientHandler clientHandler, IConfiguration configuration, ICustomerCaseService customerCaseService, IFileUploader fileUploader, AML.Web.Services.ChatHistoryService chatHistoryService)
        {
            _authenticationService = authenticationService;
            _moduleService = moduleService;
            _functionalityService = functionalityService;
            _userGroupRightService = userGroupRightService;
            _userGroupService = userGroupService;
            _configuration = configuration;
            _chatHistoryService = chatHistoryService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _commonService = commonService;
            c6BaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            _customerCaseService = customerCaseService;
            _fileUploader = fileUploader;
        }
        //[AllowAnonymous]
        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString(StaticResource.sessUserId).IsNotNullOrEmpty() 
                && HttpContext.Session.GetString(StaticResource.sessRoleId).IsNotNullOrEmpty() 
                && HttpContext.Session.GetString(StaticResource.sessUserId).ParseInt() > 0 
                && HttpContext.Session.GetString(StaticResource.sessRoleId).ParseInt() > 0)
            {
                return RedirectToAction("Index", "Home");
            }

            UserModel _UserModel = new UserModel();
            _UserModel.Clients = new SelectList(_customerCaseService.GetAllClients(), "ClientId", "ClientName");
            ViewData["Success"] = "";
            ViewData["Error"] = "";
            return View(_UserModel);
        }
        //[AllowAnonymous]
        [HttpPost]
        public IActionResult Login(UserModel _UserModel)
        {
            if (_UserModel == null || string.IsNullOrEmpty(_UserModel.UserName))
            {
                ViewData["Error"] = "Invalid login attempt.";
                UserModel emptyModel = new UserModel();
                var allClients = _customerCaseService.GetAllClients();
                emptyModel.Clients = new SelectList(allClients, "ClientId", "ClientName");
                return View(emptyModel);
            }
            
            var _UserAccess = (_authenticationService.VerifyUser(_UserModel.UserName, _UserModel.Password, _UserModel.ClientName ?? ""));
            if (_UserAccess.Result != null)
            {              
                // calling C6 Auth
                //new Thread(delegate () {
                //    _commonService.CreateC6Token(ScreeningService.C6AUTHENTICATION, c6BaseURL);
                //}).Start();

                UserModel _UserDetailModel = _mapper.Map<UserModel>(_UserAccess.Result);
                string guid = Guid.NewGuid().ToString();
                _authenticationService.UpdateLogin(_UserDetailModel.Id, guid);
                _clientHandler.SetStringSession("SessID", guid);
                _clientHandler.SetStringSession(StaticResource.sessUserId, _UserDetailModel.Id.ToString());
                _clientHandler.SetStringSession(StaticResource.sessBranchId, _UserDetailModel.BranchId.ToString());
                _clientHandler.SetStringSession(StaticResource.sessRoleId, _UserDetailModel.UserGroupId.ToString());
                _clientHandler.SetStringSession(StaticResource.sessUsername, _UserDetailModel.FName.ToString());
                _clientHandler.SetStringSession(StaticResource.sessClientId, _UserDetailModel.ClientId.ToString());
                _clientHandler.SetStringSession(StaticResource.SessEmail, _UserDetailModel.Email.ToString());
                var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());
                _clientHandler.SetStringSession(StaticResource.SessCompanyName, clientData.ClientName);
                _clientHandler.SetStringSession(StaticResource.SessDescription, clientData.Description);
                _clientHandler.SetStringSession(StaticResource.SessLogoUrl, "/img/"+clientData.DocumentFileName);


                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewData["Error"] = true;
            }
            UserModel _UModel = new UserModel();
            var clients = _customerCaseService.GetAllClients();
            _UModel.Clients = new SelectList(clients, "ClientId", "ClientName");
            return View(_UModel);
        }

        //vidya
        //[AllowAnonymous]
        [Route("auth/verifyOTP")]
        [HttpGet]
        public IActionResult VerifyOTP(string email)
        {
            UserModel _UserModel = new UserModel();
            ViewData["Email"] = email;
            ModelState.Clear();
            return View();
        }

        [HttpPost("auth/verifyOTP")]
        public IActionResult VerifyOTP(VerifyOTPModel model)
        {
            if (ModelState.IsValid)
            {
                var UserID = HttpContext.Session.GetString("sessId");
                var UserName = HttpContext.Session.GetString("sessUsername");
                var ClientId = HttpContext.Session.GetString("sessClientId").ParseInt();
                var result = (_authenticationService.VerifyOTP(UserName,model.Otp, ClientId));                
                if (result.Status == 200)
                {
                    return RedirectToAction("ResetPassword", new RouteValueDictionary(
                    new { controller = "UserAccess", action = "ResetPassword",Id = UserID}));
                }
                else
                {
                    ViewData["Error"] = true;
                }
            }
            return View();
        }




        //vidya


        [SessionAuthorize]
        [HttpGet("auth/changepassword")]
        public IActionResult UpdatePassword()
        {
            ChangePasswordModel _UserModel = new ChangePasswordModel();
            _UserModel.Id = _clientHandler.GetUserId();
            return View("UpdatePassword", _UserModel);
        }
        //[SessionAuthorize]
        [HttpGet("auth/logout")]
        public IActionResult Logout()
        {
            // Clear chat history before logging out
            _chatHistoryService.ClearAllUserChatHistory();
            
            _clientHandler.SetStringSession(StaticResource.sessUserId, "0");
            _clientHandler.SetStringSession(StaticResource.sessBranchId, "0");
            _clientHandler.SetStringSession(StaticResource.sessRoleId, "0");

            return RedirectToAction("Login", "UserAccess");
        }
        [SessionAuthorize]
        [HttpPost("auth/changepassword")]
        public IActionResult UpdatePassword(ChangePasswordModel _UserModel)
        {
            if (ModelState.IsValid)
            {
                var result = (_authenticationService.UpdateUserPassword(_UserModel.Id, _UserModel.NewPassword));
                _toastNotification.AddSuccessToastMessage("Password updated successfully");
            }
            return View("UpdatePassword", _UserModel);
        }
        public IActionResult NotAuthorized()
        {
            return View();
        }
        //[AllowAnonymous]
        [HttpGet("auth/forgot")]
        public IActionResult Forgot()
        {
            ViewData["Error"] = "";
            return View();
        }

        //[AllowAnonymous]
        [HttpPost("auth/forgot")]
        public async Task<IActionResult> Forgot(ForgotModel model)
        {
            if (ModelState.IsValid)
            {
                var result = (_authenticationService.ForgotPassword(model.UserName, model.ClientName));
                if(result.Result != null)
                {
                    string userEmail = null;
                    userEmail = result.Result.email;
                    Random rnd = new Random();
                    int randonNum = (rnd.Next(100000, 999999));
                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/OTPEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };
                    var SaveOTP = (_authenticationService.SaveOTP(model.UserName, randonNum,result.Result.ClientId));
                    HttpContext.Session.SetString("sessId", result.Result.Id.ToString());
                    HttpContext.Session.SetString("sessUsername", model.UserName);
                    HttpContext.Session.SetString("sessClientId", result.Result.ClientId.ToString());
                   

                    await SendOtpEmail(body, userEmail, randonNum);

                    

                    

                    return RedirectToAction("VerifyOTP", new RouteValueDictionary(
                        new { controller = "UserAccess", action = "VerifyOTP",email = result.Result.email }));
                }
                else
                {
                    ViewData["Error"] = true;
                }
            }
            return View();
        }

        private async Task SendOtpEmail(string body, string userEmail,int randonNum)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                var appBaseUrl = MyHttpContext.AppBaseUrl;
                body = body.Replace("{otp}", randonNum.ToString());
                body = body.Replace("{baseUrl}", appBaseUrl);
                body = body.Replace("{Username}", HttpContext.Session.GetString("SessUsername"));

                var emailsent = await _commonService.SendOtpEmail("Reset Password Info", body, userEmail);
            }
        }
        //[AllowAnonymous]
        //[SessionAuthorize]
        public ActionResult ResetPassword(int Id)
        {
            ViewData["Success"] = "";
            ViewData["Error"] = "";
            ResetPasswordModel model = new ResetPasswordModel();
            model.Id = Id;
            return View(model);
        }
        //[AllowAnonymous]
        [HttpPost]
        public ActionResult ResetPassword(ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = HttpContext.Session.GetString("sessId");
                model.Id = Convert.ToInt32(userId);
                var result = (_authenticationService.UpdateUserPassword(model.Id, model.NewPassword));
                if (result.Result == 0)
                {
                    ViewData["Success"] = "Password Updated successfully";
                    return RedirectToAction("Login", "UserAccess");
                }
                else
                {
                    ViewData["Error"] = "The User You Entered Does Not Exist";
                    return View(model);
                }
            }
            else
            {
                return View(model);
            }
        }
        [HttpGet("user/userright")]
        public ActionResult UserRight()
        {
            var clientId = _clientHandler.GetClientId();
            UserGroupRightDetailsModel _UserModel = new  UserGroupRightDetailsModel();
            _UserModel.UserGroups = new SelectList(_mapper.Map<List<UserGroupModel>>(_userGroupService.GetAll(clientId)), "Id", "Name");
            _UserModel.Modules = new SelectList(_mapper.Map<List<ModuleModel>>(_moduleService.GetAll()), "Id", "Name");
            _UserModel.Functionalities = new SelectList(_mapper.Map<List<FunctionalityModel>>(_functionalityService.GetAll(clientId).Result), "Id", "Name");
            return View(_UserModel);
        }
        [HttpPost("user/userright")]
        public ActionResult UserRight(UserGroupRightDetailsModel _UserModel)
        {
            if (ModelState.IsValid)
            {
                var userModel = new UserGroupRightDTO()
                {
                    UserGroupId = _UserModel.UserGroupId,
                    ModuleId = _UserModel.ModuleId,
                    FunctionalityId = _UserModel.FunctionalityId
                };
                _userGroupRightService.Create(_mapper.Map<UserGroupRightDTO>(userModel));
                _toastNotification.AddSuccessToastMessage("User right added successfully");
            }
            return View(_UserModel);
        }
    }
}
