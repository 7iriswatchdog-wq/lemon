using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.Branch;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.Department;
using AML.Core.ServiceContract.EWRA;
using AML.DTO.DTO.EWRA;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.EWRA;
using AML.Web.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AML.Web.Controllers.EWRA
{
    public class EwraController : Controller
    {
        private IEWRAService _ewraService;
        private IMapper _mapper;
        private IDepartmentService _departmentService;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private ICustomerCaseService _customerCaseService;
        private ICustomerMasterService _customerMasterService;
        private IBranchService _branchService;
        private ICountryService _countryService;
        public EwraController(IEWRAService ewraService, ICountryService countryService, IMapper mapper, IToastNotification toastNotification, IDepartmentService departmentService, IHttpClientHandler clientHandler, ICustomerCaseService customerCaseService, ICustomerMasterService customerMasterService, IBranchService branchService)
        {
            _toastNotification = toastNotification;
            _mapper = mapper;
            _departmentService = departmentService;
            _clientHandler = clientHandler;
            _customerCaseService = customerCaseService;
            _customerMasterService = customerMasterService;
            _branchService = branchService;
            _ewraService = ewraService;
            _countryService = countryService;
            var clientId = _clientHandler.GetClientId();
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public IActionResult Assessment()
        {
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var res = _ewraService.GetPendingEWRA(1, userId, clientId);
            var id = res.Result.ParseInt();
            if (res.Result == "")
            {
                EWRAModel model = new EWRAModel();
                model.EWRACustomerTypes = _mapper.Map<List<EWRACustomerType>>(_ewraService.GetAllCustomerTypes(1, clientId));//1 for assessment and 2 for config
                model.EWRAProductCategory = _mapper.Map<List<EWRAProductCategory>>(_ewraService.GetAllProductCategories(clientId));
                model.EWRACounterPartyType = _mapper.Map<List<EWRACounterPartyType>>(_ewraService.GetAllCounterParty(1, clientId));
                model.EWRADeliveryType = _mapper.Map<List<EWRADeliveryType>>(_ewraService.GetAllDeliveryType(clientId));
                model.MainNationalityLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");

                return View(model);
            }
            else
            {
                EWRAModel model = new EWRAModel();
                var pendingData = _mapper.Map<List<EWRAModel>>(_ewraService.GetEwraQuantitativePendingData(id));
                if (pendingData.Count != 0)
                {

                    model = pendingData[0];

                }
                model.EWRAProductCategory = _mapper.Map<List<EWRAProductCategory>>(_ewraService.GetAllProductCategories(clientId));
                for (var i = 0; i < model.EWRAProductCategory.Count; i++)
                {
                    model.EWRAProductCategory[i].Category = pendingData[0].EWRAProductCategory[i].Category;
                    model.EWRAProductCategory[i].CategoryId = pendingData[0].EWRAProductCategory[i].CategoryId;
                    model.EWRAProductCategory[i].EWRAProductList = pendingData[0].EWRAProductList;
                }
                var jurcnt = model.EWRAJurisdiction.Count;
                for (var i = jurcnt; i < 10; i++)
                {
                    EWRAJurisdiction ej = new EWRAJurisdiction();
                    model.EWRAJurisdiction.Add(ej);
                }
                model.EWRACustomerTypes = _mapper.Map<List<EWRACustomerType>>(_ewraService.GetAllCustomerTypes(1, clientId));//1 for assessment and 2 for config
                model.MainNationalityLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
                model.SaveType = 1;

                return View(model);
            }
        }

        [HttpPost]
        public IActionResult Assessment(EWRAModel model)
        {
            var clientId = _clientHandler.GetClientId();
            model.clientId = clientId;
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            int Id = 0;
            if (model.EWRAJurisdiction != null)
            {
                for (var i = 0; i < model.EWRAJurisdiction.Count; i++)
                {
                    if (model.EWRAJurisdiction[i].CountryId.ToString() == "")
                    {
                        model.EWRAJurisdiction[i].CountryId = 0;
                    }
                }
            }
            //if (ModelState.IsValid)
            //{
            model.CreatedBy = _clientHandler.GetUserId().ToString();
            DateTime dt = DateTime.Now;
            var dtname = dt.Day + "_" + dt.Month + "_" + dt.Year + "_" + dt.Hour + "_" + dt.Minute + "_" + dt.Second;
            model.QuantitativeName = string.Concat("Qnty_", dtname);
            var result = _ewraService.SaveCustomerProfile(_mapper.Map<EWRAModelDTO>(model));
            model.Id = Convert.ToInt32(result.Result != null ? result.Result.Split('Ø')[0] : "0");
            Id = model.Id;
            if (result.Status != 200)
            {
                _toastNotification.AddErrorToastMessage(result.Message);
            }
            else
            {
                _toastNotification.AddSuccessToastMessage("EWRA Saved Successfully.");

            }

            if (model.SaveType == 1)
            {
                model.EWRACustomerTypes = _mapper.Map<List<EWRACustomerType>>(_ewraService.GetAllCustomerTypes(1, clientId));
                model.MainNationalityLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
                model.Id = Id;
                return View(model);
            }
            else
            {
                EWRAModel model1 = new EWRAModel();
                model1.EWRACustomerTypes = _mapper.Map<List<EWRACustomerType>>(_ewraService.GetAllCustomerTypes(1, clientId));
                model1.EWRAProductCategory = _mapper.Map<List<EWRAProductCategory>>(_ewraService.GetAllProductCategories(clientId));
                model1.EWRACounterPartyType = _mapper.Map<List<EWRACounterPartyType>>(_ewraService.GetAllCounterParty(1, clientId));
                model1.EWRADeliveryType = _mapper.Map<List<EWRADeliveryType>>(_ewraService.GetAllDeliveryType(clientId));
                model1.MainNationalityLists = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Id", "Name");
                return View(model1);
            }
        }
        public JsonResult GetJurisdictionData(int Id)
        {
            dynamic jurisdictionData = _ewraService.GetAllJurisdictionCountry(Id);
            EWRAJurisdiction jmodel = new EWRAJurisdiction();
            jmodel.CountryRiskRating = jurisdictionData[0].CountryRiskRating;
            jmodel.CountryRiskScore = jurisdictionData[0].CountryRiskScore;
            return Json(jmodel);
        }
        [HttpPost]
        public JsonResult SaveCustomerProfile(EWRAModel model)
        {
            var clientId = _clientHandler.GetClientId();
            model.clientId = clientId;
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            if (ModelState.IsValid)
            {
                model.CreatedBy = _clientHandler.GetUserId().ToString();
                var result = _ewraService.SaveCustomerProfile(_mapper.Map<EWRAModelDTO>(model));
                //model.Id = result.Result.;
                var res = result.Result.Split('Ø');
                model.Id = Convert.ToInt32(res[0]);
                model.isVolume = res[1];
                model.isCount = res[2];
                model.isTransaction = res[3];
                if (result.Status != 200)
                {
                    _toastNotification.AddErrorToastMessage(result.Message);
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("Customer Profile Data Saved Successfully.");

                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Customer Profile Data Saving Failed.");
            }
            //return View(model);
            return Json(model);
        }
        [HttpGet]
        public IActionResult EWRAConfig()
        {
            List<EWRACustomerType> model = new List<EWRACustomerType>();
            var clientId = _clientHandler.GetClientId();

            model = _mapper.Map<List<EWRACustomerType>>(_ewraService.GetAllCustomerTypes(2, clientId));
            List<EWRACounterPartyType> countermodel = new List<EWRACounterPartyType>();
            countermodel = _mapper.Map<List<EWRACounterPartyType>>(_ewraService.GetAllCounterParty(0, clientId));


            EWRAConfigModel cm = new EWRAConfigModel();

            List<EWRAQuantitativeConfigModel> qtmlist = new List<EWRAQuantitativeConfigModel>();
            EWRAQuantitativeConfigModel qtm = new EWRAQuantitativeConfigModel();
            qtm.EWRACounterPartyType = countermodel;
            qtm.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            List<EWRACustomerType> lm = new List<EWRACustomerType>();
            foreach (var i in model)
            {
                EWRACustomerType m = new EWRACustomerType();
                m.CustTypeId = i.CustTypeId;
                m.CustType = i.CustType;
                m.isActive = i.isActive;
                lm.Add(m);
            }
            qtm.CustomerType = lm;
            qtmlist.Add(qtm);
            cm.EWRAQuantitativeConfigModel = qtmlist;





            EWRAQualitativeConfigModel qlm = new EWRAQualitativeConfigModel();
            List<EWRAQualitativeConfigModel> qlmlist = new List<EWRAQualitativeConfigModel>();
            qlm.EWRARiskTypeList = new SelectList(_ewraService.GetAllRiskConfigTypes(), "RiskTypeId", "RiskType");
            qlmlist.Add(qlm);
            cm.EWRAQualitativeConfigModel = qlmlist;

            return View(cm);
        }

        [HttpPost]
        public IActionResult EWRAConfig(EWRAConfigModel model)
        {
            var clientId = _clientHandler.GetClientId();
            model.ClientId = clientId;
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();

            if (ModelState.IsValid)
            {
                string returnMsg = null;
                bool isSuccess = true;
                _ewraService.AddCustomerType(_mapper.Map<EWRAConfigModelDTO>(model), out returnMsg, out isSuccess);
                _ewraService.AddCounterparty(_mapper.Map<EWRAConfigModelDTO>(model), out returnMsg, out isSuccess);
                _ewraService.AddEWRAConfig(_mapper.Map<EWRAConfigModelDTO>(model), out returnMsg, out isSuccess);

                if (!isSuccess)
                {
                    _toastNotification.AddErrorToastMessage(returnMsg);
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("Configuration Saved Successfully.");
                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Configuration Failed.");
            }
            List<EWRACustomerType> model1 = new List<EWRACustomerType>();
            model1 = _mapper.Map<List<EWRACustomerType>>(_ewraService.GetAllCustomerTypes(2, clientId));
            List<EWRACounterPartyType> countermodel = new List<EWRACounterPartyType>();
            countermodel = _mapper.Map<List<EWRACounterPartyType>>(_ewraService.GetAllCounterParty(0, clientId));
            EWRAConfigModel cm = new EWRAConfigModel();
            List<EWRAQuantitativeConfigModel> qtmlist = new List<EWRAQuantitativeConfigModel>();
            EWRAQuantitativeConfigModel qtm = new EWRAQuantitativeConfigModel();
            qtm.EWRACounterPartyType = countermodel;
            qtm.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            List<EWRACustomerType> lm = new List<EWRACustomerType>();
            foreach (var i in model1)
            {
                EWRACustomerType m = new EWRACustomerType();
                m.CustTypeId = i.CustTypeId;
                m.CustType = i.CustType;
                m.isActive = i.isActive;
                lm.Add(m);
            }
            qtm.CustomerType = lm;
            qtmlist.Add(qtm);
            cm.EWRAQuantitativeConfigModel = qtmlist;

            EWRAQualitativeConfigModel qlm = new EWRAQualitativeConfigModel();
            List<EWRAQualitativeConfigModel> qlmlist = new List<EWRAQualitativeConfigModel>();
            qlm.EWRARiskTypeList = new SelectList(_ewraService.GetAllRiskConfigTypes(), "RiskTypeId", "RiskType");
            qlm.RiskTypeId = 0;
            qlmlist.Add(qlm);
            cm.EWRAQualitativeConfigModel = qlmlist;
            //cm.EWRAQualitativeConfigModel[0].RiskTypeId = 0;
            return View(cm);
        }

        [HttpPost]
        public JsonResult CustomPaginationRiskDescription(DataTableModel model, int riskTypeID)
        {
            var clientId = _clientHandler.GetClientId();
            List<RiskDescriptionDataDTO> abc = _mapper.Map<List<RiskDescriptionDataDTO>>(_ewraService.getAllRiskDescription(riskTypeID, clientId));
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
        [HttpGet]
        public JsonResult GetAllRiskDescription(int riskTypeID)
        {
            var clientId = _clientHandler.GetClientId();
            List<EWRARiskModelDTO> abc = _mapper.Map<List<EWRARiskModelDTO>>(_ewraService.getAllRiskDescription(riskTypeID, clientId));
            return Json(new SelectList(abc, "DescriptionId", "RiskDescription"));
        }
        [HttpGet]
        public JsonResult CustomPaginationInherentRisk(DataTableModel model, int riskTypeID)
        {
            List<InherentRiskModelDTO> abc = _mapper.Map<List<InherentRiskModelDTO>>(_ewraService.getAllInherentRisk(riskTypeID));
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

        [HttpGet]
        public JsonResult CustomPaginationKey(DataTableModel model, int riskTypeID)
        {
            List<KeyControlModelDTO> abc = _mapper.Map<List<KeyControlModelDTO>>(_ewraService.getAllKeyControl(riskTypeID));
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

        [HttpGet]
        public JsonResult CustomPaginationResidual(DataTableModel model, int riskTypeID)
        {
            List<ResidualRiskModelDTO> abc = _mapper.Map<List<ResidualRiskModelDTO>>(_ewraService.getAllResidual(riskTypeID));
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
        public IActionResult GetEWRAReport()
        {
            EWRAModel model = new EWRAModel();
            model.FromDate = System.DateTime.Now.AddDays(-7);
            model.ToDate = System.DateTime.Now;

            return View(model);
        }
        public JsonResult EWRAReportCustomPagination(DataTableModel model, string fromDate, string toDate)
        {
            var clientId = _clientHandler.GetClientId();

            List<EWRAModel> fraudReports = _mapper.Map<List<EWRAModel>>(_ewraService.GetEwraBetweenDates(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate), clientId).Result);

            int totalcount = fraudReports.Count;
            int filteredcount = fraudReports.Count;
            var data = fraudReports.Skip(model.start).Take(model.length).ToList();
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
        public ActionResult ViewEWRAReport(int id, int Qid)
        {
            EWRAModel model = new EWRAModel();
            var x = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetConsolidatedEwra(id, 1, Qid));
            foreach (var item in x)
            {
                model.Id = item.Id;
                model.CreatedBy = item.CreatedBy;
                model.CreatedOn = item.CreatedOn;
                model.QuantitativeOverallRisk = item.QuantitativeOverallRisk;
                model.QuantitativeOverallScore = item.QuantitativeOverallScore;
                model.CustOverallRisk = item.CustOverallRisk;
                model.CustOverallScore = item.CustOverallScore;
                model.CounterOverallRisk = item.CounterOverallRisk;
                model.CounterOverallScore = item.CounterOverallScore;
                model.ProductOverallRisk = item.ProductOverallRisk;
                model.ProductOverallScore = item.ProductOverallScore;
                model.JurisdictionOverallRisk = item.JurisdictionOverallRisk;
                model.JurisdictionOverallScore = item.JurisdictionOverallScore;
                model.DeliveryOverallRisk = item.DeliveryOverallRisk;
                model.DeliveryOverallScore = item.DeliveryOverallScore;
            }
            var y = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetConsolidatedEwra(id, 2, Qid));
            foreach (var item in y)
            {
                model.QId = item.QId;
                model.QCreatedBy = item.QCreatedBy;
                model.QCreatedOn = item.QCreatedOn;
                model.QualitativeOverallRating = item.QualitativeOverallRating;
                model.QualitativeOverallScore = item.QualitativeOverallScore;
                model.QCustOverallRisk = item.QCustOverallRisk;
                model.QCustOverallScore = item.QCustOverallScore;
                model.QCounterOverallRisk = item.QCounterOverallRisk;
                model.QCounterOverallScore = item.QCounterOverallScore;
                model.QProductOverallRisk = item.QProductOverallRisk;
                model.QProductOverallScore = item.QProductOverallScore;
                model.QJurisdictionOverallRisk = item.QJurisdictionOverallRisk;
                model.QJurisdictionOverallScore = item.QJurisdictionOverallScore;
                model.QDeliveryOverallRisk = item.QDeliveryOverallRisk;
                model.QDeliveryOverallScore = item.QDeliveryOverallScore;
            }
            var avg = (Convert.ToDouble(model.QualitativeOverallScore) + Convert.ToDouble(model.QuantitativeOverallScore)) / 2;
            var v = String.Format("{0:.##}", avg);
            model.OverallScore = v.ToString();
            if (avg < 1)
            {
                model.OverallRisk = "Low";
            }
            else if (avg >= 2)
            {
                model.OverallRisk = "High";
            }
            else
            {
                model.OverallRisk = "Medium";
            }
            return View(model);
        }
        public ActionResult ViewEWRACustomerProfileReport(int id)
        {
            EWRAModel model = new EWRAModel();
            var x = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetEwraCustomerProfileData(id));
            return View(x[0]);
        }
        public ActionResult ViewEWRACounterPartyReport(int id)
        {
            EWRAModel model = new EWRAModel();
            var x = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetEwraCounterpartyData(id));
            return View(x[0]);
        }
        public ActionResult ViewEWRAProductsReport(int id)
        {
            EWRAModel model = new EWRAModel();
            var x = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetEwraProductsData(id));
            return View(x[0]);
        }
        public ActionResult ViewEWRAJurisdictionReport(int id)
        {
            EWRAModel model = new EWRAModel();
            var x = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetEwraJurisdictionData(id));
            return View(x[0]);
        }
        public ActionResult ViewEWRADeliveryReport(int id)
        {
            EWRAModel model = new EWRAModel();
            var x = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetEwraDeliveryData(id));
            return View(x[0]);
        }

        [HttpGet]
        public IActionResult CategorywiseAssessment()
        {
            EWRAModel model = new EWRAModel();
            var clientId = _clientHandler.GetClientId();
            model.EWRAQualitativeModel = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
            return View(model);
        }

        [HttpGet]
        public IActionResult CategorywiseAssessmentModal()
        {
            var clientId = _clientHandler.GetClientId();
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
            return View(model);
        }

        [HttpGet]
        public IActionResult CategorywiseAssessmentTab()
        {
            var clientId = _clientHandler.GetClientId();
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
            var x = _ewraService.GetPendingEWRA(3, 0, clientId);
            var str = x.Result.Split("¥");

            List<EWRAModel> emlist = new List<EWRAModel>();
            for (var i = 0; i < str.Length - 1; i++)
            {
                var str1 = str[i].Split("Ø");
                EWRAModel m = new EWRAModel();
                m.QId = str1[0].ParseInt();
                m.QuantitativeName = str1[1].ToString();
                emlist.Add(m);
            }
            model.QuantitativeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emlist, "QId", "QuantitativeName");
            model.SaveType = 0;

            model.DateOfAssessment = DateTime.Now;
            return View(model);
        }
        [HttpPost]
        public IActionResult CategorywiseAssessmentTab(EWRAModel model)
        {
            var clientId = _clientHandler.GetClientId();
            model.clientId = clientId;
            var errors = ModelState.Select(x => x.Value.Errors)
                           .Where(y => y.Count > 0)
                           .ToList();
            int Id = 0;
            if (ModelState.IsValid)
            {
                model.QCreatedBy = _clientHandler.GetUserId().ToString();
                var result = _ewraService.SaveCategorywiseAssessmentData(_mapper.Map<EWRAModelDTO>(model));
                model.Id = Convert.ToInt32(result.Result);
                Id = model.Id;
                if (result.Status != 200)
                {
                    _toastNotification.AddErrorToastMessage(result.Message);
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("EWRA Qualitative Assessment Saved Successfully.");

                }
            }
            else
            {
                _toastNotification.AddErrorToastMessage("EWRA Qualitative Assessment Saving Failed.");
            }
            if (model.SaveType == 1)
            {
                EWRAModel mod = new EWRAModel();

                var xyz = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetConsolidatedEwra(0, 2, model.Id));
                foreach (var item in xyz)
                {
                    mod.QId = item.QId;
                    mod.QCreatedBy = item.QCreatedBy;
                    mod.QCreatedOn = item.QCreatedOn;
                    mod.QualitativeOverallRating = item.QualitativeOverallRating;
                    mod.QualitativeOverallScore = item.QualitativeOverallScore;
                    mod.QCustOverallRisk = item.QCustOverallRisk;
                    mod.QCustOverallScore = item.QCustOverallScore;
                    mod.QCounterOverallRisk = item.QCounterOverallRisk;
                    mod.QCounterOverallScore = item.QCounterOverallScore;
                    mod.QProductOverallRisk = item.QProductOverallRisk;
                    mod.QProductOverallScore = item.QProductOverallScore;
                    mod.QJurisdictionOverallRisk = item.QJurisdictionOverallRisk;
                    mod.QJurisdictionOverallScore = item.QJurisdictionOverallScore;
                    mod.QDeliveryOverallRisk = item.QDeliveryOverallRisk;
                    mod.QDeliveryOverallScore = item.QDeliveryOverallScore;
                }
                mod.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
                var custData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(model.Id, 1));
                var counterData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(model.Id, 2));
                var productData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(model.Id, 3));
                var jurisData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(model.Id, 4));
                var deliveryData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(model.Id, 5));
                mod.EWRAQualitativeModel.Add(custData[0]);
                mod.EWRAQualitativeModel.Add(counterData[0]);
                mod.EWRAQualitativeModel.Add(productData[0]);
                mod.EWRAQualitativeModel.Add(jurisData[0]);
                mod.EWRAQualitativeModel.Add(deliveryData[0]);
                mod.EWRAQualitativeModel[0].RiskTypeId = 1;
                mod.EWRAQualitativeModel[1].RiskTypeId = 2;
                mod.EWRAQualitativeModel[2].RiskTypeId = 3;
                mod.EWRAQualitativeModel[3].RiskTypeId = 4;
                mod.EWRAQualitativeModel[4].RiskTypeId = 5;
                var masterData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
                var k = 0;
                foreach (var item in masterData)
                {
                    var l = 0;
                    foreach (var risk in item.EWRARiskModel)
                    {
                        var w = 0;
                        var y = 0;
                        var z = 0;
                        foreach (var inh in risk.InherentRiskModelData)
                        {
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].InherentRiskModelData[w].InherentRiskId = inh.InherentRiskId;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].InherentRiskModelData[w].InherentRiskDescription = inh.InherentRiskDescription;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].InherentRiskModelData[w].TypeId = inh.TypeId;
                            w++;
                        }
                        foreach (var key in risk.KeyControlModelData)
                        {

                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].KeyControlModelData[y].KeyControl = key.KeyControl;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].KeyControlModelData[y].TypeId = key.TypeId;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].KeyControlModelData[y].KeyId = key.KeyId;

                            y++;

                        }
                        foreach (var res in risk.ResidualRiskModelData)
                        {

                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].ResidualRiskModelData[z].ResidualRiskDescription = res.ResidualRiskDescription;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].ResidualRiskModelData[z].TypeId = res.TypeId;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].ResidualRiskModelData[z].ResidualRiskId = res.ResidualRiskId;
                            z++;

                        }
                        l++;
                    }
                    k++;
                }
                var x = _ewraService.GetPendingEWRA(3, 0, clientId);
                var str = x.Result.Split("¥");

                List<EWRAModel> emlist = new List<EWRAModel>();
                for (var i = 0; i < str.Length - 1; i++)
                {
                    var str1 = str[i].Split("Ø");
                    EWRAModel m = new EWRAModel();
                    m.QId = str1[0].ParseInt();
                    m.QuantitativeName = str1[1].ToString();
                    emlist.Add(m);
                }
                mod.QuantitativeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emlist, "QId", "QuantitativeName");
                mod.Id = Id;
                mod.SaveType = 1;

                return View(mod);
            }
            else
            {
                EWRAModel model1 = new EWRAModel();

                model1.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
                model1.EWRAQualitativeModel = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
                var x = _ewraService.GetPendingEWRA(3, 0, clientId);
                var str = x.Result.Split("¥");

                List<EWRAModel> emlist = new List<EWRAModel>();
                for (var i = 0; i < str.Length - 1; i++)
                {
                    var str1 = str[i].Split("Ø");
                    EWRAModel m = new EWRAModel();
                    m.QId = str1[0].ParseInt();
                    m.QuantitativeName = str1[1].ToString();
                    emlist.Add(m);
                }
                model1.QuantitativeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emlist, "QId", "QuantitativeName");
                model1.SaveType = 2;
                return View(model1);
            }
        }
        public ActionResult ViewEWRAQualitativeCustomerProfileReport(int id)
        {
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
            var x = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(id, 1));
            foreach (var item in x)
            {
                model.EWRAQualitativeModel.Add(item);
            }
            //model.EWRAQualitativeModel = x[0];
            return View(model);
        }
        public ActionResult ViewEWRAQualitativeCounterPartyReport(int id)
        {
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
            var x = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(id, 2));
            foreach (var item in x)
            {
                model.EWRAQualitativeModel.Add(item);
            }
            //model.EWRAQualitativeModel = x[0];
            return View(model);
        }
        public ActionResult ViewEWRAQualitativeProductsReport(int id)
        {
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
            var x = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(id, 3));
            foreach (var item in x)
            {
                model.EWRAQualitativeModel.Add(item);
            }
            //model.EWRAQualitativeModel = x[0];
            return View(model);
        }
        public ActionResult ViewEWRAQualitativeJurisdictionReport(int id)
        {
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
            var x = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(id, 4));
            foreach (var item in x)
            {
                model.EWRAQualitativeModel.Add(item);
            }
            //model.EWRAQualitativeModel = x[0];
            return View(model);
        }
        public ActionResult ViewEWRAQualitativeDeliveryReport(int id)
        {
            EWRAModel model = new EWRAModel();
            model.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
            var x = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(id, 5));
            foreach (var item in x)
            {
                model.EWRAQualitativeModel.Add(item);
            }
            //model.EWRAQualitativeModel = x[0];
            return View(model);
        }
        [HttpGet("/EWRA/{QId}")]
        public IActionResult GetQualitativeSavedData(int qId)
        {
            var clientId = _clientHandler.GetClientId();
            EWRAModel mod = new EWRAModel();
            var prevId = _ewraService.GetEWRAQualitativeId(qId);
            if (prevId.Result != "")
            {

                var qlId = prevId.Result.ParseInt();
                var xyz = _mapper.Map<List<EWRAModelDTO>>(_ewraService.GetConsolidatedEwra(0, 2, qlId));
                foreach (var item in xyz)
                {
                    mod.QId = item.QId;
                    mod.QCreatedBy = item.QCreatedBy;
                    mod.QCreatedOn = item.QCreatedOn;
                    mod.DateOfAssessment = DateTime.Now;
                    mod.QualitativeOverallRating = item.QualitativeOverallRating;
                    mod.QualitativeOverallScore = item.QualitativeOverallScore;
                    mod.QCustOverallRisk = item.QCustOverallRisk;
                    mod.QCustOverallScore = item.QCustOverallScore;
                    mod.QCounterOverallRisk = item.QCounterOverallRisk;
                    mod.QCounterOverallScore = item.QCounterOverallScore;
                    mod.QProductOverallRisk = item.QProductOverallRisk;
                    mod.QProductOverallScore = item.QProductOverallScore;
                    mod.QJurisdictionOverallRisk = item.QJurisdictionOverallRisk;
                    mod.QJurisdictionOverallScore = item.QJurisdictionOverallScore;
                    mod.QDeliveryOverallRisk = item.QDeliveryOverallRisk;
                    mod.QDeliveryOverallScore = item.QDeliveryOverallScore;
                }
                mod.EWRAQualitativeModel = new List<EWRAQualitativeModel>();
                var custData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(qlId, 1));
                var counterData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(qlId, 2));
                var productData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(qlId, 3));
                var jurisData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(qlId, 4));
                var deliveryData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetEwraQualitativeSavedData(qlId, 5));
                mod.EWRAQualitativeModel.Add(custData[0]);
                mod.EWRAQualitativeModel.Add(counterData[0]);
                mod.EWRAQualitativeModel.Add(productData[0]);
                mod.EWRAQualitativeModel.Add(jurisData[0]);
                mod.EWRAQualitativeModel.Add(deliveryData[0]);
                mod.EWRAQualitativeModel[0].RiskTypeId = 1;
                mod.EWRAQualitativeModel[1].RiskTypeId = 2;
                mod.EWRAQualitativeModel[2].RiskTypeId = 3;
                mod.EWRAQualitativeModel[3].RiskTypeId = 4;
                mod.EWRAQualitativeModel[4].RiskTypeId = 5;
                var masterData = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
                var k = 0;
                foreach (var item in masterData)
                {
                    var l = 0;
                    foreach (var risk in item.EWRARiskModel)
                    {
                        var w = 0;
                        var y = 0;
                        var z = 0;
                        foreach (var inh in risk.InherentRiskModelData)
                        {
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].InherentRiskModelData[w].InherentRiskId = inh.InherentRiskId;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].InherentRiskModelData[w].InherentRiskDescription = inh.InherentRiskDescription;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].InherentRiskModelData[w].TypeId = inh.TypeId;
                            w++;
                        }
                        foreach (var key in risk.KeyControlModelData)
                        {
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].KeyControlModelData[y].KeyControl = key.KeyControl;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].KeyControlModelData[y].TypeId = key.TypeId;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].KeyControlModelData[y].KeyId = key.KeyId;
                            y++;
                        }
                        foreach (var res in risk.ResidualRiskModelData)
                        {
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].ResidualRiskModelData[z].ResidualRiskDescription = res.ResidualRiskDescription;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].ResidualRiskModelData[z].TypeId = res.TypeId;
                            mod.EWRAQualitativeModel[k].EWRARiskModel[l].ResidualRiskModelData[z].ResidualRiskId = res.ResidualRiskId;
                            z++;
                        }
                        l++;
                    }
                    k++;
                }
                var x = _ewraService.GetPendingEWRA(3, 0, clientId);
                var str = x.Result.Split("¥");
                List<EWRAModel> emlist = new List<EWRAModel>();
                for (var i = 0; i < str.Length - 1; i++)
                {
                    var str1 = str[i].Split("Ø");
                    EWRAModel m = new EWRAModel();
                    m.QId = str1[0].ParseInt();
                    m.QuantitativeName = str1[1].ToString();
                    emlist.Add(m);
                }
                mod.QuantitativeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emlist, "QId", "QuantitativeName");
                mod.QuantitativeId = qId;
                mod.Id = qlId;
                mod.SaveType = 1;
                return View("CategorywiseAssessmentTab", mod);
            }
            else
            {

                EWRAModel model = new EWRAModel();
                model.EWRAQualitativeModel = _mapper.Map<List<EWRAQualitativeModel>>(_ewraService.GetAllRiskTypes(clientId));
                var x = _ewraService.GetPendingEWRA(3, 0, clientId);
                var str = x.Result.Split("¥");

                List<EWRAModel> emlist = new List<EWRAModel>();
                for (var i = 0; i < str.Length - 1; i++)
                {
                    var str1 = str[i].Split("Ø");
                    EWRAModel m = new EWRAModel();
                    m.QId = str1[0].ParseInt();
                    m.QuantitativeName = str1[1].ToString();
                    emlist.Add(m);
                }
                model.QuantitativeList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(emlist, "QId", "QuantitativeName");
                model.QuantitativeId = qId;
                model.SaveType = 0;
                return View("CategorywiseAssessmentTab", model);
            }

        }
    }
}
