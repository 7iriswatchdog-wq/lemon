using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Department;
using AML.Core.ServiceContract.Designation;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.ServiceContract.VisaType;
using AML.DTO.DTO.User;
using AML.ViewModel.ViewModels.Branch;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.Department;
using AML.ViewModel.ViewModels.Designation;
using AML.ViewModel.ViewModels.IdentityType;
using AML.ViewModel.ViewModels.User;
using AML.ViewModel.ViewModels.UserGroup;
using AML.ViewModel.ViewModels.VisaType;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AML.Web.Controllers.AdminManagement
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class AdminManagementController : Controller
    {
        private readonly IUserService _userService;
        private readonly IDepartmentService _departmentService;
        private readonly IDesignationService _designationService;
        private readonly IBranchService _branchService;
        private readonly IUserGroupService _usergroupService;
        private readonly IVisaTypeService _visatypeService;
        private readonly IIdentityTypeService _identitytypeService;
        private readonly ICountryService _countryService;
        private readonly IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private readonly IHttpClientHandler _clientHandler;
        private readonly ICustomerCaseService _customerCaseService;
        private readonly IConfiguration _configuration;
        private readonly IFileUploader _fileUploader;

        public AdminManagementController(IUserService userService, IDepartmentService departmentService,
            IToastNotification toastNotification,
            IDesignationService designationService, IBranchService branchService,
            IUserGroupService usergroupService, IVisaTypeService visatypeService,
            IIdentityTypeService identitytypeService, ICountryService countryService,
            IMapper mapper, IConfiguration configuration, IHttpClientHandler clientHandler, 
            ICustomerCaseService customerCaseService, IFileUploader fileUploader)
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
            _fileUploader = fileUploader;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("adminmanagement/custompagination")]
        public JsonResult CustomPagination(DataTableModel model, int orderColumn = 0, string orderDirection = "desc", string clientStatus = null)
        {
            try
            {
                var result = _customerCaseService.GetAllAdminClients();
                var clients = result.Select(dto => new ClientMaster
                    {
                        ClientId = dto.ClientId,
                        ClientName = dto.ClientName,
                        Prefix = dto.Prefix,
                        ApplicationStartDate = dto.ApplicationStartDate,
                        ApplicationEndDate = dto.ApplicationEndDate,
                        SearchCount = dto.SearchCount,
                        UsageCount = dto.UsageCount,
                        UserCount = dto.UserCount,
                        isActive = dto.isActive
                    }).ToList();

                if (!string.IsNullOrEmpty(clientStatus))
                {
                    int s = int.Parse(clientStatus);
                    clients = clients.Where(c => c.isActive == s).ToList();
                }

                if (!string.IsNullOrEmpty(model.search?.value))
                {
                    clients = clients.Where(m => m.ClientName.ToLower().Contains(model.search.value.ToLower())
                    || m.Prefix.ToLower().Contains(model.search.value.ToLower())).ToList();
                }

                IEnumerable<ClientMaster> sortedClients;
                switch (orderColumn)
                {
                    case 0: sortedClients = orderDirection == "asc" ? clients.OrderBy(x => x.ClientId) : clients.OrderByDescending(x => x.ClientId); break;
                    case 1: sortedClients = orderDirection == "asc" ? clients.OrderBy(x => x.ClientName ?? string.Empty) : clients.OrderByDescending(x => x.ClientName ?? string.Empty); break;
                    case 2: sortedClients = orderDirection == "asc" ? clients.OrderBy(x => x.Prefix ?? string.Empty) : clients.OrderByDescending(x => x.Prefix ?? string.Empty); break;
                    default: sortedClients = clients.OrderByDescending(x => x.ClientId); break;
                }

                int recordsTotal = clients.Count;
                int recordsFiltered = clients.Count;

                var query = sortedClients.Skip(model.start);
                if (model.length > 0)
                {
                    query = query.Take(model.length);
                }
                var pagedData = query.ToList();

                return Json(new { draw = model.draw, recordsTotal = recordsTotal, recordsFiltered = recordsFiltered, data = pagedData });
            }
            catch (Exception ex)
            {
                // Better error logging for debugging
                return Json(new { draw = model.draw, recordsTotal = 0, recordsFiltered = 0, data = new List<ClientMaster>(), error = ex.Message + " | " + ex.InnerException?.Message });
            }
        }

        [HttpGet("adminmanagement/details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var result = _customerCaseService.GetClientDetailsByID(id);
            if (result == null) return NotFound();

            ClientMaster _clientModel = new ClientMaster();
            _clientModel.ClientId = result.ClientId;
            _clientModel.ClientName = result.ClientName;
            _clientModel.Prefix = result.Prefix;
            _clientModel.C6Threshold = result.C6Threshold;
            _clientModel.Threshold = result.Threshold;
            _clientModel.C6Username = result.C6Username;
            _clientModel.Description = result.Description;
            _clientModel.Complem = result.Complem;
            _clientModel.C6BaseUrl = result.C6BaseUrl;
            _clientModel.DocumentFileName = result.DocumentFileName;

            ViewBag.ClientId = id;
            
            // SelectLists for the embedded User Create/Edit modal/form
            ViewBag.Departments = new SelectList(_mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(id)), "Id", "Name");
            ViewBag.Designations = new SelectList(_mapper.Map<List<DesignationModel>>(_designationService.GetAll(id)), "Id", "Name");
            ViewBag.Branches = new SelectList(_mapper.Map<List<BranchModel>>(_branchService.GetAll(id)), "Id", "Name");
            ViewBag.UserGroup = new SelectList(_mapper.Map<List<UserGroupModel>>(_usergroupService.GetAll(id)), "Id", "Name");
            ViewBag.VisaTypes = new SelectList(_mapper.Map<List<VisaTypeModel>>(_visatypeService.GetAll(id)), "Id", "Name");
            ViewBag.IdentityTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_identitytypeService.GetAll(id)), "Id", "Name");
            ViewBag.Countries = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(id)), "Id", "Name");

            TokenRS token = await AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, _clientModel.C6BaseUrl, _clientModel.C6Username);
            if (token.status == 400)
            {
                ViewBag.userlimit = "NA";
                ViewBag.totalcount = "NA";
                ViewBag.individualCount = "NA";
                ViewBag.corporateCount = "NA";

                _toastNotification.AddErrorToastMessage(token.message + ". Please contact the Watchdog Administrator for assistance.");
                return View(_clientModel);
            }
            ViewBag.userlimit = Convert.ToInt32(token.user.userLimit);
            ViewBag.totalcount = Convert.ToInt32(token.user.individualCount) + Convert.ToInt32(token.user.corporateCount);
            ViewBag.individualCount = Convert.ToInt32(token.user.individualCount);
            ViewBag.corporateCount =  Convert.ToInt32(token.user.corporateCount);

            return View(_clientModel);
        }

        [HttpPost("adminmanagement/userpagination")]
        public JsonResult UserPagination(DataTableModel model, int Id)
        {
            try
            {
                int clientId = Id == 0 ? _clientHandler.GetClientId() : Id;
                List<UserModel> users = _mapper.Map<List<UserModel>>(_userService.GetAll(clientId));
                users = users.Where(a => a.IsDeleted == 0 || a.IsBlocked == 0).ToList();

                if (!string.IsNullOrEmpty(model.search?.value))
                {
                    users = users.Where(m => m.UserName.ToLower().Contains(model.search.value.ToLower())
                    || m.FName.ToLower().Contains(model.search.value.ToLower()) || m.LName.ToLower().Contains(model.search.value.ToLower())
                    || m.EmpCode.ToLower().Contains(model.search.value.ToLower())).ToList();
                }

                var data = Sort(users, model.columns[model.order[0].column].data ?? "empCode", model.order[0].dir ?? "asc")
                    .Skip(model.start)
                    .Take(model.length)
                    .ToList();

                return Json(new { draw = model.draw, recordsTotal = users.Count, recordsFiltered = users.Count, data = data });
            }
            catch (Exception ex)
            {
                return Json(new { draw = model.draw, recordsTotal = 0, recordsFiltered = 0, data = new List<UserModel>(), error = ex.Message });
            }
        }

        private List<T> Sort<T>(List<T> input, string property, string dir)
        {
            var type = typeof(T);
            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
            if (sortProperty == null) return input;
            return dir == "asc" ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList() : input.OrderByDescending(p => sortProperty.GetValue(p, null)).ToList();
        }
    }
}
