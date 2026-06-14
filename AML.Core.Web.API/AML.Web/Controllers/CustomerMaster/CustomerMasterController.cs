using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.IdentityType;
using AML.DTO.DTO.CustomerCase;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.CustomerMaster;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.IdentityType;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using AML.ViewModel.ViewModels.User;
using AML.Core.ServiceContract.User;
using AML.DTO.DTO.User;
using NLog;

namespace AML.Web.Controllers.CustomerMaster
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class CustomerMasterController : Controller
    {
        private IUserService _userService;
      
        private ICustomerMasterService _customerMasterService;
        private ICountryService _countryService;
        private IIdentityTypeService _idTypeService;
        private ICustomerCategoryService _customerCategoryService;
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        public CustomerMasterController(ICustomerMasterService customerMasterService, ICountryService countryService, IIdentityTypeService idTypeService, ICustomerCategoryService customerCategoryService, 
            IToastNotification toastNotification, IMapper mapper, IHttpClientHandler clientHandler, IUserService userService)
        {
            _userService = userService;
            _customerMasterService = customerMasterService;
            _countryService = countryService;
            _idTypeService = idTypeService;
            _customerCategoryService = customerCategoryService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
        }
        [HttpGet("customermaster/list")]
        public ActionResult Index()
        {
            return View();
        }
        public JsonResult CustomPaginationPosition(DataTableModel model)
        {
            int pageSize = model != null ? Convert.ToInt32(model.length) : 10;           
            int index = model != null && model.start > 0 ? (model.start / pageSize) + 1 : 1;
            string s_dir = model.order.Select(x => x.dir).FirstOrDefault();
            int s_col = model.order.Select(x => x.column).FirstOrDefault();
            string sortCol = "id";
            switch (s_col)
            {
                case 0:
                    sortCol = "cust_ref_id";
                    break;
                case 1:
                    sortCol = "fname";
                    break;
                case 2:
                    sortCol = "dob";
                    break;
                case 3:
                    sortCol = "nationality";
                    break;
                case 4:
                    sortCol = "cust_type";
                    break;
            }
            var clientId = _clientHandler.GetClientId();
            List<CustomerMasterModel> list = _mapper.Map<List<CustomerMasterModel>>(_customerMasterService.GetPaginatedCustomerMaster(index, pageSize, sortCol, s_dir, model.search.value,clientId));

            int filteredResultsCount = list.Select(x => x.RecordCount).FirstOrDefault();
            int totalResultsCount = list.Select(x => x.RecordCount).FirstOrDefault();
            return Json(new
            {
                // this is what datatables wants sending back
                draw = model.draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = list
            });
        }

        //[HttpGet("customermaster/details/{id}")]
        [HttpGet]
        [AML.Web.CustomFilters.TenantOwned(AML.Web.CustomFilters.TenantResource.CustomerMaster, "id")]
        public ActionResult AddUpdate(int id = 0)
        {
            CustomerMasterModel model = new CustomerMasterModel();
            var clientId = _clientHandler.GetClientId();
            if (id > 0)
            {
                model = _mapper.Map<CustomerMasterModel>(_customerMasterService.GetDetailsById(id));
            }
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");

            return View(model);
        }

        [HttpPost]
        public ActionResult AddUpdate(CustomerMasterModel model)
        {
            model.ClientId = _clientHandler.GetClientId();
            try
            {
               
                UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
              
               
                if (model.Id > 0)
                {
                    _customerMasterService.Update(_mapper.Map<CustomerMasterDTO>(model));
                    log.Info($"The User '{_EditorUserModel.UserName}' with clientId '{model.ClientId}' is updating '{model.CustomerReferenceId}' customer details");
                    log.Info($"Customer with '{model.CustomerReferenceId}' Updated Successfully");
                    _toastNotification.AddSuccessToastMessage("Customer Updated Successfully");
                    return RedirectToAction("Index");
                }
                else if (model.CustomerReferenceId != null && model.FirstName != null && model.LastName != null && model.CustomerType != null)
                {
                    _customerMasterService.Insert(_mapper.Map<CustomerMasterDTO>(model));
                      log.Info($"The User '{_EditorUserModel.UserName}' with clientId '{model.ClientId}' is creating '{model.CustomerReferenceId}' customer details");
                    log.Info("Customer added successfully");
                    _toastNotification.AddSuccessToastMessage("Customer Added Successfully");
                    return RedirectToAction("Index");
                }
                else
                {
                    log.Info("Not all required fields are entered");
                    _toastNotification.AddErrorToastMessage("Not all required fields are entered");
                }
            }
            catch (Exception ex)
            {
                log.Info("An error occured while processing your request");
                _toastNotification.AddErrorToastMessage("An error occured while processing your request");
                return RedirectToAction("Index");
            }

            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(model.ClientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            return View(model);
        }

        [HttpPost]
        public JsonResult UpdateWhiteList(string Id, string CustomerReferenceId, string IsWhiteListed)
        {
            try
            {
                UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
                var clientId = _clientHandler.GetClientId();
                var res = _customerMasterService.UpdateWhiteList(Convert.ToInt32(Id), IsWhiteListed);

                if(IsWhiteListed == "YES")
                {
                    log.Info($"The User '{_EditorUserModel.UserName}' with clientId '{clientId}' Perform Whitelisting for Customer '{CustomerReferenceId}'.");
                    log.Info($"'{CustomerReferenceId}' is Successfully Whitelisted");
                }
                else
                {

                    log.Info($"The User '{_EditorUserModel.UserName}' with clientId '{clientId}' Perform Whitelisting for Customer '{CustomerReferenceId}'.");
                    log.Info($"'{CustomerReferenceId}' is Successfully Rollback from Whitelisting");
                }

                _customerMasterService.InsertCustomerWhiteListLogs(Convert.ToInt32(Id), CustomerReferenceId, _clientHandler.GetUserId(), IsWhiteListed);
            }
            catch (Exception ex)
            {

            }
            return Json("Success");
        }
        [HttpGet]
        public ActionResult Delete(CustomerMasterDTO uid)
        {
            UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
            var clientId = _clientHandler.GetClientId();
            Console.WriteLine(string.Format("cmc Id : {0}", uid.CustomerReferenceID));
            CustomerMasterModel model = new CustomerMasterModel();
            if (!string.IsNullOrEmpty(uid.CustomerReferenceID))
            {
                string UID = uid.CustomerReferenceID;
                var list = UID.Split(',').Select(item => item).Distinct();
                foreach (var id in list )
                {
                    _customerMasterService.Delete(id);
                    log.Info($"The User '{_EditorUserModel.UserName}' with clientId '{clientId}' is deleting '{id}'.");
                    log.Info($"'{id}' is Deleted Successfully");
                }

                
            }
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");

            return View("index");
        }

        [HttpGet]
        public ActionResult Undelete(CustomerMasterDTO uid)
        {
            UserModel _EditorUserModel = _mapper.Map<UserModel>(_userService.GetDetails(_clientHandler.GetUserId()));
            var clientId = _clientHandler.GetClientId();
            Console.WriteLine(string.Format("cmc Id : {0}", uid.CustomerReferenceID));
            CustomerMasterModel model = new CustomerMasterModel();
            if (!string.IsNullOrEmpty(uid.CustomerReferenceID))
            {
                _customerMasterService.Undelete(uid.CustomerReferenceID);
                log.Info($"The User '{_EditorUserModel.UserName}' with clientId '{clientId}' is undeleting '{uid.CustomerReferenceID}'.");
                log.Info($"'{uid.CustomerReferenceID}' is UnDeleted Successfully");
            }
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");

            return View("index");
        }
    }
}
