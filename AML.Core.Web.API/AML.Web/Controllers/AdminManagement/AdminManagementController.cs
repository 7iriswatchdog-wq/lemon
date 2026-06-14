using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Department;
using AML.Core.ServiceContract.Designation;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.LovMaster;
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
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NToastNotify;
using System.Threading.Tasks;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

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
        private string baseURL = string.Empty;
        private string pdfbaseURL = string.Empty;
        private string baseC6URL = string.Empty;
        private string _c6Username;
        private int checkThreshold = 0;
        private ILovMasterService _lovMasterService;


        public AdminManagementController(IUserService userService, IDepartmentService departmentService,
            IToastNotification toastNotification,
            IDesignationService designationService, IBranchService branchService,
            IUserGroupService usergroupService, IVisaTypeService visatypeService,
            IIdentityTypeService identitytypeService, ICountryService countryService,
            IMapper mapper, IConfiguration configuration, IHttpClientHandler clientHandler, 
            ICustomerCaseService customerCaseService, IFileUploader fileUploader, ILovMasterService lovMasterService)
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
            _lovMasterService = lovMasterService;
            var clientId = _clientHandler.GetClientId();
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            _c6Username = clientDetails?.C6Username;
            checkThreshold = clientDetails?.Threshold ?? 0;
            baseC6URL = clientDetails?.C6BaseUrl;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("adminmanagement/custompagination")]
        public async Task<JsonResult> CustomPagination(DataTableModel model, int orderColumn = 0, string orderDirection = "desc", string subStatus = null, string isBlocked = null)
        {
            try
            {
                TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                string url = baseC6URL + "users";


                var apiUsers = await _clientHandler.GetAsync(token, url);

                var users = JsonConvert.DeserializeObject<List<dynamic>>(apiUsers);

                var userUsageDict = ((IEnumerable<dynamic>)users)
     .GroupBy(x => ((string)x.username).ToLower())
     .ToDictionary(
         g => g.Key,
         g => (int)g.First().individualUsageCount + (int)g.First().corporateUsageCount
     );
                var result = _customerCaseService.GetAllAdminClients();
                var clients = result.Select(dto =>
                    {
                        int usageCount = 0;

                        if (!string.IsNullOrEmpty(dto.C6Username) &&
    userUsageDict.TryGetValue(dto.C6Username.ToLower(), out int apiUsage))
                        {
                            usageCount = apiUsage;
                        }

                        return new ClientMaster
                        {
                            ClientId = dto.ClientId,
                            ClientName = dto.ClientName,
                            Prefix = dto.Prefix,
                            C6Username=dto.C6Username,
                            ApplicationStartDate = dto.ApplicationStartDate,
                            ApplicationEndDate = dto.ApplicationEndDate,
                            SearchCount = dto.SearchCount,
                            TotalUsageCount = usageCount,   // API total usage
                            UserCount = dto.UserCount,
                            isActive = (dto.isActive == 1 ? 1 : 0)
                        };
                    }).ToList();

                // 1. Filter by Subscription Status (Active/Expired)
                if (!string.IsNullOrEmpty(subStatus))
                {
                    var today = DateTime.Now.Date;
                    if (subStatus == "active")
                    {
                        clients = clients.Where(c => !c.ApplicationEndDate.HasValue || c.ApplicationEndDate.Value.Date >= today).ToList();
                    }
                    else if (subStatus == "expired")
                    {
                        clients = clients.Where(c => c.ApplicationEndDate.HasValue && c.ApplicationEndDate.Value.Date < today).ToList();
                    }
                }

                // 2. Filter by Blocked Status (Active/Blocked)
                if (!string.IsNullOrEmpty(isBlocked))
                {
                    int s = int.Parse(isBlocked);
                    clients = clients.Where(c => (s == 1 ? c.isActive == 1 : c.isActive == 0)).ToList();
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
            _clientModel.SearchCount = result.SearchCount;
            _clientModel.ApplicationStartDate = result.ApplicationStartDate;
            _clientModel.ApplicationEndDate = result.ApplicationEndDate;
            _clientModel.CreatedOn = result.CreatedOn;

            ViewBag.ClientId = id;
            
            // SelectLists for the embedded User Create/Edit modal/form
            ViewBag.Departments = new SelectList(_mapper.Map<List<DepartmentModel>>(_departmentService.GetAll(id)), "Id", "Name");
            ViewBag.Designations = new SelectList(_mapper.Map<List<DesignationModel>>(_designationService.GetAll(id)), "Id", "Name");
            ViewBag.Branches = new SelectList(_mapper.Map<List<BranchModel>>(_branchService.GetAll(id)), "Id", "Name");
            ViewBag.UserGroup = new SelectList(_mapper.Map<List<UserGroupModel>>(_usergroupService.GetAll(id)), "Id", "Name");
            ViewBag.VisaTypes = new SelectList(_mapper.Map<List<VisaTypeModel>>(_visatypeService.GetAll(id)), "Id", "Name");
            ViewBag.IdentityTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_identitytypeService.GetAll(id)), "Id", "Name");
            ViewBag.Countries = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(id)), "Id", "Name");

            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, _clientModel.C6BaseUrl, _clientModel.C6Username);
            if (token.status == 400)
            {
                ViewBag.userlimit = "NA";
                ViewBag.totalcount = "NA";
                ViewBag.individualCount = "NA";
                ViewBag.corporateCount = "NA";
                ViewBag.hundredemail = "--";
                ViewBag.beforeexpdemail = "--";
                ViewBag.onexpemail = "--";
                ViewBag.Eightemail = "--";

                _toastNotification.AddErrorToastMessage(token.message + ". Please contact the Search Administrator for assistance.");
                return View(_clientModel);
            }
            ViewBag.userlimit = Convert.ToInt32(token.user.userLimit);
            ViewBag.totalcount = Convert.ToInt32(token.user.individualCount) + Convert.ToInt32(token.user.corporateCount);
            ViewBag.individualCount = Convert.ToInt32(token.user.individualCount);
            ViewBag.corporateCount =  Convert.ToInt32(token.user.corporateCount);
            ViewBag.contractExpiryDate = token.user.contractExpiryDate;
            ViewBag.hundredemail =
     string.IsNullOrEmpty(token.user?.hundredemail) || token.user.hundredemail == "0"
     ? "--"
     : DateTime.Parse(token.user.hundredemail).ToString("dd/MM/yyyy HH:mm:ss");

            ViewBag.beforeexpdemail =
                string.IsNullOrEmpty(token.user?.beforeexpdemail) || token.user.beforeexpdemail == "0"
                ? "--"
                : DateTime.Parse(token.user.beforeexpdemail).ToString("dd/MM/yyyy HH:mm:ss");

            ViewBag.onexpemail =
                string.IsNullOrEmpty(token.user?.onexpemail) || token.user.onexpemail == "0"
                ? "--"
                : DateTime.Parse(token.user.onexpemail).ToString("dd/MM/yyyy HH:mm:ss");

            ViewBag.Eightemail =
                string.IsNullOrEmpty(token.user?.Eightemail) || token.user.Eightemail == "0"
                ? "--"
                : DateTime.Parse(token.user.Eightemail).ToString("dd/MM/yyyy HH:mm:ss");

            var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            _clientModel.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
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
        [HttpGet]
        public async Task<IActionResult> AdminManagement_PDF(string searchValue, string subStatus, string isBlocked, string selectedColumns, string orientation)
        {
            ViewBag.SelectedColumns = selectedColumns;
            ViewBag.Orientation = orientation;
            try
            {
                TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                string url = baseC6URL + "users";
                var apiUsers = await _clientHandler.GetAsync(token, url);
                var users = JsonConvert.DeserializeObject<List<dynamic>>(apiUsers);

                var userUsageDict = ((IEnumerable<dynamic>)users)
                    .GroupBy(x => ((string)x.username).ToLower())
                    .ToDictionary(
                        g => g.Key,
                        g => (int)g.First().individualUsageCount + (int)g.First().corporateUsageCount
                    );

                var result = _customerCaseService.GetAllAdminClients();

                // Apply Filters
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    result = result.Where(x => 
                        (x.ClientName != null && x.ClientName.ToLower().Contains(searchValue)) ||
                        (x.Prefix != null && x.Prefix.ToLower().Contains(searchValue)) ||
                        (x.ClientId.ToString().Contains(searchValue))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(subStatus))
                {
                    var today = DateTime.Today;
                    if (subStatus == "active")
                        result = result.Where(x => x.ApplicationEndDate >= today).ToList();
                    else if (subStatus == "expired")
                        result = result.Where(x => x.ApplicationEndDate < today).ToList();
                }

                if (!string.IsNullOrEmpty(isBlocked))
                {
                    int blockedStatus = int.Parse(isBlocked);
                    result = result.Where(x => (blockedStatus == 1 ? x.isActive == 1 : x.isActive == 0)).ToList();
                }

                var clients = result.Select(dto =>
                {
                    int usageCount = 0;
                    if (!string.IsNullOrEmpty(dto.C6Username) &&
                        userUsageDict.TryGetValue(dto.C6Username.ToLower(), out int apiUsage))
                    {
                        usageCount = apiUsage;
                    }

                    return new ClientMaster
                    {
                        ClientId = dto.ClientId,
                        ClientName = dto.ClientName,
                        Prefix = dto.Prefix,
                        C6Username = dto.C6Username,
                        ApplicationStartDate = dto.ApplicationStartDate,
                        ApplicationEndDate = dto.ApplicationEndDate,
                        SearchCount = dto.SearchCount,
                        TotalUsageCount = usageCount,
                        UserCount = dto.UserCount,
                        isActive = (dto.isActive == 1 ? 1 : 0)
                    };
                }).ToList();

                return View("AdminManagement_PDF", clients);
            }
            catch
            {
                var result = _customerCaseService.GetAllAdminClients();

                // Apply Filters in Catch block as well
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    result = result.Where(x => 
                        (x.ClientName != null && x.ClientName.ToLower().Contains(searchValue)) ||
                        (x.Prefix != null && x.Prefix.ToLower().Contains(searchValue)) ||
                        (x.ClientId.ToString().Contains(searchValue))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(subStatus))
                {
                    var today = DateTime.Today;
                    if (subStatus == "active")
                        result = result.Where(x => x.ApplicationEndDate >= today).ToList();
                    else if (subStatus == "expired")
                        result = result.Where(x => x.ApplicationEndDate < today).ToList();
                }

                if (!string.IsNullOrEmpty(isBlocked))
                {
                    int blockedStatus = int.Parse(isBlocked);
                    result = result.Where(x => (blockedStatus == 1 ? x.isActive == 1 : x.isActive == 0)).ToList();
                }

                var clients = result.Select(dto => new ClientMaster
                {
                    ClientId = dto.ClientId,
                    ClientName = dto.ClientName,
                    Prefix = dto.Prefix,
                    C6Username = dto.C6Username,
                    ApplicationStartDate = dto.ApplicationStartDate,
                    ApplicationEndDate = dto.ApplicationEndDate,
                    SearchCount = dto.SearchCount,
                    UserCount = dto.UserCount,
                    isActive = (dto.isActive == 1 ? 1 : 0)
                }).ToList();
                return View("AdminManagement_PDF", clients);
            }
        }

        [HttpGet]
        public async Task<IActionResult> AdminManagement_Excel(string searchValue, string subStatus, string isBlocked)
        {
            try
            {
                TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                string url = baseC6URL + "users";
                var apiUsers = await _clientHandler.GetAsync(token, url);
                var users = JsonConvert.DeserializeObject<List<dynamic>>(apiUsers);

                var userUsageDict = ((IEnumerable<dynamic>)users)
                    .GroupBy(x => ((string)x.username).ToLower())
                    .ToDictionary(
                        g => g.Key,
                        g => (int)g.First().individualUsageCount + (int)g.First().corporateUsageCount
                    );

                var result = _customerCaseService.GetAllAdminClients();

                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    result = result.Where(x =>
                        (x.ClientName != null && x.ClientName.ToLower().Contains(searchValue)) ||
                        (x.Prefix != null && x.Prefix.ToLower().Contains(searchValue)) ||
                        (x.ClientId.ToString().ToLower().Contains(searchValue))
                    ).ToList();
                }

                if (!string.IsNullOrEmpty(subStatus))
                {
                    var today = DateTime.Today;
                    if (subStatus == "active")
                        result = result.Where(x => x.ApplicationEndDate >= today).ToList();
                    else if (subStatus == "expired")
                        result = result.Where(x => x.ApplicationEndDate < today).ToList();
                }

                if (!string.IsNullOrEmpty(isBlocked))
                {
                    int blockedStatus = int.Parse(isBlocked);
                    result = result.Where(x => x.isActive == blockedStatus).ToList();
                }

                var clients = result.Select(dto =>
                {
                    int usageCount = 0;
                    if (!string.IsNullOrEmpty(dto.C6Username) &&
                        userUsageDict.TryGetValue(dto.C6Username.ToLower(), out int apiUsage))
                    {
                        usageCount = apiUsage;
                    }

                    return new ClientMaster
                    {
                        ClientId = dto.ClientId,
                        ClientName = dto.ClientName,
                        Prefix = dto.Prefix,
                        C6Username = dto.C6Username,
                        ApplicationStartDate = dto.ApplicationStartDate,
                        ApplicationEndDate = dto.ApplicationEndDate,
                        SearchCount = dto.SearchCount,
                        TotalUsageCount = usageCount,
                        UserCount = dto.UserCount,
                        isActive = dto.isActive
                    };
                }).ToList();

                using (var package = new ExcelPackage())
                {
                    var sheet = package.Workbook.Worksheets.Add("Clients");
                    string[] headers = { "Client ID", "Client Name", "Prefix", "Users", "Limit", "Usage", "Start Date", "End Date", "Status", "Blocked" };
                    for (int i = 0; i < headers.Length; i++)
                    {
                        sheet.Cells[1, i + 1].Value = headers[i];
                    }

                    using (var range = sheet.Cells[1, 1, 1, headers.Length])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(global::System.Drawing.Color.FromArgb(0xE9, 0xEF, 0xFD));
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.White);
                    }

                    int row = 2;
                    foreach (var c in clients)
                    {
                        sheet.Cells[row, 1].Value = c.ClientId;
                        sheet.Cells[row, 2].Value = c.ClientName;
                        sheet.Cells[row, 3].Value = c.Prefix;
                        sheet.Cells[row, 4].Value = c.UserCount;
                        sheet.Cells[row, 5].Value = c.SearchCount;
                        sheet.Cells[row, 6].Value = c.TotalUsageCount;
                        sheet.Cells[row, 7].Value = c.ApplicationStartDate?.ToString("dd MMM yyyy");
                        sheet.Cells[row, 8].Value = c.ApplicationEndDate?.ToString("dd MMM yyyy");
                        sheet.Cells[row, 9].Value = (c.ApplicationEndDate >= DateTime.Today) ? "Active" : "Expired";
                        sheet.Cells[row, 10].Value = (c.isActive == 1) ? "Unblocked" : "Blocked";
                        row++;
                    }

                    sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
                    sheet.View.FreezePanes(2, 1);

                    return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"AdminClients_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating excel: " + ex.Message);
            }
        }
    }
}

