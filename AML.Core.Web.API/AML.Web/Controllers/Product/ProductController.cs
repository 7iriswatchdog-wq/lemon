using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.Service.LovMaster;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.ProductMaster;
using AML.Core.ServiceContract.Report;
using AML.Core.ServiceContract.TransactionScreening;
using AML.Core.ServiceContract.User;
using AML.DTO.DTO.LovMaster;
using AML.DTO.DTO.ProdMaster;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerMaster;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.ProductMaster;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.User;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AML.Web.Controllers.Product
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class ProductController : Controller
    {
        private IUserService _userService;
        private IMapper _mapper;
        private IReportService _reportService;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private IHttpClientHandler _clientHandler;
        private ICustomerScreeningService _customerScreeningService;
        private ICustomerCaseService _customerCaseService;
        private ICaseDocumentService _caseDocumentService;
        private IFreeSourceRepository _freeSourceRepository;
        private ICaseCommentService _caseCommentService;
        private ICustomerCategoryService _customerCategoryService;
        private ICountryService _countryService;
        private ICustomerMasterService _customerMasterService;
        private ITransactionScreeningService _transactionScreeningService;
        private ILovMasterService _lovMasterService;
        private IProdMasterService _prodMasterService;
        private readonly IToastNotification _toastNotification;
        private ICommonService _commonService;
        private string baseC6URL = string.Empty;
        private string _c6Username;


        public ProductController(IUserService userService, IMapper mapper, IReportService reportService,
         IHttpClientHandler clientHandler, ICaseCommentService caseCommentService, ICustomerCategoryService customerCategoryService,
        IViewRenderService viewRenderService, IExportDataService exportService, ICustomerScreeningService customerScreeningService,
        ICustomerCaseService customerCaseService, ICaseDocumentService caseDocumentService, IFreeSourceRepository freeSourceRepository, ILovMasterService lovMasterService, IProdMasterService prodMasterService,
        ICountryService countryService, ICustomerMasterService customerMasterService, ITransactionScreeningService transactionScreeningService, IToastNotification toastNotification, ICommonService commonService)
        {
            _userService = userService;
            _mapper = mapper;
            _toastNotification = toastNotification;
            _reportService = reportService;
            _viewRenderService = viewRenderService;
            _exportService = exportService;
            _clientHandler = clientHandler;
            _customerScreeningService = customerScreeningService;
            _customerCaseService = customerCaseService;
            _caseDocumentService = caseDocumentService;
            _freeSourceRepository = freeSourceRepository;
            _caseCommentService = caseCommentService;
            _customerCategoryService = customerCategoryService;
            _countryService = countryService;
            _customerMasterService = customerMasterService;
            _transactionScreeningService = transactionScreeningService;
            _commonService = commonService;
            var clientId = _clientHandler.GetClientId();
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            _c6Username = clientDetails?.C6Username;
            _lovMasterService = lovMasterService;
            baseC6URL = clientDetails.C6BaseUrl;
            _prodMasterService= prodMasterService;

        }

        [HttpGet("/product/create")]
        public ActionResult Create()
        {
            ProductRiskModel model= new ProductRiskModel();

            var clientId = _clientHandler.GetClientId();
            model.ProdRiskTypeCategoryDTO = _prodMasterService.GetAllProductRiskConfig(1, 0, 0, clientId);
            for (var i = 0; i < model.ProdRiskTypeCategoryDTO.Count; i++)
            {
                
                    var items = model.ProdRiskTypeCategoryDTO[i].ProdRiskItems;
                    model.ProdRiskTypeCategoryDTO[i].ProdRiskItem = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<ProdRiskItemsDTO>>(items.ToList()), "Score", "ProdRiskItem");
                
            }
            return View(model);
        }

        [HttpPost("/product/create")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ProductRiskModel model)
        {
            model.userAuthorised = true;
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            try
            {

                var result = _prodMasterService.Create(_mapper.Map<ProductRiskDTO>(model));
                if (result.Status == 200)
                {
                    _toastNotification.AddSuccessToastMessage("Risk Assessment Creation Successful.");
                    return RedirectToAction("Create");
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

            ProductRiskModel _model = new ProductRiskModel
            {
                userAuthorised = true,

                ProdRiskTypeCategoryDTO = _prodMasterService.GetAllProductRiskConfig(1, 0, 0, clientId)
        };
            for (var i = 0; i < model.ProdRiskTypeCategoryDTO.Count; i++)
            {

                var items = model.ProdRiskTypeCategoryDTO[i].ProdRiskItems;
                    model.ProdRiskTypeCategoryDTO[i].ProdRiskItem = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<ProdRiskItemsDTO>>(items.ToList()), "Score", "ProdRiskItem");
                
            }
            return View(model);
        }



        [Route("/product/risk-config-master")]
        public ActionResult ProductRiskConfigurationMaster()
        {
           
            return View();
        }


        [HttpPost]
        public JsonResult CustomPaginationProductRiskTypeCategory(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            List<ProdtypecategoryDTO> abc = _mapper.Map<List<ProdtypecategoryDTO>>(_lovMasterService.GetProductRiskCategoryType(clientId));
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
        public JsonResult custompaginationProdRiskItems(DataTableModel model, string riskTypeID)
        {
            var clientId = _clientHandler.GetClientId();
            List<ProdRiskItemsModel> abc = _mapper.Map<List<ProdRiskItemsModel>>(_prodMasterService.GetProdRiskItems(Convert.ToInt32(riskTypeID), clientId));
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
        public ActionResult SaveProductCategoryType(ProdRiskConfigurationModel model)
        {
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            if (ModelState.IsValid)
            {
                string returnMsg = null;
                bool isSuccess = true;

                _prodMasterService.AddProductRiskTypeCategory(_mapper.Map<ProdRiskConfigurationMasterDTO>(model), out returnMsg, out isSuccess);

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
            RIskConfigurationMasterModel modelVal = new RIskConfigurationMasterModel();
            //var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            //modelVal.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            modelVal.RiskCategoryID = "0";
            return View(modelVal);
        }

        [HttpPost]
        public ActionResult SaveProdRiskItems(ProdRiskConfigurationModel model)
        {
            if (ModelState.IsValid)
            {
                model.ClientId = _clientHandler.GetClientId();
                model.CreatedBy = _clientHandler.GetUserId();

                string returnMsg = null;
                bool isSuccess = true;
                model.ClientId = _clientHandler.GetClientId();
                _prodMasterService.AddProdRiskItems(_mapper.Map<ProdRiskConfigurationMasterDTO>(model), out returnMsg, out isSuccess);

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
            RIskConfigurationMasterModel modelVal = new RIskConfigurationMasterModel();
            var riskCategoriesAll = _lovMasterService.GetAllLovMasterCategories();
            modelVal.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            modelVal.RiskCategoryID = "0";
            return View(modelVal);

        }

        public JsonResult GetProdRiskCategoryTypes()
        {
            var clientId = _clientHandler.GetClientId();
            dynamic ProdriskCategoryTypes = _prodMasterService.GetProdRiskCategoryTypes(clientId);
            return Json(new SelectList(ProdriskCategoryTypes, "Id", "ProdRiskTypeCategory"));
        }

        public ActionResult GetProductRiskReport()
        {
            ProductRiskRequestModel model = new ProductRiskRequestModel();

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

        public JsonResult ProductRiskReportCustomPagination(DataTableModel model, string fromDate, string toDate, string createdByUserID,  int riskLevel = 0)
        {
            var clientId = _clientHandler.GetClientId();
            List<ProductRiskReportModel> riskReports = _mapper.Map<List<ProductRiskReportModel>>(_prodMasterService.GetProductRiskReportBetweenDateAndType(clientId, createdByUserID, Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate), riskLevel).Result);
            int totalcount = riskReports.Count;
            int filteredcount = riskReports.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                riskReports = riskReports.Where(m => m.ProductCode.ToLower().Contains(model.search.value.ToLower()) || m.ProductName.ToLower().Contains(model.search.value.ToLower())
                ||  m.ScoreBeforeOverride.ToLower().Contains(model.search.value.ToLower())).ToList();
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

        public List<T> Sort<T>(List<T> input, string property, string dir)

        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty?.GetValue(p, null)).ToList();

        }

        public ActionResult ViewProductRiskDetails(int id, string type)
        {

            dynamic model = null;
            var clientId = _clientHandler.GetClientId();
            int dt = 0;
            //RiskCorpCustomerModel _riskmodel = new RiskCorpCustomerModel();
            ProductRiskModel _prodmodel = new ProductRiskModel();
                    
                    model = _mapper.Map<ProductRiskModel>(_prodMasterService.GetProdRiskDetails(id).Result);
                    if (model != null)
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

            _prodmodel.ProdRiskTypeCategoryDTO = _prodMasterService.GetAllProductRiskConfig(1, 0, 0, clientId);

            for (var i = 0; i < _prodmodel.ProdRiskTypeCategoryDTO.Count; i++)
            {
                for (var m = 0; m < model.ProdReportDataDTO.Count; m++)
                {


                    if (_prodmodel.ProdRiskTypeCategoryDTO[i].Id == model.ProdReportDataDTO[m].Id)
                    {
                        _prodmodel.ProdRiskTypeCategoryDTO[i].prodItemTxt = model.ProdReportDataDTO[m].ProdRiskItem;
                        _prodmodel.ProdRiskTypeCategoryDTO[i].prodItemScore = model.ProdReportDataDTO[m].ProdRiskScore;
                        _prodmodel.ProdRiskTypeCategoryDTO[i].OverrideScore = model.ProdReportDataDTO[m].OverrideScore;
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
                  model.ProdRiskTypeCategoryDTO = _prodmodel.ProdRiskTypeCategoryDTO;



                    
            

            return View(model);
        }
    }
}
