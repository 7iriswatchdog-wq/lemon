using AML.Core.ServiceContract.Country;
using AML.ViewModel.ViewModels.Country;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Risk;
using AML.DTO.DTO.RiskV2;
using AML.ViewModel.ViewModels.RiskV2;
using System;
using System.Linq;
using AML.Core.ServiceContract.LovMaster;
using AML.DTO.DTO.LovMaster;
using AML.Web.CustomFilters;
using AML.Core.ServiceContract.CustomerCase;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.LovMasterModel;
using AML.ViewModel.ViewModels.CustomerMaster;
using AML.Core.ServiceContract.CustomerMaster;
using System.Threading.Tasks;
using Fingers10.ExcelExport.Attributes;
using System.ComponentModel.DataAnnotations;
using Fingers10.ExcelExport.ActionResults;
using AML.Core.DataContract.Authentication;
using AML.Core.Common.StaticResource;
using AML.ViewModel.ViewModels.User;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.ViewModel.ViewModels.Common;
using AML.Core.DataContract.Enum;
using System.Reflection;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.RiskAPI;
using System.Text;
using DocumentFormat.OpenXml.Bibliography;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Fingers10.ExcelExport.Extensions;
using AML.Core.ServiceContract.RiskV2;
using AML.Core.ServiceContract.LovMasterV2;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Office2010.Excel;

