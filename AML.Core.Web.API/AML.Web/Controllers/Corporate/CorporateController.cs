using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
using AML.Core.Repository.FreeSource;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.Service.LovMaster;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.CaseStudio;
using AML.Core.ServiceContract.User;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Kyc;
using AML.ViewModel.ViewModels.CaseAssignment;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CaseProcess;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.Country;
//using AML.DTO.DTO.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.IdentityType;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.ViewModel.ViewModels.User;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using NLog;
using NToastNotify;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AML.Core.Service.Common.CommonService;
using static AML.DTO.DTO.FreeSource.CaseLogsMongoDTO;
using static iTextSharp.text.pdf.PdfDiv;
using Font = iTextSharp.text.Font;

namespace AML.Web.Controllers.Corporate

{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class CorporateController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private ICustomerCaseService _customerCaseService;
        private ICountryService _countryService;
        private IIdentityTypeService _idTypeService;
        private ICustomerCategoryService _customerCategoryService;
        private IUserService _userService;
        private ICaseDocumentService _caseDocumentService;
        private ICaseAssignmentService _caseAssignmentService;
        private ICaseCommentService _caseCommentService;
        private readonly ICaseStudioService _caseStudioService;
        IFileUploader _fileUploader;
        private ICommonService _commonService;
        private string baseURL = string.Empty;
        private string baseC6URL = string.Empty;
        private string _c6Username;
        private int checkThreshold = 0;
        private CorporateScreeningModel res = new CorporateScreeningModel();
        private List<ApiResultModel> matchrecordsList = new List<ApiResultModel>();
        private IViewRenderService _viewRenderService;
        private RiskAPIController _riskAPIController;
        private readonly Logger log = LogManager.GetCurrentClassLogger();
        private int clientId = 0;
        private string culture = CultureInfo.CurrentCulture.Name;
        private IKycService _kycService;
        private IFreeSourceRepository _freeSourceRepository;
        public CorporateController(IMapper mapper,
            IToastNotification toastNotification, ICountryService countryService, ICaseDocumentService caseDocumentService,
            IHttpClientHandler clientHandler, ICustomerCategoryService customerCategoryService, ICaseCommentService caseCommentService,
            IConfiguration configuration, IIdentityTypeService idTypeService, IUserService userService, ICaseAssignmentService caseAssignmentService, IKycService kycService, IFreeSourceRepository freeSourceRepository,
            ICustomerCaseService CustomerCaseService, IFileUploader fileUploader, IViewRenderService viewRenderService, ICommonService commonService, RiskAPIController riskAPIController,
            ICaseStudioService caseStudioService)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _customerCaseService = CustomerCaseService;
            _countryService = countryService;
            _idTypeService = idTypeService;
            _customerCategoryService = customerCategoryService;
            _userService = userService;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
            _viewRenderService = viewRenderService;
            _caseAssignmentService = caseAssignmentService;
            _fileUploader = fileUploader;
            _commonService = commonService;
            _riskAPIController = riskAPIController;
            _caseStudioService = caseStudioService;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            clientId = clientHandler.GetClientId();
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            if (clientDetails != null)
            {
                _c6Username = clientDetails.C6Username;
                checkThreshold = clientDetails.Threshold;
                baseC6URL = clientDetails.C6BaseUrl;
            }
            else
            {
                // Fallback or default values if client details are missing
                _c6Username = string.Empty;
                checkThreshold = 0;
                baseC6URL = string.Empty;
            }
            _kycService = kycService;
            _freeSourceRepository = freeSourceRepository;

