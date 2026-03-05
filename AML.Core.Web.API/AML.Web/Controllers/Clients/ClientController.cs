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
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCase;
using AML.DTO.DTO.Common;
using Newtonsoft.Json;
using AML.Core.DataContract.Enum;
using System.Reflection;

namespace AML.Web.Controllers.Client
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class ClientController : Controller
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
        IFileUploader _fileUploader;
        public ClientController(IUserService userService, IDepartmentService departmentService,
            IToastNotification toastNotification,
            IDesignationService designationService, IBranchService branchService,
            IUserGroupService usergroupService, IVisaTypeService visatypeService,
            IIdentityTypeService identitytypeService, ICountryService countryService,
            IMapper mapper, IConfiguration _configuration, IHttpClientHandler clientHandler, ICustomerCaseService customerCaseService, IFileUploader fileUploader)
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
            _fileUploader = fileUploader;
        }
        [AllowAnonymous]
        public IActionResult Index(string isActive)
        {
            return View();
        }
        [HttpPost("client/custompagination")]
        public JsonResult CustomPagination(DataTableModel model,
    int orderColumn = 0, string orderDirection = "desc")
        {
            try
            {
                // Convert DTOs to ViewModels with all properties mapped
                var clients = _customerCaseService.GetAllClients()
                    .Select(dto => new ClientMaster
                    {
                        ClientId = dto.ClientId,
                        ClientName = dto.ClientName,
                        Prefix = dto.Prefix
                        // Add all other properties from DTO to ViewModel
                       
                    }).ToList();

                // Apply search filter if any

                // Apply sorting
                IOrderedEnumerable<ClientMaster> sortedClients;
                switch (orderColumn)
                {
                    case 0: // ClientId
                        sortedClients = orderDirection == "asc"
                            ? clients.OrderBy(x => x.ClientId)
                            : clients.OrderByDescending(x => x.ClientId);
                        break;
                    case 1: // ClientName
                        sortedClients = orderDirection == "asc"
                            ? clients.OrderBy(x => x.ClientName ?? string.Empty)
                            : clients.OrderByDescending(x => x.ClientName ?? string.Empty);
                        break;
                    case 2: // Prefix
                        sortedClients = orderDirection == "asc"
                            ? clients.OrderBy(x => x.Prefix ?? string.Empty)
                            : clients.OrderByDescending(x => x.Prefix ?? string.Empty);
                        break;
                    // Add cases for other sortable columns as needed
                    default:
                        sortedClients = clients.OrderByDescending(x => x.ClientId);
                        break;
                }

                // Get counts
                int recordsTotal = clients.Count;
                int recordsFiltered = clients.Count; // Same as recordsTotal since we filtered earlier

                // Apply pagination
                var pagedData = sortedClients
                    .Skip(model.start)
                    .Take(model.length)
                    .ToList();

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = pagedData,
                    allData = sortedClients.ToList()
                });
            }
            catch (Exception ex)
            {
                // Log error here
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<ClientMaster>(),
                    error = ex.Message
                });
            }
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
        [HttpGet("client/list")]
        public IActionResult List()
        {
            List<ClientMaster> _ClientModel = _mapper.Map<List<ClientMaster>>(_customerCaseService.GetAllClients());
            return View(_ClientModel);
        }
        [HttpGet("client/create")]
        public IActionResult Add()
        {
            ClientMaster _client = new ClientMaster();
            return View(_client);
        }
        [HttpPost("client/create")]
        public IActionResult Add(ClientMaster _ClientModel)
        {
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            if (ModelState.IsValid)
            {
                if (_ClientModel.ClientId > 0)
                {
                    _ClientModel.CreatedBy = _clientHandler.GetUserId();
                    ClientMasterDTO _clientDto = new ClientMasterDTO();
                    _clientDto.ClientId = _ClientModel.ClientId;
                    _clientDto.ClientName = _ClientModel.ClientName;
                    _clientDto.Prefix = _ClientModel.Prefix;
                    _clientDto.C6Username = _ClientModel.C6Username;
                    _clientDto.C6Threshold = _ClientModel.C6Threshold;
                    _clientDto.Threshold = _ClientModel.Threshold;
                    _clientDto.CreatedBy = _clientHandler.GetUserId();
                    _clientDto.Description = _ClientModel.Description;
                    _clientDto.CreatedBy = _clientHandler.GetUserId();
                    _clientDto.Complem = _ClientModel.Complem;
                    _clientDto.C6BaseUrl = _ClientModel.C6BaseUrl;
                    _clientDto.type = 1;
                    _clientDto.Document = _ClientModel.Document;
                    _clientDto.DocumentDetails = _ClientModel.DocumentDetails;
                    _clientDto.DocumentFileName = _ClientModel.DocumentFileName;
                    _clientDto.DocumentFullPath = _ClientModel.DocumentFullPath;
                    _clientDto.DocumentName = _ClientModel.DocumentName;
                    _customerCaseService.UpdateClient(_clientDto);
                    if (_clientDto.Document != null)
                    {
                        DocumentsModel _documentsModel = _fileUploader.UploadLogo(_clientDto.ClientId, ItemType.Logo, _clientHandler.GetBranchId(), _clientDto.Document);
                        _clientDto.DocumentFileName = _documentsModel.DocName;
                        _clientDto.DocumentFullPath = _documentsModel.DocFullPath;
                       
                    }

					var result = _customerCaseService.UploadLogo(_mapper.Map<ClientMasterDTO>(_clientDto));
					

					_toastNotification.AddSuccessToastMessage("Client updated successfully");

					return Json(result);
				}
                else
                {
                    //var client = _customerCaseService.CreateClient(_mapper.Map<ClientMasterDTO>(_ClientModel));
                    ClientMasterDTO _clientDto = new ClientMasterDTO();
                    _clientDto.ClientName = _ClientModel.ClientName;
                    _clientDto.Prefix = _ClientModel.Prefix;
                    _clientDto.C6Username = _ClientModel.C6Username;
                    _clientDto.C6Threshold = _ClientModel.C6Threshold;
                    _clientDto.Threshold = _ClientModel.Threshold;
                    _clientDto.CreatedBy = _clientHandler.GetUserId();
                    _clientDto.Description = _ClientModel.Description;
                    _clientDto.CreatedBy = _clientHandler.GetUserId();
                    _clientDto.Complem = _ClientModel.Complem;
                    _clientDto.C6BaseUrl = _ClientModel.C6BaseUrl;
                    _clientDto.type = 2;
                    _clientDto.Document = _ClientModel.Document;
                    _clientDto.DocumentDetails = _ClientModel.DocumentDetails;
                    _clientDto.DocumentFileName = _ClientModel.DocumentFileName;
                    _clientDto.DocumentFullPath = _ClientModel.DocumentFullPath;
                    _clientDto.DocumentName = _ClientModel.DocumentName;
                    var client = _customerCaseService.CreateClient(_clientDto);
                    _clientDto.ClientId = client.Result;
                    if (_clientDto.Document != null)
                    {
                        DocumentsModel _documentsModel = _fileUploader.UploadLogo(_clientDto.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), _clientDto.Document);
                        _clientDto.DocumentFileName = _documentsModel.DocName;
                        _clientDto.DocumentFullPath = _documentsModel.DocFullPath;
                    }

                    var result = _customerCaseService.UploadLogo(_mapper.Map<ClientMasterDTO>(_clientDto));

					_toastNotification.AddSuccessToastMessage("Client Created successfully");

					return Json(result);

                    

                    //if (client.Result > 0)
                    //{
                    //    _toastNotification.AddSuccessToastMessage("Client added successfully");
                    //}
                    //else
                    //{
                    //    _toastNotification.AddErrorToastMessage("Client name already exist");
                    //    return View(_ClientModel);
                    //}
                }

                //return RedirectToAction("Index", new { isActive = 0 });
            }
            else
            {
                _toastNotification.AddErrorToastMessage(JsonConvert.SerializeObject(errors));
            }
            return View(_ClientModel);
        }
        //[HttpGet("client/ClientRight/{id}")]
        public ActionResult ClientRight(int id)
        {

            List<ClientRightsModel> _clientRights = new List<ClientRightsModel>();
            var clientData = _customerCaseService.GetClientDetailsByID(id);
            var result = _customerCaseService.GetClientRightsByClientId(id);
            foreach(var res in result)
            {
                ClientRightsModel c = new ClientRightsModel();
                c.Menu_Id = res.Menu_Id;
                _clientRights.Add(c);
            }
            var menus = _customerCaseService.GetAllMenus();
            MenuModel Menu = new MenuModel();
            List<MenuModel> menuModels = new List<MenuModel>();
            foreach(var menu in menus)
            {
                MenuModel _menu = new MenuModel();
                _menu.Menu_Id = menu.Menu_Id;
                _menu.Menu_Name = menu.Menu_Name;
                foreach (var cr in _clientRights)
                {
                    if (menu.Menu_Id == cr.Menu_Id)
                    {
                        _menu.isChecked = true;
                    }
                }
                _menu.is_active = menu.is_active;
                menuModels.Add(_menu);
            }
            Menu.MenuModels = menuModels;
            Menu.ClientName = clientData.ClientName;
            Menu.ClientId = id;
            return View(Menu);
        }
        [HttpPost]
        public JsonResult ClientRight(ClientMenuRights clientRights)
        {
            var sa = new JsonSerializerSettings();
            List<ClientMenuRightsModel> cmrmList = new List<ClientMenuRightsModel>();
            foreach (var item in clientRights.MenuIds)
            {
                ClientMenuRightsModel _clientRightsMod = new ClientMenuRightsModel();
                _clientRightsMod.Client_Id = clientRights.ClientId;
                _clientRightsMod.Menu_Id = item;
                _clientRightsMod.is_Active = 1;
                _clientRightsMod.Created_By = _clientHandler.GetUserId();
                cmrmList.Add(_clientRightsMod);
            }
            var deleteResult = _customerCaseService.DeleteRightsByClientId(clientRights.ClientId);
            foreach(var menu in cmrmList)
            {
                ClientMenuRightsModelDTO m = new ClientMenuRightsModelDTO();
                m.Client_Id = menu.Client_Id;
                m.Menu_Id = menu.Menu_Id;
                m.is_Active = menu.is_Active;
                m.Created_By = menu.Created_By;
                _customerCaseService.CreateClientRight(m);
            }
            _toastNotification.AddSuccessToastMessage("Client Right added successfully");
            return Json(new { Url = "client/list" });
            //return RedirectToAction("Index", new { isActive = 0 });
        }

        [HttpDelete("client/delete/{id}")]
        public ActionResult DeleteClient(int id)
        {
            try
            {
                ClientMasterDTO _ClientModel = new ClientMasterDTO();
                _ClientModel.ClientId = id;
                _ClientModel.isActive = 0;
                _ClientModel.CreatedBy = _clientHandler.GetUserId();
                var result = _customerCaseService.DeleteClient(_ClientModel);
                _toastNotification.AddSuccessToastMessage("Client deleted successfully");

                //return RedirectToAction(nameof(Index));
                return Json("Success");
            }
            catch
            {
                return View();
            }
        }
        [HttpGet("client/edit/{Id}")]
        public IActionResult EditClient(int Id)
        {
            var clientId = _clientHandler.GetClientId();
            var result = _customerCaseService.GetClientDetailsByID(Id);

            
          

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
            return View("Add", _clientModel);
        }
    }
}
