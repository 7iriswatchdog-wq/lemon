
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
using NToastNotify;
using AML.Web.CustomFilters;
using Microsoft.AspNetCore.Authorization;
using AML.ViewModel.ViewModels.Common;
using AML.Core.ServiceContract.CustomerCase;
using System.Reflection;
using NLog;

namespace AML.Web.Controllers.User
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class UserController : Controller
    {
        private IUserService _userService;
        private IDepartmentService _departmentService;
        private IDesignationService _designationService;
        private IBranchService _branchService;
        private IUserGroupService _usergroupService;
        private IVisaTypeService _visatypeService;
        private IIdentityTypeService _identitytypeService;
        private ICountryService _countryService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private ICustomerCaseService _customerCaseService;
        private IConfiguration _configuration;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        public UserController(IUserService userService, IDepartmentService departmentService,
            IToastNotification toastNotification,
            IDesignationService designationService, IBranchService branchService,
            IUserGroupService usergroupService, IVisaTypeService visatypeService,
            IIdentityTypeService identitytypeService, ICountryService countryService,
            IMapper mapper, IConfiguration configuration, IHttpClientHandler clientHandler, ICustomerCaseService customerCaseService)
        {
            _userService = userService;
            _departmentService = departmentService;
            _designationService = designationService;
            _branchService = branchService;
            _usergroupService = usergroupService;
            _visatypeService = visatypeService;
            _identitytypeService = identitytypeService;
            _countryService = countryService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _customerCaseService = customerCaseService;
            _configuration = configuration;
        }
        [AllowAnonymous]
        public IActionResult Index(string isActive)
        {
            var userId = _clientHandler.GetUserId();
            var userDetails = _userService.GetDetails(userId);
            UserModel _UserModel = new UserModel();
            _UserModel.isSuperAdmin = userDetails.isSuperAdmin;
            _UserModel.Clients = new SelectList(_customerCaseService.GetAllClients(), "ClientId", "ClientName");
            _UserModel.ClientId = _clientHandler.GetUserId();
            List<UserModel> _UserModelList = new List<UserModel>();
            _UserModelList.Add(_UserModel);
            return View(_UserModel);
        }
        [HttpPost("user/custompagination")]
        public JsonResult CustomPagination(DataTableModel model,int Id)
        {
            var clientId = 0;
            if (Id == 0)
            {
                clientId = _clientHandler.GetClientId();
            }
            else
            {
                clientId = Id;
            }
            List<UserModel> users = _mapper.Map<List<UserModel>>(_userService.GetAll(clientId));
            users = users.Where(a => a.IsDeleted == 0 || a.IsBlocked == 0).ToList();
            if (!string.IsNullOrEmpty(model.search.value))
            {
                users = users.Where(m => m.UserName.ToLower().Contains(model.search.value.ToLower())
                || m.FName.ToLower().Contains(model.search.value.ToLower()) || m.LName.ToLower().Contains(model.search.value.ToLower())
                || m.EmpCode.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var data = Sort(users, model.columns[model.order[0].column].data ?? "empCode", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();
            //var data = users.Skip(model.start).Take(model.length).ToList();
            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = users.Count,//totalResultsCount,
                recordsFiltered = users.Count,//filteredResultsCount,
                data = data,
            });
        }
        public List<T> Sort<T>(List<T> input, string property, string dir)

        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty.GetValue(p, null)).ToList();

        }

        [AllowAnonymous]
        [HttpGet("user/list")]
        public IActionResult List()
        {
            var clientId = _clientHandler.GetClientId();
            List<UserModel> _UserModel = _mapper.Map<List<UserModel>>(_userService.GetAll(clientId));
            return View(_UserModel);
        }

        [HttpGet("user/view/{Id}")]
        public IActionResult View(int Id)
        {
            UserModel _UserModel = _mapper.Map<UserModel>(_userService.GetDetails(Id));
            return View(_UserModel);
        }

        [HttpGet("user/edit/{Id}")]
        public IActionResult Edit(int Id)
        {          
            var clientId = _clientHandler.GetClientId();
            
            UserModel _UserModel = _mapper.Map<UserModel>(_userService.GetDetails(Id));
            _UserModel.Clients = new SelectList(_customerCaseService.GetAllClients(), "ClientId", "ClientName");
            _UserModel.Departments = new SelectList(_mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(clientId)), "Id", "Name");
            _UserModel.Designations = new SelectList(_mapper.Map<List<DesignationModel>>(_designationService.GetAll(clientId)), "Id", "Name");
            _UserModel.Branches = new SelectList(_mapper.Map<List<BranchModel>>(_branchService.GetAll(clientId)), "Id", "Name");
            _UserModel.UserGroup = new SelectList(_mapper.Map<List<UserGroupModel>>(_usergroupService.GetAll(clientId)), "Id", "Name");
            _UserModel.VisaTypes = new SelectList(_mapper.Map<List<VisaTypeModel>>(_visatypeService.GetAll(clientId)), "Id", "Name");
            _UserModel.IdentityTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_identitytypeService.GetAll(clientId)), "Id", "Name");
            _UserModel.Countries = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
            _UserModel.UserContactDetails = _configuration["UserContactDetails"];
            _UserModel.UserIdentityDetails = _configuration["userIdentityDetails"];
            _UserModel.UserVisaDetails = _configuration["userVisaDetails"];
            return View("Add",_UserModel);
        }
        
        [HttpGet("user/create")]
        public IActionResult Add(int? clientId)
        {
            var cid = clientId ?? _clientHandler.GetClientId();
             UserModel _UserModel = new UserModel();
             _UserModel.ClientId = cid;
            _UserModel.Departments = new SelectList(_mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(cid)), "Id", "Name");
            _UserModel.Designations = new SelectList(_mapper.Map<List<DesignationModel>>(_designationService.GetAll(cid)), "Id", "Name") ;
            _UserModel.Branches = new SelectList(_mapper.Map<List<BranchModel>>(_branchService.GetAll(cid)), "Id", "Name") ;
            _UserModel.UserGroup = new SelectList(_mapper.Map<List<UserGroupModel>>(_usergroupService.GetAll(cid)),"Id", "Name");
            _UserModel.VisaTypes = new SelectList(_mapper.Map<List<VisaTypeModel>>(_visatypeService.GetAll(cid)),"Id", "Name");
            _UserModel.IdentityTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_identitytypeService.GetAll(cid)),"Id", "Name");
            _UserModel.Countries = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(cid)),"Id", "Name");
            var userId = _clientHandler.GetUserId();
            var userDetails = _userService.GetDetails(userId);
            _UserModel.isSuperAdmin = userDetails.isSuperAdmin;
            _UserModel.Clients = new SelectList(_customerCaseService.GetAllClients(), "ClientId", "ClientName");
            return View(_UserModel);
        }


        [HttpPost("user/create")]
        public IActionResult Add(UserModel _UserModel)
        {
            var clientId = _clientHandler.GetClientId();
            if (_UserModel.ClientId == 0)
            {
                _UserModel.ClientId = clientId;
            }
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            //if (ModelState.IsValid)
            //{
                UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
                log.Info($"'{_EditorUserModel.UserName}' is creating/updating user '{_UserModel.UserName}'");
                if (_UserModel.Id > 0)
                {
                    _userService.Update(_mapper.Map<UserDTO>(_UserModel));
                    log.Info("User updated successfully");
                    _toastNotification.AddSuccessToastMessage("User updated successfully");
                }
                else
                {
                    if (_UserModel.Password == _UserModel.RePassword)
                    {
                        var user = _userService.Create(_mapper.Map<UserDTO>(_UserModel));
                        if (user.Result > 0)
                        {
                            log.Info("User added successfully");
                            _toastNotification.AddSuccessToastMessage("User added successfully");
                        }
                        else
                        {
                            log.Info("Username already exist");
                            _toastNotification.AddErrorToastMessage("Username already exist");
                            _UserModel.Departments = new SelectList(_mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(clientId)), "Id", "Name");
                            _UserModel.Designations = new SelectList(_mapper.Map<List<DesignationModel>>(_designationService.GetAll(clientId)), "Id", "Name");
                            _UserModel.Branches = new SelectList(_mapper.Map<List<BranchModel>>(_branchService.GetAll(clientId)), "Id", "Name");
                            _UserModel.UserGroup = new SelectList(_mapper.Map<List<UserGroupModel>>(_usergroupService.GetAll(clientId)), "Id", "Name");
                            _UserModel.VisaTypes = new SelectList(_mapper.Map<List<VisaTypeModel>>(_visatypeService.GetAll(clientId)), "Id", "Name");
                            _UserModel.IdentityTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_identitytypeService.GetAll(clientId)), "Id", "Name");
                            _UserModel.Countries = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
                            _UserModel.Clients = new SelectList(_customerCaseService.GetAllClients(), "ClientId", "ClientName");
                            var userId1 = _clientHandler.GetUserId();
                            var userDetails1 = _userService.GetDetails(userId1);
                            _UserModel.isSuperAdmin = userDetails1.isSuperAdmin;
                            return View(_UserModel);
                        }
                    }
                    else
                    {
                        log.Info("The password and confirmation password do not match.");
                        _toastNotification.AddSuccessToastMessage("The password and confirmation password do not match.");
                        
                    }
                }
                
                return RedirectToAction("Details", "AdminManagement", new { id = _UserModel.ClientId });
            //}
            _UserModel.Departments = new SelectList(_mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(clientId)), "Id", "Name");
            _UserModel.Designations = new SelectList(_mapper.Map<List<DesignationModel>>(_designationService.GetAll(clientId)), "Id", "Name");
            _UserModel.Branches = new SelectList(_mapper.Map<List<BranchModel>>(_branchService.GetAll(clientId)), "Id", "Name");
            _UserModel.UserGroup = new SelectList(_mapper.Map<List<UserGroupModel>>(_usergroupService.GetAll(clientId)), "Id", "Name");
            _UserModel.VisaTypes = new SelectList(_mapper.Map<List<VisaTypeModel>>(_visatypeService.GetAll(clientId)), "Id", "Name");
            _UserModel.IdentityTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_identitytypeService.GetAll(clientId)), "Id", "Name");
            _UserModel.Countries = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
            _UserModel.Clients = new SelectList(_customerCaseService.GetAllClients(), "ClientId", "ClientName");
            var userId = _clientHandler.GetUserId();
            var userDetails = _userService.GetDetails(userId);
            _UserModel.isSuperAdmin = userDetails.isSuperAdmin;
            return View(_UserModel);
        }
        [HttpDelete("user/delete/{id}")]
        public ActionResult Delete(int id)
        {
            try
            {
                UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
                UserModel _UserModel = _mapper.Map<UserModel>(_userService.GetDetails(id));



               // UserModel _UserModel = new UserModel();
                _UserModel.Id = id;
                _UserModel.IsDeleted = 1;
                _UserModel.UpdatedBy = 1;
                var result = _userService.Delete(_mapper.Map<UserDTO>(_UserModel));
                _toastNotification.AddSuccessToastMessage("User deleted successfully");
                log.Info("User deleted successfully");
                //return RedirectToAction(nameof(Index));
                return Json("Success");
            }
            catch
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Block(int Id, int Status)
        {
            try
            {
                // to do functionality to block user
               // UserModel _UserModel = new UserModel();

                UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
                UserModel _UserModel = _mapper.Map<UserModel>(_userService.GetDetails(Id));

                log.Info($"User '{_EditorUserModel.UserName}' changed block status for user '{_UserModel.UserName}' to {Status}");

                _UserModel.Id = Id;
                _UserModel.IsBlocked = Status;
                _UserModel.UpdatedBy = 1;
                var result = _userService.BlockUser(_mapper.Map<UserDTO>(_UserModel));
                if(Status == 1)
                {
                    log.Info("User blocked successfully");
                    _toastNotification.AddSuccessToastMessage("User blocked successfully");
                }
                else
                {
                    log.Info("User unblocked successfully");
                    _toastNotification.AddSuccessToastMessage("User unblocked successfully");
                }

                
                //return RedirectToAction(nameof(Index));
                return Json("Success");
            }
            catch
            {
                return View();
            }
        }
        
    }
}