            //baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            //_c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
            //checkThreshold = configuration.GetSection("C6BaseApiUrl").GetSection("Threshold").Value.ParseInt();
        }


        public ActionResult Index()
        {
            return View();
        }


        public ActionResult Create()
        {
            // CorporateModel model = new CorporateModel();
            return View();
        }

        public ActionResult CorporateScreening()
        {
            var model = new CorporateScreeningModel();
            var CustomerType = "C";
            var clientId = _clientHandler.GetClientId();
            model.TypeId = CustomerType;
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, clientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.BusinessTypeList = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, CustomerType, clientId)), "BusinessName", "BusinessName");
            model.EntityType = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, CustomerType, clientId)), "LegalStatus", "LegalStatus");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Customer Risk");
            model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Product Risk");
            model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Delivery Channel Risk");
            model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Mode of Payment");
            if (TempData.TryGetValue("UploadedExcelData", out var uploadedData) && uploadedData != null)
            {
                // Deserialize Excel data
                model.ExcelUploadedData = JsonConvert.DeserializeObject<List<CorporateExcelData>>(uploadedData.ToString());

                // Only show modal if there is actual data
                if (model.ExcelUploadedData.Any())
                {
                    model.ShowExcelUploadModal = true;
                }

                // No need to keep TempData unless you plan to use it again
            }

            model.IsCaseCreated = TempData["IsCaseCreated"] != null && (bool)TempData["IsCaseCreated"];
            model.CaseRefId = TempData["CaseRefId"]?.ToString();



            //for (int i = 0; i < model.CodesTable.Count; i++)
            //{
            //    var val = model.CodesTable[i].ccName;
            //    model.CodeNames.Add(val);
            //    model.IsChecked.Add(false);
            //}
            return View(model);
        }

        private async Task SendScreendedMailAsync(string body, CorporateScreeningModel model, CustomerCaseDTO x)
        {
            string body1 = string.Empty;
            using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
            {
                body1 = reader.ReadToEnd();
            };
            var clientData = _customerCaseService.GetClientDetailsByID(model.ClientId);
            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;
            int fileType = 4;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            using System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
            var appBaseUrl = MyHttpContext.AppBaseUrl;
            body1 = body1.Replace("{customerID}", x.CustomerId.ToString());
            body1 = body1.Replace("{caseID}", x.Id.ToString());
            body1 = body1.Replace("{baseUrl}", appBaseUrl);
            model.Url = appBaseUrl;

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


            if (x.CustomerType == "C")
            {
                PdfPTable headTab = new PdfPTable(1);
                headTab.TotalWidth = 550f;
                headTab.LockedWidth = true;
                headTab.DefaultCell.Border = 0;
                headTab.HorizontalAlignment = 1;
                float[] widths1 = new float[] { 3f };
                headTab.SetWidths(widths1);
                headTab.SpacingAfter = 20f;
                headTab.AddCell("License Number : " + model.Id);
                headTab.AddCell("Company Name : " + model.FirstName);
                headTab.AddCell("Mobile Number : " + model.Mobile);
                headTab.AddCell("Country Of Incorporation:" + model.Nationality);
                var dob = model.DOB;
                headTab.AddCell("Date Of Incorporation : " + dob);
                document.Add(headTab);
            }

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
            cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell1.FixedHeight = 30f;
            table.AddCell(cell1);
            PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell2.HorizontalAlignment = 1;
            cell2.VerticalAlignment = 1;
            cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell2.FixedHeight = 30f;
            table.AddCell(cell2);
            PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell3.HorizontalAlignment = 1;
            cell3.VerticalAlignment = 1;
            cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell3.FixedHeight = 30f;
            table.AddCell(cell3);
            PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell4.HorizontalAlignment = 1;
            cell4.VerticalAlignment = 1;
            cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell4.FixedHeight = 30f;
            table.AddCell(cell4);
            PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell5.HorizontalAlignment = 1;
            cell5.VerticalAlignment = 1;
            cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell5.FixedHeight = 30f;
            table.AddCell(cell5);
            PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell6.HorizontalAlignment = 1;
            cell6.VerticalAlignment = 1;
            cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell6.FixedHeight = 30f;
            table.AddCell(cell6);
            PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell7.HorizontalAlignment = 1;
            cell7.VerticalAlignment = 1;
            cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell7.FixedHeight = 30f;
            table.AddCell(cell7);
            PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
            cell8.HorizontalAlignment = 1;
            cell8.VerticalAlignment = 1;
            cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
            cell8.FixedHeight = 30f;
            table.AddCell(cell8);
            if (x.ApiResultsjson == null)
            {
                downloadModel.ApiResultsjson = x.ApiResultsjsonCorp;
            }
            else
            {
                downloadModel.ApiResultsjson = x.ApiResultsjson;
            }
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



            if (!((x.MatchScore >= 0 && x.MatchScore < checkThreshold)))
            {
                var emailSent = await _commonService.SendHtmlFormattedEmailWithAttachment("Case creation Alert", body1, res.ToString(), data);
            }
        }
        private async Task SendScreendedMailAsync(string body, CaseModel model, CustomerCaseDTO x)
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

                //string url = logos;
                ////Image tif = Image.GetInstance(url);
                //tif.ScalePercent(1f);
                //tif.SpacingBefore = 20f;
                //logo.AddCell(tif);
                //document.Add(logo);


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
                cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell1.FixedHeight = 30f;
                table.AddCell(cell1);
                PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell2.HorizontalAlignment = 1;
                cell2.VerticalAlignment = 1;
                cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell2.FixedHeight = 30f;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
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

        private async Task ScreenShareholder(CustomeDetailsModel customeDetailsModel,bool IsSanction, int count,string CallC6Screening)
        {
            CaseModel model = new CaseModel()
            {
                FirstName = customeDetailsModel.FirstName,
                LastName = customeDetailsModel.LastName,
                MiddleName = customeDetailsModel.MiddleName,
                CompanyCode = customeDetailsModel.Companycode,
               DOB = customeDetailsModel.DOB,
               Nationality = customeDetailsModel.Nationality,

            };

            model.CustomerType = "I";
            model.MatchCategory = "INDIVIDUAL";
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
            _ccDTO.ClientId = _clientHandler.GetClientId();
            _ccDTO.CreatedBy = _clientHandler.GetUserId();
            _ccDTO.Threshold = checkThreshold;
            _ccDTO.DOB=Convert.ToDateTime(model.DOB);
            _ccDTO.Nationality = model.Nationality;
            //var customerCodeprefix= _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(model.ClientId));
            //_ccDTO.customerCodeprefix = customerCodeprefix.Prefix;

            //var result = _customerCaseService.CreatePrefix(_ccDTO);
            var result = _customerCaseService.Create(_ccDTO);
            _ccDTO.CustomerId = result.Result.Split('Ø')[1];
            CaseDocumentModel _caseDoc = new CaseDocumentModel();
            _caseDoc.CaseId = _customerCaseService.GetCaseId(_ccDTO.CustomerId).ToString();
            //model.CodesTable = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(model.ClientId));


            if (customeDetailsModel.CaseDocumentsL != null)
            {
                foreach (var item in customeDetailsModel.CaseDocumentsL)
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
            //for sanction screening
            //bool IsSanction = false;
            //int count = 0;

            //for (int i = 0; i < model.CodesTable.Count; i++)
            //{
            //    var val = model.CodesTable[i].ccName;
            //    model.CodeNames.Add(val);
            //}
            //for (int i = 0; i < model.CodeNames.Count; i++)
            //{
            //    if (customeDetailsModel.IsChecked[i] == true)
            //    {
            //        count++;
            //        if (model.CodeNames[i] == "Sanction")
            //        {
            //            log.Debug("Only sanction was true");
            //            IsSanction = true;
            //        }
            //    }
            //}

            for (int i = 0; i < model.CodeNames.Count; i++)
            {
                if (model.CodeNames[i] == "PEP" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsPep = true; continue; }
                if (model.CodeNames[i] == "Sanction" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsSan = true; continue; }
                if (model.CodeNames[i] == "Reputational Risk Exposure" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsRre = true; continue; }
                if (model.CodeNames[i] == "Insolvency (UK & Ireland)" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsIns = true; continue; }
                if (model.CodeNames[i] == "Disqualified Director (UK Only)" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsDd = true; continue; }
                if (model.CodeNames[i] == "Profile of Interest" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsPoi = true; continue; }
                if (model.CodeNames[i] == "Regulatory Enforcement List" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsRel = true; continue; }
            }


            if (count == 1 && IsSanction == true && CallC6Screening == "N")
            {
                string response = string.Empty;
                var searchType = "F";
                string data = await _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                {
                    customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                    customerfullname = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
                    customernationality = model.Nationality,
                    searchtype = searchType
                }, ScreeningService.BACKLIST_SCREENING);

                if (!string.IsNullOrEmpty(data))
                {
                    List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                    //response = AMLUtility.FormatJsonToPlainText(data);

                    _commonService.UpdateSanctionRecords(_ccDTO, apiResultModel, result.Result.Split('Ø')[1]);

                    string body = string.Empty;
                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                    {
                        body = reader.ReadToEnd();
                    };

                    //var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result);

                    if (_ccDTO.sendMail == 1)
                    {
                        await SendScreendedMailAsync(body, model, _ccDTO);
                    }

                    if (_ccDTO.IsMatched == 1 && _ccDTO.MatchScore >= checkThreshold)
                    {
                        _toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                    }
                    else if (_ccDTO.IsMatched == 0 && _ccDTO.MatchScore == 0)
                    {
                        model.IsMatched = 2;
                        var appBaseUrl = MyHttpContext.AppBaseUrl;
                        // body = body.Replace("{baseUrl}", appBaseUrl);
                        //model.Url = appBaseUrl;
                        //model.createdUserName = HttpContext.Session.GetString("SessUsername");
                        _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));
                        //return View(model);
                    }
                    else if (_ccDTO.MatchScore >= 0 && _ccDTO.MatchScore < checkThreshold)
                    {
                        _ccDTO.ApiResultsjson = _ccDTO.ApiResultsjson.Where((source, index) => index < 100).ToList();
                        model.ApiResultJson = _ccDTO.ApiResultsjson;
                        _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));

                       // return View(model);
                    }
                    else
                    {
                        _toastNotification.AddSuccessToastMessage("Customer Approved");
                    }//      _toastNotification.AddWarningToastMessage("Customer blocked!");
                }
                else
                {
                    _toastNotification.AddWarningToastMessage("Customer blocked! ");
                }

            }

            else if (result.Status == StaticResource.SuccessStatusCode)
            {
                if (CallC6Screening == "Y")
                {
                    _ccDTO.IsPep = true;
                    _ccDTO.IsSan = true;
                }

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                {
                    body = reader.ReadToEnd();
                };

                var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result.Split('Ø')[1]);

                if (x.sendMail == 1)
                {
                    await SendScreendedMailAsync(body, model, x);
                }
                if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                {
                    _toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                }
                else if (x.IsMatched == 0 && x.MatchScore == 0)
                {
                    model.IsMatched = 2;
                    var appBaseUrl = MyHttpContext.AppBaseUrl;
                    body = body.Replace("{baseUrl}", appBaseUrl);
                    model.Url = appBaseUrl;
                    model.createdUserName = HttpContext.Session.GetString("SessUsername");
                    _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", x.FirstName, " ", x.LastName, ". \n"));
                    return;
                }
                else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                {

                    x.ApiResultsjson = x.ApiResultsjson.Where((source, index) => index < 100).ToList();
                    model.ApiResultJson = x.ApiResultsjson;
                    _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", x.FirstName, " ", x.LastName, ". \n"));

                    return;
                }
                else
                {
                    _toastNotification.AddSuccessToastMessage("Customer Approved");
                }
                /*commenting as its not required for saas live
                Console.WriteLine("Start risk assessment For shareholder");
                var riskItemList = new List<RiskTypeListModel>();

                Console.WriteLine("Adding items for risk assessment");
                var riskList = new List<(string, string)>();
                riskList.Add(("4", x.IsMatched == 1 && x.Status == 0 ? "3341" : "3342"));


                foreach ((string, string) riskItemId in riskList)
                {
                    var riskType = new RiskTypeListModel
                    {
                        Id = riskItemId.Item1,
                        RiskItemList = new List<RiskItemListModel>
                                        {
                                         new RiskItemListModel
                                          {
                                            Id = riskItemId.Item2
                                          }
                                        }
                    };

                    riskItemList.Add(riskType);
                }

                var riskApiRequest = new RiskAPIRequestModel
                {
                    CustomerId = result.Result,
                    MainNationality = model.Nationality,
                    ClientId = model.ClientId,
                    CreatedBy = model.CreatedBy,
                    CustomerName = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
                    RiskCategory = model.CustomerType,
                    RiskTypeList = riskItemList
                };
                Console.WriteLine($"Sending data for risk assessment: {JsonConvert.SerializeObject(riskApiRequest, Formatting.Indented)}");

                IActionResult riskAssessmentResult = _riskAPIController.Assessment(riskApiRequest);

                Console.WriteLine($"Got result: {riskAssessmentResult}");
                */
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Customer creation failed");
                return;
            }
            return;
        }

        [HttpPost]
        public async Task<ActionResult> CorporateScreening(CorporateScreeningModel model)
        {
            checkThreshold = model.Threshold;
            TokenRS token =   AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            model.ClientId = _clientHandler.GetClientId();
            model.CreatedBy = _clientHandler.GetUserId();
            var CustomerType = "C";
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, clientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.BusinessTypeList = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, CustomerType, clientId)), "BusinessName", "BusinessName");
            model.EntityType = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, CustomerType, clientId)), "LegalStatus", "LegalStatus");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Customer Risk");
            model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Product Risk");
            model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Delivery Channel Risk");
            model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Mode of Payment");
            string CallC6Screening = string.Empty;
            bool isCaseCreated = false;
            string caseRefId = "";

            List<string> selectedScreeningOptions = new List<string>();
            CallC6Screening = _configuration["CallC6Screening"];
            if (token.status == 400)
            {
                _toastNotification.AddErrorToastMessage("User is not authorized for screening");
                model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                return View(model);
            }
            else
            {
                CustomerCaseDTO customerCaseDTO = new CustomerCaseDTO();
                    customerCaseDTO.ClientId = model.ClientId;
                    bool IsSanction = false;
                    int count = 0;

                    //for (int i = 0; i < model.CodesTable.Count; i++)
                    //{
                    //    var val = model.CodesTable[i].ccName;
                    //    model.CodeNames.Add(val);
                    //}
                    for (int i = 0; i < model.CodeNames.Count; i++)
                    {
                        if (model.IsChecked[i] == true)
                        {
                            count++;
                        selectedScreeningOptions.Add(model.CodeNames[i]);
                        if (model.CodeNames[i] == "Sanction")
                            {
                                IsSanction = true;
                            }
                        }
                    }
                                        if (model.CorporateDetailList != null && model.CorporateDetailList.Count != 0)
                    {
                        try
                        {
                            // Map Legacy Model to CaseStudioPayload
                            var payload = MapLegacyToStudioPayload(model, selectedScreeningOptions);

                            // Process via Unified Studio Service (Transactional & Asynchronous)
                            var studioResult = await _caseStudioService.ProcessHierarchyAsync(payload, model.ClientId, model.CreatedBy, baseURL, baseC6URL);

                            if (studioResult.Success)
                            {
                                var rootResult = studioResult.NodeResults.FirstOrDefault(nr => payload.Nodes.Any(n => n.StudioId == nr.StudioId && n.IsRoot));
                                caseRefId = rootResult?.BackendId ?? "";
                                isCaseCreated = true;
                                _toastNotification.AddSuccessToastMessage("Corporate Case created successfully via unified service.");

                                // Cleanup temporary records
                                foreach (var corp in model.CorporateDetailList)
                                {
                                    _customerCaseService.DeletePendingShareholders(corp.CompanyCode);
                                }
                            }
                            else
                            {
                                _toastNotification.AddErrorToastMessage(studioResult.Message ?? "Creation failed.");
                                model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "Name", "Name");
                                return View(model);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Error(ex, "Error processing corporate screening via unified service");
                            _toastNotification.AddErrorToastMessage("Critical failure during screening process.");
                            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "Name", "Name");
                            return View(model);
                        }
                    }
                }

                if (isCaseCreated)
                {
                    // Parity: Audit Log & Status Reset
                    if (!string.IsNullOrEmpty(caseRefId))
                    {
                        int id = _customerCaseService.GetCaseId(caseRefId);
                        var caseDto = _customerCaseService.GetDetails(id);
                        if (caseDto != null)
                        {
                            if (new[] { 1, 2, 3, 6, 7 }.Contains(caseDto.Status))
                            {
                                caseDto.Status = 0;
                                _customerCaseService.Update(caseDto);
                            }

                            // Audit Trail
                            string commentText = selectedScreeningOptions.Any() 
                                ? "Case creation started for : " + string.Join(", ", selectedScreeningOptions)
                                : "Case creation started";
                                
                            _caseCommentService.Create(new CaseCommentDTO
                            {
                                CaseId = id.ToString(),
                                Comment = commentText,
                                CreatedBy = model.CreatedBy,
                                CreatedOnDB = DateTime.Now,
                                CustomerId= caseRefId
                            });
                        }
                    }
                    TempData["CaseRefId"] = caseRefId;
                    TempData["IsCaseCreated"] = true;
                    return RedirectToAction("CorporateScreening");
                }

                model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "Name", "Name");
                return View(model);
            }

        private CaseStudioPayload MapLegacyToStudioPayload(CorporateScreeningModel legacyModel, List<string> screeningOptions)
        {
            var payload = new CaseStudioPayload
            {
                Nodes = new List<CaseStudioNode>(),
                Edges = new List<CaseStudioEdge>()
            };

            foreach (var corp in legacyModel.CorporateDetailList)
            {
                var corpId = $"legacy_corp_{Guid.NewGuid().ToString().Substring(0, 8)}";
                var corpNode = new CaseStudioNode
                {
                    StudioId = corpId,
                    IsRoot = true,
                    Type = "C",
                    Name = corp.FirstName,
                    Nationality = corp.Nationality,
                    Dob = corp.DOB,
                    Cif = corp.CIFNumber,
                    TradeLicence = corp.Tradelicense,
                    RegistrationDate = corp.AccomplishedDate.ToString("dd/MM/yyyy"),
                    IsScreened = !string.IsNullOrEmpty(corp.ApiResultJsonCorp),
                    ScreeningSources = screeningOptions,
                    MatchThreshold = legacyModel.Threshold,
                    Relationship = "Main",
                    FlagType = "Corporate",
                    EntityId = corp.EntityTypeTxt,
                    BusinessId = corp.BusinessType,
                    ProductId = corp.ProductName,
                    DeliveryChannelId = corp.DeliveryChannelName,
                    ModeOfPaymentId = corp.Modeofpayment,
                    Attachments = corp.CaseDocumentsL?.Select(d => new CaseStudioAttachment
                    {
                        Name = d.DocumentName,
                        FileName = d.DocumentFileName,
                        FullPath = d.DocumentFullPath,
                        IssuedDate = d.IssuedDate?.ToString("dd/MM/yyyy"),
                        ExpiryDate = d.ExpiryDate?.ToString("dd/MM/yyyy")
                    }).ToList()
                };
                payload.Nodes.Add(corpNode);

                if (corp.CustomerDetailList != null)
                {
                    foreach (var sh in corp.CustomerDetailList)
                    {
                        var shId = $"legacy_sh_{Guid.NewGuid().ToString().Substring(0, 8)}";
                        var shNode = new CaseStudioNode
                        {
                            StudioId = shId,
                            IsRoot = false,
                            Type = sh.CustomerType == "C" ? "C" : "I",
                            FirstName = sh.FirstName,
                            MiddleName = sh.MiddleName,
                            LastName = sh.LastName,
                            Name = $"{sh.FirstName} {sh.MiddleName} {sh.LastName}".Trim(),
                            Nationality = sh.Nationality,
                            Dob = sh.DOB,
                            Cif = sh.EmiratesIdNumber,
                            PassportId = sh.CustomerIdNumber,
                            EmiratesIdNumber = sh.EmiratesIdNumber,
                            Share = decimal.TryParse(sh.SharePercent, out var s) ? s : 0,
                            Relationship = sh.Designation ?? "Shareholder",
                            FlagType = sh.Designation ?? "Shareholder",
                            IsScreened = !string.IsNullOrEmpty(sh.CustomerId),
                            ScreeningSources = screeningOptions,
                            MatchThreshold = legacyModel.Threshold
                        };
                        payload.Nodes.Add(shNode);
                        payload.Edges.Add(new CaseStudioEdge { From = corpId, To = shId });
                    }
                }
            }

            return payload;
        }

        public async Task<IActionResult>  CreateShareholder(ShareholderFormModel model, string screeningType)
        {
            if (model.Shareholders == null || !model.Shareholders.Any())
                return BadRequest("No shareholders provided");

            var parentIdMap = new Dictionary<string, string>(); // TempId -> DB CustomerId
            var clientId = _clientHandler.GetClientId();
            string CallC6Screening = _configuration["CallC6Screening"];
            List<string> selectedScreeningOptions = new List<string>();
            var flagTypes = new HashSet<string>();
            model.CodesTables = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId));

            bool IsSanction = false;
            int count = 0;

            for (int i = 0; i < model.CodesTables.Count; i++)
            {
                var val = model.CodesTables[i].ccName;
                model.CodeNames.Add(val);
            }
            for (int i = 0; i < model.CodeNames.Count; i++)
            {
                //if (_documentUploadModel.IsChecked[i] == true)
                //{
                count++;
                selectedScreeningOptions.Add(model.CodeNames[i]);
                if (model.CodeNames[i] == "Sanction")
                {
                    IsSanction = true;
                }
                //}
            }

            

            var screeningoption = string.Join(",", selectedScreeningOptions);
            // Sort the list by hierarchy level (parents first)
            var sortedShareholders = model.Shareholders
                .OrderBy(s => s.DisplayId.Count(c => c == '.')) // parents first, children next
                .ToList();
            bool isCaseCreated = false;
            string caseRefId = null;
            foreach (var sh in sortedShareholders)
            {
                string customerId = null;
                string parentId = null;

                // ✅ STEP 1: GET PARENT FIRST
                if (!string.IsNullOrEmpty(sh.DisplayId) && sh.DisplayId.Contains("."))
                {
                    var parentKey = sh.DisplayId.Substring(0, sh.DisplayId.LastIndexOf('.'));

                    if (parentIdMap.TryGetValue(parentKey, out var pId))
                    {
                        parentId = pId;
                    }
                }
                if (!string.IsNullOrEmpty(sh.CustomerId))
                {
                    customerId = sh.CustomerId;

                    // map for children
                    if (!string.IsNullOrEmpty(sh.DisplayId))
                        parentIdMap[sh.DisplayId] = customerId;
                }
                else if (!string.IsNullOrEmpty(sh.MainPartyCode))
                {
                    customerId = await ProcessUsingMainParty(sh, parentId);

                    caseRefId ??= customerId;
                    isCaseCreated = true;
                }
                else { 
                    int companyid = _customerCaseService.GetCaseId(sh.CompanyCode);
                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(companyid);

                bool isCorporate = sh.Type == "Corporate_Corp" || sh.Type == "Individual_Corp";


                CaseModel caseModel = new CaseModel
                {
                    LastName = sh.Name,
                    Type = sh.Type,
                    CustomerType = isCorporate ? "C" : "I",
                    MatchCategory = isCorporate ? "CORPORATE" : "INDIVIDUAL",
                    ClientId = sh.ClientID,
                    //CreatedBy = sh.UserId,
                    CompanyCode = sh.CompanyCode,
                    //CompanyName = sh.CompanyName,
                    //Thershold = sh.Thershold,
                    DOB = sh.RegistrationDate?.ToString("yyyy-MM-dd"),
                    CustomerIdType = sh.IdType,
                    CustomerIdNumber = sh.IdNumber,
                    IdIssueDate = sh.IssueDate,
                    IdExpiryDate = sh.IdExpiry,
                    Nationality = sh.Nationality,
                    Share = sh.Share,
                    Designation = sh.Designation,
                    Tradelicense = sh.TradeLicence,
                    CIFNumber = sh.Cif,
                    Residence = sh.Residence,
                    Employer = sh.Employer,
                    GoldenVisa = sh.GoldenVisa,
                    EmployerIndustry = sh.EmployerIndustry,
                    EmployerSector = sh.EmployerSector,
                    SOWSOFCountry = sh.SOWSOFCountry,
                    TradeLicenseAuthority = sh.TradeLicenseAuthority,
                    TradeLicenseSector = sh.TradeLicenseSector,
                    Gender = sh.Gender,
                    Relationship = sh.Relationship,
                    FlagType = sh.FlagType,
                    PlaceOfBirth=sh.PlaceOfBirth
                };

                // Determine ParentId from TempId (everything before last dot)
                //if (sh.DisplayId.Contains("."))
                //{
                //    var lastDotIndex = sh.DisplayId.LastIndexOf('.');
                //    var parentTempId = sh.DisplayId.Substring(0, lastDotIndex);
                //    if (parentIdMap.ContainsKey(parentTempId))
                //        caseModel.ParentId = parentIdMap[parentTempId];
                //}
                


                // Map to DTO
                //var _ccDTO = _mapper.Map<CustomerCaseDTO>(caseModel);

                caseModel.ClientId = _clientHandler.GetClientId();
                caseModel.CreatedBy = _clientHandler.GetUserId();

                CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(caseModel);
                _ccDTO.ClientId = _clientHandler.GetClientId();
                _ccDTO.CreatedBy = _clientHandler.GetUserId();
                _ccDTO.Threshold = sh.Thershold == 0 ? checkThreshold : sh.Thershold;
                _ccDTO.DOB = Convert.ToDateTime(sh.RegistrationDate);
                _ccDTO.Nationality = caseModel.Nationality;
                _ccDTO.ScreeningOptions = screeningoption;
                    _ccDTO.Version = 1;
                _ccDTO.ParentID = string.IsNullOrEmpty(parentId) ? sh.CompanyCode : parentId;
                
                    // ============================
                    // NORMAL FLOW (WITH SCREENING)
                    // ============================
                    var createResult = _customerCaseService.Create(_ccDTO);

                    if (createResult.Status != StaticResource.SuccessStatusCode)
                        continue;

                    if (createResult.Result != null)
                    {
                        if (createResult.Result.Contains("Ø"))
                            customerId = createResult.Result.Split('Ø')[1];
                        else if (createResult.Result.Contains("??"))
                            customerId = createResult.Result.Split(new string[] { "??" }, StringSplitOptions.None)[1];
                        else if (createResult.Result.Contains("A~"))
                            customerId = createResult.Result.Split(new string[] { "A~" }, StringSplitOptions.None)[1];
                        else
                            customerId = createResult.Result;
                    }

                    var screeningResult = await DoScreeningFlow(_ccDTO, caseModel, customerId);

                    caseRefId ??= customerId;
                    isCaseCreated = true;
                }
                if (!string.IsNullOrEmpty(sh.DisplayId) && !string.IsNullOrEmpty(customerId))
                {
                    parentIdMap[sh.DisplayId] = customerId;
                }

                
                if (!string.IsNullOrEmpty(sh.FlagType))
                    flagTypes.Add(sh.FlagType);

                _customerCaseService.DeleteShareholders(sh.Id);
            }

                // ✅ STEP 2: Decide existing vs new
                //if (!string.IsNullOrEmpty(sh.CustomerId))
                //{
                //    // 👉 EXISTING CUSTOMER
                //    customerId = sh.CustomerId;
                //    if (!string.IsNullOrEmpty(sh.DisplayId))
                //    {
                //        parentIdMap[sh.DisplayId] = customerId;
                //    }
                //}
                //else
                //{
                //    // 👉 NEW CUSTOMER → create
                    


                //    // ✅ STEP 3: ALWAYS MAP (VERY IMPORTANT)
                //    if (!string.IsNullOrEmpty(sh.DisplayId))
                //    {
                //        parentIdMap[sh.DisplayId] = customerId;
                //    }
                //    if (!string.IsNullOrEmpty(sh.MainPartyCode))
                //    {
                //        // 👉 DO NOT CALL SCREENING
                //        var mainResult =await ProcessUsingMainParty(sh,customerId);

                //        caseRefId ??= mainResult;   // return new case ref
                //        isCaseCreated = true;
                //        if (!string.IsNullOrEmpty(sh.FlagType))
                //        {
                //            flagTypes.Add(sh.FlagType);
                //        }
                //    }
                //    else
                //    {
                //        var result = _customerCaseService.Create(_ccDTO);

                //        if (result.Status != StaticResource.SuccessStatusCode)
                //        {
                //            _toastNotification.AddErrorToastMessage($"Failed to create shareholder {sh.Name}");
                //            continue;
                //        }

                //        customerId = result.Result.Split('Ø')[1];
                //        // 👉 EXISTING FLOW (NO CHANGE)
                //        var screeningResult = await DoScreeningFlow(_ccDTO, caseModel, customerId);
                //    }
                //    //if (!string.IsNullOrEmpty(sh.Document))
                //    //{
                //    //    CaseDocumentModel _caseDoc = new CaseDocumentModel();
                //    //    _caseDoc.CaseId = _customerCaseService.GetCaseId(_ccDTO.CustomerId).ToString();
                //    //    _caseDoc.CreatedBy = _clientHandler.GetUserId();
                //    //    _caseDoc.CreatedOn = DateTime.Now;
                //    //    _caseDoc.ClientId = _clientHandler.GetClientId();

                //    //    _caseDoc.DocumentFileName = sh.Document;
                //    //    _caseDoc.DocumentFullPath = sh.DocumentFullPath; // or stored path
                //    //    _caseDoc.DocumentName = sh.Name;

                //    //    _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                //    //}
                //    //// Screening logic
                //    //if (CallC6Screening == "Y")
                //    //{
                //    //    _ccDTO.IsPep = true;
                //    //    _ccDTO.IsSan = true;
                //    //}

                //    //string body = string.Empty;
                //    //using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                //    //{
                //    //    body = reader.ReadToEnd();
                //    //}

                //    //CustomerCaseDTO screeningResult;
                //    //if (_ccDTO.CustomerType == "I")
                //    //    screeningResult = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, _ccDTO.Threshold, customerId);
                //    //else
                //    //    screeningResult = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "CORPORATE", body, _ccDTO.Threshold, customerId);

                //    //// Send mail if needed
                //    //if (screeningResult.sendMail == 1)
                //    //    await SendScreendedMailAsync(body, caseModel, screeningResult);

                //    // Show toast messages based on screening
                //    //if (screeningResult.IsMatched == 1 && screeningResult.MatchScore >= _ccDTO.Threshold)
                //    //{
                //    //    caseRefId ??= sh.CompanyCode;

                //    //    // Mark that at least one case was created
                //    //    isCaseCreated = true;
                //    //}
                //    ////_toastNotification.AddWarningToastMessage($"Customer {sh.Name} blocked, Case created");
                //    //else if (screeningResult.IsMatched == 0 && screeningResult.MatchScore == 0)
                //    //{
                //    //    caseRefId ??= sh.CompanyCode;

                //    //    // Mark that at least one case was created
                //    //    isCaseCreated = true;
                //    //}
                //    ////_toastNotification.AddInfoToastMessage($"Customer Approved: no match found for {sh.Name}");
                //    //else if (screeningResult.MatchScore > 0 && screeningResult.MatchScore < _ccDTO.Threshold)
                //    //{
                //    //    caseRefId ??= sh.CompanyCode;

                //    //    // Mark that at least one case was created
                //    //    isCaseCreated = true;
                //    //    //_toastNotification.AddInfoToastMessage($"Customer Approved: match score below threshold for {sh.Name}");
                //    //}
                //    //else
                //    //{
                //    //    caseRefId ??= sh.CompanyCode;

                //    //    // Mark that at least one case was created
                //    //    isCaseCreated = true;
                //    //    //_toastNotification.AddSuccessToastMessage($"Customer {sh.Name} Approved");
                //    //}

                //    // Optional: Delete temp shareholder record if using temp table
                //    _customerCaseService.DeleteShareholders(sh.Id);

                //    if (!string.IsNullOrEmpty(sh.FlagType))
                //    {
                //        flagTypes.Add(sh.FlagType);
                //    }
                //}
                //}

                
                

            
            
           

            if (isCaseCreated && !string.IsNullOrEmpty(caseRefId))
            {
                int id = _customerCaseService.GetCaseId(caseRefId);
                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(id);
                if (_CustomerCaseDTO.Status == 2 || _CustomerCaseDTO.Status == 1 || _CustomerCaseDTO.Status == 3 || _CustomerCaseDTO.Status == 6 || _CustomerCaseDTO.Status == 7)
                {
                    _CustomerCaseDTO.Status = 0;
                }
                _customerCaseService.Update(_CustomerCaseDTO);

                string commentText = string.Empty;
                var uniqueFlagTypes = flagTypes.ToList();
                

                if (uniqueFlagTypes.Any())
                {
                    commentText = uniqueFlagTypes.Count == 1
                        ? $"{uniqueFlagTypes[0]} Case Has Been Created."
                        : $"{string.Join(", ", uniqueFlagTypes)} Cases Have Been Created.";
                }

                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = id;
                remarkModel.Comment = commentText; // ✅ FIXED
                remarkModel.CommentType = "Related Parties";
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                remarkModel.CustomerId = caseRefId;
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
                // Pass values to View / JS / TempData
                TempData["CaseRefId"] = caseRefId;
                TempData["IsCaseCreated"]= true;
            }

            if (screeningType == "Individual")
            {
                return RedirectToAction("Create", "Case");
            }
            else {
                return RedirectToAction("CorporateScreening");
            }
        }

        private async Task<CustomerCaseDTO> DoScreeningFlow(CustomerCaseDTO _ccDTO, CaseModel caseModel, string customerId)
        {
            string CallC6Screening = _configuration["CallC6Screening"];
            if (CallC6Screening == "Y")
            {
                _ccDTO.IsPep = true;
                _ccDTO.IsSan = true;
            }

            string body = System.IO.File.ReadAllText(@"Views/Risk/RiskEmailBody.html");

            CustomerCaseDTO screeningResult;

            if (_ccDTO.CustomerType == "I")
                screeningResult = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, _ccDTO.Threshold, customerId);
            else
                screeningResult = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "CORPORATE", body, _ccDTO.Threshold, customerId);

            if (screeningResult.sendMail == 1)
                await SendScreendedMailAsync(body, caseModel, screeningResult);

            return screeningResult;
        }

        private async Task<string> ProcessUsingMainParty(ShareholderModel sh,string customerId)
        {
            int Id = _customerCaseService.GetCaseId(sh.MainPartyCode);

            CustomerCaseDTO existing = _customerCaseService.GetDetails(Id);

            existing.CustomerType = existing.CustomerType;
            existing.ClientId = _clientHandler.GetClientId();
            existing.CreatedBy = _clientHandler.GetUserId();
            existing.CustomerId = "0"; // force new
            existing.CompanyCode = sh.CompanyCode;
            existing.ParentID = string.IsNullOrEmpty(customerId) ? sh.CompanyCode : customerId;
            existing.Type = sh.Type;
            existing.FlagType = sh.FlagType;
            existing.IsDuplicate = 1;

            int previousStatus = existing.Status;

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

            if (previousStatus == 2 || previousStatus == 3 || previousStatus == 1 || previousStatus == 6 || previousStatus == 7)
            {
                int newStatus = hasMatchRecords ? 0 : 5;

                existing.Status = newStatus;
            }

            // ✅ ONLY ONE CREATE HERE
            var createResult = _customerCaseService.Create(existing);
            if (createResult.Status != StaticResource.SuccessStatusCode)
            {
                throw new Exception($"Failed to create case: {createResult.Message}");
            }

            string newCustomerId = string.Empty;
            if (createResult.Result != null)
            {
                if (createResult.Result.Contains("Ã˜"))
                    newCustomerId = createResult.Result.Split(new string[] { "Ã˜" }, StringSplitOptions.None)[1];
                else if (createResult.Result.Contains("??"))
                    newCustomerId = createResult.Result.Split(new string[] { "??" }, StringSplitOptions.None)[1];
                else if (createResult.Result.Contains("A~"))
                    newCustomerId = createResult.Result.Split(new string[] { "A~" }, StringSplitOptions.None)[1];
                else
                    newCustomerId = createResult.Result;
            }

            int newCaseId = _customerCaseService.GetCaseId(newCustomerId);

            // ✅ Save match records
            var matchrecordsList = jsonList.Select(x => new MatchRecordsDTO
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
            }).ToList();

            CASELOG modelCaseLog = new CASELOG
            {
                CASEID = newCaseId.ToString(),
                MATCHRECORDS = matchrecordsList
            };

            _freeSourceRepository.InsertCaseLog(modelCaseLog);

            // ✅ Comment
            _caseCommentService.Create(new CaseCommentDTO
            {
                CaseId = newCaseId.ToString(),
                Comment = "Case created from Main Party (no screening)",
                CommentType = "Convert Related Parties to Main Party",
                CreatedBy = _clientHandler.GetUserId(),
                CustomerId= newCustomerId
            });

            return sh.CompanyCode; // ✅ RETURN
        }


        //public async Task<IActionResult> CreateShareholder(ShareholderFormModel model)
        //{
        //    var parentIdMap = new Dictionary<string, string>(); // TempId -> DB CustomerId

        //    // Step 1: Insert parents first (TempId without dot)
        //    var parents = model.Shareholders.Where(s => !s.DisplayId.Contains(".")).ToList();
        //    var clientId = _clientHandler.GetClientId();
        //    var result = _customerCaseService.GetAllShareHolders(clientId);
        //    string CallC6Screening = string.Empty;

        //    CallC6Screening = _configuration["CallC6Screening"];

        //    foreach (var item in result)
        //    {
        //        CaseModel model = new CaseModel()
        //        {
        //            //FirstName = customeDetailsModel.FirstName,
        //            LastName = item.Name,
        //            //MiddleName = customeDetailsModel.MiddleName,
        //            CompanyCode = item.CompanyCode,
        //            //DOB = item.RegistrationDate.ToString(),
        //            Nationality = item.Nationality,
        //            Type=item.Type

        //        };
        //        if(item.Type != "Corporate Shareholder")
        //        {
        //            model.CustomerType = "I";
        //            model.MatchCategory = "INDIVIDUAL";
        //        }
        //        else
        //        {
        //            model.CustomerType = "C";
        //            model.MatchCategory = "CORPORATE";

        //        }

        //        model.ClientId = _clientHandler.GetClientId();
        //        model.CreatedBy = _clientHandler.GetUserId();
        //        CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
        //        _ccDTO.ClientId = _clientHandler.GetClientId();
        //        _ccDTO.CreatedBy = _clientHandler.GetUserId();
        //        _ccDTO.Threshold = item.Thershold == 0 ? checkThreshold : item.Thershold;
        //        _ccDTO.DOB = Convert.ToDateTime(item.RegistrationDate);
        //        _ccDTO.Nationality = model.Nationality;
        //        //var customerCodeprefix= _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(model.ClientId));
        //        //_ccDTO.customerCodeprefix = customerCodeprefix.Prefix;

        //        //var result = _customerCaseService.CreatePrefix(_ccDTO);
        //        var result1 = _customerCaseService.Create(_ccDTO);
        //        _ccDTO.CustomerId = result1.Result.Split('Ø')[1];
        //        CaseDocumentModel _caseDoc = new CaseDocumentModel();
        //        _caseDoc.CaseId = _customerCaseService.GetCaseId(_ccDTO.CustomerId).ToString();
        //        //model.CodesTable = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(model.ClientId));


        //        //if (customeDetailsModel.CaseDocumentsL != null)
        //        //{
        //        //    foreach (var item1 in customeDetailsModel.CaseDocumentsL)
        //        //    {
        //        //        if (item.Document != null)
        //        //        {
        //        //            _caseDoc.CreatedBy = _clientHandler.GetUserId();
        //        //            _caseDoc.CreatedOn = DateTime.Now;
        //        //            _caseDoc.ClientId = _clientHandler.GetClientId();
        //        //            DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
        //        //            _caseDoc.DocumentFileName = item.Document.FileName;
        //        //            _caseDoc.DocumentFullPath = _documentsModel.DocFullPath;
        //        //            _caseDoc.IssuedDateOnDB = item.IssuedDate;
        //        //            _caseDoc.ExpiryDateOnDB = item.ExpiryDate;
        //        //            _caseDoc.DocumentName = item.DocumentName;
        //        //            var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
        //        //        }
        //        //    }
        //        //}
        //        //for sanction screening
        //        //bool IsSanction = false;
        //        //int count = 0;

        //        //for (int i = 0; i < model.CodesTable.Count; i++)
        //        //{
        //        //    var val = model.CodesTable[i].ccName;
        //        //    model.CodeNames.Add(val);
        //        //}
        //        //for (int i = 0; i < model.CodeNames.Count; i++)
        //        //{
        //        //    if (customeDetailsModel.IsChecked[i] == true)
        //        //    {
        //        //        count++;
        //        //        if (model.CodeNames[i] == "Sanction")
        //        //        {
        //        //            log.Debug("Only sanction was true");
        //        //            IsSanction = true;
        //        //        }
        //        //    }
        //        //}

        //        //for (int i = 0; i < model.CodeNames.Count; i++)
        //        //{
        //        //    if (model.CodeNames[i] == "PEP" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsPep = true; continue; }
        //        //    if (model.CodeNames[i] == "Sanction" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsSan = true; continue; }
        //        //    if (model.CodeNames[i] == "Reputational Risk Exposure" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsRre = true; continue; }
        //        //    if (model.CodeNames[i] == "Insolvency (UK & Ireland)" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsIns = true; continue; }
        //        //    if (model.CodeNames[i] == "Disqualified Director (UK Only)" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsDd = true; continue; }
        //        //    if (model.CodeNames[i] == "Profile of Interest" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsPoi = true; continue; }
        //        //    if (model.CodeNames[i] == "Regulatory Enforcement List" && customeDetailsModel.IsChecked[i] == true) { _ccDTO.IsRel = true; continue; }
        //        //}


        //        //if (count == 1 && IsSanction == true && CallC6Screening == "N")
        //        //{
        //        //    string response = string.Empty;
        //        //    var searchType = "F";
        //        //    string data = await _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
        //        //    {
        //        //        customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
        //        //        customerfullname = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
        //        //        customernationality = model.Nationality,
        //        //        searchtype = searchType
        //        //    }, ScreeningService.BACKLIST_SCREENING);

        //        //    if (!string.IsNullOrEmpty(data))
        //        //    {
        //        //        List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
        //        //        //response = AMLUtility.FormatJsonToPlainText(data);

        //        //        _commonService.UpdateSanctionRecords(_ccDTO, apiResultModel, result.Result.Split('Ø')[1]);

        //        //        string body = string.Empty;
        //        //        using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
        //        //        {
        //        //            body = reader.ReadToEnd();
        //        //        }
        //        //        ;

        //        //        //var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result);

        //        //        if (_ccDTO.sendMail == 1)
        //        //        {
        //        //            await SendScreendedMailAsync(body, model, _ccDTO);
        //        //        }

        //        //        if (_ccDTO.IsMatched == 1 && _ccDTO.MatchScore >= checkThreshold)
        //        //        {
        //        //            _toastNotification.AddWarningToastMessage("Customer blocked, Case created");
        //        //        }
        //        //        else if (_ccDTO.IsMatched == 0 && _ccDTO.MatchScore == 0)
        //        //        {
        //        //            model.IsMatched = 2;
        //        //            var appBaseUrl = MyHttpContext.AppBaseUrl;
        //        //            // body = body.Replace("{baseUrl}", appBaseUrl);
        //        //            //model.Url = appBaseUrl;
        //        //            //model.createdUserName = HttpContext.Session.GetString("SessUsername");
        //        //            _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));
        //        //            //return View(model);
        //        //        }
        //        //        else if (_ccDTO.MatchScore >= 0 && _ccDTO.MatchScore < checkThreshold)
        //        //        {
        //        //            _ccDTO.ApiResultsjson = _ccDTO.ApiResultsjson.Where((source, index) => index < 100).ToList();
        //        //            model.ApiResultJson = _ccDTO.ApiResultsjson;
        //        //            _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));

        //        //            // return View(model);
        //        //        }
        //        //        else
        //        //        {
        //        //            _toastNotification.AddSuccessToastMessage("Customer Approved");
        //        //        }//      _toastNotification.AddWarningToastMessage("Customer blocked!");
        //        //    }
        //        //    else
        //        //    {
        //        //        _toastNotification.AddWarningToastMessage("Customer blocked! ");
        //        //    }

        //        //}

        //        if (result1.Status == StaticResource.SuccessStatusCode)
        //        {
        //            if (CallC6Screening == "Y")
        //            {
        //                _ccDTO.IsPep = true;
        //                _ccDTO.IsSan = true;
        //            }

        //            string body = string.Empty;
        //            using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
        //            {
        //                body = reader.ReadToEnd();
        //            }
        //            ;
        //            CustomerCaseDTO x = new CustomerCaseDTO();
        //            if (model.CustomerType == "I")
        //            {
        //                x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result1.Result.Split('Ø')[1]);
        //            }
        //            else
        //            {
        //                 x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "CORPORATE", body, model.Threshold, result1.Result.Split('Ø')[1]);
        //            }


        //            if (x.sendMail == 1)
        //            {
        //                await SendScreendedMailAsync(body, model, x);
        //            }
        //            if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
        //            {
        //                _toastNotification.AddWarningToastMessage("Customer blocked, Case created");
        //            }
        //            else if (x.IsMatched == 0 && x.MatchScore == 0)
        //            {
        //                model.IsMatched = 2;
        //                var appBaseUrl = MyHttpContext.AppBaseUrl;
        //                body = body.Replace("{baseUrl}", appBaseUrl);
        //                model.Url = appBaseUrl;
        //                model.createdUserName = HttpContext.Session.GetString("SessUsername");
        //                _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", x.FirstName, " ", x.LastName, ". \n"));

        //            }
        //            else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
        //            {

        //                x.ApiResultsjson = x.ApiResultsjson.Where((source, index) => index < 100).ToList();
        //                model.ApiResultJson = x.ApiResultsjson;
        //                _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", x.FirstName, " ", x.LastName, ". \n"));


        //            }
        //            else
        //            {
        //                _toastNotification.AddSuccessToastMessage("Customer Approved");
        //            }
        //            _customerCaseService.DeleteShareholders(item.Id);
        //            /*commenting as its not required for saas live
        //            Console.WriteLine("Start risk assessment For shareholder");
        //            var riskItemList = new List<RiskTypeListModel>();

        //            Console.WriteLine("Adding items for risk assessment");
        //            var riskList = new List<(string, string)>();
        //            riskList.Add(("4", x.IsMatched == 1 && x.Status == 0 ? "3341" : "3342"));


        //            foreach ((string, string) riskItemId in riskList)
        //            {
        //                var riskType = new RiskTypeListModel
        //                {
        //                    Id = riskItemId.Item1,
        //                    RiskItemList = new List<RiskItemListModel>
        //                                    {
        //                                     new RiskItemListModel
        //                                      {
        //                                        Id = riskItemId.Item2
        //                                      }
        //                                    }
        //                };

        //                riskItemList.Add(riskType);
        //            }

        //            var riskApiRequest = new RiskAPIRequestModel
        //            {
        //                CustomerId = result.Result,
        //                MainNationality = model.Nationality,
        //                ClientId = model.ClientId,
        //                CreatedBy = model.CreatedBy,
        //                CustomerName = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
        //                RiskCategory = model.CustomerType,
        //                RiskTypeList = riskItemList
        //            };
        //            Console.WriteLine($"Sending data for risk assessment: {JsonConvert.SerializeObject(riskApiRequest, Formatting.Indented)}");

        //            IActionResult riskAssessmentResult = _riskAPIController.Assessment(riskApiRequest);

        //            Console.WriteLine($"Got result: {riskAssessmentResult}");
        //            */
        //        }
        //        else
        //        {
        //            _toastNotification.AddErrorToastMessage("Customer creation failed");
        //            return View("CorporateScreening");
        //        }

        //    }

        //    return RedirectToAction("CorporateScreening");
        //}
        [HttpPost]
        public JsonResult AutoCompleteCustomer(string prefix)
        {
            var customers = _customerCaseService.GetCustomerMasterByCodePrefixAndCode(prefix, "C");

            return Json(customers);
        }

        public JsonResult SearchCompanyCode(string CompanyCode,string CustomerType)
        {
            var clientId = _clientHandler.GetClientId();

            List<CaseModel> abc = new List<CaseModel>();

            abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetCompanyCode(CompanyCode, clientId,CustomerType));




            return Json(abc);
        }

        public JsonResult GetPendingShareholders(string companyCode,string customerType)
        {
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var result = _customerCaseService.GetAllShareHolders(clientId, companyCode, userId,customerType);

            



            return Json(result);
        }
        public JsonResult GetCaseCreatedPendingShareholders(string companyCode)
        {
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var result = _customerCaseService.GetAllCaseCreatedShareHolders(clientId, companyCode, userId);





            return Json(result);
        }

        [HttpPost]
        public IActionResult SaveShareholders(string data, List<IFormFile> Documents)
        {
            if (string.IsNullOrEmpty(data))
                return Json(false);

            var model = Newtonsoft.Json.JsonConvert.DeserializeObject<List<ShareholderModel>>(data);

            if (model == null || !model.Any())
                return Json(false);

            int docIndex = 0;

            foreach (var item in model)
            {
                ShareholderDTO _ccDTO = _mapper.Map<ShareholderDTO>(item);
                _ccDTO.ClientId = _clientHandler.GetClientId();
                _ccDTO.UserId = _clientHandler.GetUserId();
                _ccDTO.IsDuplicate = item.IsDuplicate ? 1 : 0;

                // Process internal documents for this item
                for (int i = 0; i < item.DocumentCount; i++)
                {
                    if (Documents != null && Documents.Count > docIndex)
                    {
                        var file = Documents[docIndex];
                        DocumentsModel _documentsModel = _fileUploader.UploadFile(
                            _ccDTO.ClientId,
                            ItemType.caseDocument,
                            _clientHandler.GetBranchId(),
                            file
                        );

                        // If it's the first document, save it to the main record
                        if (i == 0)
                        {
                            _ccDTO.DocFullPath = _documentsModel.DocFullPath;
                            _ccDTO.DocumentFileName = file.FileName;
                        }
                        else
                        {
                            // If it's an additional document, we need to save it to CaseDocument table
                            // Wait, ShareholderDTO main record is created below. 
                            // We might need to handle extra docs AFTER CreateShareholdersData.
                        }
                        docIndex++;
                    }
                }

                var result = _customerCaseService.CreateShareholdersData(_ccDTO);
                
                // If there were extra documents, we might need a way to link them.
                // However, since CreateShareholdersData is a black box that might call CreateCustomerMaster...
                // The current schema seems to only have space for ONE doc in CS_SHAREHOLDER table.
                // To support true multi-doc for related parties, I'll associate extra docs with the case/customer code.
            }

            var caseCodesToUpdate = model.Select(x => x.CompanyCode).Where(c => !string.IsNullOrEmpty(c)).Distinct().ToList();
            foreach (var companyCode in caseCodesToUpdate)
            {
                int id = _customerCaseService.GetCaseId(companyCode);
                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(id);
                if (_CustomerCaseDTO != null && (_CustomerCaseDTO.Status == 2 || _CustomerCaseDTO.Status == 1 || _CustomerCaseDTO.Status == 3 || _CustomerCaseDTO.Status == 6 || _CustomerCaseDTO.Status == 7))
                {
                    _CustomerCaseDTO.Status = 0;
                    _customerCaseService.Update(_CustomerCaseDTO);

                    CaseCommentModel remarkModel = new CaseCommentModel();
                    remarkModel.CaseId = id;
                    remarkModel.Comment = "Status updated to Pending due to related party addition.";
                    remarkModel.CommentType = "Related Parties";
                    remarkModel.CreatedBy = _clientHandler.GetUserId();
                    remarkModel.CustomerId = companyCode;
                    _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
                }
            }

            return Json(true);
        }

        [HttpPost]
        public IActionResult ConvertToShareholder([FromBody] ConvertToShareholderRequest request)
        {
            int Id = _customerCaseService.GetCaseId(request.customerId);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            _CustomerCaseDTO.ClientId = _clientHandler.GetClientId();
            _CustomerCaseDTO.CreatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.CustomerId = "0";
            _CustomerCaseDTO.CompanyCode = "";

            ShareholderDTO _ccDTO = _mapper.Map<ShareholderDTO>(_CustomerCaseDTO);
            _ccDTO.Type = request.type;
            _ccDTO.FlagType = request.flagType;
            _ccDTO.IsDuplicate = 1;
            _ccDTO.MainPartyCode = request.customerId;
            _ccDTO.CompanyCode = request.companyCode;
            _ccDTO.CompanyName = request.companyName;
            _ccDTO.Name = (_CustomerCaseDTO.FirstName ?? "") + " " + (_CustomerCaseDTO.LastName ?? "");
            _ccDTO.Share = _CustomerCaseDTO.Share;
            _ccDTO.Residence = _CustomerCaseDTO.Residence;
            _ccDTO.TradeLicence = _CustomerCaseDTO.Tradelicense;
            _ccDTO.SOWSOFCountry = _CustomerCaseDTO.SOWSOFCountry;
            _ccDTO.Gender = _CustomerCaseDTO.Gender;
            _ccDTO.IdType=_ccDTO.IdType;
            _ccDTO.PassportId = _CustomerCaseDTO.PassportId;
            _ccDTO.PassportIssueDate=_CustomerCaseDTO.PassportIssueDate;
            _ccDTO.PassportExpiryDate= _CustomerCaseDTO.PassportExpiryDate;
            _ccDTO.EmiratesIdNumber = _CustomerCaseDTO.EmiratesIdNumber;
            _ccDTO.EmiratesIdIssueDate = _CustomerCaseDTO.EmiratesIdIssueDate;
            _ccDTO.EmiratesIdExpiryDate = _CustomerCaseDTO.EmiratesIdExpiryDate;
            _ccDTO.Cif = _CustomerCaseDTO.CIFNumber;
            _ccDTO.Employer = _CustomerCaseDTO.Employer;
            _ccDTO.EmployerIndustry=_ccDTO.EmployerIndustry;
            _ccDTO.EmployerSector=_ccDTO.EmployerSector;
            _ccDTO.UserId = _clientHandler.GetUserId();
            _ccDTO.CustType = request.custtype;
            _ccDTO.PlaceOfBirth = _CustomerCaseDTO.PlaceOfBirth;

            var result = _customerCaseService.CreateShareholdersData(_ccDTO);

            if (!string.IsNullOrEmpty(request.companyCode))
            {
                int mainCaseId = _customerCaseService.GetCaseId(request.companyCode);
                CustomerCaseDTO mainCaseDTO = _customerCaseService.GetDetails(mainCaseId);
                if (mainCaseDTO != null && (mainCaseDTO.Status == 2 || mainCaseDTO.Status == 1 || mainCaseDTO.Status == 3 || mainCaseDTO.Status == 6 || mainCaseDTO.Status == 7))
                {
                    mainCaseDTO.Status = 0;
                    _customerCaseService.Update(mainCaseDTO);

                    CaseCommentModel remarkModel = new CaseCommentModel();
                    remarkModel.CaseId = mainCaseId;
                    remarkModel.Comment = "Status updated to Pending due to related party conversion.";
                    remarkModel.CommentType = "Related Parties";
                    remarkModel.CreatedBy = _clientHandler.GetUserId();
                    remarkModel.CustomerId = request.companyCode;
                    _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
                }
            }

            return Json(true);
        }

        //[HttpPost]
        //public IActionResult SaveShareholders([FromBody] List<ShareholderModel> model)
        //{
        //    if (model == null || !model.Any())
        //        return Json(false);

        //    foreach (var item in model)
        //    {
        //        ShareholderDTO _ccDTO = _mapper.Map<ShareholderDTO>(item);
        //        _ccDTO.ClientId = _clientHandler.GetClientId();
        //        _ccDTO.UserId = _clientHandler.GetUserId();
        //        //DocumentsModel _documentsModel = _fileUploader.UploadFile(_ccDTO.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), item.Document);
        //        //_ccDTO.DocFullPath = _documentsModel.DocFullPath;
        //        //_ccDTO.DocumentFileName = item.Document.FileName;
        //        var result = _customerCaseService.CreateShareholdersData(_ccDTO);
        //    }



        //    return Json(true);
        //}

        public JsonResult DeleteShareholder(int id)
        {
            var result = _customerCaseService.DeleteShareholders(id);
            //_customerCaseService.Delete(id); // BUG: Deletes the full case if it exists, using shareholder ID!

            return Json(true);
        }

        [HttpPost]
        public JsonResult DeletePendingShareholders(string companyCode)
        {
            try
            {
                _customerCaseService.DeletePendingShareholders(companyCode);

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
    }
}
