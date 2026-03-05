using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
using AML.Core.Service.Country;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.Kyc;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.EtlBatch;
using AML.ViewModel.ViewModels.InternalWathcList;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using NToastNotify;
using OfficeOpenXml;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AML.Web.Controllers.ClientCase
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class ClientCaseController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private ICustomerScreeningService _customerScreeningService;
        IFileUploader _fileUploader;
        private ICustomerCaseService _customerCaseService;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private string baseURL = string.Empty;
        private string base6URL = string.Empty;
        private ICommonService _commonService;
        private string _c6Username;
        private RiskAPIController _riskAPIController;
        private string baseC6URL = string.Empty;
        private string culture = CultureInfo.CurrentCulture.Name;
        private IKycService _kycService;
        private ICountryService _countryService;
        public ClientCaseController(IMapper mapper,
            IToastNotification toastNotification, IHttpClientHandler clientHandler, IConfiguration configuration, IKycService kycService, ICountryService countryService,
            ICustomerCaseService customerCaseService, IViewRenderService viewRenderService, IExportDataService exportService, ICommonService commonService, ICustomerScreeningService CustomerScreeningService,
            IFileUploader fileUploader, RiskAPIController riskAPIController)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _fileUploader = fileUploader;
            _customerCaseService = customerCaseService;
            _customerScreeningService = CustomerScreeningService;
            _viewRenderService = viewRenderService;
            _exportService = exportService;
            _commonService = commonService;
            _riskAPIController = riskAPIController;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            var clientId = _clientHandler.GetClientId();
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            _c6Username = clientDetails?.C6Username;
            base6URL = clientDetails?.C6BaseUrl;
            baseC6URL = clientDetails?.C6BaseUrl;
            _kycService = kycService;
            _countryService = countryService;
            //base6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            //baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            //_c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("customer/case-excel-upload")]
		public IActionResult UploadExcel(string type)
		{
			//CaseModel model = new CaseModel();
			var clientId = _clientHandler.GetClientId();
            DocumentUploadModel _docUpload = new DocumentUploadModel();


            _docUpload.itemId = (int)ItemType.clientcase;
            _docUpload.branchId = _clientHandler.GetBranchId();
            _docUpload.typeId = type;
            _docUpload.CodesTables = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId));
			for (int i = 0; i < _docUpload.CodesTables.Count; i++)
			{
				var val = _docUpload.CodesTables[i].ccName;
				_docUpload.CodeNames.Add(val);
                _docUpload.IsChecked.Add(false);
            }

			return View(_docUpload);
		}

		[HttpGet("corporate/case-excel-upload")]
        public IActionResult UploadExcelCorporate(string type)
        {
            DocumentUploadModel _docUpload = new DocumentUploadModel();
			var clientId = _clientHandler.GetClientId();

			_docUpload.itemId = (int)ItemType.clientcase;
			_docUpload.branchId = _clientHandler.GetBranchId();
			_docUpload.typeId = type;
			_docUpload.CodesTables = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId));
			for (int i = 0; i < _docUpload.CodesTables.Count; i++)
			{
				var val = _docUpload.CodesTables[i].ccName;
				_docUpload.CodeNames.Add(val);
                _docUpload.IsChecked.Add(false);
            }
			return View(_docUpload);
        }
        [HttpPost("customer/case-excel-upload")]
        public async Task<IActionResult> UploadExcel(DocumentUploadModel _docUpload)
        {
            string CallC6Screening = string.Empty;

            CallC6Screening = _configuration["CallC6Screening"];
            List<string> selectedScreeningOptions = new List<string>();
            DocumentUploadModel _documentUploadModel = _mapper.Map <DocumentUploadModel>(_docUpload);
            _documentUploadModel.ClientId = _clientHandler.GetClientId();
            TokenRS token = await AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                _toastNotification.AddErrorToastMessage("User is not authorized for screening");
                ViewBag.message = "User is not authorized for screening";
                return View(_documentUploadModel);
            }
            else
            {
               
                var checkThreshold = _documentUploadModel.Threshold;
                _documentUploadModel.CodesTables = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(_documentUploadModel.ClientId));

                bool IsSanction = false;
                int count = 0;

                for (int i = 0; i < _documentUploadModel.CodesTables.Count; i++)
                {
                    var val = _documentUploadModel.CodesTables[i].ccName;
                    _documentUploadModel.CodeNames.Add(val);
                }
                for (int i = 0; i < _documentUploadModel.CodeNames.Count; i++)
                {
                    //if (_documentUploadModel.IsChecked[i] == true)
                    //{
                        count++;
                        selectedScreeningOptions.Add(_documentUploadModel.CodeNames[i]);
                        if (_documentUploadModel.CodeNames[i] == "Sanction")
                        {
                            IsSanction = true;
                        }
                    //}
                }

                var screeningoption= string.Join(",", selectedScreeningOptions);

                //CustomerExcelData _excelD = new CustomerExcelData();



                if (_documentUploadModel.fileUpload != null)
                {
                    var newFileName = "";
                    if (_documentUploadModel.typeId == "I")
                    {
                        newFileName = "Individual_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + _documentUploadModel.fileUpload.FileName;
                    }
                    else if (_documentUploadModel.typeId == "C")
                    {
                        newFileName = "Corporate_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + _documentUploadModel.fileUpload.FileName;
                    }
                    //_documentUploadModel.ClientId = _clientHandler.GetClientId();

                    DocumentsModel _documentsModel = _fileUploader.UploadFile(_documentUploadModel.ClientId, (ItemType)_documentUploadModel.itemId, _documentUploadModel.branchId, _documentUploadModel.fileUpload, newFileName);
                    
                    
                    _documentsModel.AddedBy = _clientHandler.GetUserId();
                    _documentsModel.ClientId = _clientHandler.GetClientId();



                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    List<CustomerExcelData> _excelData = _mapper.Map<List<CustomerExcelData>>(_customerCaseService.SaveCustomerCaseExcelData(_mapper.Map<DocumentsDTO>(_documentsModel), _documentUploadModel.typeId, _documentUploadModel.C6Threshold, _documentUploadModel.Threshold, screeningoption).Result);

                    if (_excelData.Count ==0 )
                    {
                        _toastNotification.AddErrorToastMessage("This file is empty");
                        return View(_documentUploadModel);
                    }

                    else if (_excelData.Count > 50)
                    {
                        _toastNotification.AddErrorToastMessage("The number of customer data per file should not exceed 50.");
                        return View(_documentUploadModel);
                    }
                    else
                    {
                        foreach (var excelDetails in _excelData)
                        {
                            if (excelDetails.Id == 0)
                                _toastNotification.AddErrorToastMessage(string.Format("CustomerId {0} exists ", excelDetails.CustomerID));
                            else
                            {
                                if (count == 1 && IsSanction == true && CallC6Screening == "N")
                                {
                                    string response = string.Empty;
                                    var searchType = "F";
                                    string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                                    {
                                        customerdob = excelDetails.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                                        customerfullname = excelDetails.LastName,
                                        customernationality = excelDetails.Nationality,
                                        searchtype = searchType
                                    }, ScreeningService.BACKLIST_SCREENING).Result;

                                    if (!string.IsNullOrEmpty(data))
                                    {
                                        List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                                        bool isOkToProceed = _commonService.CustomerScreeningCallOnlySanction(apiResultModel, excelDetails.Id, baseURL, base6URL, _documentUploadModel, "INDIVIDUAL", body, checkThreshold);
                                        if (!isOkToProceed)
                                        {
                                            excelDetails.ScreeningStatus = "Pending";
                                            //_toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", excelDetails.LastName, ". \n"));
                                            //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
                                        }
                                        else
                                        {
                                            excelDetails.ScreeningStatus = "Approved";
                                            //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved for ", excelDetails.LastName, ". \n"));
                                            //resultString = string.Concat(resultString, "Customer Approved for ", x.FirstName, " ", x.LastName, ". \n");
                                        }

                                        //response = AMLUtility.FormatJsonToPlainText(data);

                                        //dynamic mdob;
                                        //try
                                        //{
                                        //    mdob = Convert.ToDateTime(apiResultModel[0].matchdob);
                                        //}
                                        //catch
                                        //{
                                        //    mdob = DateTime.Now;
                                        //}
                                        ////Insert Logs
                                        //_customerScreeningService.InsertSanctionScreeningLogs(new DTO.DTO.Sanction.SanctionScreeningLogDTO
                                        //{
                                        //    CustomerName = string.Concat(excelDetails.Firstname, " ", excelDetails.MiddleName, " ", excelDetails.LastName),
                                        //    Nationality = excelDetails.Nationality,
                                        //    DOB = Convert.ToDateTime(excelDetails.DOB),
                                        //    SearchType = searchType,
                                        //    MatchName = apiResultModel[0].matchname,
                                        //    MatchScore = Convert.ToInt32(apiResultModel[0].matchscore),
                                        //    MatchUID = apiResultModel[0].matchuid,
                                        //    MatchCategory = apiResultModel[0].matchcategory,
                                        //    MatchType = apiResultModel[0].matchtype,
                                        //    MatchNationality = apiResultModel[0].nationality,
                                        //    MatchIDNum = Convert.ToInt32(apiResultModel[0].matchidnumber),
                                        //    MatchDOB = mdob,
                                        //    CreatedBy = _clientHandler.GetUserId(),
                                        //    ClientId = _clientHandler.GetClientId()
                                        //});
                                        // _toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", excelDetails.LastName, ". \n"));
                                    }
                                    else
                                    {
                                        //  _toastNotification.AddWarningToastMessage(string.Concat("Sanction - Customer blocked! ", excelDetails.LastName, ". \n"));
                                    }

                                    //return View(model);
                                }
                                else
                                {


                                    var x = await _commonService.CustomerScreeningCall(CallC6Screening, excelDetails.Id, baseURL, base6URL, _documentUploadModel, "INDIVIDUAL", body, _docUpload.Threshold);



                                    if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                                    {
                                        excelDetails.ScreeningStatus = "Pending";
                                        //_toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", x.LastName, ". \n"));
                                        //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
                                    }
                                    else
                                    {
                                        excelDetails.ScreeningStatus = "Approved";
                                        //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved for ", x.LastName, ". \n"));
                                        //resultString = string.Concat(resultString, "Customer Approved for ", x.FirstName, " ", x.LastName, ". \n");
                                    }
                                }
                                    CorporateKycDTO corpModel = new CorporateKycDTO();



                                    //To check if risk assessment is enabled for the client.
                                    var results = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(_documentUploadModel.ClientId));
                                    if (results != null)
                                    {
                                        var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(excelDetails), corpModel, "I", culture, _documentUploadModel.ClientId);
                                        if (str1.Result == null)
                                        {
                                            TempData["ShowDuplicateModal"] = true;
                                            TempData["UploadedExcelData"] = JsonConvert.SerializeObject(_excelData);

                                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                            return View("Create", "Case");
                                        }
                                        var spStr1 = str1.Result.Split('Ø');
                                        var proflovId = spStr1[0];
                                        var natlovId = spStr1[1];
                                        var reslovId = spStr1[5];
                                        //var IspeplovId = spStr1[13];
                                        var IndprodlovId = spStr1[13];
                                        var InddelilovId = spStr1[14];
                                        var IndmodeofpaymentlovId = spStr1[15];
                                        var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(excelDetails), corpModel, "I", culture, _documentUploadModel.ClientId);
                                        if (str.Result == null)
                                        {
                                            TempData["ShowDuplicateModal"] = true;
                                            TempData["UploadedExcelData"] = JsonConvert.SerializeObject(_excelData);

                                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                            return View("Create", "Case");
                                        }
                                        var spStr = str.Result.Split('Ø');
                                        var profId = spStr[0];
                                        var natId = spStr[1];
                                        var resId = spStr[5];
                                        //var IspepId = spStr[13];
                                        var IndprodId = spStr[13];
                                        var InddeliId = spStr[14];
                                        var Indmodeofpaymentid = spStr[15];
                                        RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                                        riskModel.CustomerId = excelDetails.CustomerID;
                                        riskModel.CustomerName = excelDetails.LastName;


                                        riskModel.ClientId = _clientHandler.GetClientId();
                                        riskModel.CreatedBy = _clientHandler.GetUserId();
                                        if (excelDetails.Nationality == "0" && excelDetails.Nationality == "")
                                        {
                                            riskModel.MainNationality = "";
                                        }
                                        else
                                        {
                                            riskModel.MainNationality = excelDetails.Nationality;
                                        }
                                        riskModel.RiskCategory = "I";

                                        var riskTypeList = new List<RiskTypeListModel>();

                                        //for profession start
                                        if (profId != "0")
                                        {
                                            var riskType4 = new RiskTypeListModel();
                                            riskType4.Id = Convert.ToString(proflovId);
                                            var riskItem4 = new RiskItemListModel();
                                            riskItem4.Id = profId.ToString();//Convert.ToString(1);
                                            var riskItemList4 = new List<RiskItemListModel>();
                                            riskItemList4.Add(riskItem4);
                                            riskType4.RiskItemList = riskItemList4;
                                            riskTypeList.Add(riskType4);
                                        }
                                        //for profession end
                                        //for residence start
                                        if (resId != "0")
                                        {
                                            var riskType5 = new RiskTypeListModel();
                                            riskType5.Id = Convert.ToString(reslovId);
                                            var riskItem5 = new RiskItemListModel();
                                            riskItem5.Id = resId.ToString();//Convert.ToString(1);
                                            var riskItemList5 = new List<RiskItemListModel>();
                                            riskItemList5.Add(riskItem5);
                                            riskType5.RiskItemList = riskItemList5;
                                            riskTypeList.Add(riskType5);
                                        }
                                        //for residence end
                                        //for nationality start
                                        if (natId != "0")
                                        {
                                            var riskType3 = new RiskTypeListModel();
                                            riskType3.Id = Convert.ToString(natlovId);
                                            var riskItem3 = new RiskItemListModel();
                                            riskItem3.Id = natId.ToString();
                                            var riskItemList3 = new List<RiskItemListModel>();
                                            riskItemList3.Add(riskItem3);
                                            riskType3.RiskItemList = riskItemList3;
                                            riskTypeList.Add(riskType3);
                                        }
                                        //for nationality end
                                        //for product start
                                        if (IndprodId != "0")
                                        {
                                            var riskType6 = new RiskTypeListModel();
                                            riskType6.Id = Convert.ToString(IndprodlovId);
                                            var riskItem6 = new RiskItemListModel();
                                            riskItem6.Id = IndprodId.ToString();//Convert.ToString(1);
                                            var riskItemList6 = new List<RiskItemListModel>();
                                            riskItemList6.Add(riskItem6);
                                            riskType6.RiskItemList = riskItemList6;
                                            riskTypeList.Add(riskType6);
                                        }
                                        //for product end
                                        //for delivery channel start
                                        if (InddeliId != "0")
                                        {
                                            var riskType7 = new RiskTypeListModel();
                                            riskType7.Id = Convert.ToString(InddelilovId);
                                            var riskItem7 = new RiskItemListModel();
                                            riskItem7.Id = InddeliId.ToString();//Convert.ToString(1);
                                            var riskItemList7 = new List<RiskItemListModel>();
                                            riskItemList7.Add(riskItem7);
                                            riskType7.RiskItemList = riskItemList7;
                                            riskTypeList.Add(riskType7);
                                        }
                                        //for delivery channel end
                                        //for pep start
                                        //if (IspepId != "0")
                                        //{
                                        //    var riskType1 = new RiskTypeListModel();
                                        //    riskType1.Id = Convert.ToString(IspeplovId);
                                        //    var riskItem1 = new RiskItemListModel();
                                        //    riskItem1.Id = IspepId.ToString();
                                        //    var riskItemList1 = new List<RiskItemListModel>();
                                        //    riskItemList1.Add(riskItem1);
                                        //    riskType1.RiskItemList = riskItemList1;
                                        //    riskTypeList.Add(riskType1);
                                        //}
                                        //for pep end
                                        if (Indmodeofpaymentid != "0")
                                        {
                                            var riskType8 = new RiskTypeListModel();
                                            riskType8.Id = Convert.ToString(IndmodeofpaymentlovId);
                                            var riskItem8 = new RiskItemListModel();
                                            riskItem8.Id = Indmodeofpaymentid.ToString();//Convert.ToString(1);
                                            var riskItemList8 = new List<RiskItemListModel>();
                                            riskItemList8.Add(riskItem8);
                                            riskType8.RiskItemList = riskItemList8;
                                            riskTypeList.Add(riskType8);
                                        }


                                        riskModel.RiskTypeList = riskTypeList;
                                        var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                                        var xyz = riskResult;

                                        Console.WriteLine($"Risk assessment result: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");

                                    }
                                
                                
                            }
                        }
                        TempData["ShowDuplicateModal"] = true;
                        TempData["UploadedExcelData"] = JsonConvert.SerializeObject(_excelData);

                        return RedirectToAction("Create", "Case");
                    }
                }

                ViewBag.message = "Unable to Upload File";
                return View(_documentUploadModel);
            }
        }
        [HttpPost("corporate/case-excel-upload")]
        public async Task<IActionResult> UploadExcelCorporate(DocumentUploadModel _docUpload)
        {
            string CallC6Screening = string.Empty;

            CallC6Screening = _configuration["CallC6Screening"];
            List<string> selectedScreeningOptions = new List<string>();
            TokenRS token = await AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);

            DocumentUploadModel _documentUploadModel = _mapper.Map<DocumentUploadModel>(_docUpload);
            _documentUploadModel.ClientId = _clientHandler.GetClientId();


            if (token.status == 400)
            {
                _toastNotification.AddErrorToastMessage("User is not authorized for screening");
                ViewBag.message = "User is not authorized for screening";
                return View(_documentUploadModel);
            }
            else
            {
                var checkThreshold = _documentUploadModel.Threshold;
                _documentUploadModel.CodesTables = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(_documentUploadModel.ClientId));

                bool IsSanction = false;
                int count = 0;

                for (int i = 0; i < _documentUploadModel.CodesTables.Count; i++)
                {
                    var val = _documentUploadModel.CodesTables[i].ccName;
                    _documentUploadModel.CodeNames.Add(val);
                }
                for (int i = 0; i < _documentUploadModel.CodeNames.Count; i++)
                {
                    //if (_documentUploadModel.IsChecked[i] == true)
                    //{
                    count++;
                    selectedScreeningOptions.Add(_documentUploadModel.CodeNames[i]);
                    if (_documentUploadModel.CodeNames[i] == "Sanction")
                    {
                        IsSanction = true;
                    }
                    //}
                }

                var screeningoption = string.Join(",", selectedScreeningOptions);

                if (_documentUploadModel.fileUpload != null)
                {
                    var newFileName = "";

                    newFileName = "Corporate_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + _documentUploadModel.fileUpload.FileName;


                    DocumentsModel _documentsModel = _fileUploader.UploadFile(_documentUploadModel.ClientId, (ItemType)_documentUploadModel.itemId, _documentUploadModel.branchId, _documentUploadModel.fileUpload, newFileName);
                    _documentsModel.AddedBy = _clientHandler.GetUserId();
                    _documentsModel.ClientId = _clientHandler.GetClientId();

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    List<CorporateExcelData> _excelData = _mapper.Map<List<CorporateExcelData>>(_customerCaseService.SaveCorporateCaseExcelData(_mapper.Map<DocumentsDTO>(_documentsModel), _documentUploadModel.typeId, _documentUploadModel.C6Threshold, _documentUploadModel.Threshold, _documentUploadModel.CompanyCode,screeningoption).Result);

                    if (_excelData.Count == 0)
                    {
                        _toastNotification.AddErrorToastMessage("This file is empty");
                        return View(_documentUploadModel);
                    }
                    else if (_excelData.Count > 50)
                    {
                        _toastNotification.AddErrorToastMessage("The number of customer data per file should not exceed 50.");
                        return View(_documentUploadModel);
                    }
                    else
                    {
                        foreach (var excelDetails in _excelData)
                        {
                            if (excelDetails.Id == 0)
                                _toastNotification.AddErrorToastMessage(string.Format("CustomerId {0} exists ", excelDetails.CustomerID));
                            else
                            {
                                if (count == 1 && IsSanction == true && CallC6Screening == "N")
                                {
                                    string response = string.Empty;
                                    var searchType = "F";
                                    string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                                    {
                                        customerdob = excelDetails.DateofIncorporation.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                                        customerfullname = string.Concat(excelDetails.EntityName),
                                        customernationality = excelDetails.CountryofIncorporation,
                                        searchtype = searchType
                                    }, ScreeningService.BACKLIST_SCREENING).Result;

                                    if (!string.IsNullOrEmpty(data))
                                    {

                                        // _toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", excelDetails.EntityName, ". \n"));
                                        List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                                        bool isOkToProceed = _commonService.CustomerScreeningCallOnlySanction(apiResultModel, excelDetails.Id, baseURL, base6URL, _documentUploadModel, "CORPORATE", body, checkThreshold);
                                        if (!isOkToProceed)
                                        {
                                            _toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", excelDetails.EntityName, ". \n"));
                                            //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
                                        }
                                        else
                                        {
                                            _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved for ", excelDetails.EntityName, ". \n"));
                                            //resultString = string.Concat(resultString, "Customer Approved for ", x.FirstName, " ", x.LastName, ". \n");
                                        }
                                    }
                                    else
                                    {
                                        // _toastNotification.AddWarningToastMessage(string.Concat("Sanction - Customer blocked! ", excelDetails.EntityName, ". \n"));
                                    }

                                    //return View(model);
                                }
                                else
                                {



                                    //var x = await _commonService.CustomerScreeningCall(excelDetails.Id, baseURL, base6URL, "CORPORATE", body);
                                    var x = await _commonService.CustomerScreeningCall(CallC6Screening, excelDetails.Id, baseURL, base6URL, _documentUploadModel, "CORPORATE", body, _documentUploadModel.Threshold);

                                    if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                                    {
                                        excelDetails.ScreeningStatus = "Pending";
                                        //_toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", x.LastName, ". \n"));
                                        //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
                                    }
                                    else
                                    {
                                        excelDetails.ScreeningStatus = "Approved";
                                        //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved for ", x.LastName, ". \n"));
                                        //resultString = string.Concat(resultString, "Customer Approved for ", x.FirstName, " ", x.LastName, ". \n");
                                    }
                                }
                                    CorporateKycDTO corpModel = new CorporateKycDTO();

                                    //To check if risk assessment is enabled for the client.
                                    var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(_documentUploadModel.ClientId));

                                    if (result != null)
                                    {
                                        Console.WriteLine("Generate risk");


                                        KycIndividualDTO imodel = new KycIndividualDTO();
                                        //model.IsPeP = isPep;
                                        var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(excelDetails), "C", culture, _documentUploadModel.ClientId);
                                        if (str1.Result == null)
                                        {
                                            TempData["ShowDuplicateModal"] = true;
                                            TempData["UploadedExcelData"] = JsonConvert.SerializeObject(_excelData);

                                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                            return RedirectToAction("Create", "Case");
                                        }
                                        var spStr1 = str1.Result.Split('Ø');
                                        var entlovId = spStr1[2];
                                        var buslovId = spStr1[4];
                                        var incorplovId = spStr1[3];
                                        var productlovId = spStr1[11];
                                        var deliverylovId = spStr1[12];
                                        var nationality1lovId = spStr1[6];
                                        var nationality2lovId = spStr1[7];
                                        var nationality3lovId = spStr1[8];
                                        var nationality4lovId = spStr1[9];
                                        var nationality5lovId = spStr1[10];
                                        var modeofpaymentlovId = spStr1[16];
                                        // var IsPeplovId = spStr1[14];

                                        var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(excelDetails), "C", culture, _documentUploadModel.ClientId);

                                        if (str.Result == null)
                                        {
                                            TempData["ShowDuplicateModal"] = true;
                                            TempData["UploadedExcelData"] = JsonConvert.SerializeObject(_excelData);

                                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                            return RedirectToAction("Create", "Case");
                                        }
                                        var spStr = str.Result.Split('Ø');
                                        var entId = spStr[2];
                                        var busId = spStr[4];
                                        var incorpId = spStr[3];
                                        var productId = spStr[11];  
                                        var deliveryId = spStr[12];
                                        var nationality1Id = spStr[6];
                                        var nationality2Id = spStr[7];
                                        var nationality3Id = spStr[8];
                                        var nationality4Id = spStr[9];
                                        var nationality5Id = spStr[10];
                                        var modeofpaymentId = spStr[16];
                                        //var IsPepId = spStr[14];
                                        Console.WriteLine(
                                            $"entId: {entId}\n" +
                                            $"busId: {busId}\n" +
                                            $"incorpId: {incorpId}\n" +
                                            $"productId: {productId}\n" +
                                            $"deliveryId: {deliveryId}\n" +
                                            $"nationality1Id: {nationality1Id}\n" +
                                            $"nationality2Id: {nationality2Id}\n" +
                                            $"nationality3Id: {nationality3Id}\n" +
                                            $"nationality4Id: {nationality4Id}\n" +
                                            $"nationality5Id: {nationality5Id}\n"
                                        );

                                        RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                                        riskModel.CustomerId = excelDetails.CustomerID;
                                        riskModel.CustomerName = excelDetails.EntityName;
                                        riskModel.MainNationality = excelDetails.CountryofIncorporation;
                                        riskModel.ClientId = _clientHandler.GetClientId();
                                        riskModel.CreatedBy = _clientHandler.GetUserId();

                                        if (excelDetails.CountryofIncorporation == "0" || excelDetails.CountryofIncorporation == "")
                                        {
                                            riskModel.MainNationality = "";
                                        }
                                        else
                                        {
                                            riskModel.MainNationality = excelDetails.CountryofIncorporation;
                                        }
                                        riskModel.RiskCategory = "C";


                                        var riskTypeList = new List<RiskTypeListModel>();
                                        if (entId != "0" || busId != "0" || incorpId != "0" || nationality1Id != "0" || nationality2Id != "0" || nationality3Id != "0" || nationality4Id != "0" || nationality5Id != "0" || productId != "0" || deliveryId != "0" || modeofpaymentId != "0")
                                        {
                                            //for legal status of entity start
                                            if (entId != "0")
                                            {
                                                var riskType3 = new RiskTypeListModel();
                                                riskType3.Id = Convert.ToString(entlovId);
                                                var riskItem3 = new RiskItemListModel();
                                                riskItem3.Id = entId.ToString();
                                                var riskItemList3 = new List<RiskItemListModel>();
                                                riskItemList3.Add(riskItem3);
                                                riskType3.RiskItemList = riskItemList3;
                                                riskTypeList.Add(riskType3);
                                            }
                                            //for legal status of entity end
                                            //for nature of business start
                                            if (busId != "0")
                                            {
                                                var riskType1 = new RiskTypeListModel();
                                                riskType1.Id = Convert.ToString(buslovId);
                                                var riskItem1 = new RiskItemListModel();
                                                riskItem1.Id = busId.ToString();
                                                var riskItemList1 = new List<RiskItemListModel>();
                                                riskItemList1.Add(riskItem1);
                                                riskType1.RiskItemList = riskItemList1;
                                                riskTypeList.Add(riskType1);
                                            }
                                            //for nature of business end
                                            //for country of incorporation start
                                            if (incorpId != "0")
                                            {
                                                var riskType4 = new RiskTypeListModel();
                                                riskType4.Id = Convert.ToString(incorplovId);
                                                var riskItem4 = new RiskItemListModel();
                                                riskItem4.Id = incorpId.ToString();//Convert.ToString(1);
                                                var riskItemList4 = new List<RiskItemListModel>();
                                                riskItemList4.Add(riskItem4);
                                                riskType4.RiskItemList = riskItemList4;
                                                riskTypeList.Add(riskType4);
                                            }
                                            //for country of incorporation end
                                            //nationality partner 1 start
                                            if (nationality1Id != "0")
                                            {
                                                var riskType5 = new RiskTypeListModel();
                                                riskType5.Id = Convert.ToString(nationality1lovId);
                                                var riskItem5 = new RiskItemListModel();
                                                riskItem5.Id = nationality1Id.ToString();//Convert.ToString(1);
                                                var riskItemList5 = new List<RiskItemListModel>();
                                                riskItemList5.Add(riskItem5);
                                                riskType5.RiskItemList = riskItemList5;
                                                riskTypeList.Add(riskType5);
                                            }
                                            //nationality partner 1 end
                                            //nationality partner 2 start
                                            if (nationality2Id != "0")
                                            {
                                                var riskType6 = new RiskTypeListModel();
                                                riskType6.Id = Convert.ToString(nationality2lovId);
                                                var riskItem6 = new RiskItemListModel();
                                                riskItem6.Id = nationality2Id.ToString();//Convert.ToString(1);
                                                var riskItemList6 = new List<RiskItemListModel>();
                                                riskItemList6.Add(riskItem6);
                                                riskType6.RiskItemList = riskItemList6;
                                                riskTypeList.Add(riskType6);
                                            }
                                            //nationality partner 2 end
                                            //nationality partner 3 start
                                            if (nationality3Id != "0")
                                            {
                                                var riskType7 = new RiskTypeListModel();
                                                riskType7.Id = Convert.ToString(nationality3lovId);
                                                var riskItem7 = new RiskItemListModel();
                                                riskItem7.Id = nationality3Id.ToString();//Convert.ToString(1);
                                                var riskItemList7 = new List<RiskItemListModel>();
                                                riskItemList7.Add(riskItem7);
                                                riskType7.RiskItemList = riskItemList7;
                                                riskTypeList.Add(riskType7);
                                            }
                                            //nationality partner 3 end
                                            //nationality partner 4 start
                                            if (nationality4Id != "0")
                                            {
                                                var riskType8 = new RiskTypeListModel();
                                                riskType8.Id = Convert.ToString(nationality4lovId);
                                                var riskItem8 = new RiskItemListModel();
                                                riskItem8.Id = nationality4Id.ToString();//Convert.ToString(1);
                                                var riskItemList8 = new List<RiskItemListModel>();
                                                riskItemList8.Add(riskItem8);
                                                riskType8.RiskItemList = riskItemList8;
                                                riskTypeList.Add(riskType8);
                                            }
                                            //nationality partner 4 end
                                            //nationality partner 5 start
                                            if (nationality5Id != "0")
                                            {
                                                var riskType9 = new RiskTypeListModel();
                                                riskType9.Id = Convert.ToString(nationality5lovId);
                                                var riskItem9 = new RiskItemListModel();
                                                riskItem9.Id = nationality5Id.ToString();//Convert.ToString(1);
                                                var riskItemList9 = new List<RiskItemListModel>();
                                                riskItemList9.Add(riskItem9);
                                                riskType9.RiskItemList = riskItemList9;
                                                riskTypeList.Add(riskType9);
                                            }
                                            //nationality partner 5 end
                                            //product start
                                            if (productId != "0")
                                            {
                                                var riskType10 = new RiskTypeListModel();
                                                riskType10.Id = Convert.ToString(productlovId);
                                                var riskItem10 = new RiskItemListModel();
                                                riskItem10.Id = productId.ToString();//Convert.ToString(1);
                                                var riskItemList10 = new List<RiskItemListModel>();
                                                riskItemList10.Add(riskItem10);
                                                riskType10.RiskItemList = riskItemList10;
                                                riskTypeList.Add(riskType10);
                                            }
                                            //product end
                                            //delivery start
                                            if (deliveryId != "0")
                                            {
                                                var riskType11 = new RiskTypeListModel();
                                                riskType11.Id = Convert.ToString(deliverylovId);
                                                var riskItem11 = new RiskItemListModel();
                                                riskItem11.Id = deliveryId.ToString();//Convert.ToString(1);
                                                var riskItemList11 = new List<RiskItemListModel>();
                                                riskItemList11.Add(riskItem11);
                                                riskType11.RiskItemList = riskItemList11;
                                                riskTypeList.Add(riskType11);
                                            }
                                            //delivery end
                                            ////for pep start
                                            //if (IsPepId != "0")
                                            //{
                                            //    var riskType2 = new RiskTypeListModel();
                                            //    riskType2.Id = Convert.ToString(IsPeplovId);
                                            //    var riskItem2 = new RiskItemListModel();
                                            //    riskItem2.Id = IsPepId.ToString();
                                            //    var riskItemList2 = new List<RiskItemListModel>();
                                            //    riskItemList2.Add(riskItem2);
                                            //    riskType2.RiskItemList = riskItemList2;
                                            //    riskTypeList.Add(riskType2);
                                            //}
                                            ////for pep end
                                            ///for mode of payment start
                                            if (modeofpaymentId != "0")
                                            {
                                                var riskType12 = new RiskTypeListModel();
                                                riskType12.Id = Convert.ToString(modeofpaymentlovId);
                                                var riskItem12 = new RiskItemListModel();
                                                riskItem12.Id = modeofpaymentId.ToString();//Convert.ToString(1);
                                                var riskItemList12 = new List<RiskItemListModel>();
                                                riskItemList12.Add(riskItem12);
                                                riskType12.RiskItemList = riskItemList12;
                                                riskTypeList.Add(riskType12);
                                            }
                                            //for mode of payment end

                                            riskModel.RiskTypeList = riskTypeList;
                                            var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                                            var xyz = riskResult;
                                            Console.WriteLine($"generated risk for customer: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");
                                            Console.WriteLine($"Finished generating risk for customer: {JsonConvert.SerializeObject(riskResult, Formatting.Indented)}");
                                        }
                                    }


                                
                            }
                            TempData["ShowDuplicateModal"] = true;
                            TempData["UploadedExcelData"] = JsonConvert.SerializeObject(_excelData);

                            return RedirectToAction("CorporateScreening", "Corporate");
                        }
                    }

                    ViewBag.message = "Unable to Upload File";
                    return View(_documentUploadModel);
                }
            }
            return View(_documentUploadModel);
        }



        //[HttpGet("customer/case-excel-review")]
        //public IActionResult ViewExcelContent()
        //{
        //    DocumentsModel _documentsModel = _clientHandler.GetSessionObject<DocumentsModel>("ExcelData");
        //    ExcelReader _excelReader = new ExcelReader();
        //    List<CustomerExcelData> _excelData = _excelReader.LoadExcelData(_documentsModel.DocFullPath, 1);
        //    return View(_excelData);
        //}
        [HttpGet("etl/reports/dataload")]
        public IActionResult DataLoadReport()
        {
            ETLReport model = new ETLReport();
            var items = from MatchingType d in Enum.GetValues(typeof(MatchingType))
                        select new { Id = (int)d, Name = d.ToString() };
            model.FromDate = DateTime.Now;
            model.ToDate = DateTime.Now;
            model.MatchingTypeList = new SelectList(items, "Id", "Name");
            model.ClientId = _clientHandler.GetClientId();
            ETLDataLoadReportDTO dto = _mapper.Map<ETLDataLoadReportDTO>(model);
            model.DataList = _mapper.Map<List<EtlBatchModel>>(_customerCaseService.DataLoadReport(dto).Result);
            return View("DataLoad", model); 
        }
        [HttpPost("etl/reports/dataload")]
        public IActionResult DataLoadReport(ETLReport model)
        {
            var items = from MatchingType d in Enum.GetValues(typeof(MatchingType))
                        select new { Id = (int)d, Name = d.ToString() };
            model.MatchingTypeList = new SelectList(items, "Id", "Name");
            model.ClientId = _clientHandler.GetClientId();
            ETLDataLoadReportDTO dto = _mapper.Map<ETLDataLoadReportDTO>(model);
            model.DataList = _mapper.Map<List<EtlBatchModel>>(_customerCaseService.DataLoadReport(dto).Result);
            return View("DataLoad", model);
        }
        public IActionResult DataLoadDownload(ETLReportDownloadModel model)
        {
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> GeneratePdf(ETLReport model, int BatchID)
        {
            int clientId = _clientHandler.GetClientId();
            List<CaseModel> response = _mapper.Map<List<CaseModel>>(_customerCaseService.DataLoadReportByBatch(BatchID, clientId).Result);
            ETLReportDownloadModel downloadModel = new ETLReportDownloadModel();
            downloadModel.Data = response;
            downloadModel.TotalRows = response.Count;
            string body = string.Empty;
            var clientData = _customerCaseService.GetClientDetailsByID(clientId);
            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;
            string customer = "";
            if (model.Customer != null)
            {
                customer = model.Customer;
            }
            else
            {
                customer = "NA";
            }
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                Document document = new Document(PageSize.A4, 15, 15, 15, 15);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                document.Add(new Paragraph("\n"));

                PdfPTable logo = new PdfPTable(2);
                logo.TotalWidth = 550f;
                float[] logowidth = new float[] { 3f, 0.5f };
                logo.SetWidths(logowidth);
                logo.LockedWidth = true;
                logo.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                logo.DefaultCell.VerticalAlignment = 1;
                logo.DefaultCell.HorizontalAlignment = 1;
                logo.SpacingBefore = 20f;
                logo.SpacingAfter = 30f;
                logo.DefaultCell.Border = 0;
                PdfPCell compname = new PdfPCell(new Phrase(companyName.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                compname.FixedHeight = 40f;
                compname.VerticalAlignment = 1;
                compname.HorizontalAlignment = 1;
                compname.Border = 0;
                logo.AddCell(compname);
                string url = logos;
                Image tif = Image.GetInstance(url);
                tif.ScalePercent(1f);
                tif.SpacingBefore = 20f;
                logo.AddCell(tif);
                document.Add(logo);


                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                PdfPCell hd = new PdfPCell(new Phrase("Report                :   Customer Listing"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                PdfPCell dateRange = new PdfPCell(new Phrase("Date Range        :   " + model.FromDate.ToString("dd-MM-yyyy") + "  to  " + model.ToDate.ToString("dd-MM-yyyy")));
                PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                PdfPCell caseStat = new PdfPCell(new Phrase("Match Type        :   " + model.MatchType));
                PdfPCell _caseStat = new PdfPCell(new Phrase("\n"));
                PdfPCell createdBy = new PdfPCell(new Phrase("Created by         :   " + companyName));
                PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));
                PdfPCell filters = new PdfPCell(new Phrase("Customer            :   " + model.Customer));
                PdfPCell _filters = new PdfPCell(new Phrase("\n"));
                hd.Border = 0;
                dateRange.Border = 0;
                createdBy.Border = 0;
                caseStat.Border = 0;
                _hd.Border = 0;
                _dateRange.Border = 0;
                _createdBy.Border = 0;
                _caseStat.Border = 0;
                header.AddCell(hd);
                header.AddCell(_hd);
                header.AddCell(dateRange);
                header.AddCell(_dateRange);
                header.AddCell(caseStat);
                header.AddCell(_caseStat);
                _filters.Border = 0;
                filters.Border = 0;
                header.AddCell(filters);
                header.AddCell(_filters);
                header.AddCell(createdBy);
                header.AddCell(_createdBy);
                
                document.Add(header);

                PdfPTable table = new PdfPTable(9);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 2f, 1.5f, 1f, 1f, 1f, 1f };
                table.SetWidths(widths);
                table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                table.SpacingAfter = 30f;
                PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell1.HorizontalAlignment = 1;
                cell1.VerticalAlignment = 1;
                cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell1.FixedHeight = 30f;
                table.AddCell(cell1);
                
                PdfPCell cell2 = new PdfPCell(new Phrase("CUSTOMER ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell2.HorizontalAlignment = 1;
                cell2.VerticalAlignment = 1;
                cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell2.FixedHeight = 30f;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("MATCHED", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("SOURCE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("UID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell8.FixedHeight = 30f;
                table.AddCell(cell8);
                PdfPCell cell9 = new PdfPCell(new Phrase("DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell9.HorizontalAlignment = 1;
                cell9.VerticalAlignment = 1;
                cell9.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell9.FixedHeight = 30f;
                table.AddCell(cell9);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerId, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].FirstName + " " + downloadModel.Data[i].MiddleName + " " + downloadModel.Data[i].LastName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].DOB, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

                    if (downloadModel.Data[i].IsMatched == 1)
                    {
                        table.AddCell(new Phrase("True", new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    }
                    else
                    {
                        table.AddCell(new Phrase("False", new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    }
                    table.AddCell(new Phrase(downloadModel.Data[i].Source, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].UID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

                }
                document.Add(table);




                document.Add(new Paragraph("\n"));
                iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                document.Add(new Chunk(line1));

                PdfPTable footer2 = new PdfPTable(1);
                footer2.TotalWidth = 550f;
                footer2.LockedWidth = true;
                footer2.DefaultCell.Border = 0;
                footer2.AddCell("Computer generated report; hence no signature is required. ");
                footer2.AddCell("Date of Extraction :   " + DateTime.Now.ToString());
                document.Add(footer2);

                PdfContentByte content = writer.DirectContent;
                Rectangle rectangle = new Rectangle(document.PageSize);
                rectangle.Left += document.LeftMargin;
                rectangle.Right -= document.RightMargin;
                rectangle.Top -= document.TopMargin;
                rectangle.Bottom += document.BottomMargin;
                content.SetColorStroke(GrayColor.BLACK);
                content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
                content.Stroke();


                document.Close();


                byte[] data = memoryStream.ToArray();


                var result = data.ToString();



                List<ETLReport> list = new List<ETLReport>();

                var file = _exportService.ExportDataWithHeader<ETLReport>(list, result, (int)OperationType.PDF, "ETL_Dataload_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else
                {
                    return View("DataLoad");
                }
            }
        }

        //[HttpGet]
        //public ActionResult DownloadCustomerUploadSample(string type)
        //{
        //    //string filePath = @"Files/Data-Customer Bulk upload Sample.xlsx";
        //    //string fileName = "Data-Customer Bulk upload Sample.xlsx";
        //    //string filePathCorp = @"Files/Data-Corporate Bulk upload Sample.xlsx";
        //    var clientId = _clientHandler.GetClientId();

        //    var Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
        //    var NationalitiesList = Nationalities.Select(x => x.Value).ToList();


        //    //if (type == "I")
        //    //{
        //    //    fileName = "Data-Customer_Bulk_upload_Sample_Individual.xlsx";
        //    //}
        //    //else if (type == "C")
        //    //{
        //    //    fileName = "Data-Customer_Bulk_upload_Sample_Corporate.xlsx";
        //    //}
        //    if (type == "I")
        //    {
        //        string filePath = Path.Combine(Directory.GetCurrentDirectory(),"Files","Data-Customer Bulk upload Sample.xlsx");

        //        string fileName = "Customer_Bulk_Upload_Sample.xlsx";

        //        // ? Fetch dropdown values from backend
        //        var productTypes = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, type, clientId)), "ProductName", "ProductName");

        //        var productTypeList = productTypes.Select(x => x.Value).ToList();
        //        var deliveryChannels = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, type, clientId)), "DeliveryChannelName", "DeliveryChannelName");
        //        var deliveryChannelsList = deliveryChannels.Select(x => x.Value).ToList();
        //        var residentStatuses = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_residence_status(culture, clientId, type)), "DeliveryChannelName", "DeliveryChannelName");
        //        var residentStatusesList = residentStatuses.Select(x => x.Value).ToList();
        //        var profession = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, type, clientId)), "DeliveryChannelName", "DeliveryChannelName");
        //        var professionList = profession.Select(x => x.Value).ToList();
        //        var modelofpayment = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, type, clientId)), "DeliveryChannelName", "DeliveryChannelName");
        //        var modeofpaymentList = modelofpayment.Select(x => x.Value).ToList();

        //        using (var package = new ExcelPackage(new FileInfo(filePath)))
        //        {
        //            var worksheet = package.Workbook.Worksheets[1];

        //            AddDropdown(package, worksheet, "C2:C51", NationalitiesList, "DropdownData", 1);
        //            // ? professional Type ? Column D
        //            AddDropdown(package, worksheet, "I2:I51", professionList, "DropdownData", 2);

        //            // ? resident Type ? Column E
        //            AddDropdown(package, worksheet, "J2:J51", residentStatusesList, "DropdownData", 3);
        //            // ? Product Type ? Column F
        //            AddDropdown(package, worksheet, "K2:K51", productTypeList, "DropdownData", 4);

        //            // ? Delivery Channel ? Column G
        //            AddDropdown(package, worksheet, "L2:L51", deliveryChannelsList, "DropdownData", 5);

        //            // ? Mode of Payment ? Column H
        //            AddDropdown(package, worksheet, "M2:M51", modeofpaymentList, "DropdownData", 6);




        //            var stream = new MemoryStream();
        //            package.SaveAs(stream);
        //            stream.Position = 0;

        //            return File(stream,
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                fileName);
        //        }
        //    }
        //    else
        //    {
        //        string filePath = Path.Combine(Directory.GetCurrentDirectory(), "Files", "Data-Corporate Bulk upload Sample.xlsx");


        //        string fileName = "Corporate Bulk upload Sample.xlsx";

        //        // ? Fetch dropdown values from backend
        //        var productTypes = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, type, clientId)), "ProductName", "ProductName");

        //        var productTypeList = productTypes.Select(x => x.Value).ToList();
        //        var deliveryChannels = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, type, clientId)), "DeliveryChannelName", "DeliveryChannelName");
        //        var deliveryChannelsList = deliveryChannels.Select(x => x.Value).ToList();
        //        var legalstatus = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, type, clientId)), "LegalStatus", "LegalStatus");
        //        var legalstatusList = legalstatus.Select(x => x.Value).ToList();
        //        var businesstype = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, type, clientId)), "BusinessName", "BusinessName");
        //        var businesstypeList = businesstype.Select(x => x.Value).ToList();
        //        var modelofpayment = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, type, clientId)), "DeliveryChannelName", "DeliveryChannelName");
        //        var modeofpaymentList = modelofpayment.Select(x => x.Value).ToList();
        //        using (var package = new ExcelPackage(new FileInfo(filePath)))
        //        {
        //            var worksheet = package.Workbook.Worksheets[1];

        //            AddDropdown(package, worksheet, "C2:C51", NationalitiesList, "DropdownData", 1);
        //            // ? professional Type ? Column D
        //            AddDropdown(package,worksheet, "E2:E51", legalstatusList, "DropdownData", 2);

        //            // ? resident Type ? Column E
        //            AddDropdown(package,worksheet, "F2:F51", businesstypeList, "DropdownData", 3);
        //            // ? Product Type ? Column F
        //            AddDropdown(package, worksheet, "G2:G51", productTypeList, "DropdownData", 4);

        //            // ? Delivery Channel ? Column G
        //            AddDropdown(package, worksheet, "H2:H51", deliveryChannelsList, "DropdownData", 5);

        //            // ? Delivery Channel ? Column H
        //            AddDropdown(package,worksheet, "I2:I51", modeofpaymentList, "DropdownData", 6);




        //            var stream = new MemoryStream();
        //            package.SaveAs(stream);
        //            stream.Position = 0;

        //            return File(stream,
        //                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //                fileName);
        //        }
        //    }
        //    //if (type == "I")
        //    //{

        //    //    fileName = "Data-Customer_Bulk_upload_Sample_Individual.xlsx";
        //    //    byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

        //    //    return File(fileBytes, "application/force-download", fileName);
        //    //}
        //    //else if (type == "C")
        //    //{
        //    //    fileName = "Data-Customer_Bulk_upload_Sample_Corporate.xlsx";
        //    //    byte[] fileBytes = System.IO.File.ReadAllBytes(filePathCorp);

        //    //    return File(fileBytes, "application/force-download", fileName);
        //    //}
        //    return null;
        //    //byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

        //    //return File(fileBytes, "application/force-download", fileName);
        //}

        [HttpGet]
        public ActionResult DownloadCustomerUploadSample(string type)
        {
            var clientId = _clientHandler.GetClientId();

            var Nationalities = new SelectList(
                _mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)),
                "Name",
                "Name");

            var NationalitiesList = Nationalities.Select(x => x.Value).ToList();

            string filePath;
            string fileName;

            if (type == "I")
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(),
                                        "Files",
                                        "Data-Customer Bulk upload Sample.xlsx");

                fileName = "Customer_Bulk_Upload_Sample.xlsx";
            }
            else
            {
                filePath = Path.Combine(Directory.GetCurrentDirectory(),
                                        "Files",
                                        "Data-Corporate Bulk upload Sample.xlsx");

                fileName = "Corporate_Bulk_Upload_Sample.xlsx";
            }

            if (!System.IO.File.Exists(filePath))
                return NotFound("Template file not found.");

            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.First();

                // ---------- COMMON DROPDOWNS ----------
                AddDropdown(package, worksheet, "C2:C51", NationalitiesList, "DropdownData", 1);

                var productTypes = new SelectList(
                    _mapper.Map<List<ProductType>>(
                        _kycService.GetAllProduct(culture, type, clientId)),
                    "ProductName",
                    "ProductName");

                var productTypeList = productTypes.Select(x => x.Value).ToList();

                var deliveryChannels = new SelectList(
                    _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                        _kycService.GetAllDeliveryChannel(culture, type, clientId)),
                    "DeliveryChannelName",
                    "DeliveryChannelName");

                var deliveryChannelsList = deliveryChannels.Select(x => x.Value).ToList();

                var modeOfPayment = new SelectList(
                    _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                        _kycService.get_all_mode_of_payment(culture,clientId,type)),
                    "DeliveryChannelName",
                    "DeliveryChannelName");

                var modeOfPaymentList = modeOfPayment.Select(x => x.Value).ToList();


                // ---------- INDIVIDUAL ----------
                if (type == "I")
                {
                    var residentStatuses = new SelectList(
                        _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                            _kycService.get_all_residence_status(culture, clientId, type)),
                        "DeliveryChannelName",
                        "DeliveryChannelName");

                    var residentStatusesList = residentStatuses.Select(x => x.Value).ToList();

                    var profession = new SelectList(
                        _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                            _kycService.GetProfessionalStatus(culture, type, clientId)),
                        "DeliveryChannelName",
                        "DeliveryChannelName");

                    var professionList = profession.Select(x => x.Value).ToList();

                    AddDropdown(package, worksheet, "I2:I51", professionList, "DropdownData", 2);
                    AddDropdown(package, worksheet, "J2:J51", residentStatusesList, "DropdownData", 3);
                    AddDropdown(package, worksheet, "K2:K51", productTypeList, "DropdownData", 4);
                    AddDropdown(package, worksheet, "L2:L51", deliveryChannelsList, "DropdownData", 5);
                    AddDropdown(package, worksheet, "M2:M51", modeOfPaymentList, "DropdownData", 6);
                }
                else
                {
                    // ---------- CORPORATE ----------
                    var legalStatus = new SelectList(
                        _mapper.Map<List<LegalStatusModel>>(
                            _kycService.GetLegalStatus(culture, type, clientId)),
                        "LegalStatus",
                        "LegalStatus");

                    var legalStatusList = legalStatus.Select(x => x.Value).ToList();

                    var businessType = new SelectList(
                        _mapper.Map<List<BusinessNature>>(
                            _kycService.GetBusinessType(culture, type, clientId)),
                        "BusinessName",
                        "BusinessName");

                    var businessTypeList = businessType.Select(x => x.Value).ToList();

                    AddDropdown(package, worksheet, "E2:E51", legalStatusList, "DropdownData", 2);
                    AddDropdown(package, worksheet, "F2:F51", businessTypeList, "DropdownData", 3);
                    AddDropdown(package, worksheet, "G2:G51", productTypeList, "DropdownData", 4);
                    AddDropdown(package, worksheet, "H2:H51", deliveryChannelsList, "DropdownData", 5);
                    AddDropdown(package, worksheet, "I2:I51", modeOfPaymentList, "DropdownData", 6);
                }

                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream,
                     "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                     fileName);
            }
        }


        private void AddDropdown(ExcelPackage package,ExcelWorksheet worksheet,string cellRange,List<string> values,string hiddenSheetName,int hiddenColumn)
        {
            // Create or get hidden sheet
            var hiddenSheet = package.Workbook.Worksheets[hiddenSheetName]
                              ?? package.Workbook.Worksheets.Add(hiddenSheetName);

            hiddenSheet.Hidden = eWorkSheetHidden.VeryHidden;

            // Add values into hidden sheet column
            for (int i = 0; i < values.Count; i++)
            {
                hiddenSheet.Cells[i + 1, hiddenColumn].Value = values[i];
            }

            // Create validation
            var validation = worksheet.DataValidations.AddListValidation(cellRange);

            string columnLetter = GetExcelColumnLetter(hiddenColumn);

            validation.Formula.ExcelFormula =
                $"{hiddenSheetName}!${columnLetter}$1:${columnLetter}${values.Count}";

            validation.ShowErrorMessage = true;
            validation.Error = "Please select value from dropdown";
        }
        private string GetExcelColumnLetter(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;

            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }

            return columnName;
        }
        [HttpGet]
        public JsonResult ViewCustomerData(int BatchID)
        {
            int clientId = _clientHandler.GetClientId();
            List<CaseModel> response = _mapper.Map<List<CaseModel>>(_customerCaseService.DataLoadReportByBatch(BatchID, clientId).Result);
            return Json(response);
        }


    }
}