namespace AML.Web.Controllers.Risk
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class RiskV2Controller : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private IRiskV2Service _riskService;
        private ICountryService _countryService;
        private IIdentityTypeService _idTypeService;
        private ICustomerCategoryService _customerCategoryService;
        private IUserService _userService;
        private ICaseDocumentService _caseDocumentService;
        private ICaseAssignmentService _caseAssignmentService;
        private ICaseCommentService _caseCommentService;
        private ILovMasterV2Service _lovMasterService;
        private ICustomerCaseService _customerCaseService;
        private ICustomerMasterService _customerMasterService;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private string baseC6URL = string.Empty;
        private string _c6Username;
        private int clientId = 0;

        IFileUploader _fileUploader;
        private ICommonService _commonService;
        private string baseURL = string.Empty;
        public RiskV2Controller(IMapper mapper,
            IToastNotification toastNotification, ICountryService countryService, ICaseDocumentService caseDocumentService,
            IHttpClientHandler clientHandler, ICustomerCategoryService customerCategoryService, ICaseCommentService caseCommentService,
            IConfiguration configuration, IIdentityTypeService idTypeService, IUserService userService, ICaseAssignmentService caseAssignmentService, ICommonService commonService,
            IRiskV2Service RiskService, IFileUploader fileUploader, ILovMasterV2Service lovMasterService, ICustomerCaseService customerCaseService,
            ICustomerMasterService customerMasterService, IViewRenderService viewRenderService, IExportDataService exportService)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _riskService = RiskService;
            _countryService = countryService;
            _idTypeService = idTypeService;
            _customerCategoryService = customerCategoryService;
            _userService = userService;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
            _caseAssignmentService = caseAssignmentService;
            _fileUploader = fileUploader;
            _commonService = commonService;
            clientId = clientHandler.GetClientId();
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            _lovMasterService = lovMasterService;
            _customerCaseService = customerCaseService;
            _customerMasterService = customerMasterService;
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            baseC6URL = clientDetails.C6BaseUrl;
            //baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            //_c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
            _c6Username = clientDetails.C6Username;
            _viewRenderService = viewRenderService;
            _exportService = exportService;
        }

        [HttpGet("/riskV2")]
        public ActionResult Index()
        {
            return View();
        }


        [HttpGet("/riskV2/RiskCreate")]
        public async Task<ActionResult> RiskCreate()
        {
            //TODO: to bring all the hard-code values to static or enums
            RiskModelV2 model = new RiskModelV2();
            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                //_toastNotification.AddErrorToastMessage("User is not authorized for using this module.");
                model.userAuthorised = false;
            }
            else
            {
                model.userAuthorised = true;
                model.DateofAssessment = DateTime.Now;
                var lovMastersLists = _lovMasterService.GetAll();
                var clientId =_clientHandler.GetClientId();
                model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("I",clientId)), "Id", "FullName");

                model.MainNationalityLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                //       model.Countrylist = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll()), "RiskRating", "Name");
                //     model.CountryFATlist = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetListFATFAll()), "FATFRiskRating", "Name");

                model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0,clientId);
                for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                    }
                }
            }
            return View(model);
        }

        [HttpPost]
        public JsonResult AutoCompleteCustomer(string prefix)
        {
            var customers = _customerCaseService.GetCustomerMasterByCodePrefix(prefix);

            return Json(customers);
        }

        public JsonResult GetCustomerDetailById(string id)
        {
            var res = _mapper.Map<CustomerMasterModel>(_customerMasterService.GetDetailsById(Convert.ToInt32(id)));
            
            return Json(res);
        }

        [HttpPost("/riskV2/RiskCreate")]
        [ValidateAntiForgeryToken]
        public ActionResult RiskCreate(RiskModelV2 model)
        {
            model.userAuthorised = true;
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            try
            {
               
                    var result = _riskService.Create(_mapper.Map<RiskDTOV2>(model));
                    if (result.Status == 200)
                    {
                        _toastNotification.AddSuccessToastMessage("Risk Assessment Creation Successful.");
                        return RedirectToAction("RiskCreate");
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Risk Assessment Creation Failed.");
                        //_toastNotification.AddSuccessToastMessage("Risk Assessment Creation Successful.");
                    }
                //}
                //else
                //{
                //    var modelErrors = ModelState.Values.SelectMany(v => v.Errors).Select(v => v.ErrorMessage + " " + v.Exception);
                //    Console.WriteLine(String.Join(Environment.NewLine, modelErrors));
                //}
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            var clientId = _clientHandler.GetClientId();
            var lovMastersLists = _lovMasterService.GetAll();

            RiskModelV2 _model = new RiskModelV2
            {
                userAuthorised = true,
                MainNationalityLists = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name"),
                CustomerList = new SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("I", clientId)), "Id", "RiskSelectData"),
                RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId)
            };
            for (var i = 0; i < _model.RiskTypeCategoryDTO.Count; i++)
            {
                for (var j = 0; j < _model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                {
                    var items = _model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                }
            }
            return View(model);
        }

        [HttpGet("/riskV2/corpcustomer")]
        public async Task<ActionResult> RiskAssessmentForCorpCustomer()
        {
            //TODO: to bring all the hard-code values to static or enums
            RiskCorpCustomerModelV2 model = new RiskCorpCustomerModelV2();
            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                //_toastNotification.AddErrorToastMessage("User is not authorized for using this module.");
                model.userAuthorised = false;
            }
            else
            {
                model.userAuthorised = true;
                model.DateofAssessment = DateTime.Now;

                var clientId = _clientHandler.GetClientId();
                model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("C",clientId)), "Id", "FullName");
                model.CountryOfIncorporationList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0,clientId);
                for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                    }
                }
            }
            return View(model);
        }

        [HttpPost("/riskV2/corpcustomer")]
        public ActionResult RiskAssessmentForCorpCustomer(RiskCorpCustomerModelV2 model)
        {
            model.userAuthorised = true;
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            try
            {
               
                    var result = _riskService.CreateCorpCustomerRisk(_mapper.Map<RiskCorpCustomerDTOV2>(model));

                    if (result.Status == 200)
                    {
                        _toastNotification.AddSuccessToastMessage("Risk Assessment Creation Successful.");
                        return RedirectToAction("RiskAssessmentForCorpCustomer");
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Risk Assessment Creation Failed.");
                    }
                
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
            }
            //Reload lists on failure
            var lovMastersLists = _lovMasterService.GetAll();
            var allCountriedList = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "RiskRating", "Name");
            var allFATFCountriedList = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetListAll()), "FATFRiskRating", "Name");
            var clientId = _clientHandler.GetClientId();
            model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("C",clientId)), "Id", "CustomerReferenceId");

            //model.NationalityLists = allCountriedList;
            //model.RiskMonitoringJurisdictionLists = allFATFCountriedList;
            //model.CompanySubidaryLists = allFATFCountriedList;
            model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0,clientId);
            for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
            {
                for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                {
                    var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                }
            }
            return View(model);
        }

        [HttpGet("/riskV2/vendor")]
        public async Task<ActionResult> RiskAssessmentForVendors()
        {
            RiskAssessmentVendorModelV2 model = new RiskAssessmentVendorModelV2();
            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                //_toastNotification.AddErrorToastMessage("User is not authorized for using this module.");
                model.userAuthorised = false;
            }
            else
            {
                model.userAuthorised = true;
                model.RegisteredDate = DateTime.Now;
                var lovMastersLists = _lovMasterService.GetAll();
                var clientId = _clientHandler.GetClientId();
                model.CountryLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("C",clientId)), "Id", "CustomerReferenceId");

                model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("V", 1, 0, 0,clientId);
                for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                    }
                }
            }
            return View(model);
        }

        [HttpPost("/riskV2/vendor")]
        public ActionResult RiskAssessmentForVendors(RiskAssessmentVendorModelV2 model)
        {
            model.userAuthorised = true;
            var clientId = _clientHandler.GetClientId();
            model.ClientId = clientId;
            model.CreatedBy = _clientHandler.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    var result = _riskService.CreateVendorRisk(_mapper.Map<RiskAssessmentVendorDTOV2>(model));

                    if (result.Status == 200)
                    {
                        _toastNotification.AddSuccessToastMessage("Risk Assessment Creation Successful.");
                        return RedirectToAction("RiskAssessmentForVendors");
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Risk Assessment Creation Failed.");
                    }
                }
            }
            catch (Exception ex)
            {
            }
            var lovMastersLists = _lovMasterService.GetAll();
            model.CountryLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");

            model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("C",clientId)), "Id", "CustomerReferenceId");

            model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("V", 1, 0, 0,clientId);
            for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
            {
                for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                {
                    var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                }
            }
            return View(model);
        }

        [HttpGet("/riskV2/Bank")]
        public async Task<ActionResult> RiskAssessmetnForBank()
        {
            RiskAssessmentBankModelV2 model = new RiskAssessmentBankModelV2();
            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                //_toastNotification.AddErrorToastMessage("User is not authorized for using this module.");
                model.userAuthorised = false;
            }
            else
            {
                model.userAuthorised = true;
                model.EntityDate = DateTime.Now;
                var clientId = _clientHandler.GetClientId();
                var lovMastersLists = _lovMasterService.GetAll();
                var countriesList = _countryService.GetAll(clientId);

                model.CountryOfIncorporationLists = new SelectList(_mapper.Map<List<CountryModel>>(countriesList), "Name", "Name");
                model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("C",clientId)), "Id", "CustomerReferenceId");

                model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("B", 1, 0, 0,clientId);
                for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                    }
                }
            }
            return View(model);
        }

        [HttpPost("/riskV2/Bank")]
        public ActionResult RiskAssessmetnForBank(RiskAssessmentBankModelV2 model)
        {
            model.userAuthorised = true;
            var clientId = _clientHandler.GetClientId();
            model.ClientId = clientId;
            model.CreatedBy = _clientHandler.GetUserId();
            try
            {
                if (ModelState.IsValid)
                {
                    var result = _riskService.CreateBankRisk(_mapper.Map<RiskAssessmentBankDTOV2>(model));

                    if (result.Status == 200)
                    {
                        _toastNotification.AddSuccessToastMessage("Risk Assessment Creation Successful.");
                        return RedirectToAction("RiskAssessmetnForBank");
                    }
                    else
                    {
                        _toastNotification.AddErrorToastMessage("Risk Assessment Creation Failed.");
                    }
                }
            }
            catch (Exception ex)
            {
            }
            var lovMastersLists = _lovMasterService.GetAll();
            var countriesList = _countryService.GetAll(clientId);

            model.CountryOfIncorporationLists = new SelectList(_mapper.Map<List<CountryModel>>(countriesList), "Name", "Name");
            model.CustomerList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CustomerMasterModel>>(_customerCaseService.GetCustomerMasterByCode("C",clientId)), "Id", "CustomerReferenceId");


            model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("B", 1, 0, 0,clientId);
            for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
            {
                for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                {
                    var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTOV2>>(items.ToList()), "Score", "RiskItem");
                }
            }
            return View(model);
        }

        [Route("/risk-config-masterV2")]
        public async Task<ActionResult> RiskConfigurationMaster()
        {
            RIskConfigurationMasterModelV2 model = new RIskConfigurationMasterModelV2();

            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                //_toastNotification.AddErrorToastMessage("User is not authorized for using this module.");
                model.userAuthorised = false;
            }
            else
            {
                model.userAuthorised = true;
                var ClientId= _clientHandler.GetClientId();
                var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
                model.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            }
            return View(model);
        }

        [HttpPost("/risk-config-masterV2")]
        public ActionResult RiskConfigurationMaster(RIskConfigurationMasterModelV2 model)
        {
            model.userAuthorised = true;
            model.CreatedBy= _clientHandler.GetUserId();
            if (ModelState.IsValid)
            {
                string returnMsg = null;
                bool isSuccess = true;

                _lovMasterService.AddRiskTypes(_mapper.Map<RIskConfigurationMasterDTOV2>(model), out returnMsg, out isSuccess);

                if (!isSuccess)
                {
                    _toastNotification.AddErrorToastMessage(returnMsg);
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("Risk Item Creation Successful.");
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Risk Item Creation Failed.");
            }
            RIskConfigurationMasterModelV2 modelVal = new RIskConfigurationMasterModelV2();
            var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            modelVal.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            modelVal.RiskCategoryID = "0";
            return View(modelVal);
        }

        public JsonResult GetRiskCategoryTypes(string riskCategoryID)
        {
            var clientId = _clientHandler.GetClientId();
            dynamic riskCategoryTypes = _lovMasterService.GetRiskCategoryTypes(riskCategoryID,clientId);
            return Json(new SelectList(riskCategoryTypes, "LovTypeCategoryId", "LovCategoryType"));
        }

        public JsonResult GetRiskTypes(string riskTypeCategoryID)
        {
            var clientId = _clientHandler.GetClientId();
            var riskTypes = _lovMasterService.GetRiskTypes(riskTypeCategoryID, clientId);
            return Json(new SelectList(riskTypes, "LovTypeId", "LovTypeName"));
        }

        [HttpPost("/riskV2/custompaginationRiskItems")]
        public JsonResult CustomPagination(DataTableModel model, string riskTypeID,string risKCategoryId)
        {
            var clientId = _clientHandler.GetClientId();
            List<LovMasterModel> abc = _mapper.Map<List<LovMasterModel>>(_lovMasterService.GetRiskItems(Convert.ToInt32(riskTypeID),clientId));
            int totalcount = abc.Count;
            int filteredcount = abc.Count;
            var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data
            });
            return response;
        }
        [HttpPost]
        public JsonResult CustomPaginationRiskTypeCategory(DataTableModel model, string riskCategoryID)
        {
            var clientId = _clientHandler.GetClientId();
            List<LovTypeCategoryDTO> abc = _mapper.Map<List<LovTypeCategoryDTO>>(_lovMasterService.GetRiskCategoryType(riskCategoryID,clientId));
            int totalcount = abc.Count;
            int filteredcount = abc.Count;
            var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data
            });
            return response;
        }

        public JsonResult CustomPaginationRiskType(DataTableModel model, string RiskTypeCategoryID,string RiskCategoryID)
        {
            var clientId = _clientHandler.GetClientId();
            List<LovMasterModel> abc = _mapper.Map<List<LovMasterModel>>(_lovMasterService.GetRiskType(RiskTypeCategoryID,  clientId));
            int totalcount = abc.Count;
            int filteredcount = abc.Count;
            var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data
            });
            return response;
        }
        public IActionResult ChangeStatus(int id, int status)
        {
            var result = _lovMasterService.UpdateRiskItemStatus(id, status);
            return Json("Success");
        }

        [HttpPost]
        public ActionResult SaveRiskItems(RIskConfigurationMasterModelV2 model)
        {
            if (ModelState.IsValid)
            {
                model.ClientId = _clientHandler.GetClientId();
                model.CreatedBy = _clientHandler.GetUserId();

                string returnMsg = null;
                bool isSuccess = true;
                model.ClientId = _clientHandler.GetClientId();
                _lovMasterService.AddRiskTypes(_mapper.Map<RIskConfigurationMasterDTOV2>(model), out returnMsg, out isSuccess);

                if (!isSuccess)
                {
                    _toastNotification.AddErrorToastMessage(returnMsg);
                }
                else
                {
                    RedirectToAction("RiskConfigurationMaster", "Risk");
                    _toastNotification.AddSuccessToastMessage("Risk Item Creation Successful.");
      
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Risk Item Creation Failed.");
            }
            RIskConfigurationMasterModelV2 modelVal = new RIskConfigurationMasterModelV2();
            var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            modelVal.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            modelVal.RiskCategoryID = "0";
            return View(modelVal);

        }
        [HttpPost]
        public ActionResult SaveType(RIskConfigurationMasterModelV2 model)
        {
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            string returnMsg = null;
            bool isSuccess = true;
           model.ClientId = _clientHandler.GetClientId();
            RIskConfigurationMasterDTOV2 riskConfigurationMasterDTO = _mapper.Map<RIskConfigurationMasterDTOV2>(model);
            _lovMasterService.AddRiskType(riskConfigurationMasterDTO, out returnMsg, out isSuccess);

            if (!isSuccess)
            {
                _toastNotification.AddErrorToastMessage(returnMsg);
            }
            else
            {
                _toastNotification.AddSuccessToastMessage("Risk Item Creation Successful.");
              
            }
           

            RIskConfigurationMasterModelV2 modelVal = new RIskConfigurationMasterModelV2();
            var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            modelVal.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            modelVal.RiskCategoryID = "0";
            return View(modelVal);
        }


        [HttpPost]
        public ActionResult SaveCategoryType(RIskConfigurationMasterModelV2 model)
        {
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            if (ModelState.IsValid)
            {
                string returnMsg = null;
                bool isSuccess = true;

                _lovMasterService.AddRiskTypeCategory(_mapper.Map<RIskConfigurationMasterDTOV2>(model), out returnMsg, out isSuccess);

                if (!isSuccess)
                {
                    _toastNotification.AddErrorToastMessage(returnMsg);
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("Risk Item Creation Successful.");
                  
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Risk Item Creation Failed.");
            }
            RIskConfigurationMasterModelV2 modelVal = new RIskConfigurationMasterModelV2();
            var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            modelVal.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            modelVal.RiskCategoryID = "0";
            return View(modelVal);
        }
        [HttpGet]
        public ActionResult GetRiskSummary()
        {
            RiskRequestModelV2 model = new RiskRequestModelV2();
            model.OnDate = DateTime.Now;
            return View(model);
        }

        [HttpPost]
        public JsonResult RiskSummaryCustomPagination(DataTableModel model, string OnDate)
        {
            var clientId = _clientHandler.GetClientId();
            List<RiskSummaryModelV2> abc = _mapper.Map<List<RiskSummaryModelV2>>(_riskService.GetRiskSummary(Convert.ToDateTime(OnDate),clientId).Result);
            int totalcount = abc.Count;
            int filteredcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.RiskType.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var data = Sort(abc, model.columns[model.order[0].column].data ?? "Total", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();
            //var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data
            });
            return response;
        }

        public List<T> Sort<T>(List<T> input, string property, string dir)

        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty?.GetValue(p, null)).ToList();

        }

        public class RiskSummaryExcelModel
        {
            public int Id { get; set; }
            [IncludeInReport(Order = 1)]
            [Display(Name = "Risk Type")]
            public string RiskType { get; set; }
            [IncludeInReport(Order = 1)]
            [Display(Name = "High Risk Count")]
            public int HighRiskCount { get; set; }
            [IncludeInReport(Order = 1)]
            [Display(Name = "Medium Risk Count")]
            public int MediumRiskCount { get; set; }
            [IncludeInReport(Order = 1)]
            [Display(Name = "Low Risk Count")]
            public int LowRiskCount { get; set; }
            [IncludeInReport(Order = 8)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }
        }

        public async Task<IActionResult> ExportRiskSummary(string OnDate)
        {
            var clientId = _clientHandler.GetClientId();
            List<RiskSummaryModelV2> riskSummaryList = _mapper.Map<List<RiskSummaryModelV2>>(_riskService.GetRiskSummary(Convert.ToDateTime(OnDate),clientId).Result);
            RiskSummaryExcelModel excelModel = new RiskSummaryExcelModel();
            var excelData = (from res in riskSummaryList
                             select new RiskSummaryExcelModel
                             {
                                 RiskType = res.RiskType,
                                 HighRiskCount = res.HighRiskCount,
                                 MediumRiskCount = res.MediumRiskCount,
                                 LowRiskCount = res.LowRiskCount
                             }).ToList();
            string details = "Report               :   Risk Summary Report\r\n" +
                "On Date          :   " + OnDate+ "\r\n" ;
            excelModel.Details= details;
            excelData.Add(excelModel);
            return new ExcelResult<RiskSummaryExcelModel>(excelData, "Risk Summary Report", "risk_summary_Report_" + DateTime.Now.Ticks);
        }
        
        public ActionResult GetRiskReport()
        {
            RiskRequestModelV2 model = new RiskRequestModelV2();

            model.FromDate = DateTime.Now;
            model.ToDate = DateTime.Now;
            model.RiskType = "All";
            model.WithDuplicate = false;
            var clientID = _clientHandler.GetClientId();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientID))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            return View(model);
        }

        public JsonResult RiskReportCustomPagination(DataTableModel model, string fromDate, string toDate, string createdByUserID, string riskType, bool withDuplicate = false, int riskLevel = 0)
        {
            var clientId = _clientHandler.GetClientId();
            List<RiskReportModelV2> riskReports = _mapper.Map<List<RiskReportModelV2>>(_riskService.GetRiskReportBetweenDateAndType(clientId, createdByUserID,Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate), riskType, withDuplicate, riskLevel).Result);
            int totalcount = riskReports.Count;
            int filteredcount = riskReports.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                riskReports = riskReports.Where(m => m.CustomerName.ToLower().Contains(model.search.value.ToLower()) || m.CustomerCode.ToLower().Contains(model.search.value.ToLower())
                || m.RiskType.ToLower().Contains(model.search.value.ToLower()) || m.ScoreBeforeOverride.ToLower().Contains(model.search.value.ToLower())).ToList();
            }


            var data = Sort(riskReports, model.columns[model.order[0].column].data ?? "customerCode", model.order[0].dir ?? "dec")

  .Skip(model.start)

  .Take(model.length)

  .ToList();

            //var data = riskReports.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data
            });

            return response;
        }

        public JsonResult GetAllRiskVersionCustomPagination(DataTableModel model,string customerCode, string riskType)
        {
            var clientId = _clientHandler.GetClientId();
            List<RiskReportModelV2> riskReports = _mapper.Map<List<RiskReportModelV2>>(_riskService.GetAllVersionRiskReportByid(customerCode, riskType, clientId).Result);
            int totalcount = riskReports.Count;
            int filteredcount = riskReports.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                riskReports = riskReports.Where(m => m.CustomerName.ToLower().Contains(model.search.value.ToLower()) || m.CustomerCode.ToLower().Contains(model.search.value.ToLower())
                || m.RiskType.ToLower().Contains(model.search.value.ToLower()) || m.ScoreBeforeOverride.ToLower().Contains(model.search.value.ToLower())).ToList();
            }


            var data = Sort(riskReports, model.columns[model.order[0].column].data ?? "customerCode", model.order[0].dir ?? "dec")

  .Skip(model.start)

  .Take(model.length)

  .ToList();

            //var data = riskReports.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data
            });

            return response;
        }


        [HttpGet]
        public ActionResult DeleteRiskDetail(int id, string type)
        {
            var viewName = "GetRiskReport";
            switch (type.ToLower())
            {
                case "individual":
                    _riskService.DeleteRiskIndiviual(id);
                    break;
                case "corporate":
                    _riskService.DeleteRiskCorporate(id);
                    break;
                case "bank":
                    _riskService.DeleteRiskBank(id);
                    break;
                case "vendor":
                    _riskService.DeleteRiskVendor(id);
                    break;
            }
            return View(viewName);
        }



        public ActionResult ViewIndividualRiskDetail(int id, string type)
        {
            var viewName = string.Empty;
            dynamic model = null;
            var clientId = _clientHandler.GetClientId();
            int dt=0;
            //RiskCorpCustomerModel _riskmodel = new RiskCorpCustomerModel();
            switch (type.ToLower())
            {
                case "individual":
                    RiskModelV2 _riskmodel = new RiskModelV2();
                    viewName = "ViewIndividualRiskDetail";
                    model = _mapper.Map<RiskModelV2>(_riskService.GetRiskDetailsOfIndividual(id).Result);
                    if(model != null)
                    {
                        var properties = model.GetType().GetProperties();
                        foreach (var property in properties)
                        {
                            var PropertyName = property.Name;
                            var PropetyValue = model.GetType().GetProperty(property.Name).GetValue(model, null);
                            if (PropertyName == "version")
                            {
                                dt = Convert.ToInt32(PropetyValue);
                                break;
                            }

                        }
                    }

                    _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("I", 1, 0, 0,clientId,dt);


                    for (var i = 0; i < _riskmodel.RiskTypeCategoryDTO.Count; i++)
                    {
                        for (var j = 0; j < _riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                        {
                            var items = _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                            for (var m = 0; m < model.ReportDataDTO.Count; m++)
                            {
                                var isCountry = _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                                var isNotCountryAndIdMatches = !isCountry && _riskmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id;

                                if (isNotCountryAndIdMatches || isCountry)
                                {
                                    if (_riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                                    {
                                        _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                        _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                        _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                                    }
                                }
                            }
                            
                        }
                    }

                    //for (var i = 0; i < _riskmodel.RiskTypeCategoryDTO.Count; i++)
                    //{
                    //    for (var j = 0; j < _riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    //    {
                    //        var items = _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    //        for (var m = 0; m < model.ReportDataDTO.Count; m++)
                    //        {
                    //            if (_riskmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                    //            {
                    //                if (_riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                    //                {
                    //                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                    //                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                    //                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                    //                }
                    //            }
                    //        }
                    //        //model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    //    }
                    //}
                    model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;



                    break;
                case "corporate":
                    viewName = "ViewCorporateRiskDetail";
                    RiskCorpCustomerModelV2 _riskCorpmodel = new RiskCorpCustomerModelV2();
                    model = _mapper.Map<RiskCorpCustomerModelV2>(_riskService.GetRiskDetailsOfCorporate(id).Result);
                    if(model != null)
                    {
                        var cproperties = model.GetType().GetProperties();
                        foreach (var property in cproperties)
                        {
                            var PropertyName = property.Name;
                            var PropetyValue = model.GetType().GetProperty(property.Name).GetValue(model, null);
                            if (PropertyName == "version")
                            {
                                dt = Convert.ToInt32(PropetyValue);
                                break;
                            }

                        }
                    }

                    _riskCorpmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("C", 1, 0, 0,clientId,dt);
                    for (var i = 0; i < _riskCorpmodel.RiskTypeCategoryDTO.Count; i++)
                    {
                        for (var j = 0; j < _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                        {
                            var items = _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                            for (var m = 0; m < model.ReportDataDTO.Count; m++)
                            {
                                var isCountry = _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                                var isNotCountryAndIdMatches = !isCountry && _riskCorpmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id;

                                if (isNotCountryAndIdMatches || isCountry)
                                {
                                    if (_riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                                    {
                                        _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                        _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                        _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                                    }
                                }
                            }
                        }
                    }
                    //for (var i = 0; i < _riskCorpmodel.RiskTypeCategoryDTO.Count; i++)
                    //{
                    //    for (var j = 0; j < _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    //    {
                    //        var items = _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    //        for (var m = 0; m < model.ReportDataDTO.Count; m++)
                    //        {
                    //            if (_riskCorpmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                    //            {
                    //                if (_riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                    //                {
                    //                    _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                    //                    _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                    //                    _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                    //                }
                    //            }
                    //        }
                    //        //model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    //    }
                    //}
                    model.RiskTypeCategoryDTO = _riskCorpmodel.RiskTypeCategoryDTO;

                    break;
                case "bank":
                    viewName = "ViewBankRiskDetail";
                    model = _mapper.Map<RiskAssessmentBankModelV2>(_riskService.GetRiskDetailsOfBank(id).Result);
                    var bproperties = model.GetType().GetProperties();
                    if(model !=null)
                    {
                        foreach (var property in bproperties)
                        {
                            var PropertyName = property.Name;
                            var PropetyValue = model.GetType().GetProperty(property.Name).GetValue(model, null);
                            if (PropertyName == "version")
                            {
                                dt = Convert.ToInt32(PropetyValue);
                                break;
                            }

                        }
                    }

                    RiskAssessmentBankModelV2 _riskmodel1 = new RiskAssessmentBankModelV2();
                    _riskmodel1.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("B", 1, 0, 0,clientId,dt);

                    for (var i = 0; i < _riskmodel1.RiskTypeCategoryDTO.Count; i++)
                    {
                        for (var j = 0; j < _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                        {
                            var items = _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                            for (var m = 0; m < model.ReportDataDTO.Count; m++)
                            {
                                if (_riskmodel1.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                                {
                                    var isCountry = _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                                    var isNotCountryAndIdMatches = !isCountry && _riskmodel1.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id;

                                    if (isNotCountryAndIdMatches || isCountry)
                                    {
                                        _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                        _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                        _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                                    }
                                }
                            }
                            //model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                        }
                    }


                    //for (var i = 0; i < _riskmodel1.RiskTypeCategoryDTO.Count; i++)
                    //{
                    //    for (var j = 0; j < _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    //    {
                    //        var items = _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    //        for (var m = 0; m < model.ReportDataDTO.Count; m++)
                    //        {
                    //            if (_riskmodel1.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                    //            {
                    //                if (_riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                    //                {
                    //                    _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                    //                    _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                    //                    _riskmodel1.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                    //                }
                    //            }
                    //        }
                    //        //model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    //    }
                    //}
                    model.RiskTypeCategoryDTO = _riskmodel1.RiskTypeCategoryDTO;
                    break;
                case "vendor":
                    RiskAssessmentVendorModelV2 riskvendorModel = new RiskAssessmentVendorModelV2();
                    viewName = "ViewVendorRiskDetail";
                    model = _mapper.Map<RiskAssessmentVendorModelV2>(_riskService.GetRiskDetailsOfVendor(id).Result);
                    if(model != null)
                    {
                        var vproperties = model.GetType().GetProperties();
                        foreach (var property in vproperties)
                        {
                            var PropertyName = property.Name;
                            var PropetyValue = model.GetType().GetProperty(property.Name).GetValue(model, null);
                            if (PropertyName == "version")
                            {
                                dt = Convert.ToInt32(PropetyValue);
                                break;
                            }

                        }
                    }

                    
                    riskvendorModel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("V", 1, 0, 0,clientId,dt);

                    for (var i = 0; i < riskvendorModel.RiskTypeCategoryDTO.Count; i++)
                    {
                        for (var j = 0; j < riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                        {
                            var items = riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                            for (var m = 0; m < model.ReportDataDTO.Count; m++)
                            {
                                if (riskvendorModel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                                {
                                    var isCountry = riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                                    var isNotCountryAndIdMatches = !isCountry && riskvendorModel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id;

                                    if (isNotCountryAndIdMatches || isCountry)
                                    {
                                        riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                        riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                        riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                                    }
                                }
                            }
                            //model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                        }
                    }


                    //for (var i = 0; i < riskvendorModel.RiskTypeCategoryDTO.Count; i++)
                    //{
                    //    for (var j = 0; j < riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    //    {
                    //        var items = riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    //        for (var m = 0; m < model.ReportDataDTO.Count; m++)
                    //        {
                    //            if (riskvendorModel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                    //            {
                    //                if (riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                    //                {
                    //                    riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                    //                    riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                    //                    riskvendorModel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                    //                }
                    //            }
                    //        }
                    //        //model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    //    }
                    //}
                    if (riskvendorModel.RiskTypeCategoryDTO.Count > 0)
                        model.RiskTypeCategoryDTO = riskvendorModel.RiskTypeCategoryDTO;

                    break;
            }

            return View(viewName, model);
        }

        public class RiskReportExcelModel
        {
            public int Id { get; set; }
            [IncludeInReport(Order = 1)]
            [Display(Name = "Customer Code")]
            public string CustomerCode { get; set; }
            [IncludeInReport(Order = 2)]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; }
            [IncludeInReport(Order = 3)]
            [Display(Name = "Risk Type")]
            public string RiskType { get; set; }
            [IncludeInReport(Order = 4)]
            [Display(Name = "Final Score")]
            public string FinalScore { get; set; }
            [IncludeInReport(Order = 5)]
            [Display(Name = "Score Before Override")]
            public string ScoreBeforeOverride { get; set; }
        }
        //Risk Bulk Upload
        [HttpGet]
        public ActionResult DownloadIndividualexcel()
        {
            string filePath = @"Files/Bulk_Risk_format_Individual.xlsx";
            string fileName = "Individual Risk Bulk upload Sample.xlsx";
                fileName = "Individual_Risk_Bulk_upload_Sample_Individual.xlsx";
                byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
                return File(fileBytes, "application/force-download", fileName);
      
        }
        
        [HttpGet]
        public ActionResult DownloadCustomerUploadSample()
        {
            string filePath = @"Files/Bulk_Risk_format_Corporate.xlsx";
            string fileName = "Corporate Risk Bulk upload Sample.xlsx";
            fileName = "Corporate_Risk_Bulk_upload_Sample_Individual.xlsx";
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/force-download", fileName);

        }
        //public IActionResult RiskIndividualBulkUpload()
        //{
        //    string type = "I";
        //    DocumentUploadModel _docUpload = new DocumentUploadModel { itemId = (int)ItemType.clientcase, branchId = _clientHandler.GetBranchId(), typeId = type };
        //    _docUpload.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId);
        //    for (var i = 0; i < _docUpload.RiskTypeCategoryDTO.Count; i++)
        //    {
        //        for (var j = 0; j < _docUpload.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
        //        {
        //            var items = _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
        //            _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
        //        }
        //    }


        //    return View(_docUpload);
        //}

        //[HttpPost]
        //public async Task<IActionResult> RiskIndividualBulkUpload(DocumentUploadModel _documentUploadModel)
        //{
        //    string type = "I";
        //    DocumentUploadModel _docUpload = new DocumentUploadModel { itemId = (int)ItemType.clientcase, branchId = _clientHandler.GetBranchId(), typeId = type };
        //    if (_documentUploadModel.fileUpload != null)
        //    {
        //        var newFileName = "";
        //        if (_documentUploadModel.typeId == "I")
        //        {
        //            newFileName = "Individual_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + _documentUploadModel.fileUpload.FileName;
        //        }
        //        _documentUploadModel.ClientId = _clientHandler.GetClientId();
        //        DocumentsModel _documentsModel = _fileUploader.UploadFile(_documentUploadModel.ClientId, (ItemType)_documentUploadModel.itemId, _documentUploadModel.branchId, _documentUploadModel.fileUpload, newFileName);
        //        _documentsModel.AddedBy = _clientHandler.GetUserId();
        //        _documentsModel.ClientId = _clientHandler.GetClientId();

        //        Tuple<ServiceResponse<List<IndividualExcel>>, RiskAPIRequestModel> A = _customerCaseService.SaveIndividualRiskExcelData(_mapper.Map<DocumentsDTO>(_documentsModel), _documentUploadModel.typeId, _documentUploadModel.C6Threshold, _documentUploadModel.Threshold);
        //        List<IndividualExcel> _excelData = _mapper.Map<List<IndividualExcel>>(A.Item1.Result);
        //        //List<RiskAPIRequestModel> _RisskData = _mapper.Map<List<RiskAPIRequestModel>>(A.Item2);
        //        if (_excelData.Count > 50)
        //        {
        //            _toastNotification.AddErrorToastMessage("The number of customer data per file should not exceed 50.");
        //            return View(_documentUploadModel);
        //        }
        //        else
        //        {
                   
        //            foreach (var e1 in _excelData)
        //            {
        //                if (e1.risk == "No")
        //                {
        //                    _toastNotification.AddErrorToastMessage(string.Format("CustomerId {0} - Insuffiectent Data for Risk Assessment", e1.CustomerId));
        //                }
        //                else
        //                {

        //                    var customerId = e1.CustomerId;
        //                    var result = _customerCaseService.customerIdcheck(customerId);

        //                    if (result.Result == null || result.Result == "")
        //                    {
        //                        _toastNotification.AddErrorToastMessage(string.Format("CustomerId {0} Is Not Screened ", e1.CustomerId));
        //                    }
        //                    else
        //                    {
        //                        if (e1.CustomerName == "" || e1.CustomerName == null)
        //                        {
        //                            _toastNotification.AddErrorToastMessage(string.Format("Please Enter Customer Name For Customer Id {0} ", e1.CustomerId));
                                   
        //                        }

        //                        else
        //                        {
        //                            _toastNotification.AddSuccessToastMessage("Risk Assessment Sucessfully ");
        //                        }
        //                    }
        //                }
                        
        //                _docUpload.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId);
        //                for (var i = 0; i < _docUpload.RiskTypeCategoryDTO.Count; i++)
        //                {
        //                    for (var j = 0; j < _docUpload.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
        //                    {
        //                        var items = _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
        //                        _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
        //                    }
        //                }


        //            }
        //        }
                
        //    }
        //    return View(_docUpload);
        //}
        //    public IActionResult RiskCorporateBulkUpload()
        //{
        //    string type = "C";
        //    DocumentUploadModel _docUpload = new DocumentUploadModel { itemId = (int)ItemType.clientcase, branchId = _clientHandler.GetBranchId(), typeId = type };

        //    _docUpload.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId);
        //    for (var i = 0; i < _docUpload.RiskTypeCategoryDTO.Count; i++)
        //    {
        //        for (var j = 0; j < _docUpload.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
        //        {
        //            var items = _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
        //            _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
        //        }
        //    }

        //    return View(_docUpload);
        //}

        //public static string StringToCSVCell(string str)
        //{
        //    bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n"));

        //    if (mustQuote)
        //    {
        //        StringBuilder sb = new StringBuilder();
        //        sb.Append("\"");
        //        foreach (char nextChar in str)
        //        {
        //            sb.Append(nextChar);
        //            if (nextChar == '"') sb.Append("\"");
        //        }
        //        sb.Append("\"");
        //        return sb.ToString();
        //    }

        //    return str;
        //}

        //public async Task<IActionResult> ExportRiskReport(string fromDate, string toDate, string createdByUserID, string searchvalue, string riskType, int riskLevel = 0)
        //{
        //    var clientId = _clientHandler.GetClientId();

        //    List<RiskExcelReportDTO> riskReports = _mapper.Map<List<RiskExcelReportDTO>>(_riskService.GetRiskReportForExcel(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate), createdByUserID, searchvalue, riskType, riskLevel, clientId).Result);

        //    string CSVText = "";

        //    string details = "Report: Risk Reports\r\n\r\n" +
        //        "Date Range       :   " + fromDate + "  to  " + toDate + "\r\n" +
        //        "Risk   Type      :   " + riskType + "\r\n" +
        //        "Search Value     :   " + searchvalue + "\r\n" +
        //        "Created By       :   " + createdByUserID + "\r\n" +
        //        "Risk Level        :   " + riskLevel + "\r\n";
        //    CSVText += details;
        //    CSVText += "\r\n";

        //    if (riskReports.Count == 0)
        //    {
        //        Console.WriteLine("Got 0 records from database return empty CSV");

        //        CSVText += "CustomerCode,CustomerName,DateOfAssessment,RiskScoreSum,RiskScoreCount,RiskScoreBeforeOverride,FinalRiskScore,CustomerNationality";
        //        return File(Encoding.ASCII.GetBytes(CSVText), "text/csv");
        //    }

        //    var riskReportDict = new Dictionary<string, List<string>>();

        //    List<string> riskTypeItems = new List<string>
        //    {
        //        "Customer code", "Customer name", "Customer nationality", "Risk type",
        //        "Date of assessment", "Assessment version", "Risk score sum",
        //        "Risk score count", "Risk score before override", "Final risk score","Remarks"
        //    };

        //    foreach (var riskItem in riskReports)
        //    {
        //        if (riskTypeItems.Contains(riskItem.LovTypeName)) { continue; }
        //        riskTypeItems.Add(riskItem.LovTypeName);
        //    }

        //    foreach (var riskTypeItem in riskTypeItems)
        //    {
        //        riskReportDict.Add(riskTypeItem, new List<string>());
        //    }

        //    foreach (var riskItem in riskReports)
        //    {
        //        int entryIndex = riskReportDict["Customer code"].FindIndex(val => val == riskItem.CustomerCode);

        //        string riskTypeToAdd = riskItem.LovTypeName;
        //        string riskTypeValueToAdd = $"{riskItem.LovRiskData} ({(riskItem.OverRideScore != "0" ? riskItem.OverRideScore : riskItem.LovRiskScore)})";

        //        if (entryIndex == -1)
        //        {
        //            riskReportDict["Customer code"].Add(riskItem.CustomerCode);
        //            riskReportDict["Customer name"].Add(riskItem.CustomerName);
        //            riskReportDict["Date of assessment"].Add(riskItem.DateOfAssessment);
        //            riskReportDict["Assessment version"].Add(riskItem.RiskAssessmentVersion);
        //            riskReportDict["Risk type"].Add(riskItem.RiskType);
        //            riskReportDict["Risk score sum"].Add(riskItem.RiskScoreSum);
        //            riskReportDict["Risk score count"].Add(riskItem.RiskScoreCount);
        //            riskReportDict["Risk score before override"].Add(riskItem.RiskScoreBeforeOverride);
        //            riskReportDict["Final risk score"].Add(riskItem.FinalRiskScore);
        //            riskReportDict["Customer nationality"].Add(riskItem.CustomerNationality);
        //            riskReportDict["Remarks"].Add(riskItem.Remarks);

        //            foreach (var riskTypeItem in riskTypeItems.Skip(10))
        //            {
        //                if (riskTypeItem == riskTypeToAdd)
        //                {
        //                    riskReportDict[riskTypeItem].Add(riskTypeValueToAdd);
        //                    continue;
        //                }

        //                riskReportDict[riskTypeItem].Add("");
        //            }

        //            continue;
        //        }

        //        riskReportDict[riskTypeToAdd][entryIndex] = riskTypeValueToAdd;
        //    }

        //    var HeaderNames = riskReportDict.Keys;
        //    int maxSize = riskReportDict.Values.Max(a => a != null ? a.Count : 0);

        //    CSVText += string.Join(",", HeaderNames.Select(val => StringToCSVCell(val?.Trim() ?? ""))) + '\n';

        //    for (int i = 0; i < maxSize; i++)
        //    {
        //        foreach (string HeaderName in HeaderNames)
        //        {
        //            List<string> value = riskReportDict[HeaderName];

        //            if ((value != null) && (i < value.Count)) { CSVText += StringToCSVCell(value[i]?.Trim() ?? ""); }
        //            if (HeaderName != HeaderNames.Last()) { CSVText += ','; }
        //        }

        //        CSVText += '\n';
        //    }

        //    return File(Encoding.ASCII.GetBytes(CSVText), "text/csv");
        //}

        public JsonResult GetRiskDetailsByCIdVendor(string id)
        {
            RiskAssessmentVendorModelV2 _riskVendormodel = new RiskAssessmentVendorModelV2
            {
                RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("V", 1, 0, 0, clientId)
            };

            RiskAssessmentVendorDTOV2 model = _mapper.Map<RiskAssessmentVendorDTOV2>(_riskService.GetRiskDetailsOfVendorByCID(id).Result);

            for (var i = 0; i < _riskVendormodel.RiskTypeCategoryDTO.Count; i++)
            {
                for (var j = 0; j < _riskVendormodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                {
                    var items = _riskVendormodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    if (model.ReportDataDTO == null) { continue; }
                    for (var m = 0; m < model.ReportDataDTO.Count; m++)
                    {
                        if (_riskVendormodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                        {
                            if (_riskVendormodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                            {
                                _riskVendormodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                _riskVendormodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                _riskVendormodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                            }
                        }
                    }
                }
            }
            if(_riskVendormodel.RiskTypeCategoryDTO.Count>0)

            model.RiskTypeCategoryDTO = _riskVendormodel.RiskTypeCategoryDTO;

            return Json(model);
        }

        //[HttpPost]
        //public async Task<IActionResult> RiskCorporateBulkUpload(DocumentUploadModel _documentUploadModel)
        //{
        //    string type = "C";
        //    DocumentUploadModel _docUpload = new DocumentUploadModel { itemId = (int)ItemType.clientcase, branchId = _clientHandler.GetBranchId(), typeId = type };
        //    if (_documentUploadModel.fileUpload != null)
        //    {
        //        var newFileName = "";
        //        if (_documentUploadModel.typeId == "C")
        //        {
        //            newFileName = "Corporate_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + _documentUploadModel.fileUpload.FileName;
        //        }
        //        _documentUploadModel.ClientId = _clientHandler.GetClientId();
        //        DocumentsModel _documentsModel = _fileUploader.UploadFile(_documentUploadModel.ClientId, (ItemType)_documentUploadModel.itemId, _documentUploadModel.branchId, _documentUploadModel.fileUpload, newFileName);
        //        _documentsModel.AddedBy = _clientHandler.GetUserId();
        //        _documentsModel.ClientId = _clientHandler.GetClientId();
        //        Tuple<ServiceResponse<List<CorporateExcel>>, RiskAPIRequestModel> A = _customerCaseService.SaveCorporateRiskExcelData(_mapper.Map<DocumentsDTO>(_documentsModel), _documentUploadModel.typeId, _documentUploadModel.C6Threshold, _documentUploadModel.Threshold);
        //        List<CorporateExcel> _excelData = _mapper.Map<List<CorporateExcel>>(A.Item1.Result);

        //        if (_excelData.Count > 50)
        //        {
        //            _toastNotification.AddErrorToastMessage("The number of customer data per file should not exceed 50.");
        //            return View(_documentUploadModel);
        //        }
        //        else
        //        {
        //            foreach (var e1 in _excelData)
        //            {
        //                if (e1.risk == "No")
        //                {
        //                    _toastNotification.AddErrorToastMessage(string.Format("CustomerId {0} - Insuffiectent Data for Risk Assessment", e1.CustomerId));
        //                }
        //                else
        //                {

        //                    var customerId = e1.CustomerId;
        //                    var result = _customerCaseService.customerIdcheck(customerId);

        //                    if (result.Result == null || result.Result == "")
        //                    {
        //                        _toastNotification.AddErrorToastMessage(string.Format("CustomerId {0} Is Not Screened ", e1.CustomerId));
        //                    }
        //                    else
        //                    {
        //                        if (e1.LegalNameOfEntity == "" || e1.LegalNameOfEntity == null)
        //                        {
        //                            _toastNotification.AddErrorToastMessage("Customer Name Is Empty.");
        //                        }

        //                        else
        //                        {
        //                            _toastNotification.AddSuccessToastMessage("Risk Assessment Sucessfully ");
        //                        }
        //                    }
        //                }
        //            }
        //            _docUpload.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId);
        //            for (var i = 0; i < _docUpload.RiskTypeCategoryDTO.Count; i++)
        //            {
        //                for (var j = 0; j < _docUpload.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
        //                {
        //                    var items = _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
        //                    _docUpload.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
        //                }
        //            }

        //        }
        //    }
                        
        //    return View(_docUpload);
        //}


        public JsonResult GetRiskDetailsByCIdIndividual(string id)
        {
            RiskModelV2 _riskmodel = new RiskModelV2
            {
                RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId)
            };

            RiskModelV2 model = _mapper.Map<RiskModelV2>(_riskService.GetRiskDetailsOfIndividualByCID(id).Result);

            if (model != null)
            {
                for (var i = 0; i < _riskmodel.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < _riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        if (model.ReportDataDTO == null) { continue; }
                        var items = _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        for (var m = 0; m < model.ReportDataDTO.Count; m++)
                        {
                            var isCountry = _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                            var isNotCountryAndIdMatches = !isCountry && _riskmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id;

                            if (isNotCountryAndIdMatches || isCountry)
                            {
                                if (_riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                                {
                                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                                }
                            }
                        }

                    }
                }





                //for (var i = 0; i < _riskmodel.RiskTypeCategoryDTO.Count; i++)
                //{
                //    for (var j = 0; j < _riskmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                //    {
                //        if (model.ReportDataDTO == null) { continue; }
                //        for (var m = 0; m < model.ReportDataDTO.Count; m++)
                //        {
                //            if (_riskmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
                //            {
                //                if (_riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                //                {
                //                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                //                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                //                    _riskmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                //                }
                //            }
                //        }
                //    }
                //}

                model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
            }
            return Json(model);
        }
        public JsonResult GetRiskDetailsByCIdCorporate(string id)
        {
            RiskCorpCustomerModelV2 _riskCorpmodel = new RiskCorpCustomerModelV2
            {
                RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId)
            };

            RiskCorpCustomerDTOV2 model = _mapper.Map<RiskCorpCustomerDTOV2>(_riskService.GetRiskDetailsOfCorporateByCID(id).Result);

            for (var i = 0; i < _riskCorpmodel.RiskTypeCategoryDTO.Count; i++)
            {
                for (var j = 0; j < _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                {
                    var items = _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                    if (model != null) 
                    {
                        if (model.ReportDataDTO == null) { continue; }
                    }

                    for (var m = 0; m < model.ReportDataDTO.Count; m++)
                    {
                        var isCountry = _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].lov_country_duplicate == 1;
                        var isNotCountryAndIdMatches = !isCountry && _riskCorpmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id;

                        if (isNotCountryAndIdMatches || isCountry)
                        {
                            if (_riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
                            {
                                _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
                                _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
                                _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
                            }
                        }
                    }
                }
            }


            //for (var i = 0; i < _riskCorpmodel.RiskTypeCategoryDTO.Count; i++)
            //{
            //    for (var j = 0; j < _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
            //    {
            //        var items = _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
            //        if (model.ReportDataDTO == null) { continue; }
            //        for (var m = 0; m < model.ReportDataDTO.Count; m++)
            //        {
            //            if (_riskCorpmodel.RiskTypeCategoryDTO[i].Id == model.ReportDataDTO[m].lov_type_category_id)
            //            {
            //                if (_riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].Id == model.ReportDataDTO[m].lov_type_id)
            //                {
            //                    _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemTxt = model.ReportDataDTO[m].lov_risk_data;
            //                    _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].ItemScore = model.ReportDataDTO[m].lov_risk_score;
            //                    _riskCorpmodel.RiskTypeCategoryDTO[i].RiskTypes[j].OverrideScore = model.ReportDataDTO[m].Over_ride_Score;
            //                }
            //            }
            //        }
            //    }
            //}

            model.RiskTypeCategoryDTO = _riskCorpmodel.RiskTypeCategoryDTO;

            return Json(model);
        }



        public JsonResult GetcustomerRecords(string searchTerm,string cust_type) 
        {


            var clientId = _clientHandler.GetClientId();

            var res = _mapper.Map<List<CustomerMasterModel>>(_customerMasterService.GetDetailsBySearch(searchTerm, cust_type, clientId));


            return Json(res);

        }


    }
}
