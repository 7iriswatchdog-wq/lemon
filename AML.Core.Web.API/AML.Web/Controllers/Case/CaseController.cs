using Amazon.Auth.AccessControlPolicy;
using Amazon.Runtime;
using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
using AML.Core.Repository;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.Service.CustomerScreening;
using AML.Core.Service.UserAccess;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.UserAccess;
using AML.Core.ServiceContract.UserGroup;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.ProliferationFinance;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.CaseAssignment;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.CaseDetail;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CaseProcess;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.CorporateDetail;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.IdentityType;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.ViewModel.ViewModels.Sanction;
using AML.ViewModel.ViewModels.User;
using AML.ViewModel.ViewModels.UserGroup;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.collection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using NLog;
using NToastNotify;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AML.Core.Service.Common.CommonService;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using static AML.DTO.DTO.FreeSource.CaseLogsMongoDTO;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using ApiResultModel = AML.DTO.DTO.CustomerCase.ApiResultModel;
using Document = iTextSharp.text.Document;
using Font = iTextSharp.text.Font;
using Formatting = Newtonsoft.Json.Formatting;
using PageSize = iTextSharp.text.PageSize;
using Paragraph = iTextSharp.text.Paragraph;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;


namespace AML.Web.Controllers.Case
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class CaseController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private ICustomerCaseService _customerCaseService;
        private ICustomerScreeningService _customerScreeningService;
        private ICustomerMasterService _customerMasterService;
        private ICountryService _countryService;
        private IIdentityTypeService _idTypeService;
        private ICustomerCategoryService _customerCategoryService;
        private IUserService _userService;
        private ICaseDocumentService _caseDocumentService;
        private ICaseAssignmentService _caseAssignmentService;
        private ICaseCommentService _caseCommentService;
        IFileUploader _fileUploader;
        private ICommonService _commonService;
        private string baseURL = string.Empty;
        private string pdfbaseURL = string.Empty;
        private string baseC6URL = string.Empty;
        private string _c6Username;
        private int checkThreshold = 0;
        private RiskAPIController _riskAPIController;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private IFreeSourceRepository _freeSourceRepository;
        private string culture = CultureInfo.CurrentCulture.Name;
        private IKycService _kycService;
        private ILovMasterService _lovMasterService;
        private IRiskService _riskService;
        private IUserGroupService _UserGroupService;
        IUserGroupRightService _userGroupRightService;
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public CaseController(IMapper mapper,
            IToastNotification toastNotification, ICountryService countryService, ICaseDocumentService caseDocumentService, ICustomerMasterService customerMasterService, IHttpClientFactory httpClientFactory,
            IHttpClientHandler clientHandler, ICustomerCategoryService customerCategoryService, ICaseCommentService caseCommentService, IUserGroupService UserGroupService, IUserGroupRightService userGroupRightService,
            IConfiguration configuration, IIdentityTypeService idTypeService, IUserService userService, ICaseAssignmentService caseAssignmentService, ICommonService commonService, IKycService kycService, ILovMasterService lovMasterService, IRiskService RiskService,
            ICustomerCaseService CustomerCaseService, ICustomerScreeningService CustomerScreeningService, IFileUploader fileUploader, IExportDataService exportService, IViewRenderService viewRenderService, RiskAPIController riskAPIController, IFreeSourceRepository freeSourceRepository)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _customerCaseService = CustomerCaseService;
            _customerScreeningService = CustomerScreeningService;
            _customerMasterService = customerMasterService;
            _countryService = countryService;
            _idTypeService = idTypeService;
            _customerCategoryService = customerCategoryService;
            _userService = userService;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
            _caseAssignmentService = caseAssignmentService;
            _fileUploader = fileUploader;
            _commonService = commonService;
            _riskAPIController = riskAPIController;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            pdfbaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("PDFbaseUrl").Value;
            _kycService = kycService;
            var clientId = _clientHandler.GetClientId();
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            _c6Username = clientDetails?.C6Username;
            checkThreshold = clientDetails?.Threshold ?? 0;
            baseC6URL = clientDetails?.C6BaseUrl;
            //baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            //_c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
            //checkThreshold = configuration.GetSection("C6BaseApiUrl").GetSection("Threshold").Value.ParseInt();
            _viewRenderService = viewRenderService;
            _exportService = exportService;
            _freeSourceRepository = freeSourceRepository;
            _riskService = RiskService;
            _lovMasterService = lovMasterService;
            _UserGroupService = UserGroupService;
            _userGroupRightService = userGroupRightService;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet("/case")]
        public ActionResult Index()
        {

            ReportLogSearchModel model = new ReportLogSearchModel();
            var clientId = _clientHandler.GetClientId();
            model.StartDate = System.DateTime.Now.AddYears(-1);
            //model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            //model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.CustomerCategories = new SelectList(
    _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
        .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
        .ToList(),
    "Code",
    "Name"
);

            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                        select new
                        {
                            Id = (int)d,
                            Name = Regex.Replace(d.ToString(), "(\\B[A-Z])", " $1")
                        };

            model.CaseStatusList = new SelectList(items, "Id", "Name");
            
           



            return View(model);
        }
        [HttpPost("/case/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model, string startDate, string endDate, string cust_type,string searchValue,int createdBy,string matchScore,int caseStatus,string caseStatusChange,string riskLevel)
        {
            
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var clientId = _clientHandler.GetClientId();
            
            
             var userId = _clientHandler.GetUserId();
            

            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            List<CaseModel> abc = new List<CaseModel>();
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if(cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            
                if(riskLevel == "1")
                {
                    riskLevel = "Low Risk";
                }else if (riskLevel == "2")
                {
                    riskLevel = "Medium Risk";
                }else if(riskLevel == "3")
                {
                    riskLevel = "High Risk";
                }
            
            if (caseStatusChange == "0")
            {
                caseStatusChange = null;
            }
            
            if (searchValue != "" && searchValue != null)
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _UserGroupModel.Name, clientId));

            }
            else
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore,createdBy,caseStatus,riskLevel, _UserGroupModel.Name,clientId));

            }
                int totalcount = abc.Count;
            //if (!string.IsNullOrEmpty(model.search.value))
            //{
            //    var words = model.search.value.Trim().Split(' ');
            //    foreach (var item in words)
            //    {
            //        abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
            //    || m.FirstName.ToLower().Contains(item.ToLower())
            //    || m.LastName.ToLower().Contains(item.ToLower())
            //    || m.MiddleName.ToLower().Contains(item.ToLower())
            //    || m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
            //    ).ToList();
            //    }
            //    //abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
            //    //|| m.FirstName.ToLower().Contains(model.search.value.ToLower())
            //    //|| m.LastName.ToLower().Contains(model.search.value.ToLower())
            //    //|| m.MiddleName.ToLower().Contains(model.search.value.ToLower()) 
            //    //|| m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
            //    //).ToList();
            //}
            int filteredcount = abc.Count;
            //var data = abc.Skip(model.start).Take(model.length).ToList();

            var sortColumn = model?.order?.Any() == true
    ? model.columns[model.order[0].column].data ?? "updatedOn"
    : "updatedOn";

            var sortDir = model?.order?.Any() == true
                ? model.order[0].dir ?? "desc"
                : "desc";

            var data = SortData(abc, sortColumn, sortDir)
                        .Skip(model.start)
                        .Take(model.length)
                        .ToList();

            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data,
            });
            return response;
        }

        public List<T> SortData<T>(List<T> input, string property, string dir)
        {
            var type = typeof(T);

            var prop = type.GetProperty(property,
                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (prop == null)
                return input;

            if (dir.ToLower() == "asc")
            {
                return input.OrderBy(x => prop.GetValue(x, null)).ToList();
            }
            else
            {
                return input.OrderByDescending(x => prop.GetValue(x, null)).ToList();
            }
        }


        public List<T> Sort<T>(List<T> input, string property, string dir)
        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty?.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty?.GetValue(p, null)).ToList();

        }


        [HttpGet("/case/create")]
        public ActionResult Create()
        {
            CaseModel model = new CaseModel();
            model.CreatedBy = _clientHandler.GetUserId();
            var CustomerType = "I";
            var clientId = _clientHandler.GetClientId();
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.TypeId = CustomerType;
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
            model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Customer Risk");
            model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Product Risk");
            model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Delivery Channel Risk");
            model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Mode of Payment");
            model.ProfessionalList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ResidentialStatusList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_residence_status(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, clientId)), "ProductName", "ProductName");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            //for (int i = 0; i < model.CodesTable.Count; i++)
            //{
            //    var val = model.CodesTable[i].ccName;
            //    model.CodeNames.Add(val);
            //    model.IsChecked.Add(false);
            //}
            model.ClientId = _clientHandler.GetClientId();
            model.itemId = (int)ItemType.clientcase;
            model.branchId = _clientHandler.GetBranchId();

            if (TempData.TryGetValue("UploadedExcelData", out var uploadedData) && uploadedData != null)
            {
                // Deserialize Excel data
                model.ExcelUploadedData = JsonConvert.DeserializeObject<List<CustomerExcelData>>(uploadedData.ToString());

                // Only show modal if there is actual data
                if (model.ExcelUploadedData.Any())
                {
                    model.ShowExcelUploadModal = true;
                }

                // No need to keep TempData unless you plan to use it again
            }
            model.IsCaseCreated = TempData["IsCaseCreated"] != null && (bool)TempData["IsCaseCreated"];
            model.CaseRefId = TempData["CaseRefId"]?.ToString();

            return View(model);
        }

        [HttpPost]
        public JsonResult AutoCompleteCustomer(string prefix)
        {
            var customers = _customerCaseService.GetCustomerMasterByCodePrefixAndCode(prefix, "I");

            return Json(customers);
        }

        private async void SendScreendedMailAsync(string body, CaseModel model, CustomerCaseDTO x)
        {
            try
            {
                log.Debug("Start generating email pdf");
                DateTime startTime, endTime;
                startTime = DateTime.Now;
                int fileType = 4;
                CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
                downloadModel.ApiResultsjson = x.ApiResultsjson;
                downloadModel.TotalRows = x.ApiResultsjson.Count;
                var res = await _viewRenderService.RenderToStringAsync("Report/CaseReportDetailDownload", downloadModel);
                List<CaseReportListModel> list = new List<CaseReportListModel>();

                var clientData = _customerCaseService.GetClientDetailsByID(model.ClientId);
                var companyName = clientData.ClientName;

                using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
                {

                    var appBaseUrl = MyHttpContext.AppBaseUrl;
                    body = body.Replace("{customerID}", x.CustomerId.ToString());
                    body = body.Replace("{caseID}", x.Id.ToString());
                    body = body.Replace("{baseUrl}", appBaseUrl);
                    model.Url = appBaseUrl;
                    model.createdUserName = HttpContext.Session.GetString("SessUsername");


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
                    //var companyName = HttpContext.Session.GetString("SessCompanyName");
                    PdfPCell compname = new PdfPCell(new Phrase(companyName, new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                    compname.FixedHeight = 40f;
                    compname.VerticalAlignment = 1;
                    compname.HorizontalAlignment = 1;
                    compname.Border = 0;
                    logo.AddCell(compname);
                    var logos = "wwwroot/img/" + clientData.DocumentFileName;

                    string url = logos;
                    Image tif = Image.GetInstance(url);
                    tif.ScalePercent(1f);
                    tif.SpacingBefore = 20f;
                    logo.AddCell(tif);
                    document.Add(logo);


                    PdfPTable header = new PdfPTable(1);
                    header.TotalWidth = 550f;
                    header.LockedWidth = true;
                    header.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                    header.SpacingAfter = 30f;
                    header.DefaultCell.Border = 0;
                    PdfPCell hd = new PdfPCell(new Phrase("Potential Hits"));
                    hd.HorizontalAlignment = 1;
                    hd.FixedHeight = 20f;
                    hd.VerticalAlignment = 1;
                    hd.Border = 0;
                    header.AddCell(hd);
                    document.Add(header);


                    PdfPTable headTab = new PdfPTable(1);
                    headTab.TotalWidth = 550f;
                    headTab.LockedWidth = true;
                    headTab.DefaultCell.Border = 0;
                    headTab.HorizontalAlignment = 1;
                    headTab.DefaultCell.FixedHeight = 30f;
                    float[] widths1 = new float[] { 3f };
                    headTab.SetWidths(widths1);
                    headTab.AddCell("First Name : " + x.FirstName);
                    headTab.AddCell("Middle Name : " + x.MiddleName);
                    headTab.AddCell("Last Name : " + x.LastName);
                    headTab.AddCell("Nationality : " + x.Nationality);
                    //var dateAndTime = x.DOB;
                    //var dob = dateAndTime.Date;
                    //var dateAndTime = x.DOB;
                    var dob = x.DOB.ToUIDDateFormat();

                    //headTab.AddCell("Date Of Birth : " + dob.Day + "/" + dob.Month + "/" + dob.Year);
                    headTab.AddCell("Date Of Birth : " + dob);
                    headTab.AddCell("ID Type : " + model.CustomerIdType);
                    headTab.AddCell("ID Number : " + model.CustomerIdNumber);
                    headTab.AddCell("Mobile : " + x.Mobile);
                    document.Add(headTab);

                    PdfPTable table = new PdfPTable(8);
                    table.TotalWidth = 550f;
                    table.LockedWidth = true;
                    float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 3f, 1f, 1f, 1f };
                    table.SetWidths(widths);
                    table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                    table.SpacingAfter = 30f;
                    PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell1.HorizontalAlignment = 1;
                    cell1.VerticalAlignment = 1;
                    cell1.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell1.FixedHeight = 30f;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell2.HorizontalAlignment = 1;
                    cell2.VerticalAlignment = 1;
                    cell2.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell2.FixedHeight = 30f;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell3.HorizontalAlignment = 1;
                    cell3.VerticalAlignment = 1;
                    cell3.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell3.FixedHeight = 30f;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell4.HorizontalAlignment = 1;
                    cell4.VerticalAlignment = 1;
                    cell4.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell4.FixedHeight = 30f;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell5.HorizontalAlignment = 1;
                    cell5.VerticalAlignment = 1;
                    cell5.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell5.FixedHeight = 30f;
                    table.AddCell(cell5);
                    PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell6.HorizontalAlignment = 1;
                    cell6.VerticalAlignment = 1;
                    cell6.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell6.FixedHeight = 30f;
                    table.AddCell(cell6);
                    PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell7.HorizontalAlignment = 1;
                    cell7.VerticalAlignment = 1;
                    cell7.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell7.FixedHeight = 30f;
                    table.AddCell(cell7);
                    PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell8.HorizontalAlignment = 1;
                    cell8.VerticalAlignment = 1;
                    cell8.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell8.FixedHeight = 30f;
                    table.AddCell(cell8);
                    for (int i = 0; i < downloadModel.ApiResultsjson.Count; i++)
                    {
                        if (Convert.ToInt32(downloadModel.ApiResultsjson[i].matchscore) >= checkThreshold)
                        {
                            table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchuid, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchtype, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchcategory, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchname, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchscore, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                            table.AddCell(new Phrase(downloadModel.ApiResultsjson[i].matchdob, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                        }
                    }
                    document.Add(table);

                    PdfPTable footTab = new PdfPTable(3);
                    footTab.TotalWidth = 550f;
                    footTab.LockedWidth = true;
                    footTab.DefaultCell.Border = 0;
                    float[] footWidth = new float[] { 20f, 40f, 40 };
                    footTab.SetWidths(footWidth);
                    footTab.HorizontalAlignment = 1;
                    footTab.SpacingAfter = 30f;
                    footTab.AddCell("Created By : ");
                    string username = HttpContext.Session.GetString("SessUsername");
                    footTab.AddCell(username);
                    footTab.AddCell("");
                    footTab.AddCell("Created On : ");
                    footTab.AddCell(DateTime.Now.ToString());
                    footTab.AddCell("");
                    footTab.AddCell("Url : ");
                    footTab.AddCell(appBaseUrl);
                    footTab.AddCell("");
                    document.Add(footTab);

                    PdfPTable footer1 = new PdfPTable(1);
                    footer1.TotalWidth = 550f;
                    footer1.LockedWidth = true;
                    footer1.DefaultCell.Border = 0;
                    footer1.AddCell("Please click on the VIEW report under CASE Tab for full details ");

                    document.Add(footer1);


                    document.Add(new Paragraph("\n"));
                    iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
                    document.Add(new Chunk(line1));

                    PdfPTable footer2 = new PdfPTable(1);
                    footer2.TotalWidth = 550f;
                    footer2.LockedWidth = true;
                    footer2.DefaultCell.Border = 0;
                    footer2.AddCell("Computer generated report; hence no signature is required. ");
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

                    endTime = DateTime.Now;
                    log.Debug($"Finished creating pdf in {((TimeSpan)(endTime - startTime)).TotalMilliseconds}");

                    if (!(x.MatchScore >= 0 && x.MatchScore < checkThreshold))
                    {
                        log.Debug("Sending email");
                        startTime = DateTime.Now;

                        await _commonService.SendHtmlFormattedEmailWithAttachment("Case creation Alert", body, res.ToString(), data);

                        endTime = DateTime.Now;
                        log.Debug($"Finished sending email in {((TimeSpan)(endTime - startTime)).TotalMilliseconds}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException.Message;
                error.module = "Sending_email";
                error.comments = "Sending email Error";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                //Console.Error.WriteLine(ex);


            }
        }

        [HttpPost("/case/create")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CaseModel model)
        {
            model.ClientId = _clientHandler.GetClientId();
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(model.ClientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.IsDelete = 0;
            var CustomerType = "I";
            model.CreatedBy = _clientHandler.GetUserId();
            model.CustomerId = "0";
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(model.ClientId)), "ccName", "ccName");
            model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Customer Risk");
            model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Product Risk");
            model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Delivery Channel Risk");
            model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Mode of Payment");
            model.ProfessionalList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, CustomerType, model.ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ResidentialStatusList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_residence_status(culture, model.ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, model.ClientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, model.ClientId)), "ProductName", "ProductName");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, model.ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            //model.CodesTable = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(model.ClientId));

            string CallC6Screening = string.Empty;

            CallC6Screening = _configuration["CallC6Screening"];
            checkThreshold = model.Threshold;
            try
            {
                TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                if (token.status == 400)
                {
                    _toastNotification.AddErrorToastMessage("User is not authorized for screening");
                }
                else
                {
                    //if (ModelState.IsValid)
                    //{
                        try
                        {
                        List<string> selectedScreeningOptions = new List<string>();
                        model.CustomerType = "I";
                        model.MatchCategory = "INDIVIDUAL";
                        model.Type = "Individual";
                        CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
                        bool IsSanction = false;
                                    int count = 0;
                                    for (int i = 0; i < model.CodeNames.Count; i++)
                                    {
                                        if (model.IsChecked[i] == true)
                                        {
                                            count++;
                                            selectedScreeningOptions.Add(model.CodeNames[i]);
                                            if (model.CodeNames[i] == "Sanction")
                                            {
                                                log.Debug("Only sanction was true");
                                                IsSanction = true;
                                            }
                                        }
                                    }

                                    for (int i = 0; i < model.CodeNames.Count; i++)
                                    {
                                        if (model.CodeNames[i] == "PEP" && model.IsChecked[i] == true) { _ccDTO.IsPep = true; continue; }
                                        if (model.CodeNames[i] == "Sanction" && model.IsChecked[i] == true) { _ccDTO.IsSan = true; continue; }
                                        if (model.CodeNames[i] == "Reputational Risk Exposure" && model.IsChecked[i] == true) { _ccDTO.IsRre = true; continue; }
                                        if (model.CodeNames[i] == "Insolvency (UK & Ireland)" && model.IsChecked[i] == true) { _ccDTO.IsIns = true; continue; }
                                        if (model.CodeNames[i] == "Disqualified Director (UK Only)" && model.IsChecked[i] == true) { _ccDTO.IsDd = true; continue; }
                                        if (model.CodeNames[i] == "Profile of Interest" && model.IsChecked[i] == true) { _ccDTO.IsPoi = true; continue; }
                                        if (model.CodeNames[i] == "Regulatory Enforcement List" && model.IsChecked[i] == true) { _ccDTO.IsRel = true; continue; }
                                    }

                        //var customerCodeprefix = _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(model.ClientId));
                        //_ccDTO.customerCodeprefix = customerCodeprefix.Prefix;
                        //var result = _customerCaseService.CreatePrefix(_ccDTO);
                        _ccDTO.ScreeningOptions = string.Join(", ", selectedScreeningOptions);
                        var result = _customerCaseService.Create(_ccDTO);
                            _ccDTO.CustomerId = result.Result.Split('Ø')[1];

                            Console.WriteLine(result.Result);





                            //Document upload 
                            CaseDocumentModel _caseDoc = new CaseDocumentModel();
                            _caseDoc.CaseId = _customerCaseService.GetCaseId(_ccDTO.CustomerId).ToString();
                            if (model.CaseDocumentsL != null)
                            {
                                foreach (var item in model.CaseDocumentsL)
                                {
                                    if (item.Document != null)
                                    {
                                        _caseDoc.CreatedBy = _clientHandler.GetUserId();
                                        _caseDoc.CreatedOn = DateTime.Now;
                                        _caseDoc.ClientId = _clientHandler.GetClientId();
                                        DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
                                        _caseDoc.DocumentFileName = item.Document.FileName;
                                        _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
                                        _caseDoc.IssuedDateOnDB = item.IssuedDate;
                                        _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
                                        _caseDoc.DocumentName = item.DocumentName;
                                        var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                                    }
                                }
                            }
                            
                            CaseCommentModel remarkModel = new CaseCommentModel();
                            remarkModel.CaseId = Convert.ToInt32(_caseDoc.CaseId);
                            remarkModel.Comment = "Case is Created"; // ✅ FIXED
                            remarkModel.CommentType = "Individual Screening";
                            remarkModel.CreatedBy = _clientHandler.GetUserId();

                            var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));

                            //foreach (var item in model.CodesTable)
                            //{
                            //    model.CodeNames.Add(item.Value);
                            //}
                            //for (int i = 0; i < model.CodesTable.Count; i++)
                            //{
                            //    var val = model.CodesTable[i].ccName;
                            //    model.CodeNames.Add(val);
                            //}
                            


                            if (count == 1 && IsSanction == true && CallC6Screening == "N")
                            {
                                log.Debug("Only Sanction Screening searching from mongo db(internal watchlist)");
                                string data = "";
                                try
                                {
                                    string response = string.Empty;
                                    var searchType = "F";
                                    data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                                    {
                                        customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                                        customerfullname = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
                                        customernationality = model.Nationality,
                                        searchtype = searchType
                                    }, ScreeningService.BACKLIST_SCREENING).Result;

                                    log.Debug(data);

                                }
                                catch (Exception ex)
                                {
                                    log.Debug(ex);
                                }


                                if (!string.IsNullOrEmpty(data))
                                {
                                    List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                                    //response = AMLUtility.FormatJsonToPlainText(data);
                                    log.Debug("$got apiresults:", apiResultModel.Count());

                                    log.Debug("After checking from interanl watch list");

                                    UpdateSanctionRecords(_ccDTO, apiResultModel, result.Result.Split('Ø')[1]);

                                    string body = string.Empty;
                                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                                    {
                                        body = reader.ReadToEnd();
                                    }
                                    ;

                                    //var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result);

                                    if (_ccDTO.sendMail == 1)
                                    {
                                        await Task.Run(() => SendScreendedMailAsync(body, model, _ccDTO));
                                    }

                                    if (_ccDTO.IsMatched == 1 && _ccDTO.MatchScore >= checkThreshold)
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        //_toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                                    }
                                    else if (_ccDTO.IsMatched == 0 && _ccDTO.MatchScore == 0)
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        model.IsMatched = 2;
                                        var appBaseUrl = MyHttpContext.AppBaseUrl;
                                        // body = body.Replace("{baseUrl}", appBaseUrl);
                                        //model.Url = appBaseUrl;
                                        //model.createdUserName = HttpContext.Session.GetString("SessUsername");
                                        //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));
                                        //return View(model);
                                    }
                                    else if (_ccDTO.MatchScore >= 0 && _ccDTO.MatchScore < checkThreshold)
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        _ccDTO.ApiResultsjson = _ccDTO.ApiResultsjson.Where((source, index) => index < 100).ToList();
                                        model.ApiResultJson = _ccDTO.ApiResultsjson;
                                        //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));

                                        //return View(model);
                                    }
                                    else
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        //_toastNotification.AddSuccessToastMessage("Customer Approved");
                                    }//      _toastNotification.AddWarningToastMessage("Customer blocked!");
                                }
                                else
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    //_toastNotification.AddWarningToastMessage("Customer blocked! ");
                                }

                                
                            }
                            else if (result.Status == StaticResource.SuccessStatusCode)
                            {
                                if (CallC6Screening == "Y")
                                {
                                    {
                                        _ccDTO.IsPep = true;
                                        _ccDTO.IsSan = true;
                                    }
                                }
                                string body = string.Empty;
                                using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                                {
                                    body = reader.ReadToEnd();
                                }
                                ;

                                var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result.Split('Ø')[1]);

                                if (x.sendMail == 1)
                                {
                                    await Task.Run(() => SendScreendedMailAsync(body, model, x));
                                }
                                if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                   // _toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                                }
                                else if (x.IsMatched == 0 && x.MatchScore == 0)
                                {

                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    model.IsMatched = 2;
                                    var appBaseUrl = MyHttpContext.AppBaseUrl;
                                    body = body.Replace("{baseUrl}", appBaseUrl);
                                    model.Url = appBaseUrl;
                                    model.createdUserName = HttpContext.Session.GetString("SessUsername");
                                    //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", x.FirstName, " ", x.LastName, ". \n"));
                                    
                                }
                                else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    x.ApiResultsjson = x.ApiResultsjson.Where((source, index) => index < 100).ToList();
                                    model.ApiResultJson = x.ApiResultsjson;
                                    //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", x.FirstName, " ", x.LastName, ". \n"));

                                    
                                }
                                else
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    //_toastNotification.AddSuccessToastMessage("Customer Approved");
                                }
                            }
                            else
                            {
                                _toastNotification.AddErrorToastMessage("Customer creation failed");
                                return View(model);
                            }
                            CorporateKycDTO corpModel = new CorporateKycDTO();
                           
                            //To check if risk assessment is enabled for the client.
                            //var results = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(model.ClientId));
                            //if (results != null)
                            //{
                                var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, model.ClientId);
                                if (str1.Result == null)
                                {
                                    _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                    return View(model);
                                }
                                var spStr1 = str1.Result.Split('Ø');
                                var proflovId = spStr1[0];
                                var natlovId = spStr1[1];
                                var reslovId = spStr1[5];
                                //var IspeplovId = spStr1[13];
                                var IndprodlovId = spStr1[13];
                                var InddelilovId = spStr1[14];
                                var IndmodeofpaymentlovId = spStr1[15];
                                var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, model.ClientId);
                                if (str.Result == null)
                                {
                                    _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                    return View(model);
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

                                riskModel.CustomerId = _ccDTO.CustomerId;
                                riskModel.CustomerName = model.FirstName;


                                riskModel.ClientId = _clientHandler.GetClientId();
                                riskModel.CreatedBy = _clientHandler.GetUserId();
                                if (model.Nationality == "0")
                                {
                                    riskModel.MainNationality = "";
                                }
                                else
                                {
                                    riskModel.MainNationality = model.Nationality;
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
                                return View(model);
                            //}
                            //return RedirectToAction("Create");
                        }
                        catch (Exception ex)
                        {
                            //var error = ex.Message;
                            log.Debug(ex.Message);
                            ErrorLogDTO error = new ErrorLogDTO();
                            error.created_on = DateTime.Now;
                            error.createdBy = _clientHandler.GetUserId();
                            error.description = ex.InnerException.Message;
                            error.module = "Screening_I";
                            error.comments = "API call while screening";
                            error.status_code = 404;

                            var result = _commonService.createErrorlog(error);

                            _toastNotification.AddErrorToastMessage(error.description);

                            Console.Error.WriteLine(ex);

                            return RedirectToAction("Create");


                        }
                        return RedirectToAction("Create");
                    }
                //}
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);

                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException.Message;
                error.module = "Screening_I";
                error.comments = "API call Error while creating token";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Create");
            }
            return View(model);
        }

        private void UpdateSanctionRecords(CustomerCaseDTO _CustomerCaseDTO, List<ApiResultModel> apiResp, string cid)
        {


            string id;
            if (_CustomerCaseDTO.Id == 0 && cid != "")
            {
                id = cid;
            }
            else
            {
                id = _CustomerCaseDTO.Id.ToString();
            }

            _CustomerCaseDTO.Id = _customerCaseService.GetCaseId(id);
            bool isRecordCreated = false;
            (bool exists, List<NAMELIST> response, List<NAMELIST> response2) IsBlackListed = _freeSourceRepository.SearchNameList(_CustomerCaseDTO.FirstName, false, _CustomerCaseDTO.ClientId);

            _CustomerCaseDTO.Status = 5;

            if (true) { _CustomerCaseDTO.IsMatched = ((apiResp.IsNotNullOrEmpty() && IsBlackListed.exists) || apiResp.Count > 0) ? 1 : 0; }
            if (_CustomerCaseDTO.IsWhiteListed == "YES") { _CustomerCaseDTO.Source = "WHITELIST"; }



            if (true)
            {

                _CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : Convert.ToInt32(apiResp.FirstOrDefault().matchscore);
                _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : Convert.ToString(apiResp.FirstOrDefault().matchuid);
                _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
                _CustomerCaseDTO.Source = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : apiResp.FirstOrDefault().matchtype;

                _CustomerCaseDTO.ApiResultsjson = apiResp;

                var matchrecordsList = new List<MatchRecordsDTO>();
                var stat = 0;
                if (!IsBlackListed.exists)
                {
                    log.Debug($"Get C6 results below threshold ({checkThreshold}) for CaseLogUnderThreshold");

                    foreach (var item in apiResp)
                    {

                        var matchrecords = new MatchRecordsDTO();
                        if (item.IsNotNullOrEmpty())
                        {
                            matchrecords.MATCHUID = item.matchuid.ToString();
                            matchrecords.MATCHTYPE = item.matchtype;// "KYC6";
                            matchrecords.MATCHCATEGORY = item.matchcategory;
                            matchrecords.MATCHNAME = item.matchname.ToUpper();
                            matchrecords.MATCHSCORE = Convert.ToInt32(item.matchscore);
                            //matchrecords.MATCHNATIONALITY = item.nationality.IsNotNullOrEmpty() ? item.nationality.FirstOrDefault().ToString() : string.Empty;
                            matchrecords.MATCHNATIONALITY = item.nationality.IsNotNullOrEmpty() ? item.nationality.ToString() : string.Empty;
                            matchrecords.MATCHIDNO = item.matchidnumber != null ? item.matchidnumber.ToString() : String.Empty;
                            //matchrecords.MATCHDOB = item.matchdob != null ? item.matchdob.FirstOrDefault().ToString() : "";
                            matchrecords.MATCHDOB = item.matchdob != null ? item.matchdob.ToString() : "";

                            if (Convert.ToInt32(item.matchscore) >= checkThreshold)
                            {
                                matchrecordsList.Add(matchrecords);
                                stat++;
                            }

                            //if (Convert.ToInt32(item.matchscore) < checkThreshold)
                            //{
                            //    matchrecordsList.Add(matchrecords);
                            //    stat = 1;
                            //}
                            //else
                            //{
                            //    stat = 0;
                            //}
                        }
                    }

                    log.Debug($"Got {matchrecordsList.Count} results");
                }
                else
                {
                    stat = 0;
                }

                if (stat >= 1)
                {
                    CASELOG modelCaseLog = new CASELOG
                    {
                        CASEID = _CustomerCaseDTO.Id.ToString(),
                        MATCHRECORDS = matchrecordsList
                    };

                    log.Debug("Add case log under threshold to MongoDB");
                    _freeSourceRepository.InsertCaseLog(modelCaseLog);
                }
                try
                {

                    _CustomerCaseDTO.sendMail = 1;
                    //var emailSent = await SendHtmlFormattedEmail("Risk creation Alert", emailBody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    //throw ex;
                }
            }

            log.Debug("Update customer case table with details after screening");

            _customerCaseService.Update(_CustomerCaseDTO);
            /*if(_CustomerCaseDTO.ApiResultsjson.Count>0)
            {
                isRecordCreated = true;
              
            }
            return isRecordCreated;*/
            //throw new NotImplementedException();
        }

        [HttpGet("/case/Process/{CaseId}")]
        public async Task<ActionResult> Process(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
               
                model.Case = new CaseModel();

                

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
                HttpContext.Session.SetString("CorporateId", _CustomerCaseDTO.CustomerId);
                HttpContext.Session.SetString("CorporateCaseId", CaseId.ToString());

                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path + HttpContext.Request.QueryString);

                var dualMatchStatus = _customerCaseService.GetDualGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.DualGoodsMatchStatus = dualMatchStatus;
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;
                if (_UserGroupModel.Name .Contains("Compliance") && _CustomerCaseDTO.Status == 4) { model.IsReadOnly = true; ViewBag.HideChatbot = true; }
                var dob = model.Case.DOB;
                var createddated = model.Case.CreatedOn;
                string dobText;

                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM")
                {
                    dobText = "NA";
                }
                else
                {
                    // Parse the string to DateTime first to format it
                    if (DateTime.TryParse(dob, out DateTime parsedDob))
                    {
                        dobText = parsedDob.ToString("dd/MM/yyyy"); // or "dd/MM/yyyy"
                    }
                    else
                    {
                        dobText = dob; // fallback if parsing fails
                    }
                }
                var createdDate = model.Case.CreatedOn;
                string createdDateText;

                if (createdDate == DateTime.MinValue)
                {
                    createdDateText = "NA";
                }
                else
                {
                    createdDateText = createdDate.ToString("dd/MM/yyyy"); // or "dd/MM/yyyy"
                }

                model.Case.CreatedOnText = createdDateText;

                model.Case.DOB = dobText;
                string RiskactionName;
                string CreatecontrollerName;
                if (model.Case.CustomerType == "I")
                {
                     RiskactionName = "risk";
                     CreatecontrollerName = "Create";
                }
                else
                {
                     RiskactionName = "risk";
                     CreatecontrollerName = "RiskAssessmentForCorpCustomer";
                }
                string sessionId = this.HttpContext.Session.GetString("SessID");
                var riskCreation = _userGroupRightService.CheckUserRightExixts(RiskactionName, CreatecontrollerName, userId, GroupId, sessionId);
                model.RiskCreation = riskCreation.Result;
                var actionRights = new Dictionary<string, string>();
                var actionsToCheck = new List<string>
                {
                    "Approve",
                    "Reject",
                    "Senior Management",
                    "On Hold",
                    "Whitelist"
                    //"SaveSearchResult"
                };
                foreach (var action in actionsToCheck)
                {
                    // Call the service to check if user has the right
                    var serviceResponse = _userGroupRightService.CheckNameuserrightExists(
                        "case",
                        "close",// Controller
                        userId,
                        GroupId,
                        sessionId ,
                        action// Action to check
                    );

                    // If user has the right, store the actual action name, else "1"
                    actionRights[action] = (serviceResponse?.Result ?? false).ToString().ToLower();
                }

                model.ActionRights = actionRights;

                

           
                var CommentCase = _userGroupRightService.CheckUserRightExixts("case","comment", userId, GroupId, sessionId);
                model.CommentCase = CommentCase.Result;

                var DocumentCase = _userGroupRightService.CheckUserRightExixts("case","document",  userId, GroupId, sessionId);
                model.DocumentsCase = DocumentCase.Result;

                var TransferCase = _userGroupRightService.CheckUserRightExixts("case","assign", userId, GroupId, sessionId);
                model.TransferCase = TransferCase.Result;

                var savesearch = _userGroupRightService.CheckUserRightExixts("case", "saveRemark", userId, GroupId, sessionId);
                model.SaveSearResult = savesearch.Result;

                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                List<ProliferationFinanceCaseDTO> proliferationData = _customerCaseService.GetProliferationData(_CustomerCaseDTO.CustomerId);
                model.ProliferationFinanceData = _mapper.Map<List<ProliferationFinanceModel>>(proliferationData);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId,_CustomerCaseDTO.CustomerType));
                model.RiskVersionData= _mapper.Map<List<RiskReportModel>>(riskReports);

                //List<PassportDetailsDTO> passportDetails= _customerCaseService.GetPassportdetails(CaseId);
                //model.passportDetails = _mapper.Map<List<PassportDetails>>(passportDetails);
                //model.Case.Id = CaseId;
                //model.DocumentCategories = new SelectList(_mapper.Map<List<DocumentCategoryModel>>(_caseDocumentService.GetAllCategories()), "Id", "Name");
                //model.DocumentTypes = new SelectList(_mapper.Map<List<DocumentTypeModel>>(_caseDocumentService.GetAllTypes()), "Id", "Name");
                var clientId = _clientHandler.GetClientId();
                //IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                //                                       select new SelectListItem
                //                                       {
                //                                           Value = Convert.ToString(s.Id),
                //                                           Text = s.FName + " " + s.LName.ToString()
                //
                //       };

                
                //RiskModel _riskmodel = new RiskModel();


                //int riskid = _riskService.GetRiskIdByCustomercode(model.Case.CustomerId, model.Case.CustomerType).Result;

                //if (riskid != 0)
                //{
                //    dynamic modelrisk = null;

                //    if (model.Case.CustomerType == "I")
                //    {
                //        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId);

                //        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);

                //        MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                //        model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
                //        model.FinalRiskScore = modelrisk.FinalRiskScore;
                //        model.RiskScoreCount = modelrisk.RiskScoreCount;
                //        model.RiskScoreSum = modelrisk.RiskScoreSum;
                //        model.DateofAssessment = modelrisk.DateofAssessment;
                //        model.MainNationalityTxt = modelrisk.MainNationalityTxt;
                //        model.Address = modelrisk.Address;
                //    }
                //    else
                //    {
                //        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId);

                //        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);

                //        MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                //        model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
                //        model.FinalRiskScore = modelrisk.RiskAssessmentRating;
                //        model.RiskScoreBeforeOverride = modelrisk.RiskAssessmentRatingWithoutOverride;
                //        model.RiskScoreCount = modelrisk.RiskScoreCount;
                //        model.RiskScoreSum = modelrisk.RiskScoreSum;
                //        model.DateofAssessment = modelrisk.DateofAssessment;
                //        model.MainNationalityTxt = modelrisk.CountryOfIncorporationTxt;
                //    }
                //}

                model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig(model.Case.CustomerType, 1, 0, 0, clientId);
                for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    }
                }

                var id = _clientHandler.GetUserId();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                       select new SelectListItem
                                                       {
                                                           Value = Convert.ToString(s.Id),
                                                           Text = s.FName + " " + s.LName.ToString()
                                                       };

                model.Users = new SelectList(userList, "Value", "Text");
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                //log.Debug($"apiresults ({result}) ");
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException.Message;
                error.module = "CaseManagement_Process";
                error.comments = "API call Error while Getting data from manogo db";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet("/case/Process_PDF/{CaseId}")]
        public async Task<ActionResult> Process_PDF(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
               
                model.Case = new CaseModel();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);

                var dualMatchStatus = _customerCaseService.GetDualGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.DualGoodsMatchStatus = dualMatchStatus;
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;
                var dob = model.Case.DOB;
                var createddated = model.Case.CreatedOn;
                string dobText;

                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM")
                {
                    dobText = "NA";
                }
                else
                {
                    if (DateTime.TryParse(dob, out DateTime parsedDob))
                    {
                        dobText = parsedDob.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        dobText = dob;
                    }
                }
                var createdDate = model.Case.CreatedOn;
                string createdDateText;

                if (createdDate == DateTime.MinValue)
                {
                    createdDateText = "NA";
                }
                else
                {
                    createdDateText = createdDate.ToString("dd/MM/yyyy");
                }

                model.Case.CreatedOnText = createdDateText;
                model.Case.DOB = dobText;
                
                string sessionId = this.HttpContext.Session.GetString("SessID");
                
                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId,_CustomerCaseDTO.CustomerType));
                model.RiskVersionData= _mapper.Map<List<RiskReportModel>>(riskReports);

                var clientId = _clientHandler.GetClientId();
                RiskModel _riskmodel = new RiskModel();
                int riskid = _riskService.GetRiskIdByCustomercode(model.Case.CustomerId, model.Case.CustomerType).Result;

                if (riskid != 0)
                {
                    dynamic modelrisk = null;

                    if (model.Case.CustomerType == "I")
                    {
                        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);
                        int riskVersion = modelrisk != null ? modelrisk.version : 1;
                        if (riskVersion == 0) riskVersion = 1;

                        model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("I", 1, 0, 0, clientId, riskVersion);

                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;

                        // Check for Override (O)
                        bool hasHighOverride = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                        if (model.FinalRiskScore != null && model.FinalRiskScore.ToLower().Contains("high risk") && hasHighOverride)
                        {
                            model.FinalRiskScore = "High Risk (O)";
                        }
                    }
                    else
                    {
                        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);
                        int riskVersion = modelrisk != null ? modelrisk.version : 1;
                        if (riskVersion == 0) riskVersion = 1;

                        model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("C", 1, 0, 0, clientId, riskVersion);

                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.RiskAssessmentRating = modelrisk.RiskAssessmentRating;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;

                        // Check for Override (O)
                        bool hasHighOverrideCor = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                        if (model.RiskAssessmentRating != null && model.RiskAssessmentRating.ToLower().Contains("high risk") && hasHighOverrideCor)
                        {
                            model.RiskAssessmentRating = "High Risk (O)";
                        }
                    }
                }

                var allComments = _caseCommentService.GetAllByCase(CaseId);
                if (allComments != null)
                {
                    model.CaseComments = allComments.Select(c => new CaseModel
                    {
                        Comments = c.Comment,
                        CreatedUser = c.CreatedUser,
                        CreatedOn = c.CreatedOnDB ?? DateTime.MinValue,
                        MatchType = c.CommentType
                    }).ToList();
                }
                
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
                
                return View("Process_PDF", model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet("/case/DownloadProcessPDF/{CaseId}")]
        public async Task<IActionResult> DownloadProcessPDF(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

                model.Case = new CaseModel();
                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);

                var dualMatchStatus = _customerCaseService.GetDualGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.DualGoodsMatchStatus = dualMatchStatus;
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;

                var dob = model.Case.DOB;
                string dobText;
                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM")
                    dobText = "NA";
                else if (DateTime.TryParse(dob, out DateTime parsedDob))
                    dobText = parsedDob.ToString("dd/MM/yyyy");
                else
                    dobText = dob;

                var createdDate = model.Case.CreatedOn;
                model.Case.CreatedOnText = createdDate == DateTime.MinValue ? "NA" : createdDate.ToString("dd/MM/yyyy");
                model.Case.DOB = dobText;

                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);

                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType));
                model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);

                var clientId = _clientHandler.GetClientId();
                var sessionId = this.HttpContext.Session.GetString("SessID");

                // Risk Assessment
                int riskid = _riskService.GetRiskIdByCustomercode(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType).Result;
                if (riskid != 0)
                {
                    var riskReportVersion = 1;
                    model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport(_CustomerCaseDTO.CustomerType, 1, 0, 0, clientId, riskReportVersion);

                    if (_CustomerCaseDTO.CustomerType == "I")
                    {
                        dynamic modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);
                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);
                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        
                        // For Individual, RiskAssessmentRating comes from FinalRiskScore
                        model.RiskAssessmentRating = modelrisk.FinalRiskScore;
                    }
                    else if (_CustomerCaseDTO.CustomerType == "C")
                    {
                        dynamic modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);
                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.RiskAssessmentRating = modelrisk.RiskAssessmentRating;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        // For Corporate, FinalRiskScore is mapped from RiskAssessmentRating
                        model.FinalRiskScore = modelrisk.RiskAssessmentRating;
                    }

                    // Check for Override (O)
                    bool hasHighOverrideCor = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                    if (model.RiskAssessmentRating != null && model.RiskAssessmentRating.ToLower().Contains("high risk") && hasHighOverrideCor)
                    {
                        model.RiskAssessmentRating = "High Risk (O)";
                        model.FinalRiskScore = "High Risk (O)";
                    }
                }



                var allComments = _caseCommentService.GetAllByCase(CaseId);
                if (allComments != null)
                {
                    model.CaseComments = allComments.Select(c => new CaseModel
                    {
                        Comments = c.Comment,
                        CreatedUser = c.CreatedUser,
                        CreatedOn = c.CreatedOnDB ?? DateTime.MinValue,
                        MatchType = c.CommentType
                    }).ToList();
                }

                // Screening results

                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }

                // Render view to HTML string then convert to PDF
                string html = await _viewRenderService.RenderToStringAsync("Case/Process_PDF", model);
                byte[] pdfBytes = _exportService.HtmlToPDFforChecklistLogs(html);
                string fileName = $"Case_{_CustomerCaseDTO.CustomerId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating PDF: " + ex.Message);
            }
        }


        public IActionResult CreateMainparty(string customerCode, string custtype)
        {
            int Id = _customerCaseService.GetCaseId(customerCode);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            _CustomerCaseDTO.CustomerType = custtype;
            _CustomerCaseDTO.ClientId = _clientHandler.GetClientId();
            _CustomerCaseDTO.CreatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.CustomerId = "0";

            int previousStatus = _CustomerCaseDTO.Status;

            if (custtype == "I")
            {
                _CustomerCaseDTO.Type = "Individual";
            }
            else
            {
                _CustomerCaseDTO.Type = "Corporate";
            }

            var result = _clientHandler.PostAsync(new { caseid = Id.ToString() }, ScreeningService.GETBYCASEID).Result;

            // Declare jsonList outside
            List<DataListModel> jsonList = new List<DataListModel>();
            bool hasMatchRecords = false;

            if (!string.IsNullOrEmpty(result))
            {
                jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                hasMatchRecords = jsonList != null && jsonList.Any();
            }

             // default first time

            if (previousStatus == 2 || previousStatus == 3)
            {
                int newStatus = hasMatchRecords ? 0 : 5;

                _CustomerCaseDTO.Status = newStatus;
            }
            

            // Assign status before creating case


            var result1 = _customerCaseService.Create(_CustomerCaseDTO);

            int newCaseId = _customerCaseService.GetCaseId(result1.Result.Split('Ø')[1]);

            bool isCaseCreated = false;
            string caseRefId = null;

            if (result1 != null)
            {
                List<MatchRecordsDTO> matchrecordsList = jsonList
                    .Select(x => new MatchRecordsDTO
                    {
                        MATCHUID = x.matchuid?.ToString(),
                        MATCHTYPE = x.matchtype,
                        MATCHCATEGORY = x.matchcategory,
                        MATCHNAME = x.matchname?.ToUpper(),
                        MATCHSCORE = Convert.ToInt32(x.matchscore),
                        MATCHNATIONALITY = x.matchnationality ?? "",
                        MATCHIDNO = x.matchidno?.ToString(),
                        MATCHDOB = x.matchdob ?? "",
                        MATCHRESOURCESID = x.matchresourcesid,

                        MATCHDATASETS = !string.IsNullOrEmpty(x.matchdatasets)
                            ? string.Join(", ",
                                x.matchdatasets
                                .Split(',')
                                .Select(d => d.Contains("-")
                                    ? d.Split('-')[0].Trim()
                                    : d.Trim()))
                            : "--",

                        MATCHGENDER = x.matchgender ?? ""
                    })
                    .ToList();

                CASELOG modelCaseLog = new CASELOG
                {
                    CASEID = newCaseId.ToString(),
                    MATCHRECORDS = matchrecordsList
                };

                log.Debug("Add case log under threshold to MongoDB");

                _freeSourceRepository.InsertCaseLog(modelCaseLog);

                caseRefId = result1.Result.Split('Ø')[1];
                isCaseCreated = true;
            }

            if (isCaseCreated && !string.IsNullOrEmpty(caseRefId))
            {
                int id = _customerCaseService.GetCaseId(caseRefId);
                

                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = id;
                remarkModel.Comment = "Case is Created"; // ✅ FIXED
                remarkModel.CommentType = "Convert Related Parties to Main Party";
                remarkModel.CreatedBy = _clientHandler.GetUserId();

                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));

                int previousId = _customerCaseService.GetCaseId(customerCode);

                CustomerCaseDTO _CustomerCaseDTO1 = _customerCaseService.GetDetails(previousId);
                DateTime createdDate;
                CaseCommentModel remarkModel1 = new CaseCommentModel();
                remarkModel1.CaseId = id;
                var formats = new[]
                {
                    "dd/MM/yyyy HH:mm:ss",
                    "dd-MM-yyyy HH:mm:ss"
                };

                if (DateTime.TryParseExact(
                    _CustomerCaseDTO1.CreatedOn,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out createdDate))
                {
                    remarkModel1.Comment = $"Case is Screened on {createdDate:dd/MM/yyyy}";
                }
                remarkModel1.CommentType = "Convert Related Parties to Main Party";
                remarkModel1.CreatedBy = _clientHandler.GetUserId();

                var remarkResult1 = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel1));

                TempData["CaseRefId"] = caseRefId;
                TempData["IsCaseCreated"] = true;
            }

            if (_CustomerCaseDTO.Type == "Individual")
            {
                return Json(new { redirectUrl = Url.Action("Create") });
            }
            else
            {
                return Json(new { redirectUrl = Url.Action("CorporateScreening", "Corporate") });
            }
        }

        [HttpGet("/case/Shareholder/{CaseId}")]
        public async Task<ActionResult> Shareholder(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

                var corporateId = HttpContext.Session.GetString("CorporateId");
                var corporatecaseId = HttpContext.Session.GetString("CorporateCaseId");
                
                TempData["CorporateId"] = corporateId;
                TempData["CorporateCaseId"] = corporatecaseId;

                

                var returnUrl = HttpContext.Session.GetString("ReturnUrl");
                ViewBag.ReturnUrl = returnUrl;

                HttpContext.Session.SetString("ShareholderReturnUrl", HttpContext.Request.Path + HttpContext.Request.QueryString);

                model.Case = new CaseModel();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;
                var dob = model.Case.DOB;
                var createddated = model.Case.CreatedOn;
                string dobText;

                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM")
                {
                    dobText = "NA";
                }
                else
                {
                    // Parse the string to DateTime first to format it
                    if (DateTime.TryParse(dob, out DateTime parsedDob))
                    {
                        dobText = parsedDob.ToString("dd/MM/yyyy"); // or "dd/MM/yyyy"
                    }
                    else
                    {
                        dobText = dob; // fallback if parsing fails
                    }
                }
                var createdDate = model.Case.CreatedOn;
                string createdDateText;

                if (createdDate == DateTime.MinValue)
                {
                    createdDateText = "NA";
                }
                else
                {
                    createdDateText = createdDate.ToString("dd/MM/yyyy"); // or "dd/MM/yyyy"
                }

                model.Case.CreatedOnText = createdDateText;

                model.Case.DOB = dobText;


                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByParentCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType));
                model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);

                //List<PassportDetailsDTO> passportDetails= _customerCaseService.GetPassportdetails(CaseId);
                //model.passportDetails = _mapper.Map<List<PassportDetails>>(passportDetails);
                //model.Case.Id = CaseId;
                //model.DocumentCategories = new SelectList(_mapper.Map<List<DocumentCategoryModel>>(_caseDocumentService.GetAllCategories()), "Id", "Name");
                //model.DocumentTypes = new SelectList(_mapper.Map<List<DocumentTypeModel>>(_caseDocumentService.GetAllTypes()), "Id", "Name");
                var clientId = _clientHandler.GetClientId();
                //IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                //                                       select new SelectListItem
                //                                       {
                //                                           Value = Convert.ToString(s.Id),
                //                                           Text = s.FName + " " + s.LName.ToString()
                //
                //       };
                RiskModel _riskmodel = new RiskModel();
                int riskid = _riskService.GetRiskIdByCustomercode(model.Case.CustomerId, model.Case.CustomerType).Result;

                if (riskid != 0)
                {
                    dynamic modelrisk = null;

                    if (model.Case.CustomerType == "I")
                    {
                        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId);

                        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);

                        MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        model.MainNationalityTxt = modelrisk.MainNationalityTxt;
                        model.Address = modelrisk.Address;
                    }
                    else
                    {
                        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId);

                        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);

                        MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
                        model.FinalRiskScore = modelrisk.RiskAssessmentRating;
                        model.RiskScoreBeforeOverride = modelrisk.RiskAssessmentRatingWithoutOverride;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        model.MainNationalityTxt = modelrisk.CountryOfIncorporationTxt;
                    }
                }

                var id = _clientHandler.GetUserId();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                       select new SelectListItem
                                                       {
                                                           Value = Convert.ToString(s.Id),
                                                           Text = s.FName + " " + s.LName.ToString()
                                                       };

                model.Users = new SelectList(userList, "Value", "Text");
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                //log.Debug($"apiresults ({result}) ");
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException.Message;
                error.module = "CaseManagement_Process";
                error.comments = "API call Error while Getting data from manogo db";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet("/case/Shareholder_PDF/{CaseId}")]
        public async Task<ActionResult> Shareholder_PDF(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

                var corporateId = HttpContext.Session.GetString("CorporateId");
                TempData["CorporateId"] = corporateId;

                model.Case = new CaseModel();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;
                var dob = model.Case.DOB;
                var createddated = model.Case.CreatedOn;
                string dobText;

                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM")
                {
                    dobText = "NA";
                }
                else
                {
                    if (DateTime.TryParse(dob, out DateTime parsedDob))
                    {
                        dobText = parsedDob.ToString("dd/MM/yyyy");
                    }
                    else
                    {
                        dobText = dob;
                    }
                }
                var createdDate = model.Case.CreatedOn;
                string createdDateText;

                if (createdDate == DateTime.MinValue)
                {
                    createdDateText = "NA";
                }
                else
                {
                    createdDateText = createdDate.ToString("dd/MM/yyyy");
                }

                model.Case.CreatedOnText = createdDateText;
                model.Case.DOB = dobText;


                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType));
                model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);

                var clientId = _clientHandler.GetClientId();
                RiskModel _riskmodel = new RiskModel();
                int riskid = _riskService.GetRiskIdByCustomercode(model.Case.CustomerId, model.Case.CustomerType).Result;

                if (riskid != 0)
                {
                    dynamic modelrisk = null;

                    if (model.Case.CustomerType == "I")
                    {
                        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId);

                        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);

                        MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        model.MainNationalityTxt = modelrisk.MainNationalityTxt;
                        model.Address = modelrisk.Address;
                    }
                    else
                    {
                        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId);

                        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);

                        MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.RiskTypeCategoryDTO = _riskmodel.RiskTypeCategoryDTO;
                        model.FinalRiskScore = modelrisk.RiskAssessmentRating;
                        model.RiskScoreBeforeOverride = modelrisk.RiskAssessmentRatingWithoutOverride;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        model.MainNationalityTxt = modelrisk.CountryOfIncorporationTxt;
                    }
                }

                var id = _clientHandler.GetUserId();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                       select new SelectListItem
                                                       {
                                                           Value = Convert.ToString(s.Id),
                                                           Text = s.FName + " " + s.LName.ToString()
                                                       };

                model.Users = new SelectList(userList, "Value", "Text");
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
                
                return View("Shareholder_PDF", model);
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException?.Message ?? ex.Message;
                error.module = "CaseManagement_Shareholder_PDF";
                error.comments = "API call Error while Getting data from mongo db";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Index");
            }
        }

        private void MapRiskValues(List<RiskTypeCategoryDTO> categories, List<ReportDataDTO> reportData)
        {
            if (reportData == null) return;

            foreach (var category in categories)
            {
                foreach (var riskType in category.RiskTypes)
                {
                    var isCountry = riskType.lov_country_duplicate == 1;
                    var match = reportData.FirstOrDefault(x =>
                        (isCountry || x.lov_type_category_id == category.Id) &&
                        x.lov_type_id == riskType.Id);

                    if (match != null)
                    {
                        riskType.ItemTxt = match.lov_risk_data;
                        riskType.ItemScore = match.lov_risk_score;
                        riskType.OverrideScore = match.Over_ride_Score;

                        // ? Important for dropdown selection
                        //riskType.SelectedItemId = match.;
                    }

                    // ? Always Build SelectList
                    riskType.Items = new SelectList(
                        riskType.RiskItems,
                        "Score",
                        "RiskItem",
                        riskType.SelectedItemId
                    );
                }
            }
        }

        [HttpGet]
        [Route("Case/process/Reject")]
        public ActionResult Reject()
        {
            return View();
        }

        //[HttpGet]
        //[Route("Case/process/Download")]
        public async Task<FileResult> CallApiMatchuid(string id, string category)
        {
            HttpResponseMessage file = new HttpResponseMessage();


            using (HttpClient httpClient = new HttpClient())
            {
                //string url = pdfbaseURL;
                string url = baseC6URL;
                
                TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                //     string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1ZmQ4NzVjNWVmMmFmYjMxNGNhMWE1YjIiLCJpYXQiOjE2MTI2OTE4MTMsImV4cCI6MTYxMzI5NjYxM30.trsanUcNCINZTa0gkWD_5LfofoHG1aD2wX8tq3XsP8I";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                if (category == "INDIVIDUAL")
                    url = baseC6URL + "personal/" + id;
                else
                    url = baseC6URL + "business/" + id;
                Task<HttpResponseMessage> response = httpClient.GetAsync(url);
                file = response.Result;

            }


            return File(file.Content.ReadAsByteArrayAsync().Result, "application/pdf; charset=utf-8", id + ".pdf");
        }

        [HttpGet]
        [Route("Case/process/Download")]
        public async Task<FileResult> CallApi(string id, string category)
        {
            HttpResponseMessage file = new HttpResponseMessage();


            using (HttpClient httpClient = new HttpClient())
            {
                //string url = pdfbaseURL;
                string url = baseC6URL;
            
                TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                //     string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1ZmQ4NzVjNWVmMmFmYjMxNGNhMWE1YjIiLCJpYXQiOjE2MTI2OTE4MTMsImV4cCI6MTYxMzI5NjYxM30.trsanUcNCINZTa0gkWD_5LfofoHG1aD2wX8tq3XsP8I";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                if (category == "INDIVIDUAL")
                    url = baseC6URL + "personal/" + id;
                else
                    url = baseC6URL + "business/" + id;
                Task<HttpResponseMessage> response = httpClient.GetAsync(url);
                file = response.Result;

            }


            return File(file.Content.ReadAsByteArrayAsync().Result, "application/pdf; charset=utf-8", id + ".pdf");
        }
        [HttpGet]
        [Route("Case/process/DownloadPdf")]
        public async Task<IActionResult> DownloadPdf(int caseId, int index)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            model.Case.Id = caseId;
            model.Index = index;
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(caseId);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);

            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(caseId);
            model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

            var clientId = _clientHandler.GetClientId();
            int fileType = (int)OperationType.PDF;
            var ClientId = _clientHandler.GetUserId();
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != ClientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            var result = _clientHandler.PostAsync(new { caseid = caseId.ToString() }, ScreeningService.GETBYCASEID).Result;
            if (!string.IsNullOrEmpty(result))
            {
                List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                model.DataList = jsonList;
            }

            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.Case.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = string.Concat(model.Case.FirstName, " ", model.Case.MiddleName, " ", model.Case.LastName),
                customernationality = model.Case.Nationality,
                searchtype = "F"
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(data))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                model.apiResultModels = apiResultModel;
            }

            // High-fidelity HTML to PDF conversion
            string html = await _viewRenderService.RenderToStringAsync("Case/GetDetails_PDF", model);
            byte[] pdfBytes = _exportService.HtmlToPDFforChecklistLogs(html);
            string fileName = $"CaseMatchReport_{caseId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }



        [HttpGet]
        [Route("Case/process/Details")]
        public async Task<ActionResult> GetDetailsApi(string id, string category, int CaseId,string Type,string Resourcesid ,string screenType)
        {

            UsersModel umodel = new UsersModel();
            BusinessModel bmodel = new BusinessModel();
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
            string url;
            var returnUrl = "";
            if (screenType == "shareholder")
            {
                returnUrl = HttpContext.Session.GetString("ShareholderReturnUrl");
            }
            else
            {
                returnUrl = HttpContext.Session.GetString("ReturnUrl");
            }


            ViewBag.ReturnUrl = returnUrl;

            
            TokenRS token =  AMLUtility.CreateC6Token("users/authenticate", baseC6URL, _c6Username);
            using (HttpClient httpClient = new HttpClient())
            {

                if (category == "INDIVIDUAL")
                {
                    url = baseC6URL + "personal/person/" + id;




                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Content(response.ToString());
                    }



                    var details = response.Content.ReadAsStringAsync();
                    UsersModel jsonList = JsonConvert.DeserializeObject<UsersModel>(details.Result);
                    umodel = jsonList;
                    umodel.caseid = CaseId;
                    umodel.Type = Type;
                    umodel.Resourcesid = Resourcesid;
                    umodel.Category = category;
                    umodel.MatchUid = id;
                    umodel.CreationDate = _CustomerCaseDTO.CreatedOn;
                    umodel.Case= _mapper.Map<CaseModel>(_CustomerCaseDTO);
                    ViewBag.user = umodel;
                }
                else
                {
                    url = baseC6URL + "business/person/" + id;
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return Content(response.ToString());
                    }



                    var details = response.Content.ReadAsStringAsync();
                    BusinessModel jsonList = JsonConvert.DeserializeObject<BusinessModel>(details.Result);
                    bmodel = jsonList;
                    bmodel.caseid = CaseId;
                    bmodel.Type = Type;
                    bmodel.Resourcesid = Resourcesid;
                    bmodel.Category = category;
                    bmodel.MatchUid = id;
                    bmodel.CreationDate = _CustomerCaseDTO.CreatedOn;
                    bmodel.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                    ViewBag.business = bmodel;

                }



            }
            return View();
        }

        private async void SendCaseTransferedMailAsync(string body, CaseAssignmentModel model)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                var appBaseUrl = MyHttpContext.AppBaseUrl;
                body = body.Replace("{caseID}", model.CaseId.ToString());
                body = body.Replace("{baseUrl}", appBaseUrl);
                body = body.Replace("{Username}", HttpContext.Session.GetString("SessUsername"));

                var emailSent = await _commonService.sendCaseTransferedEmaiLog("Case Transfered Alert", body, model);
            }
        }



        [HttpPost("/case/assign")]
        public async Task<JsonResult> Assign(CaseAssignmentModel model)
        {
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.UtcNow.AddHours(4);
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(model.CaseId);
            if (_UserGroupModel.Name == "Senior Management") 
            {
                if(_CustomerCaseDTO.MatchScore == 0)
                {
                    _CustomerCaseDTO.Status = 5;
                }
                else
                {
                    _CustomerCaseDTO.Status = 0;
                }

                    _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
                _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.UtcNow.AddHours(4));
                _CustomerCaseDTO.Comments = model.Comment;

                 _customerCaseService.Update(_CustomerCaseDTO);

            }


            
            var result = _caseAssignmentService.Create(_mapper.Map<CaseAssignmentDTO>(model));
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = string.Format("Transferred Case To {0}", model.TransferUser);


            UserModel _UserModel = _mapper.Map<UserModel>(_userService.GetDetails(model.UserId));
            model.Email = _UserModel.UserDetail.Email;
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(@"Views/Risk/CaseTransferedEmailBody.html"))
            {
                body = reader.ReadToEnd();
            }
            ;

            if (model.Comment != "" && model.Comment != null)
            {
                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }
            CaseCommentModel commentModel = new CaseCommentModel();
            commentModel.CaseId = model.CaseId;
            commentModel.Comment = comment;
            commentModel.CreatedBy = _clientHandler.GetUserId();
            commentModel.CommentType = "Transferred Case Section";
            var commentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));

            await Task.Run(() => SendCaseTransferedMailAsync(body, model));

            return Json(result);
        }
        [HttpPost("/case/onhold")]
        public async Task<JsonResult> OnHold(CaseAssignmentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;

            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            CustomerCaseDTO currentCase = _customerCaseService.GetDetails(model.CaseId);
            if (_UserGroupModel.Name .Contains("Compliance") && currentCase.Status == 4)
            {
                return Json("Edit access restricted for cases submitted to senior management.");
            }
            
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = string.Format("Customer Case Onhold");
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(model.CaseId);
            _CustomerCaseDTO.Status = 7;
            _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
            _CustomerCaseDTO.Comments = model.Comment;

            var result = _customerCaseService.Update(_CustomerCaseDTO);
            if(_CustomerCaseDTO.Type !="Individual" && _CustomerCaseDTO.Type != "Corporate")
            {
                
                
                int companyId= _customerCaseService.GetCaseId(_CustomerCaseDTO.CompanyCode);
                CustomerCaseDTO _CompanyCustomerCaseDTO = _customerCaseService.GetDetails(companyId);
                _CompanyCustomerCaseDTO.Status = 0;
                _CompanyCustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
                _CompanyCustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
                _CompanyCustomerCaseDTO.Comments = model.Comment;

                var result1 = _customerCaseService.Update(_CompanyCustomerCaseDTO);
                CaseCommentModel CompanycommentModel = new CaseCommentModel();
                CompanycommentModel.CaseId = companyId;
                CompanycommentModel.Comment = _CustomerCaseDTO.FlagType +" case has been on hold";// ✅ FIXED
                CompanycommentModel.CommentType = "Related Parties On Hold";
                CompanycommentModel.CreatedBy = _clientHandler.GetUserId();
                var CompanycommentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(CompanycommentModel));
                if(_CustomerCaseDTO.ParentID != null) 
                {
                    int parentId = _customerCaseService.GetCaseId(_CustomerCaseDTO.ParentID);
                    //CustomerCaseDTO _parentCustomerCaseDTO = _customerCaseService.GetDetails(parentId);
                    //_parentCustomerCaseDTO.Status = 0;
                    //_parentCustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
                    //_parentCustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
                    //_parentCustomerCaseDTO.Comments = model.Comment;

                    var result2 = _customerCaseService.Update(_CompanyCustomerCaseDTO);
                    CaseCommentModel parentIdcommentModel = new CaseCommentModel();
                    parentIdcommentModel.CaseId = parentId;
                    parentIdcommentModel.Comment = _CustomerCaseDTO.FlagType + " case has been on hold";// ✅ FIXED
                    parentIdcommentModel.CommentType = "Related Parties On Hold";
                    parentIdcommentModel.CreatedBy = _clientHandler.GetUserId();
                    var parentIdcommentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(parentIdcommentModel));

                }
            }
            if (model.Comment != "" && model.Comment != null)
            {
                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CommentType = "Hold Remarks";
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }
            CaseCommentModel commentModel = new CaseCommentModel();
            commentModel.CaseId = model.CaseId;
            commentModel.Comment = comment;// ✅ FIXED
            commentModel.CommentType = "Hold";
            commentModel.CreatedBy = _clientHandler.GetUserId();
            var commentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));


            return Json(model);
        }
        [HttpPost("/case/document")]
        public JsonResult Document(CaseDocumentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            model.ClientId = _clientHandler.GetClientId();
            if (model.Document != null)
            {
                DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), model.Document);
                model.DocumentFileName = _documentsModel.DocName;
                model.DocumentFullPath = _documentsModel.DocFullPath;
            }
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            model.IssuedDateOnDB = model.IssuedDate;
            model.ExpiryDateOnDB = model.ExpiryDate;
            var result = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(model));
            _customerCaseService.UpdateCase(model.Id,model.CreatedBy);
            return Json(result);
        }
        [HttpPost("/case/comment")]
        public JsonResult Comment(CaseCommentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            var result = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(model));

            return Json(_caseCommentService.GetAllByCase(model.CaseId));
        }
        [HttpPost("/case/close")]
        public JsonResult Close(CaseCloseModel model)
        {
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            CustomerCaseDTO currentCase = _customerCaseService.GetDetails(model.CaseId);
            if (_UserGroupModel.Name .Contains("Compliance") && currentCase.Status == 4) { return Json("Edit access restricted for cases submitted to senior management."); }

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(model.CaseId);
            _CustomerCaseDTO.Status = model.Action == 1 ? model.Action : 2;
            _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
            _CustomerCaseDTO.Comments = model.Comment;
            _CustomerCaseDTO.NoMatch = model.NoMatch;
            _CustomerCaseDTO.TrueDomesticpep = model.TrueDomesticpep;
            _CustomerCaseDTO.TrueForeignpep = model.TrueForeignpep;
            _CustomerCaseDTO.TrueAdverseMedia = model.TrueAdverseMedia;
            _CustomerCaseDTO.PartialDomesticpep = model.PartialDomesticpep;
            _CustomerCaseDTO.PartialForeignpep = model.PartialForeignpep;
            _CustomerCaseDTO.Partialadversemedia = model.Partialadversemedia;
            _CustomerCaseDTO.TrueUAEUNSanction = model.TrueUAEUNSanction;
            _CustomerCaseDTO.TrueOtherSanction = model.TrueOtherSanction;
           

            

            var result = _customerCaseService.Update(_CustomerCaseDTO);

            //Update White List
            if (model.Action == 1)
            {

                var res = _customerMasterService.UpdateWhiteList(_CustomerCaseDTO.CustomerMasterId, "YES");
                _customerMasterService.InsertCustomerWhiteListLogs(_CustomerCaseDTO.CustomerMasterId, _CustomerCaseDTO.CustomerId, _clientHandler.GetUserId(), "YES");
            }
            //else
            //{
            //    var res = _customerMasterService.UpdateWhiteList(_CustomerCaseDTO.CustomerMasterId, "NO");
            //    _customerMasterService.InsertCustomerWhiteListLogs(_CustomerCaseDTO.CustomerMasterId, _CustomerCaseDTO.CustomerId, _clientHandler.GetUserId(), "NO");
            //}

            string response = string.Empty;
            string commenttype = string.Empty;
            if (model.Action == 2)
            {
                response = "Customer case approved.";
                commenttype = "Approved";
            }
            else if (model.Action == 3)
            {
                response = "Customer case rejected.";
                commenttype = "Rejected";
            }
            else if(model.Action == 4)
            {
                response = "Customer case is  forwarded to Senior Management.";
                commenttype = "Senior Management";
            }
            //response = model.Action == 2 ? "Customer case approved." : "Customer case rejected.";
            if (model.Action == 1)
            {

                response = "Customer Case Whitelisted";
                commenttype = "whitelisted";
            }
            //_toastNotification.AddErrorToastMessage(reposne);
            if (model.Comment != "" && model.Comment != null)
            {
                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }
            CaseCommentModel commentModel = new CaseCommentModel();
            commentModel.CaseId = model.CaseId;
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = "";
            switch (model.Action)
            {
                case 1:
                    comment = string.Format("Whitelisted Case");
                    break;
                case 2:
                    comment = string.Format("Approved Case");
                    break;
                case 3:
                    comment = string.Format("Rejected Case");
                    break;
                case 4:
                    comment = string.Format("Forwarded to Senior Management");
                    break;
                default:
                    comment = "";
                    break;
            }
            commentModel.Comment = comment;
            commentModel.CreatedBy = _clientHandler.GetUserId();
            commentModel.CommentType = commenttype; 
            var commentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));

            if (model.screenType == "Shareholder")
            {
                int id = _customerCaseService.GetCaseId(model.CorporateId);

                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = Convert.ToInt32(id);
                remarkModel.Comment = model.FlagType + " Case is Updated"; // ✅ FIXED
                remarkModel.CommentType = model.FlagType;
                remarkModel.CreatedBy = _clientHandler.GetUserId();

                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }


            return Json(response);

        }
        [HttpGet("/case/comment/{CaseId}")]
        public JsonResult Comment(int CaseId)
        {
            return Json(_caseCommentService.GetAllByCase(CaseId));
        }
        [HttpGet]
        public ActionResult DownloadCaseDocument(string filePath, string fileName)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);



            return File(fileBytes, "application/force-download", fileName);
        }

        [Route("/case/saveRemark")]
        [HttpPost]
        public JsonResult SaveRemark(int id,string customerid,string type, List<DataListModel> model)
        {
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            CustomerCaseDTO currentCase = _customerCaseService.GetDetails(id);
            if (_UserGroupModel.Name .Contains("Compliance") && currentCase.Status == 4)
            {
                return Json("Edit access restricted for cases submitted to senior management.");
            }

            var userid = _clientHandler.GetUserId();
            var clientid = _clientHandler.GetClientId();
            CorporateKycDTO corpModel = new CorporateKycDTO();
            KycIndividualDTO imodel = new KycIndividualDTO();
            CaseModel model1=new CaseModel();
            CorporateDetailsModel corporateDetailsModel = new CorporateDetailsModel();
            var result = _commonService.UpdateCaseRemark(id, model);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(id);

            string response = string.Empty;
            response = result ? "Remarks Updated" : "Saving Remarks failed";
            _customerCaseService.UpdateCase(id, userid);

            
            if (type == "Individual" || type == "Corporate")
            {
                bool hasOFAC = false;
                bool hasKYC6 = false;
                var rowtype = "";

                /* -------------------------
                   FIRST LOOP
                   Detect match types
                --------------------------*/
                model1.Domesticpep = "No";
                model1.ForeignPep = "No";
                model1.RedFlags = "No";
                model1.SanctionMatch = "No";
                model1.UAEORUNSC = "No";
                model1.HighNetworkIndividual = "No";
                corporateDetailsModel.Domesticpep = "No";
                corporateDetailsModel.SanctionMatch = "No";
                corporateDetailsModel.ForeignPep = "No";
                corporateDetailsModel.RedFlags = "No";
                corporateDetailsModel.UAEORUNSC = "No";
                corporateDetailsModel.HighNetworkIndividual = "No";

                foreach (var row in model)
                {
                    if (row.matchcategory == "INDIVIDUAL")
                    {
                        rowtype = row.matchcategory;
                        // Skip if searchTypes empty
                        if (row.searchTypes == null || !row.searchTypes.Any())
                            continue;

                        bool hasNone = row.searchTypes.Contains("None");

                        // If only None selected → treat as no selection
                        if (hasNone && row.searchTypes.Count == 1)
                            continue;

                        // Remove None if mixed with others
                        if (hasNone)
                        {
                            row.searchTypes = row.searchTypes
                                .Where(x => x != "None")
                                .ToList();
                        }

                        if (!string.IsNullOrWhiteSpace(row.matchtype))
                        {
                            var type1 = row.matchtype?.ToUpper() ?? string.Empty;

                            // Group 1
                            var sanctionTypes = new List<string> { "UN", "OFAC", "UAE IEC LIST", "BL", "CBWL", "INTERNAL","" };
                            if (sanctionTypes.Any(t => type1.Contains(t)))
                            
                                hasOFAC = true;   // You can rename this to hasSanction if needed
                            
                            

                            if (type1.Contains("KYC6"))
                                hasKYC6 = true;
                        }
                        if (hasKYC6 && row.searchTypes.Contains("Domestic PEP"))
                        {
                            model1.Domesticpep = "Yes";
                        }
                        

                        if (hasKYC6 && row.searchTypes.Contains("Foreign PEP"))
                        {
                            model1.ForeignPep = "Yes";
                        }
                        
                        var hasHighRisk = row.searchTypes.Any(x => x == "REL" || x == "RRE" || x == "GRI" || x == "SOE");

                        var hasMediumRisk = row.searchTypes.Any(x =>
                            x == "INS" || x == "DD");

                        if (hasKYC6 && hasHighRisk)
                        {
                            model1.RedFlags = "Yes";
                        }else if (hasKYC6 && hasMediumRisk)
                        {
                            model1.RedFlags = "Might Be";
                        }
                        



                        if ((hasOFAC || hasKYC6) && row.searchTypes.Contains("Other Sanctions"))
                        {
                            model1.SanctionMatch = "Yes";
                        }

                        if ((hasOFAC || hasKYC6) &&
    row.searchTypes.Any(x =>
        x.Trim().Equals("UAE Sanction", StringComparison.OrdinalIgnoreCase) || x.Trim().Equals("UAE Sanctions", StringComparison.OrdinalIgnoreCase) ||
        x.Trim().Equals("UN Sanction", StringComparison.OrdinalIgnoreCase)|| x.Trim().Equals("UN Sanctions", StringComparison.OrdinalIgnoreCase)))
                        {
                            model1.UAEORUNSC = "Yes";
                        }


                        if (hasKYC6 && row.searchTypes.Contains("VHNWI"))
                        {
                            model1.HighNetworkIndividual = "Yes";
                        }
                        
                            
                        if (row.riskAssessments != null && row.riskAssessments.Any())
                        {
                            foreach (var risk in row.riskAssessments)
                            {
                                if (string.IsNullOrWhiteSpace(risk.RiskTypeText) ||
                                    string.IsNullOrWhiteSpace(risk.ItemText))
                                    continue;

                                switch (risk.RiskTypeText?.Trim().ToLower())
                                {
                                    case "profession":
                                        model1.OccupatinTypeTxt = risk.ItemText;
                                        break;
                                    case "residence status":
                                        model1.ResidenceStatus = risk.ItemText;
                                        break;
                                    case "nationality":
                                        model1.Nationality = risk.ItemText;
                                        break;
                                    case "Second Nationality (if applicable)":
                                        model1.Nationality = risk.ItemText;
                                        break;
                                    case "product, service & activity":
                                        model1.ProductName = risk.ItemText;
                                        break;
                                    case "if more than one product(put the riskiest product)":
                                        model1.HighestRiskProduct = risk.ItemText;
                                        break;
                                    case "delivery channel":
                                        model1.DeliveryChannelName = risk.ItemText;
                                        break;

                                    case "mode of payment":
                                        model1.Modeofpayment = risk.ItemText;
                                        break;
                                    case "dual use goods match":
                                        model1.DualUseGoods = risk.ItemText;
                                    break;
                                    case "if more dual use goods match":
                                        model1.MoreDualUseGoods = risk.ItemText;
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {

                        rowtype = row.matchcategory;
                        if (row.searchTypes == null || !row.searchTypes.Any())
                            continue;

                        bool hasNone = row.searchTypes.Contains("None");

                        // If only None selected → treat as no selection
                        if (hasNone && row.searchTypes.Count == 1)
                            continue;

                        // Remove None if mixed with others
                        if (hasNone)
                        {
                            row.searchTypes = row.searchTypes
                                .Where(x => x != "None")
                                .ToList();
                        }

                        if (!string.IsNullOrWhiteSpace(row.matchtype))
                        {
                            var type1 = row.matchtype.ToUpper();

                            var sanctionTypes = new List<string> { "UN", "OFAC", "UAE IEC LIST", "BL", "CBWL", "INTERNAL" };
                            if (sanctionTypes.Any(t => type1.Contains(t)))

                                hasOFAC = true;   // You can rename this to hasSanction if needed


                            if (type1.Contains("KYC6"))
                                hasKYC6 = true;
                        }

                        if (hasKYC6 && row.searchTypes.Contains("Domestic PEP"))
                        {
                            corporateDetailsModel.Domesticpep = "Yes";
                        }
                        

                        if (hasKYC6 && row.searchTypes.Contains("Foreign PEP"))
                        {
                            corporateDetailsModel.ForeignPep = "Yes";
                        }
                       
                        var hasHighRisk = row.searchTypes.Any(x => x == "REL" || x == "RRE" || x == "GRI" || x == "SOE");

                        var hasMediumRisk = row.searchTypes.Any(x =>
                            x == "INS" || x == "DD");

                        if (hasKYC6 && hasHighRisk)
                        {
                            corporateDetailsModel.RedFlags = "Yes";
                        }
                        else if(hasKYC6 && hasMediumRisk) { 

                        
                            corporateDetailsModel.RedFlags = "Might Be";
                        }
                        

                        if ((hasOFAC || hasKYC6) && row.searchTypes.Contains("Other Sanctions"))
                        {
                            corporateDetailsModel.SanctionMatch = "Yes";
                        }
                        

                        if ((hasOFAC || hasKYC6) &&
                            row.searchTypes.Any(x => x == "UAE Sanction" || x == "UN Sanction"))
                        {
                            corporateDetailsModel.UAEORUNSC = "Yes";
                        }
                        

                        if (hasKYC6 && row.searchTypes.Contains("VHNWI"))
                        {
                            corporateDetailsModel.HighNetworkIndividual = "Yes";
                        }
                        

                        

                        if (row.riskAssessments != null && row.riskAssessments.Any())
                        {
                            foreach (var risk in row.riskAssessments)
                            {
                                if (string.IsNullOrWhiteSpace(risk.RiskTypeText) ||
                                    string.IsNullOrWhiteSpace(risk.ItemText))
                                    continue;

                                switch (risk.RiskTypeText?.Trim().ToLower())
                                {
                                    case "legal status of the entity":
                                        corporateDetailsModel.EntityTypeTxt = risk.ItemText;
                                        break;

                                    case "nature of business":
                                        corporateDetailsModel.BusinessType = risk.ItemText;
                                        break;

                                    case "country of incorporation":
                                        corporateDetailsModel.Nationality = risk.ItemText;
                                        break;

                                    case "does the company have any subsidiary, affiliate, branch or group/holding company in fatf listed high risk monitored jurisdiction?":
                                        corporateDetailsModel.FATF = risk.ItemText;
                                        break;

                                    case "product":
                                        corporateDetailsModel.ProductName = risk.ItemText;
                                        break;

                                    case "if more than one product(put the riskiest product)":
                                        corporateDetailsModel.HighestRiskProduct = risk.ItemText;
                                        break;

                                    case "delivery channel":
                                        corporateDetailsModel.DeliveryChannelName = risk.ItemText;
                                        break;

                                    case "mode of payment":
                                        corporateDetailsModel.Modeofpayment = risk.ItemText;
                                        break;
                                    case "dual use goods match":
                                        corporateDetailsModel.DualUseGoods = risk.ItemText;
                                        break;
                                    case "if more dual use goods match":
                                        corporateDetailsModel.MoreDualUseGoods = risk.ItemText;
                                        break;

                                    default:
                                        // Optional: log unmatched value
                                        break;
                                }
                            }
                        }
                    }
                }



                if (rowtype == "INDIVIDUAL")
                {

                    //var results = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientid));
                    //if (results != null)
                    //{
                        var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(model1), corpModel, "I", culture, clientid);
                        if (str1.Result == null)
                        {
                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

                        }
                        var spStr1 = str1.Result.Split('Ø');
                        var proflovId = spStr1[0];
                        var natlovId = spStr1[1];
                        var reslovId = spStr1[5];
                        //var IspeplovId = spStr1[13];
                        var IndprodlovId = spStr1[13];
                        var InddelilovId = spStr1[14];
                        var IndmodeofpaymentlovId = spStr1[15];
                        var domesticpeplovId = spStr1[17];
                        var foreignlovId = spStr1[19];
                        var redflagslovId = spStr1[21];
                        var sanctionlovId = spStr1[23];
                        var UAEORUNSClovId = spStr1[25];
                        var highestriskproductlovId = spStr1[28];
                        var veryhighnetworkIdlovId = spStr1[30];
                    var dualusegoodslovId = spStr1[33];
                    var MoredualusegoodslovId = spStr1[35];

                    var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(model1), corpModel, "I", culture, clientid);
                        if (str.Result == null)
                        {
                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

                        }
                        var spStr = str.Result.Split('Ø');
                        var profId = spStr[0];
                        var natId = spStr[1];
                        var resId = spStr[5];
                        //var IspepId = spStr[13];
                        var IndprodId = spStr[13];
                        var InddeliId = spStr[14];
                        var Indmodeofpaymentid = spStr[15];
                        var domesticpepId = spStr[17];
                        var foreignId = spStr[19];
                        var redflagsId = spStr[21];
                        var sanctionId = spStr[23];
                        var UAEORUNSCId = spStr[25];
                        var highestriskproductId = spStr[28];
                        var veryhighnetworkId = spStr[30];
                    var dualusegoodsId = spStr[33];
                    var MoredualusegoodsId = spStr[35];

                    RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                        riskModel.CustomerId = customerid;
                        riskModel.CustomerName = _CustomerCaseDTO.FirstName;


                        riskModel.ClientId = _clientHandler.GetClientId();
                        riskModel.CreatedBy = _clientHandler.GetUserId();
                        if (_CustomerCaseDTO.Nationality == "0")
                        {
                            riskModel.MainNationality = "";
                        }
                        else
                        {
                            riskModel.MainNationality = _CustomerCaseDTO.Nationality;
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
                        if (domesticpepId != "0")
                        {
                            var riskType1 = new RiskTypeListModel();
                            riskType1.Id = Convert.ToString(domesticpeplovId);
                            var riskItem1 = new RiskItemListModel();
                            riskItem1.Id = domesticpepId.ToString();
                            var riskItemList1 = new List<RiskItemListModel>();
                            riskItemList1.Add(riskItem1);
                            riskType1.RiskItemList = riskItemList1;
                            riskTypeList.Add(riskType1);
                        }
                        //for pep end
                        //for foreignId start
                        if (foreignId != "0")
                        {
                            var riskType2 = new RiskTypeListModel();
                            riskType2.Id = Convert.ToString(foreignlovId);
                            var riskItem2 = new RiskItemListModel();
                            riskItem2.Id = foreignId.ToString();
                            var riskItemList2 = new List<RiskItemListModel>();
                            riskItemList2.Add(riskItem2);
                            riskType2.RiskItemList = riskItemList2;
                            riskTypeList.Add(riskType2);
                        }
                        //for foreign pep end
                        //for redflags start
                        if (redflagsId != "0")
                        {
                            var riskType9 = new RiskTypeListModel();
                            riskType9.Id = Convert.ToString(redflagslovId);
                            var riskItem9 = new RiskItemListModel();
                            riskItem9.Id = redflagsId.ToString();
                            var riskItemList9 = new List<RiskItemListModel>();
                            riskItemList9.Add(riskItem9);
                            riskType9.RiskItemList = riskItemList9;
                            riskTypeList.Add(riskType9);
                        }
                        //for red flags end
                        //for very high metwork Individual start
                        if (veryhighnetworkId != "0")
                        {
                            var riskType10 = new RiskTypeListModel();
                            riskType10.Id = Convert.ToString(veryhighnetworkIdlovId);
                            var riskItem10 = new RiskItemListModel();
                            riskItem10.Id = veryhighnetworkId.ToString();
                            var riskItemList10 = new List<RiskItemListModel>();
                            riskItemList10.Add(riskItem10);
                            riskType10.RiskItemList = riskItemList10;
                            riskTypeList.Add(riskType10);
                        }
                        //for very high metwork Individual end
                        //for other sanction start
                        if (sanctionId != "0")
                        {
                            var riskType11 = new RiskTypeListModel();
                            riskType11.Id = Convert.ToString(sanctionlovId);
                            var riskItem11 = new RiskItemListModel();
                            riskItem11.Id = sanctionId.ToString();
                            var riskItemList11 = new List<RiskItemListModel>();
                            riskItemList11.Add(riskItem11);
                            riskType11.RiskItemList = riskItemList11;
                            riskTypeList.Add(riskType11);
                        }
                        //for other sanction end
                        //for UaeUnsc start
                        if (UAEORUNSCId != "0")
                        {
                            var riskType12 = new RiskTypeListModel();
                            riskType12.Id = Convert.ToString(UAEORUNSClovId);
                            var riskItem12 = new RiskItemListModel();
                            riskItem12.Id = UAEORUNSCId.ToString();
                            var riskItemList12 = new List<RiskItemListModel>();
                            riskItemList12.Add(riskItem12);
                            riskType12.RiskItemList = riskItemList12;
                            riskTypeList.Add(riskType12);
                        }
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
                    if (dualusegoodsId != "0")
                    {
                        var riskType13 = new RiskTypeListModel();
                        riskType13.Id = Convert.ToString(dualusegoodslovId);
                        var riskItem13 = new RiskItemListModel();
                        riskItem13.Id = dualusegoodsId.ToString();//Convert.ToString(1);
                        var riskItemList13 = new List<RiskItemListModel>();
                        riskItemList13.Add(riskItem13);
                        riskType13.RiskItemList = riskItemList13;
                        riskTypeList.Add(riskType13);
                    }
                    if (MoredualusegoodsId != "0")
                    {
                        var riskType14 = new RiskTypeListModel();
                        riskType14.Id = Convert.ToString(MoredualusegoodslovId);
                        var riskItem14 = new RiskItemListModel();
                        riskItem14.Id = MoredualusegoodsId.ToString();//Convert.ToString(1);
                        var riskItemList14 = new List<RiskItemListModel>();
                        riskItemList14.Add(riskItem14);
                        riskType14.RiskItemList = riskItemList14;
                        riskTypeList.Add(riskType14);
                    }


                    riskModel.RiskTypeList = riskTypeList;
                        var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                        var xyz = riskResult;


                    //}

                }
                else
                {

                    //model.IsPeP = isPep;
                    var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(corporateDetailsModel), "C", culture, clientid);
                    if (str1.Result == null)
                    {


                        _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

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
                    var domesticpeplovId = spStr1[18];
                    var foreignlovId = spStr1[20];
                    var redflagslovId = spStr1[22];
                    var sanctionlovId = spStr1[24];
                    var UAEORUNSClovId = spStr1[26];
                    var corpfaftlovId = spStr1[27];
                    var highestriskproductlovId = spStr1[29];
                    var veryhighnetworkIdlovId = spStr1[31];
                    var dualusegoodslovId = spStr1[32];
                    var MoredualusegoodslovId = spStr1[34];
                    // var IsPeplovId = spStr1[14];

                    var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(corporateDetailsModel), "C", culture, clientid);

                    if (str.Result == null)
                    {
                        _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

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
                    var domesticpepId = spStr[18];
                    var foreignId = spStr[20];
                    var redflagsId = spStr[22];
                    var sanctionId = spStr[24];
                    var UAEORUNSCId = spStr[26];
                    var corpfaftId = spStr[27];
                    var highestriskproductId = spStr[29];
                    var veryhighnetworkId = spStr[31];
                    var dualusegoodsId = spStr[32];
                    var MoredualusegoodsId = spStr[34];

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
                    riskModel.CustomerId = customerid;
                    riskModel.CustomerName = _CustomerCaseDTO.FirstName;
                    riskModel.MainNationality = _CustomerCaseDTO.Nationality;
                    riskModel.ClientId = _clientHandler.GetClientId();
                    riskModel.CreatedBy = _clientHandler.GetUserId();

                    if (_CustomerCaseDTO.Nationality == "0")
                    {
                        riskModel.MainNationality = "";
                    }
                    else
                    {
                        riskModel.MainNationality = _CustomerCaseDTO.Nationality;
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
                        if (corpfaftId != "0")
                        {
                            var riskType19 = new RiskTypeListModel();
                            riskType19.Id = Convert.ToString(corpfaftlovId);
                            var riskItem19 = new RiskItemListModel();
                            riskItem19.Id = corpfaftId.ToString();//Convert.ToString(1);
                            var riskItemList19 = new List<RiskItemListModel>();
                            riskItemList19.Add(riskItem19);
                            riskType19.RiskItemList = riskItemList19;
                            riskTypeList.Add(riskType19);
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
                        //for pep start
                        if (domesticpepId != "0")
                        {
                            var riskType13 = new RiskTypeListModel();
                            riskType13.Id = Convert.ToString(domesticpeplovId);
                            var riskItem13 = new RiskItemListModel();
                            riskItem13.Id = domesticpepId.ToString();
                            var riskItemList13 = new List<RiskItemListModel>();
                            riskItemList13.Add(riskItem13);
                            riskType13.RiskItemList = riskItemList13;
                            riskTypeList.Add(riskType13);
                        }
                        //for pep end
                        //for foreignId start
                        if (foreignId != "0")
                        {
                            var riskType14 = new RiskTypeListModel();
                            riskType14.Id = Convert.ToString(foreignlovId);
                            var riskItem14 = new RiskItemListModel();
                            riskItem14.Id = foreignId.ToString();
                            var riskItemList14 = new List<RiskItemListModel>();
                            riskItemList14.Add(riskItem14);
                            riskType14.RiskItemList = riskItemList14;
                            riskTypeList.Add(riskType14);
                        }
                        //for foreign pep end
                        //for redflags start
                        if (redflagsId != "0")
                        {
                            var riskType15 = new RiskTypeListModel();
                            riskType15.Id = Convert.ToString(redflagslovId);
                            var riskItem15 = new RiskItemListModel();
                            riskItem15.Id = redflagsId.ToString();
                            var riskItemList15 = new List<RiskItemListModel>();
                            riskItemList15.Add(riskItem15);
                            riskType15.RiskItemList = riskItemList15;
                            riskTypeList.Add(riskType15);
                        }
                        //for red flags end
                        //for very high metwork Individual start
                        if (veryhighnetworkId != "0")
                        {
                            var riskType16 = new RiskTypeListModel();
                            riskType16.Id = Convert.ToString(veryhighnetworkIdlovId);
                            var riskItem16 = new RiskItemListModel();
                            riskItem16.Id = veryhighnetworkId.ToString();
                            var riskItemList16 = new List<RiskItemListModel>();
                            riskItemList16.Add(riskItem16);
                            riskType16.RiskItemList = riskItemList16;
                            riskTypeList.Add(riskType16);
                        }
                        //for very high metwork Individual end
                        //for other sanction start
                        if (sanctionId != "0")
                        {
                            var riskType17 = new RiskTypeListModel();
                            riskType17.Id = Convert.ToString(sanctionlovId);
                            var riskItem17 = new RiskItemListModel();
                            riskItem17.Id = sanctionId.ToString();
                            var riskItemList17 = new List<RiskItemListModel>();
                            riskItemList17.Add(riskItem17);
                            riskType17.RiskItemList = riskItemList17;
                            riskTypeList.Add(riskType17);
                        }
                        //for other sanction end
                        //for UaeUnsc start
                        if (UAEORUNSCId != "0")
                        {
                            var riskType18 = new RiskTypeListModel();
                            riskType18.Id = Convert.ToString(UAEORUNSClovId);
                            var riskItem18 = new RiskItemListModel();
                            riskItem18.Id = UAEORUNSCId.ToString();
                            var riskItemList18 = new List<RiskItemListModel>();
                            riskItemList18.Add(riskItem18);
                            riskType18.RiskItemList = riskItemList18;
                            riskTypeList.Add(riskType18);
                        }

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
                        if (dualusegoodsId != "0")
                        {
                            var riskType13 = new RiskTypeListModel();
                            riskType13.Id = Convert.ToString(dualusegoodslovId);
                            var riskItem13 = new RiskItemListModel();
                            riskItem13.Id = dualusegoodsId.ToString();//Convert.ToString(1);
                            var riskItemList13 = new List<RiskItemListModel>();
                            riskItemList13.Add(riskItem13);
                            riskType13.RiskItemList = riskItemList13;
                            riskTypeList.Add(riskType13);
                        }

                        if (MoredualusegoodsId != "0")
                        {
                            var riskType14 = new RiskTypeListModel();
                            riskType14.Id = Convert.ToString(MoredualusegoodslovId);
                            var riskItem14 = new RiskItemListModel();
                            riskItem14.Id = MoredualusegoodsId.ToString();//Convert.ToString(1);
                            var riskItemList14 = new List<RiskItemListModel>();
                            riskItemList14.Add(riskItem14);
                            riskType14.RiskItemList = riskItemList14;
                            riskTypeList.Add(riskType14);
                        }

                        riskModel.RiskTypeList = riskTypeList;
                        var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                        var xyz = riskResult;
                        Console.WriteLine($"generated risk for customer: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");
                        Console.WriteLine($"Finished generating risk for customer: {JsonConvert.SerializeObject(riskResult, Formatting.Indented)}");
                    }

                }
                //// Example: High Risk logic




                ////To check if risk assessment is enabled for the client.


            }
                return Json(response);
        }

        [HttpGet]
        [Route("/case/GetDetails")]
        public async Task<ActionResult> GetDetails(int caseId, int index,string type )
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            model.Case.Id = caseId;
            model.Index = index;

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(caseId);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
            var returnUrl="";
            if (type == "shareholder")
            {
                 returnUrl = HttpContext.Session.GetString("ShareholderReturnUrl");
            }
            else
            {
                returnUrl = HttpContext.Session.GetString("ReturnUrl");
            }


                ViewBag.ReturnUrl = returnUrl;
            
            
            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(caseId);
            model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

            var clientId = _clientHandler.GetClientId();
            var ClientId = _clientHandler.GetUserId();

            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != ClientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");

            var result = _clientHandler.PostAsync(new { caseid = caseId.ToString() }, ScreeningService.GETBYCASEID).Result;
            if (!string.IsNullOrEmpty(result))
            {
                List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                model.DataList = jsonList;
            }

            string datas = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.Case.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = string.Concat(model.Case.FirstName, " ", model.Case.MiddleName, " ", model.Case.LastName),
                customernationality = model.Case.Nationality,
                searchtype = "F"
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(datas))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(datas);
                model.apiResultModels = apiResultModel;
            }

            return View(model);

        }

        public JsonResult Checkduplicatenames(string fullname,string type)
        {
            var clientId = _clientHandler.GetClientId();

            List<CaseModel> abc = new List<CaseModel>();

            abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetDuplicateNames(fullname,type, clientId));

            


            return Json(abc);
        }

        [HttpGet("/CompletedCaseIndex")]
        public ActionResult CompletedCaseIndex()
        {

            ReportLogSearchModel model = new ReportLogSearchModel();
            var clientId = _clientHandler.GetClientId();
            model.StartDate = System.DateTime.Now.AddYears(-1);
            //model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            //model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.CustomerCategories = new SelectList(
   _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
       .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
       .ToList(),
   "Code",
   "Name"
);
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            //var items = from CompletedCaseStatus d in Enum.GetValues(typeof(CompletedCaseStatus))
            //            select new { Id = (int)d, Name = d.ToString() };
            //model.CaseStatusList = new SelectList(items, "Id", "Name");
            var items = from CompletedCaseStatus d in Enum.GetValues(typeof(CompletedCaseStatus))
                        select new
                        {
                            Id = (int)d,
                            Name = Regex.Replace(d.ToString(), "(\\B[A-Z])", " $1")
                        };

            model.CaseStatusList = new SelectList(items, "Id", "Name");


            return View(model);
        }

        //public static string GetEnumDisplayName(Enum value)
        //{
        //    return value.GetType()
        //        .GetMember(value.ToString())
        //        .First()
        //        .GetCustomAttribute<DisplayAttribute>()?
        //        .GetName() ?? value.ToString();
        //}

        [HttpPost("/case/completedcasescustompagination")]
        //ToDo
        public JsonResult completedcasescustompagination(DataTableModel model, string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string caseStatusChange, string riskLevel)
        {
            var userId = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            List<CaseModel> abc = new List<CaseModel>();
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if (cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            if (riskLevel == "1")
            {
                riskLevel = "Low Risk";
            }
            else if (riskLevel == "2")
            {
                riskLevel = "Medium Risk";
            }
            else if (riskLevel == "3")
            {
                riskLevel = "High Risk";
            }
            if (caseStatusChange == "0")
            {
                caseStatusChange = null;
            }
            if (searchValue != "" && searchValue != null)
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, clientId));

            }
            else
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, clientId));

            }
            int totalcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                var words = model.search.value.Trim().Split(' ');
                foreach (var item in words)
                {
                    abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
                || m.FirstName.ToLower().Contains(item.ToLower())
                || m.LastName.ToLower().Contains(item.ToLower())
                || m.MiddleName.ToLower().Contains(item.ToLower())
                || m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
                ).ToList();
                }
                //abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
                //|| m.FirstName.ToLower().Contains(model.search.value.ToLower())
                //|| m.LastName.ToLower().Contains(model.search.value.ToLower())
                //|| m.MiddleName.ToLower().Contains(model.search.value.ToLower()) 
                //|| m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
                //).ToList();
            }
            int filteredcount = abc.Count;
            //var data = abc.Skip(model.start).Take(model.length).ToList();

            var sortColumn = model?.order?.Any() == true
    ? model.columns[model.order[0].column].data ?? "updatedOn"
    : "updatedOn";

            var sortDir = model?.order?.Any() == true
                ? model.order[0].dir ?? "desc"
                : "desc";

            var data = SortData(abc, sortColumn, sortDir)
                        .Skip(model.start)
                        .Take(model.length)
                        .ToList();

            //     var data = Sort(abc, model.columns[model.order[0].column].data ?? "createdOn", model.order[0].dir ?? "desc")

            //.Skip(model.start)

            //.Take(model.length)

            //.ToList();

            //      var data = Sort(abc, model.columns[model.order[0].column].data ?? "createdOnDB", model.order[0].dir ?? "dec")

            //.Skip(model.start)

            //.Take(model.length)

            //.ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data,
            });
            return response;
        }

        public ActionResult ViewIndividualCaseDetail(string id, int type)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            int Id = _customerCaseService.GetCaseId(id);//returns id of the customercase table instead of caseid
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
            var appBaseUrl = MyHttpContext.AppBaseUrl;
            model.Url = appBaseUrl;
            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(Id);
            model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
            List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
            model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
            List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType));
            model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);

            //List<PassportDetailsDTO> passportDetails = _customerCaseService.GetPassportdetails(Id);
            //model.passportDetails = _mapper.Map<List<PassportDetails>>(passportDetails);
            //model.Case.Id = CaseId;
            //model.DocumentCategories = new SelectList(_mapper.Map<List<DocumentCategoryModel>>(_caseDocumentService.GetAllCategories()), "Id", "Name");
            //model.DocumentTypes = new SelectList(_mapper.Map<List<DocumentTypeModel>>(_caseDocumentService.GetAllTypes()), "Id", "Name");
            var clientId = _clientHandler.GetClientId();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");

            if (type == 1)
            {
                var result = _clientHandler.PostAsync(new { caseid = Id.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
            }
            else if (type == 2)
            {
                var result = _freeSourceRepository.GetDetailsByCaseId(Id);
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
            }
            else if (type == 3)
            {
                var result = _freeSourceRepository.GetPendingDetailsByCaseId(Id);
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
            }


            if (model.Case == null)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = "With this id there is no data";
                error.module = "ViewIndividualcaseDetails_R";
                error.comments = "Customer id was not found or not there";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                return RedirectToAction("PageNotFound", "Error");


            }

            return View(model);


        }

        //[HttpPost]
        //public async Task<IActionResult> UploadMRZ(List<IFormFile> file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //            return Json(new { success = false, message = "No file uploaded" });

        //        string base64String;

        //        using (var ms = new MemoryStream())
        //        {
        //            await file.CopyToAsync(ms);
        //            base64String = Convert.ToBase64String(ms.ToArray());
        //        }

        //        using (var client = new HttpClient())
        //        {
        //            using (var formData = new MultipartFormDataContent())
        //            {
        //                var streamContent = new StreamContent(file.OpenReadStream());
        //                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        //                formData.Add(streamContent, "file", file.FileName);

        //                string apiUrl = $"https://astrid-unpavilioned-pearlene.ngrok-free.dev/process_document";

        //                var response = await client.PostAsync(apiUrl, formData);

        //                var result = await response.Content.ReadAsStringAsync();

        //                return Content(result, "application/json");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }
        //}

        [HttpPost]
        //public async Task<IActionResult> UploadMRZ(List<IFormFile> files)
        //{
        //    try
        //    {
        //        if (files == null || files.Count == 0)
        //            return Json(new { success = false, message = "No file uploaded" });

        //        using (var client = new HttpClient()) {
        //            client.Timeout = TimeSpan.FromMinutes(20);
        //            using (var formData = new MultipartFormDataContent())
        //            {
        //                foreach (var file in files)
        //                {
        //                    if (file.Length > 0)
        //                    {
        //                        var streamContent = new StreamContent(file.OpenReadStream());
        //                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        //                        // "files" should match API parameter name
        //                        formData.Add(streamContent, "files", file.FileName);
        //                    }
        //                }

        //                string apiUrl = "https://astrid-unpavilioned-pearlene.ngrok-free.dev/process_document";

        //                var response = await client.PostAsync(apiUrl, formData);

        //                var result = await response.Content.ReadAsStringAsync();

        //                return Content(result, "application/json");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }
        //}
        public async Task<IActionResult> UploadMRZ(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return Json(new { success = false, message = "No file uploaded" });

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(20);

                using var formData = new MultipartFormDataContent();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType =
                            new MediaTypeHeaderValue(file.ContentType);

                        formData.Add(streamContent, "file", file.FileName);
                    }
                }

                string apiUrl =
                "https://astrid-unpavilioned-pearlene.ngrok-free.dev/process_document";

                var response = await client.PostAsync(apiUrl, formData);

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "External API error",
                        status = response.StatusCode
                    });
                }

                var result = await response.Content.ReadAsStringAsync();

                return Content(result, "application/json");
            }
            catch (TaskCanceledException)
            {
                return Json(new
                {
                    success = false,
                    message = "Request timed out while processing document"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetCountryNameByIso(string isoCode)
        {
            var clientId = _clientHandler.GetClientId();
            var country = _countryService.GetCountryNameByCode(isoCode, clientId);

            return Json(country?.Name);
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadMRZ(IFormFile file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //            return BadRequest(new { success = false, message = "No file uploaded" });

        //        var apiKey = "b967cd9398544f0325322c3ad88321f6a2f5227dabaf2ce79b68fce81cfa6861";
        //        var apiUrl = $"https://mrzbk.7iris.ae/extract-mrz?key={apiKey}";

        //        using var client = new HttpClient();
        //        using var content = new MultipartFormDataContent();

        //        using var stream = file.OpenReadStream();
        //        content.Add(new StreamContent(stream), "file", file.FileName);

        //        var response = await client.PostAsync(apiUrl, content);

        //        if (!response.IsSuccessStatusCode)
        //            return BadRequest(new { success = false, message = "MRZ API Failed" });

        //        var result = await response.Content.ReadAsStringAsync();

        //        return Json(new { success = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}

        

        

        
        

    [HttpGet]
    public async Task<IActionResult> DueDiligence_PDF(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string selectedColumns = null, string orientation = "portrait")
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();
        var GroupId = _clientHandler.GetGroupId();
        var _userGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString();
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId));
        }

        ViewBag.SelectedColumns = selectedColumns;
        ViewBag.Orientation = orientation;
        return View("DueDiligence_PDF", cases);
    }

    [HttpGet]
    public async Task<IActionResult> CompletedCases_PDF(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string selectedColumns = null, string orientation = "portrait")
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString();
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, clientId));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, clientId));
        }

        ViewBag.SelectedColumns = selectedColumns;
        ViewBag.Orientation = orientation;
        return View("CompletedCases_PDF", cases);
    }
    [HttpGet]
    public async Task<IActionResult> ExportDueDiligenceExcel(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string selectedColumns = null)
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();
        var GroupId = _clientHandler.GetGroupId();
        var _userGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString("yyyy-MM-dd");
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId));
        }

        var columnMap = new Dictionary<string, (string Header, Func<CaseModel, object> Value)>
        {
            { "CustomerId", ("Customer ID", c => c.CustomerId) },
            { "CreatedOn", ("Created On", c => c.CreatedOn.ToString("dd MMM yyyy HH:mm:ss")) },
            { "UpdatedOnDB", ("Updated On", c => c.UpdatedOn) },
            { "CustomerType", ("Customer Type", c => c.CustomerType == "I" ? "Individual" : "Corporate") },
            { "CustomerName", ("Customer Name", c => (c.FirstName + " " + c.LastName).Trim()) },
            { "CaseChangeStatus", ("Datasets", c => c.CaseChangeStatus) },
            { "MatchScore", ("Screening Score", c => c.MatchScore) },
            { "riskScore", ("Risk Rating", c => c.Individual_final_risk_score ?? c.corporate_final_risk_score ?? "Low Risk") },
            { "CreatedUser", ("User", c => c.CreatedUser) },
            { "CaseStatus", ("Status", c => FormatExcelStatus(c.CaseStatus)) }
        };

        var selectedCols = string.IsNullOrEmpty(selectedColumns) 
            ? columnMap.Keys.ToList() 
            : selectedColumns.Split(',').ToList();

        using (var package = new ExcelPackage())
        {
            var sheet = package.Workbook.Worksheets.Add("Due Diligence Report");
            int colIndex = 1;
            foreach (var colId in selectedCols)
            {
                if (columnMap.ContainsKey(colId))
                {
                    sheet.Cells[1, colIndex].Value = columnMap[colId].Header;
                    colIndex++;
                }
            }

            using (var range = sheet.Cells[1, 1, 1, Math.Max(1, colIndex - 1)])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0xE9, 0xEF, 0xFD));
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var item in cases)
            {
                colIndex = 1;
                foreach (var colId in selectedCols)
                {
                    if (columnMap.ContainsKey(colId))
                    {
                        sheet.Cells[row, colIndex].Value = columnMap[colId].Value(item);
                        colIndex++;
                    }
                }
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DueDiligence_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportCompletedCasesExcel(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string selectedColumns = null)
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString("yyyy-MM-dd");
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, clientId));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, clientId));
        }

        var columnMap = new Dictionary<string, (string Header, Func<CaseModel, object> Value)>
        {
            { "CustomerId", ("Customer ID", c => c.CustomerId) },
            { "CreatedOn", ("Created On", c => c.CreatedOn.ToString("dd MMM yyyy HH:mm:ss")) },
            { "UpdatedOnDB", ("Updated On", c => c.UpdatedOn) },
            { "CustomerType", ("Customer Type", c => c.CustomerType == "I" ? "Individual" : "Corporate") },
            { "CustomerName", ("Customer Name", c => (c.FirstName + " " + c.LastName).Trim()) },
            { "CaseChangeStatus", ("Datasets", c => c.CaseChangeStatus) },
            { "MatchScore", ("Screening Score", c => c.MatchScore) },
            { "riskScore", ("Risk Rating", c => c.Individual_final_risk_score ?? c.corporate_final_risk_score ?? "Low Risk") },
            { "CreatedUser", ("User", c => c.CreatedUser) },
            { "CaseStatus", ("Status", c => FormatExcelStatus(c.CaseStatus)) }
        };

        var selectedCols = string.IsNullOrEmpty(selectedColumns) 
            ? columnMap.Keys.ToList() 
            : selectedColumns.Split(',').ToList();

        using (var package = new ExcelPackage())
        {
            var sheet = package.Workbook.Worksheets.Add("Completed Cases Report");
            int colIndex = 1;
            foreach (var colId in selectedCols)
            {
                if (columnMap.ContainsKey(colId))
                {
                    sheet.Cells[1, colIndex].Value = columnMap[colId].Header;
                    colIndex++;
                }
            }

            using (var range = sheet.Cells[1, 1, 1, Math.Max(1, colIndex - 1)])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0xE9, 0xEF, 0xFD));
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var item in cases)
            {
                colIndex = 1;
                foreach (var colId in selectedCols)
                {
                    if (columnMap.ContainsKey(colId))
                    {
                        sheet.Cells[row, colIndex].Value = columnMap[colId].Value(item);
                        colIndex++;
                    }
                }
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CompletedCases_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }

    private string FormatExcelStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return "";
        if (!status.Contains("| Shareholders:")) return status;

        var parts = status.Split('|');
        var mainStatus = parts[0].Trim();
        var shInfo = parts[1].Replace("Shareholders:", "").Trim();
        
        var metrics = new List<string>();
        var patterns = new Dictionary<string, string> { 
            { "Approved", "AP" }, { "Auto", "A" }, { "Pending", "P" }, 
            { "Rejected", "R" }, { "OnHold", "OH" }, { "Waitlist", "W" }, { "Whitelist", "W" } 
        };

        foreach (var p in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(shInfo, p.Key + @"\s+(\d+)");
            if (match.Success && int.Parse(match.Groups[1].Value) > 0)
            {
                metrics.Add($"{p.Value}:{match.Groups[1].Value}");
            }
        }

        return metrics.Count > 0 ? $"{mainStatus} ({string.Join(", ", metrics)})" : mainStatus;
    }
}
}
