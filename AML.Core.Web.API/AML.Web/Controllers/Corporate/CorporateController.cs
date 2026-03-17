using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
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
using AML.Core.ServiceContract.User;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
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
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
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
        public CorporateController(IMapper mapper,
            IToastNotification toastNotification, ICountryService countryService, ICaseDocumentService caseDocumentService,
            IHttpClientHandler clientHandler, ICustomerCategoryService customerCategoryService, ICaseCommentService caseCommentService,
            IConfiguration configuration, IIdentityTypeService idTypeService, IUserService userService, ICaseAssignmentService caseAssignmentService, IKycService kycService,
            ICustomerCaseService CustomerCaseService, IFileUploader fileUploader, IViewRenderService viewRenderService, ICommonService commonService, RiskAPIController riskAPIController)
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
            var CustomerType = "C";
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, clientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.BusinessTypeList = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, CustomerType, clientId)), "BusinessName", "BusinessName");
            model.EntityType = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, CustomerType, clientId)), "LegalStatus", "LegalStatus");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            string CallC6Screening = string.Empty;
            bool isCaseCreated = false;
            string caseRefId = "";

            CallC6Screening = _configuration["CallC6Screening"];
            if (token.status == 400)
            {
                _toastNotification.AddErrorToastMessage("User is not authorized for screening");
                model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                return View(model);
            }
            else
            {
                //if (!ModelState.IsValid)
                //{
                //    var errors = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();

                //    log.Error($"Model is not valid: {string.Join(", ", errors.ConvertAll(val => val[0].ErrorMessage))}");

                //    model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");

                //    _toastNotification.AddInfoToastMessage(string.Concat(errors.ConvertAll(val => val[0].ErrorMessage)));


                //    return View(model);
                //}
                //else
                //{
                List<string> selectedScreeningOptions = new List<string>();
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
                    

                    if (model.CorporateDetailList.Count != 0)
                    {
                        foreach (CorporateDetailsModel corporateDetailsModel in model.CorporateDetailList)
                        {
                            if (string.IsNullOrEmpty(Regex.Replace(corporateDetailsModel.FirstName, @"\s+", "")))
                            {
                                continue;
                            }
                            corporateDetailsModel.ClientId = _clientHandler.GetClientId();
                            corporateDetailsModel.CreatedBy = _clientHandler.GetUserId();
                            corporateDetailsModel.Threshold = checkThreshold;
                            var customerCodeprefix= _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(model.ClientId));
                            corporateDetailsModel.customerCodeprefix = customerCodeprefix.Prefix;
                            
                            if (corporateDetailsModel.CustomerDetailList != null && corporateDetailsModel.CustomerDetailList.Count != 0)
                            {
                                foreach (CustomeDetailsModel customeDetailsModel in corporateDetailsModel.CustomerDetailList)
                                {
                                    if (string.IsNullOrEmpty(Regex.Replace($"{customeDetailsModel.FirstName}{customeDetailsModel.MiddleName}{customeDetailsModel.LastName}", @"\s+", "")))
                                    {
                                        continue;
                                    }
                                    customeDetailsModel.Companycode = corporateDetailsModel.CompanyCode;
                                    customeDetailsModel.IsChecked= model.IsChecked;
                                    
                                    await ScreenShareholder(customeDetailsModel,IsSanction,count,CallC6Screening);
                                }
                            }
                            var responseList = new List<string>();
                            var resultString = string.Empty;
                            model.CreatedBy = _clientHandler.GetUserId();
                            model.C6Threshold = model.C6Threshold;
                            model.Threshold = model.Threshold;
                            checkThreshold = model.Threshold;
                            model.ClientId = _clientHandler.GetClientId();
                            


                            try
                            {
								customerCaseDTO.CompanyCode = corporateDetailsModel.CompanyCode;
                                customerCaseDTO.Type = "Corporate";
                            corporateDetailsModel.Type = "Corporate";
                            customerCaseDTO.CIFNumber = corporateDetailsModel.CIFNumber;
                            customerCaseDTO.ScreeningOptions= string.Join(",", selectedScreeningOptions);
                            corporateDetailsModel.ScreeningOptions= string.Join(",", selectedScreeningOptions);
                           
                             
                            //    model.CustomerCode = "0";
                            (responseList, customerCaseDTO) = _customerCaseService.SaveCorporateScreeningDetails(_mapper.Map<CorporateScreeningDTO>(corporateDetailsModel));

                                //Document upload 
                                CaseDocumentModel _caseDoc = new CaseDocumentModel();
                                _caseDoc.CaseId = _customerCaseService.GetCaseId(customerCaseDTO.CustomerId).ToString();
                                if (corporateDetailsModel.CaseDocumentsL != null)
                                {
                                    foreach (var item in corporateDetailsModel.CaseDocumentsL)
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

                                string body = string.Empty;
                                using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                                {
                                    body = reader.ReadToEnd();
                                };

                                foreach (string _i in responseList)
                                {


                                    for (int i = 0; i < model.CodeNames.Count; i++)
                                    {
                                        if (model.CodeNames[i] == "PEP" && model.IsChecked[i] == true) { customerCaseDTO.IsPep = true; continue; }
                                        if (model.CodeNames[i] == "Sanction" && model.IsChecked[i] == true) { customerCaseDTO.IsSan = true; continue; }
                                        if (model.CodeNames[i] == "Reputational Risk Exposure" && model.IsChecked[i] == true) { customerCaseDTO.IsRre = true; continue; }
                                        if (model.CodeNames[i] == "Insolvency (UK & Ireland)" && model.IsChecked[i] == true) { customerCaseDTO.IsIns = true; continue; }
                                        if (model.CodeNames[i] == "Disqualified Director (UK Only)" && model.IsChecked[i] == true) { customerCaseDTO.IsDd = true; continue; }
                                        if (model.CodeNames[i] == "Profile of Interest" && model.IsChecked[i] == true) { customerCaseDTO.IsPoi = true; continue; }
                                        if (model.CodeNames[i] == "Regulatory Enforcement List" && model.IsChecked[i] == true) { customerCaseDTO.IsRel = true; continue; }
                                    }

                                    if (count == 1 && IsSanction == true && CallC6Screening == "N")
                                    {
                                        string response = string.Empty;
                                        var searchType = "F";
                                        string data = await _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                                        {
                                            customerdob = corporateDetailsModel.AccomplishedDate.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                                            customerfullname = corporateDetailsModel.FirstName,
                                            customernationality = corporateDetailsModel.AccomplishedCountry,
                                            searchtype = searchType
                                        }, ScreeningService.BACKLIST_SCREENING);

                                        if (!string.IsNullOrEmpty(data))
                                        {
                                            List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                                            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetCaseFullDetailsByCustId(customerCaseDTO.CustomerMasterId);
                                            customerCaseDTO.Id = _CustomerCaseDTO.Id;
                                            _commonService.UpdateSanctionRecords(customerCaseDTO, apiResultModel, String.Empty);



                                            //var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result);

                                            if (customerCaseDTO.sendMail == 1)
                                            {
                                                await SendScreendedMailAsync(body, model, customerCaseDTO);
                                            }

                                            if (customerCaseDTO.IsMatched == 1 && customerCaseDTO.MatchScore >= checkThreshold)
                                            {
                                            caseRefId = _i;
                                            isCaseCreated = true;


                                            //_toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                                            }
                                            else if (customerCaseDTO.IsMatched == 0 && customerCaseDTO.MatchScore == 0)
                                            {
                                                caseRefId = _i;
                                                isCaseCreated = true;

                                            model.IsMatched = 2;
                                                var appBaseUrl = MyHttpContext.AppBaseUrl;
                                                // body = body.Replace("{baseUrl}", appBaseUrl);
                                                //model.Url = appBaseUrl;
                                                //model.createdUserName = HttpContext.Session.GetString("SessUsername");
                                                //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", customerCaseDTO.FirstName, " ", customerCaseDTO.LastName, ". \n"));
                                                
                                            }
                                            else if (customerCaseDTO.MatchScore >= 0 && customerCaseDTO.MatchScore < checkThreshold)
                                            {
                                                customerCaseDTO.ApiResultsjson = customerCaseDTO.ApiResultsjson.Where((source, index) => index < 100).ToList();
                                                model.ApiResultJson = customerCaseDTO.ApiResultsjson;
                                                //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", customerCaseDTO.FirstName, " ", customerCaseDTO.LastName, ". \n"));

                                                
                                            }
                                            else
                                            {
                                            caseRefId = _i;
                                            isCaseCreated = true;

                                            //_toastNotification.AddSuccessToastMessage("Customer Approved");
                                            }//      _toastNotification.AddWarningToastMessage("Customer blocked!");
                                        }
                                        else
                                        {
                                        caseRefId = _i;
                                        isCaseCreated = true;

                                        //_toastNotification.AddWarningToastMessage("Customer blocked! ");
                                        }

                                        
                                    }
                                    else
                                    {
                                        if (CallC6Screening == "Y")
                                        {
                                            customerCaseDTO.IsPep = true;
                                            customerCaseDTO.IsSan = true;
                                        }
                                        var x = await _commonService.CustomerScreeningCall(customerCaseDTO, baseURL, baseC6URL, "CORPORATE", body, checkThreshold, _i);
                                        if (x.sendMail == 1)
                                        {
                                            await SendScreendedMailAsync(body, model, x);
                                        }
                                        if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                                        {
                                            caseRefId = _i;
                                            isCaseCreated = true;
                                        
                                        //_toastNotification.AddWarningToastMessage(string.Concat("Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n"));
                                            //resultString = string.Concat(resultString, "Customer blocked, Case created for ", x.FirstName, " ", x.LastName, ". \n");
                                        }
                                        else if (x.IsMatched == 0 && x.MatchScore == 0)
                                        {
                                            caseRefId = _i;
                                            isCaseCreated = true;


                                            model.IsMatched = 2;
                                            //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there were no cases found for ", x.FirstName, " ", x.LastName, ". \n"));
                                            //return View(model);
                                        }
                                        else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                                        {

                                            //x.ApiResultsjsonCorp = x.ApiResultsjsonCorp.Where((source, index) => index < 100).ToList();
                                            model.ApiResultJson = x.ApiResultsjson;
                                            if (x.CustomerType == "C")
                                            {
                                                caseRefId = _i;
                                                isCaseCreated = true;

                                            //res.ApiResultJsonCorp = model.ApiResultJsonCorp;
                                            foreach (var item in model.ApiResultJson)
                                                {
                                                    var matchrecords = new ApiResultModel();
                                                    matchrecords.matchuid = item.matchuid;
                                                    matchrecords.matchtype = item.matchtype;
                                                    matchrecords.matchcategory = item.matchcategory;
                                                    matchrecords.matchname = item.matchname;
                                                    matchrecords.matchscore = item.matchscore;
                                                    matchrecords.nationality = item.nationality;
                                                    matchrecordsList.Add(matchrecords);
                                                }
                                                //apires += res.ApiResultJsonCorp;
                                                res.ApiResultJson = matchrecordsList;
                                            }
                                           // _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", x.FirstName, " ", x.LastName, ". \n"));
                                            //resultString = string.Concat(resultString, "Customer Approved for ", x.FirstName, " ", x.LastName, ". \n");
                                        }
                                        else
                                        {
                                            caseRefId = _i;
                                            isCaseCreated = true;

                                        _toastNotification.AddInfoToastMessage(string.Concat("Customer Approved for ", x.FirstName, " ", x.LastName, ". \n"));
                                        }
                                    }

                                    CorporateKycDTO corpModel = new CorporateKycDTO();

                                    //To check if risk assessment is enabled for the client.
                                    var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientId));

                                    if (result != null)
                                    {
                                        Console.WriteLine("Generate risk");


                                        KycIndividualDTO imodel = new KycIndividualDTO();
                                        //model.IsPeP = isPep;
                                        var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(corporateDetailsModel), "C", culture, clientId);
                                        if (str1.Result == null)
                                        {
                                            

                                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                            return View(model);
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

                                        var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(corporateDetailsModel), "C", culture, clientId);

                                        if (str.Result == null)
                                        {
                                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                            return View(model);
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
                                        riskModel.CustomerId = _i;
                                        riskModel.CustomerName = corporateDetailsModel.FirstName;
                                        riskModel.MainNationality = corporateDetailsModel.Nationality;
                                        riskModel.ClientId = _clientHandler.GetClientId();
                                        riskModel.CreatedBy = _clientHandler.GetUserId();

                                        if (corporateDetailsModel.Nationality == "0")
                                        {
                                            riskModel.MainNationality = "";
                                        }
                                        else
                                        {
                                            riskModel.MainNationality = corporateDetailsModel.Nationality;
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

                                        //_toastNotification.AddInfoToastMessage(resultString);
                                        //res = model.ApiResultJsonCorp;
                                        var mod = new CorporateScreeningModel();
                                mod.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                                mod.DOB = DateTime.Now.ToString();
                                mod.ApiResultJson = res.ApiResultJson;
                                //mod.ApiResultJsonCorp.Add((ApiResultModel)apires);
                                //return View(mod);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }
                        }


                    }
                    var modl = new CorporateScreeningModel();
                    modl.ClientId = _clientHandler.GetClientId();
                    modl.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
                    modl.DOB = DateTime.Now.ToString();
                    //modl.CodesTable = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(modl.ClientId));

                    //for (int i = 0; i < model.CodesTable.Count; i++)
                    //{
                    //    var val = model.CodesTable[i].ccName;
                    //    modl.CodeNames.Add(val);
                    //    modl.IsChecked.Add(false);
                    //}

                    if (res != null)
                    {
                        modl.ApiResultJson = res.ApiResultJson;
                        //modl.ApiResultJsonCorp.Add((ApiResultModel)apires);
                    }
                    modl.FirstName = "";
                    modl.Mobile = "";
                    modl.Nationality = "";
                    modl.Id = 0;
                model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
                
                model.CaseRefId = caseRefId;
                model.IsCaseCreated = isCaseCreated;
                return View(model);
                    //return RedirectToAction("CorporateScreening");
                //}
            }
        }

        public async Task<IActionResult>  CreateShareholder(ShareholderFormModel model, string screeningType)
        {
            if (model.Shareholders == null || !model.Shareholders.Any())
                return BadRequest("No shareholders provided");

            var parentIdMap = new Dictionary<string, string>(); // TempId -> DB CustomerId
            var clientId = _clientHandler.GetClientId();
            string CallC6Screening = _configuration["CallC6Screening"];
            List<string> selectedScreeningOptions = new List<string>();
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
                bool isCorporate = sh.Type == "Corporate_Corp" || sh.Type == "Individual_Corp";

                CaseModel caseModel = new CaseModel
                {
                    LastName = sh.Name,
                    Type = sh.Type,
                    CustomerType = isCorporate  ? "C" : "I",
                    MatchCategory = isCorporate  ? "CORPORATE" : "INDIVIDUAL",
                    ClientId = sh.ClientID,
                    //CreatedBy = sh.UserId,
                    CompanyCode = sh.CompanyCode,
                    //CompanyName = sh.CompanyName,
                    //Thershold = sh.Thershold,
                    DOB = sh.RegistrationDate?.ToString("yyyy-mm-dd"),
                    CustomerIdType = sh.IdType,
                    CustomerIdNumber = sh.IdNumber,
                    IdIssueDate = sh.IssueDate,
                    IdExpiryDate = sh.IdExpiry,
                    Nationality = sh.Nationality,
                    Share = sh.Share,
                    Designation = sh.Designation,
                    Tradelicense = sh.TradeLicence,
                    CIFNumber = sh.Cif,
                    Residence=sh.Residence,
                    Employer=sh.Employer,
                    GoldenVisa=sh.GoldenVisa,
                    EmployerIndustry=sh.EmployerIndustry,
                    EmployerSector=sh.EmployerSector,
                    SOWSOFCountry=sh.SOWSOFCountry,
                    TradeLicenseAuthority=sh.TradeLicenseAuthority,
                    TradeLicenseSector=sh.TradeLicenseSector,
                    Gender=sh.Gender,
                    Relationship=sh.Relationship,
                    FlagType=sh.FlagType
                };

                // Determine ParentId from TempId (everything before last dot)
                if (sh.DisplayId.Contains("."))
                {
                    var lastDotIndex = sh.DisplayId.LastIndexOf('.');
                    var parentTempId = sh.DisplayId.Substring(0, lastDotIndex);
                    if (parentIdMap.ContainsKey(parentTempId))
                        caseModel.ParentId = parentIdMap[parentTempId];
                }

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
                
                
                // Create case in DB
                var result =  _customerCaseService.Create(_ccDTO);
                if (result.Status != StaticResource.SuccessStatusCode)
                {
                    _toastNotification.AddErrorToastMessage($"Failed to create shareholder {sh.Name}");
                    continue;
                }

                // Get real CustomerId from DB
                var customerId = result.Result.Split('Ø')[1];
                parentIdMap[sh.DisplayId] = customerId; // map TempId -> real CustomerId

                if (!string.IsNullOrEmpty(sh.Document))
                {
                    CaseDocumentModel _caseDoc = new CaseDocumentModel();
                    _caseDoc.CaseId = _customerCaseService.GetCaseId(_ccDTO.CustomerId).ToString();
                    _caseDoc.CreatedBy = _clientHandler.GetUserId();
                    _caseDoc.CreatedOn = DateTime.Now;
                    _caseDoc.ClientId = _clientHandler.GetClientId();

                    _caseDoc.DocumentFileName = sh.Document;
                    _caseDoc.DocumentFullPath = sh.DocumentFullPath; // or stored path
                    _caseDoc.DocumentName = sh.Name;

                    _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                }
                // Screening logic
                if (CallC6Screening == "Y")
                {
                    _ccDTO.IsPep = true;
                    _ccDTO.IsSan = true;
                }

                string body = string.Empty;
                using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                {
                    body = reader.ReadToEnd();
                }

                CustomerCaseDTO screeningResult;
                if (_ccDTO.CustomerType == "I")
                    screeningResult = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, _ccDTO.Threshold, customerId);
                else
                    screeningResult = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "CORPORATE", body, _ccDTO.Threshold, customerId);

                // Send mail if needed
                if (screeningResult.sendMail == 1)
                    await SendScreendedMailAsync(body, caseModel, screeningResult);

                // Show toast messages based on screening
                if (screeningResult.IsMatched == 1 && screeningResult.MatchScore >= _ccDTO.Threshold)
                {
                    caseRefId ??= sh.CompanyCode;

                    // Mark that at least one case was created
                    isCaseCreated = true;
                }
                //_toastNotification.AddWarningToastMessage($"Customer {sh.Name} blocked, Case created");
                else if (screeningResult.IsMatched == 0 && screeningResult.MatchScore == 0)
                {
                    caseRefId ??= sh.CompanyCode;

                    // Mark that at least one case was created
                    isCaseCreated = true;
                }
                //_toastNotification.AddInfoToastMessage($"Customer Approved: no match found for {sh.Name}");
                else if (screeningResult.MatchScore > 0 && screeningResult.MatchScore < _ccDTO.Threshold)
                {
                    caseRefId ??= sh.CompanyCode;

                    // Mark that at least one case was created
                    isCaseCreated = true;
                    //_toastNotification.AddInfoToastMessage($"Customer Approved: match score below threshold for {sh.Name}");
                }
                else
                {
                    caseRefId ??= sh.CompanyCode;

                    // Mark that at least one case was created
                    isCaseCreated = true;
                    //_toastNotification.AddSuccessToastMessage($"Customer {sh.Name} Approved");
                }

                // Optional: Delete temp shareholder record if using temp table
                _customerCaseService.DeleteShareholders(sh.Id);
                var id=_customerCaseService.GetCaseId(sh.CompanyCode);
                _customerCaseService.UpdateCase(id, _ccDTO.CreatedBy);
            }

            if (isCaseCreated && !string.IsNullOrEmpty(caseRefId))
            {
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

                //Attach document if available
                if (Documents != null && Documents.Count > docIndex)
                    {
                           var file = Documents[docIndex];

                    DocumentsModel _documentsModel = _fileUploader.UploadFile(
                        _ccDTO.ClientId,
                        ItemType.caseDocument,
                        _clientHandler.GetBranchId(),
                        file
                    );

                    _ccDTO.DocFullPath = _documentsModel.DocFullPath;
                    _ccDTO.DocumentFileName = file.FileName;
                }

                docIndex++;

                var result = _customerCaseService.CreateShareholdersData(_ccDTO);
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
