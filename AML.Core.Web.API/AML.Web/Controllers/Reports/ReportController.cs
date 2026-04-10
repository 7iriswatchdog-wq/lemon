using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.Service.UserGroup;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.Report;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.TransactionScreening;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.UserGroup;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Report;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.TransactionScreening;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CaseProcess;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.Sanction;
using AML.ViewModel.ViewModels.TransactionScreening;
using AML.ViewModel.ViewModels.User;
using AML.ViewModel.ViewModels.UserGroup;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.EMMA;
using DocumentFormat.OpenXml.Spreadsheet;
using Fingers10.ExcelExport.ActionResults;
using Fingers10.ExcelExport.Attributes;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using NToastNotify;
using SixLabors.ImageSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AML.Core.Service.Common.CommonService;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Font = iTextSharp.text.Font;
using Image = iTextSharp.text.Image;
using Rectangle = iTextSharp.text.Rectangle;
using SourceType = AML.Core.DataContract.Enum.SourceType;


namespace AML.Web.Controllers.Reports
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class ReportController : Controller
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
        private readonly IToastNotification _toastNotification;
        private ICommonService _commonService;
        private string baseC6URL = string.Empty;
        private string _c6Username;
        private IRiskService _riskService;
        private IKycService _kycService;
        private ILovMasterService _lovMasterService;
        private IUserGroupService _UserGroupService;
        

        public ReportController(IUserService userService, IMapper mapper, IReportService reportService,
         IHttpClientHandler clientHandler, ICaseCommentService caseCommentService, ICustomerCategoryService customerCategoryService,
        IViewRenderService viewRenderService, IExportDataService exportService, ICustomerScreeningService customerScreeningService, IUserGroupService UserGroupService,
        ICustomerCaseService customerCaseService, ICaseDocumentService caseDocumentService, IFreeSourceRepository freeSourceRepository, IKycService kycService, ILovMasterService lovMasterService, IRiskService RiskService,
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
            _riskService = RiskService;
            _lovMasterService = lovMasterService;
            _kycService = kycService;
            baseC6URL = clientDetails?.C6BaseUrl;
            _UserGroupService = UserGroupService;
            

        }
        public IActionResult SanctionLogs()
        {
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now;
            model.EndDate = System.DateTime.Now;
            return View(model);
        }

        private bool normaliseAndCheckIfContains(string item, string searchValue)
        {
            return item != null && item.ToLower().Contains(searchValue?.ToLower());
        }

        public JsonResult SanctionList(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            List<SanctionScreeningModel> list = _mapper.Map<List<SanctionScreeningModel>>(_customerScreeningService.GetAllByDate(model.StartDate.ToString(), model.EndDate.ToString(), clientId));

            if (!string.IsNullOrEmpty(model.search.value))
            {
                list = list.Where(m =>
                    normaliseAndCheckIfContains(m.CustomerName, model.search.value) ||
                    normaliseAndCheckIfContains(m.Nationality, model.search.value) ||
                    normaliseAndCheckIfContains(m.MatchCategory, model.search.value)
                ).ToList();
            }
            var data = Sort(list, model.columns[model.order[0].column].data ?? "matchScore", model.order[0].dir ?? "dec")

.Skip(model.start)

.Take(model.length)

.ToList();

            //var data = list.Skip(model.start).Take(model.length).ToList();
            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = list.Count,//totalResultsCount,
                recordsFiltered = list.Count,//filteredResultsCount,
                data = data,
            });
        }
        public IActionResult ExportSanctionList(ReportLogSearchModel model)
        {
            return Json("");
        }

        public IActionResult CustomerListingLogs(int type)
        {
            var model = new ReportLogSearchModel();

            var items = from MatchingType d in Enum.GetValues(typeof(MatchingType))
                        select new { Id = (int)d, Name = d.ToString() };
            model.StartDate = System.DateTime.Now;
            model.EndDate = System.DateTime.Now;
            model.MatchType = "1";
            model.MatchTypeList = new SelectList(items, "Id", "Name");
            if (type == 1)
            {
                DateTime dt = new DateTime();
                model.StartDate = dt;
                model.MatchType = null;
                model.MatchType = "1";
                model.MatchTypeList = new SelectList(items, "Id", "Name");
            }
            return View(model);
        }
        [HttpPost("Report/CustomerListing")]
public IActionResult CustomerList(DataTableModel model, 
    string startDate, string endDate, string match,
    int orderColumn = 0, string orderDirection = "asc") // Added sorting parameters
{
    try
    {
        if (string.IsNullOrEmpty(endDate))
        {
            endDate = DateTime.Now.ToString("yyyy-MM-dd");
        }

        var clientId = _clientHandler.GetClientId();
        var data = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCustomerReportList(new ReportLogSearchModel()
        {
            StartDate = DateTime.TryParse(startDate, out var start) ? start : DateTime.MinValue,
            EndDate = DateTime.TryParse(endDate, out var end) ? end : DateTime.Now,
            MatchType = match,
            ClientId = clientId
        }));

        // Apply sorting
        IOrderedEnumerable<CaseReportListModel> sortedData;
                switch (orderColumn)
                {
                    case 0: // CustomerType
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.CustomerType ?? string.Empty) :
                            data.OrderByDescending(x => x.CustomerType ?? string.Empty);
                        break;
                    case 1: // CustomerID
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.CustomerID ?? string.Empty) :
                            data.OrderByDescending(x => x.CustomerID ?? string.Empty);
                        break;
                    case 2: // CustomerName
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.CustomerName ?? string.Empty) :
                            data.OrderByDescending(x => x.CustomerName ?? string.Empty);
                        break;
                    case 3: // Dob (special handling for date strings)
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => DateTime.TryParse(x.dob, out var dobDate) ? dobDate : DateTime.MinValue) :
                            data.OrderByDescending(x => DateTime.TryParse(x.dob, out var dobDate) ? dobDate : DateTime.MaxValue);
                        break;
                    case 4: // Nationality
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.Nationality ?? string.Empty) :
                            data.OrderByDescending(x => x.Nationality ?? string.Empty);
                        break;
                    case 5: // Match
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.Match ?? string.Empty) :
                            data.OrderByDescending(x => x.Match ?? string.Empty);
                        break;
                    case 6: // CreatedBy
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.CreatedBy ?? string.Empty) :
                            data.OrderByDescending(x => x.CreatedBy ?? string.Empty);
                        break;
                    case 7: // CreatedOn (assuming this is a DateTime)
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.CreatedOn) :
                            data.OrderByDescending(x => x.CreatedOn);
                        break;
                    default: // Default sort by CustomerType
                        sortedData = orderDirection == "asc" ?
                            data.OrderBy(x => x.CustomerType ?? string.Empty) :
                            data.OrderByDescending(x => x.CustomerType ?? string.Empty);
                        break;
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(model.search?.value))
        {
            var searchValue = model.search.value.ToLower();
            sortedData = (IOrderedEnumerable<CaseReportListModel>)sortedData
                .Where(x => (x.CustomerType?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.CustomerID?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.CustomerName?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.dob?.ToString()?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.Nationality?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.Match?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.CreatedBy?.ToLower()?.Contains(searchValue) ?? false) ||
                         (x.CreatedOn.ToString()?.ToLower()?.Contains(searchValue) ?? false));
        }

        // Apply pagination
        var totalRecords = sortedData.Count();
        var paginatedData = sortedData
            .Skip(model.start)
            .Take(model.length)
            .ToList();

        return Json(new
        {
            draw = model.draw,
            recordsTotal = totalRecords,
            recordsFiltered = totalRecords,
            data = paginatedData,
            allRows = sortedData.ToList()
        });
    }
    catch (Exception ex)
    {
        ErrorLogDTO error = new ErrorLogDTO()
        {
            created_on = DateTime.Now,
            createdBy = _clientHandler.GetUserId(),
            description = ex.InnerException?.Message ?? ex.Message,
            module = "CaseManagement_Process",
            comments = "API call Error while Getting data from manogo db",
            status_code = 404
        };

        var result = _commonService.createErrorlog(error);
        _toastNotification.AddErrorToastMessage(error.description);
        Console.Error.WriteLine(ex);

        return Json(new
        {
            draw = model.draw,
            recordsTotal = 0,
            recordsFiltered = 0,
            data = new List<CaseReportListModel>(),
            error = error.description
        });
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


        [HttpGet("Report/ExportCustomerList")]
        public async Task<IActionResult> ExportCustomerList(string startDate, string endDate, string match, bool isPDF)
        {
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCustomerReportList(new ReportLogSearchModel()
            {
                StartDate = Convert.ToDateTime(startDate),
                EndDate = Convert.ToDateTime(endDate),
                MatchType = match,
                ClientId = _clientHandler.GetClientId()
            }));
            #region match Type
            string matchType = "";
            switch (match)
            {
                case "1":
                    matchType = "All";
                    break;
                case "2":
                    matchType = "Matched";
                    break;
                case "3":
                    matchType = "Not Matched";
                    break;
                default:
                    matchType = "";
                    break;
            }
            #endregion
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());
            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

            //Excel Export
            if (!isPDF)
            {
                CustomerReportExcelModel excelModel = new CustomerReportExcelModel();
                List<CustomerReportExcelModel> excelData = new List<CustomerReportExcelModel>();
                string details = "Report               :   Customer Listing" +
                    "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n" +
                    "Match Type       :   " + matchType + "\r\n" +
                    "Created by        :   " + companyName + "\r\n";
                excelModel.Details = details;

                excelData = (from res in abc
                                 select new CustomerReportExcelModel
                                 {

                                     CustomerId = res.CustomerID,
                                     CustomerType = res.CustomerType,
                                     CustomerName = res.CustomerName,
                                     CaseStatus = res.Match,
                                     CreatedBy = res.CreatedBy,
                                     CreationDate = res.CreatedOn,
                                 }).ToList();
                excelData.Add(excelModel);
                return new ExcelResult<CustomerReportExcelModel>(excelData, "CustomerLogs", "Customer_Listing_" + DateTime.Now.Ticks);
            }

            int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            downloadModel.Data = abc;
            downloadModel.TotalRows = abc.Count;
            var result = await _viewRenderService.RenderToStringAsync("Report/CustomerListDownload", downloadModel);

            string body = string.Empty;
            
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
                header.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                PdfPCell hd = new PdfPCell(new Phrase("Report                :   Customer Listing"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                PdfPCell dateRange = new PdfPCell(new Phrase("Date Range        :   " + startDate + "  to  " + endDate));
                PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                PdfPCell caseStat = new PdfPCell(new Phrase("Match Type        :   " + matchType));
                PdfPCell _caseStat = new PdfPCell(new Phrase("\n"));
                PdfPCell createdBy = new PdfPCell(new Phrase("Created by         :   " + companyName));
                PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));
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
                header.AddCell(createdBy);
                header.AddCell(_createdBy);
                document.Add(header);

                PdfPTable table = new PdfPTable(7);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 2f, 1f, 1f };
                table.SetWidths(widths);
                table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                table.SpacingAfter = 30f;
                PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell1.HorizontalAlignment = 1;
                cell1.VerticalAlignment = 1;
                cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell1.FixedHeight = 30f;
                table.AddCell(cell1);

                PdfPCell cell2 = new PdfPCell(new Phrase("CUSTOMER TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell2.HorizontalAlignment = 1;
                cell2.VerticalAlignment = 1;
                cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell2.FixedHeight = 30f;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("CUSTOMER ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("MATCHED", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("CREATION DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerType, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Match, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedBy, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

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
                footer2.AddCell("Date of Extraction :   " + DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss") + " GST");
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


                //var result = data.ToString();




                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Customer_List_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else
                {
                    var model = new ReportLogSearchModel();

                    var items = from MatchingType d in Enum.GetValues(typeof(MatchingType))
                                select new { Id = (int)d, Name = d.ToString() };
                    model.StartDate = Convert.ToDateTime(startDate);
                    model.EndDate = Convert.ToDateTime(endDate);
                    model.MatchType = match;
                    model.MatchTypeList = new SelectList(items, "Id", "Name");

                    return View("CustomerListingLogs", model);
                }
            }
        }
        public IActionResult OngoingmonitoringSchedulerLogs()
        {
            return View();
        }
        [HttpPost("Report/OngoingmonitoringSchedulerLog")]
        public IActionResult OngoingmonitoringSchedulerLog(DataTableModel model, string startDate = null, string endDate = null)
        {
            var clientId = _clientHandler.GetClientId();
            List<DigiSchedulerLogModel> abc;
            
            // Use date-filtered method if dates are provided, otherwise use the original method
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                abc = _mapper.Map<List<DigiSchedulerLogModel>>(_reportService.GetDigiSchedulerList(clientId, startDate, endDate));
            }
            else
            {
                abc = _mapper.Map<List<DigiSchedulerLogModel>>(_reportService.GetDigiSchedulerList(clientId));
            }
            
            // Apply search filtering if search value is provided
            if (!string.IsNullOrEmpty(model.search?.value))
            {
                abc = abc.Where(m => m.Source.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var data = abc.Skip(model.start).Take(model.length).ToList();

            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = abc.Count,//totalResultsCount,
                recordsFiltered = abc.Count,//filteredResultsCount,
                data = data,
                allRows = abc
            });
        }


        public IActionResult SanctionDatabaseLog()
        {
            return View();
        }
        [HttpPost("Report/SanctionDBUploadListing")]
        public IActionResult SanctionDBUploadList(DataTableModel model,
    string startDate, string endDate, string match,
    int orderColumn = 0, string orderDirection = "asc")
        {
            // Get base data from service
            var data = _mapper.Map<List<UploadLogsListModel>>(_reportService.GetUploadLogstList());

            // Apply date filtering if provided
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                var start = DateTime.Parse(startDate);
                var end = DateTime.Parse(endDate).AddDays(1); // Include the entire end date
                data = data
    .Where(x => DateTime.Parse(x.CreatedOn) >= start &&
                DateTime.Parse(x.CreatedOn) < end)
    .ToList();
            }

            // Apply match filtering if provided
            if (!string.IsNullOrEmpty(match))
            {
                data = data.Where(x => x.Source?.Contains(match, StringComparison.OrdinalIgnoreCase) ?? false).ToList();
            }

            // Apply sorting
            IOrderedEnumerable<UploadLogsListModel> sortedData;
            switch (orderColumn)
            {
                case 0: // Id
                    sortedData = orderDirection == "asc" ?
                        data.OrderBy(x => x.Id) :
                        data.OrderByDescending(x => x.Id);
                    break;
                case 1: // Source
                    sortedData = orderDirection == "asc" ?
                        data.OrderBy(x => x.Source) :
                        data.OrderByDescending(x => x.Source);
                    break;
                case 2: // Uploaded On
                    sortedData = orderDirection == "asc" ?
                        data.OrderBy(x => x.CreatedOn) :
                        data.OrderByDescending(x => x.CreatedOn);
                    break;
                case 3: // Total Records
                    sortedData = orderDirection == "asc" ?
                        data.OrderBy(x => x.totalrecords) :
                        data.OrderByDescending(x => x.totalrecords);
                    break;
                default: // Default sort by Id
                    sortedData = orderDirection == "asc" ?
                        data.OrderBy(x => x.Id) :
                        data.OrderByDescending(x => x.Id);
                    break;
            }

            // Apply search filter if provided
            //if (!string.IsNullOrEmpty(model.search?.value))
            //{
            //    var searchValue = model.search.value.ToLower();
            //    sortedData = (IOrderedEnumerable<UploadLogsListModel>)sortedData
            //        .Where(x => (x.Source?.ToLower()?.Contains(searchValue) ?? false) ||
            //                   (x.Id.ToString().Contains(searchValue)) ||
            //                   (x.totalrecords.ToString().Contains(searchValue)) ||
            //                   (x.CreatedOn.ToString().ToLower().Contains(searchValue)));
            //}

            // Apply pagination
            var totalRecords = sortedData.Count();
            var paginatedData = sortedData
                .Skip(model.start)
                .Take(model.length)
                .ToList();

            return Json(new
            {
                draw = model.draw,
                recordsTotal = totalRecords,
                recordsFiltered = totalRecords,
                data = paginatedData,
                allRows = sortedData.ToList()
            });
        }

        public IActionResult InternalWatchlistUpdateLogs()
        {
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now;
            model.EndDate = System.DateTime.Now;
            var items = from SourceType d in Enum.GetValues(typeof(SourceType))
                        select new { Id = (int)d, Name = d.ToString() };
            model.SourceTypeList = new SelectList(items, "Id", "Name");
            return View(model);
        }
        public async Task<IActionResult> InternalWatchlistUpdateList(ReportLogSearchModel model)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();
                if (model.SourceType == "UAE IEC LIST")
                {
                    var response = await _clientHandler.PostAsync(new { StartDate = model.StartDate.ToString("dd/MM/yyyy"), EndDate = model.EndDate.ToString("dd/MM/yyyy"), Type = model.SourceType }, ScreeningService.INTERNALWATCHLISTREPORT);
                    return Json(response);
                }
                else
                {
                    var response = await _clientHandler.PostAsync(new { StartDate = model.StartDate.ToString("dd/MM/yyyy"), EndDate = model.EndDate.ToString("dd/MM/yyyy"), Type = model.SourceType, client_id = clientId }, ScreeningService.INTERNALWATCHLISTREPORT);
                    return Json(response);
                }


            }
            catch(Exception ex)
            {
                Console.WriteLine($"exception in calling place {ex.InnerException + ex.Message}");
                return Json(null);

            }
        }
        //public void CreateTerrorristList()
        //{
        //    TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
        //    if (token.status == 400)
        //    {
        //        _toastNotification.AddErrorToastMessage("User is not authorized for screening");
        //    }
        //    else
        //    {
        //        if (true)
        //        {
        //            Console.WriteLine($"internal baseC6URL {baseC6URL + ScreeningService.GETTERRORISLLIST}");
        //            var result = _clientHandler.GetAsync(token, baseC6URL + ScreeningService.GETTERRORISLLIST).Result;
        //            if (true)
        //            {
        //                try
        //                {
        //                    Console.WriteLine($"internal before jsonlist {result}");
        //                    List<TerroristList> jsonList = JsonConvert.DeserializeObject<List<TerroristList>>(result);
        //                    Console.WriteLine($"internal after jsonlist {jsonList.Count}");
        //                    foreach (var item in jsonList)
        //                    {
        //                        var request = new { fullname = item.FullName, dob = item.DOB, nationality = item.Nationality, type = item.Source, category = item.Type };
        //                        var apiresponse = _clientHandler.PostAsync(request, ScreeningService.ADDTOBLACKLIST).Result;
        //                    }
        //                    Console.WriteLine($"internal after foreach ");
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine($"exception {ex.InnerException+ex.Message}");
        //                    //SendScreendedMailAsync(body, model, x);

        //                }

        //            }


        //        }
        //    }
        //}

        /*private  void SendScreendedMailAsync(string body, CaseModel model, CustomerCaseDTO x)
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
        }*/
        public IActionResult DownloadWatchList(List<ReportInternalWatchListLogModel> downloadModel)
        {
            return View(downloadModel);
        }
        [HttpGet("Report/ExportWatchList")]
        public async Task<IActionResult> ExportWatchList(DateTime startDate, DateTime endDate, string sourceType, bool isPDF)
        {
            var response = await _clientHandler.PostAsync(new { StartDate = startDate.ToString("dd/MM/yyyy"), EndDate = endDate.ToString("dd/MM/yyyy"), Type = sourceType }, ScreeningService.INTERNALWATCHLISTREPORT);
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());
            List<ReportInternalWatchListLogModel> downloadModel = new List<ReportInternalWatchListLogModel>();
            string details = "Report               :   Internal Watchlist Update Logs<br /><br />" + 
                "Date Range       :   " + startDate.ToString("dd/MM/yyyy") + "  to  " + endDate.ToString("dd/MM/yyyy") + "<br />" + 
                "Source Type      :   " + sourceType + "<br />" +
                "Created by        :   " + clientData.ClientName + "<br />";

            if (!string.IsNullOrEmpty(response))
            {
                downloadModel = JsonConvert.DeserializeObject<List<ReportInternalWatchListLogModel>>(response);
            }

            if (!isPDF)
            {
                List<InternalReportExcelModel> excelData = new List<InternalReportExcelModel>();
                InternalReportExcelModel excelModel = new InternalReportExcelModel();
                string reportdetails = "Report               :   Internal Watchlist Update Logs\r\n" + 
                "Date Range       :   " + startDate.ToString("dd/MM/yyyy") + "  to  " + endDate.ToString("dd/MM/yyyy") + "\r\n" +
                "Source Type      :   " + sourceType + "\r\n" +
                "Created by        :   " + clientData.ClientName + "\r\n";

                excelData = (from res in downloadModel
                                 select new InternalReportExcelModel
                                 {
                                     uid = res.uid,
                                     category = res.category,
                                     fullname = res.fullname,
                                     nationality = res.nationality,
                                     dob = res.dob,
                                     type = res.type
                                 }).ToList();
                excelModel.Details = reportdetails;
                excelData.Add(excelModel);
                return new ExcelResult<InternalReportExcelModel>(excelData, "Internal Watchlist Report", "internal_watchlist_Report_" + DateTime.Now.Ticks);
            }
            else
            {
                downloadModel = JsonConvert.DeserializeObject<List<ReportInternalWatchListLogModel>>(response);
                var result = details;
                result += await _viewRenderService.RenderToStringAsync("Report/DownloadWatchList", downloadModel);
                result += "<br /><br />Date of Extraction : " + DateTime.Now;
                var file = _exportService.ExportData<ReportInternalWatchListLogModel>(downloadModel, result, (int)OperationType.PDF, "InternalWatchListLogReport_" + DateTime.Now.Ticks);
                if (file != null)
                {
                    return file;
                }
            }
            var model = new ReportLogSearchModel();
            model.StartDate = startDate;
            model.EndDate = endDate;
            var items = from SourceType d in Enum.GetValues(typeof(SourceType))
                        select new { Id = (int)d, Name = d.ToString() };
            model.SourceTypeList = new SelectList(items, "Id", "Name");

            return View("/report/InternalWatchlistUpdateLogs", model);
        }

        public class InternalReportExcelModel
        {
            public int Id { get; set; }
            [IncludeInReport(Order = 1)]
            [Display(Name = "UID")]
            public string uid { get; set; }
            [IncludeInReport(Order = 2)]
            [Display(Name = "Category")]
            public string category { get; set; }
            [IncludeInReport(Order = 3)]
            [Display(Name = "FullName")]
            public string fullname { get; set; }
            [IncludeInReport(Order = 4)]
            [Display(Name = "Nationality")]
            public string nationality { get; set; }
            [IncludeInReport(Order = 5)]
            [Display(Name = "DOB")]
            public string dob { get; set; }
            [IncludeInReport(Order = 6)]
            [Display(Name = "Type")]
            public string type { get; set; }
            [IncludeInReport(Order = 7)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }
        }

        [HttpGet("Report/CaseReport")]
        public IActionResult CaseReport(int type,string schedulerTrackerId,string option)
        {
            TempData["option"] = option;
            TempData["schedulerTrackerId"] = schedulerTrackerId;
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now.AddYears(-1);
            //model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            //var items = from ReportsCaseStatus d in Enum.GetValues(typeof(ReportsCaseStatus))
            //            select new { Id = (int)d, Name = d.ToString() };
            //model.CaseStatusList = new SelectList(items, "Id", "Name");

            var items = from ReportsCaseStatus d in Enum.GetValues(typeof(ReportsCaseStatus))
                        select new
                        {
                            Id = (int)d,
                            Name = Regex.Replace(d.ToString(), "(\\B[A-Z])", " $1")
                        };

            model.CaseStatusList = new SelectList(items, "Id", "Name");
            model.CaseStatus = "10"; //TODO: Remove/Update the default value
            var clientID = _clientHandler.GetClientId();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientID))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            if (type == 1)
            {
                model.StartDate = new DateTime();
                model.CaseStatus = "0";
            }
            else if (type == 2)
            {
                model.StartDate = new DateTime();
                model.CaseStatus = "5";
            }
            else if (type == 3)
            {
                model.StartDate = new DateTime();
                model.CaseStatus = "3";
            }
            return View(model);
        }
        [HttpPost("Report/CaseReportCustompagination")]
        public JsonResult CustomPagination(DataTableModel model,string startDate,
    string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string caseStatusChange, string riskLevel,string option,string schedulerTrackerId)
        {
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();

            
            var userId = _clientHandler.GetUserId();


            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            List<CaseReportListModel> abc = new List<CaseReportListModel>();
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            //if (cust_type == "CORPORATE")
            //{
            //    cust_type = "C";
            //}
            //else if (cust_type == "INDIVIDUAL")
            //{
            //    cust_type = "I";
            //}

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
            if (option == "OnGoing")
            {
                abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportListBySchedulerTrackerId(new CaseReportRequestDTO
                {
                    //User = userID,
                   SchedulerTrackerId=schedulerTrackerId,
                   ClientId= _clientHandler.GetClientId()

                }));

            }
            else
            {
                if (searchValue != "" && searchValue != null)
                {
                    abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportListBySearch(new CaseReportRequestDTO
                    {
                        //User = userID,
                        StartDate = startDate,
                        EndDate = endDate,
                        caseStatus = caseStatus,
                        Cust_type = cust_type,
                        // UpdatedByUserId = updatedByUserID,
                        ClientId = _clientHandler.GetClientId(),
                        SearchValue = searchValue,
                        createdBy = createdBy,
                        matchscore = matchScore,
                        riskLevel = riskLevel

                    }));
                }
                else
                {
                    abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportList(new CaseReportRequestDTO
                    {
                        //User = userID,
                        StartDate = startDate,
                        EndDate = endDate,
                        caseStatus = caseStatus,
                        Cust_type = cust_type,
                        // UpdatedByUserId = updatedByUserID,
                        ClientId = _clientHandler.GetClientId(),
                        createdBy = createdBy,
                        matchscore = matchScore,
                        riskLevel = riskLevel

                    }));

                }
            }
            int totalcount = abc.Count;

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
            // Get the base data


            // Apply sorting based on the column and direction


            // Apply pagination

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
        //[HttpGet("Report/ExportCaseReport")]
        //public async Task<IActionResult> ExportCaseReport(string userID, string updatedByUserID, string startDate, string endDate, string status, string cust_type, bool isPDF)
        //{
        //    if (endDate == null)
        //    {
        //        endDate = System.DateTime.Now.ToString();
        //    }
        //    if (cust_type == "INDIVIDUAL")
        //    {
        //        cust_type = "I";
        //    }
        //    else if (cust_type == "CORPORATE")
        //    {
        //        cust_type = "C";
        //    }
        //    else if (cust_type == "SHAREHOLDER")
        //    {
        //        cust_type = "S";
        //    }
        //    //else
        //    //{
        //    //    cust_type = " ";
        //    //}
        //    List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportList(new CaseReportRequestDTO()
        //    {
        //        //User = userID,
        //        //StartDate = startDate,
        //        //EndDate = endDate,
        //        //Status = status,
        //        //Cust_type = cust_type,
        //        //UpdatedByUserId = updatedByUserID,
        //        //ClientId = _clientHandler.GetClientId()
        //    }));
        //    int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
        //    CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
        //    downloadModel.Data = abc;
        //    downloadModel.TotalRows = abc.Count;
        //    var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());

        //    #region FiltersCheck
        //    string filter = "";
        //    if (userID != "0")
        //    {
        //        filter += "User ";
        //    }

        //    if (updatedByUserID != "0")
        //    {
        //        if (filter == "")
        //        {
        //            filter += "Updated By";
        //        }
        //        else
        //        {
        //            filter += ", Updated By";
        //        }
        //    }
        //    if (cust_type != "0")
        //    {
        //        if (filter == "")
        //        {
        //            filter += "Customer Type";
        //            ;
        //        }
        //        else
        //        {
        //            filter += ", Customer Type";
        //        }
        //    }
        //    #endregion
        //    #region status
        //    string caseStatus = "";
        //    switch (status)
        //    {
        //        case "0":
        //            caseStatus = "Pending";
        //            break;
        //        case "1":
        //            caseStatus = "Assigned";
        //            break;
        //        case "2":
        //            caseStatus = "Approved";
        //            break;
        //        case "3":
        //            caseStatus = "Rejected";
        //            break;
        //        case "4":
        //            caseStatus = "Closed";
        //            break;
        //        case "5":
        //            caseStatus = "Auto Approved";
        //            break;
        //        case "6":
        //            caseStatus = "Pending Case Created From Daily Scheduler";
        //            break;
        //        case "10":
        //            caseStatus = "All";
        //            break;
        //        default:
        //            caseStatus = "";
        //            break;
        //    }
        //    #endregion

        //    //Excel Export
        //    if (!isPDF)
        //    {
        //        CaseReportExcelModel excelModel= new CaseReportExcelModel();
        //        List<CaseReportExcelModel> excelData =new List<CaseReportExcelModel>();
        //        //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
        //        string details = "Report               :   Case Report" + "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n" +
        //                                  "Filters Applied  :   " + filter + "\r\n" + "Created by        :   " + clientData.ClientName + "\r\n" +
        //                                  "Case Status       :   " + caseStatus;
        //        excelModel.Details = details;

        //        excelData = (from res in abc
        //                         select new CaseReportExcelModel
        //                         {

        //                             CustomerId = res.CustomerID,
        //                             CustomerType = res.CustomerType,
        //                             CustomerName = res.CustomerName,
        //                             CaseStatus = res.Status,
        //                             CompanyCode = res.CompanyCode,
        //                             CreatedBy = res.CreatedBy,
        //                             CreationDate = res.CreatedOn,

        //                         }).ToList();
        //        excelData.Add(excelModel);

        //        return new ExcelResult<CaseReportExcelModel>((excelData), "Case Report", "Case_Report_" + DateTime.Now.Ticks);
        //    }

        //    //var result = await _viewRenderService.RenderToStringAsync("Report/CaseReportDownload", downloadModel);

        //    var logos = "wwwroot/img/" + clientData.DocumentFileName;
        //    var companyName = clientData.ClientName;

        //    string body = string.Empty;
        //    using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
        //    {
        //        Document document = new Document(PageSize.A4, 15, 15, 15, 15);
        //        PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
        //        document.Open();

        //        document.Add(new Paragraph("\n"));

        //        PdfPTable logo = new PdfPTable(2);
        //        logo.TotalWidth = 550f;
        //        float[] logowidth = new float[] { 3f, 0.5f };
        //        logo.SetWidths(logowidth);
        //        logo.LockedWidth = true;
        //        logo.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
        //        logo.DefaultCell.VerticalAlignment = 1;
        //        logo.DefaultCell.HorizontalAlignment = 1;
        //        logo.SpacingBefore = 20f;
        //        logo.SpacingAfter = 30f;
        //        logo.DefaultCell.Border = 0;
        //        PdfPCell compname = new PdfPCell(new Phrase(companyName.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
        //        compname.FixedHeight = 40f;
        //        compname.VerticalAlignment = 1;
        //        compname.HorizontalAlignment = 1;
        //        compname.Border = 0;
        //        logo.AddCell(compname);
        //        if (logos != null)
        //        {
        //            string url = logos;
        //            Image tif = Image.GetInstance(url);
        //            tif.ScalePercent(1f);
        //            tif.SpacingBefore = 20f;
        //            logo.AddCell(tif);
        //            document.Add(logo);
        //        }



        //        PdfPTable header = new PdfPTable(1);
        //        header.TotalWidth = 550f;
        //        header.LockedWidth = true;
        //        header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
        //        header.SpacingAfter = 30f;
        //        header.DefaultCell.Border = 0;
        //        //header.DefaultCell.ExtraParagraphSpace= 1;
        //        PdfPCell hd = new PdfPCell(new Phrase(          "Report               :   Case Report"));
        //        PdfPCell _hd = new PdfPCell(new Phrase("\n"));
        //        PdfPCell dateRange = new PdfPCell(new Phrase(   "Date Range       :   "+startDate + "  to  " +endDate));
        //        PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
        //        PdfPCell filters = new PdfPCell(new Phrase(     "Filters Applied   :   "+filter));
        //        PdfPCell _filters = new PdfPCell(new Phrase("\n"));
        //        PdfPCell createdBy = new PdfPCell(new Phrase(   "Created by        :   "+clientData.ClientName));
        //        PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));
        //        PdfPCell caseStat = new PdfPCell(new Phrase(    "Case Status      :   "+ caseStatus));
        //        PdfPCell _caseStat = new PdfPCell(new Phrase("\n"));

        //        //hd.HorizontalAlignment = Element.ALIGN_LEFT;
        //        //hd.FixedHeight = 20f;
        //        //hd.VerticalAlignment = 1;
        //        hd.Border = 0;
        //        dateRange.Border= 0;
        //        filters.Border = 0;
        //        createdBy.Border = 0;
        //        caseStat.Border = 0;
        //        _hd.Border = 0;
        //        _dateRange.Border = 0;
        //        _filters.Border = 0;
        //        _createdBy.Border = 0;
        //        _caseStat.Border = 0;
        //        header.AddCell(hd);
        //        header.AddCell(_hd);
        //        header.AddCell(dateRange);
        //        header.AddCell(_dateRange);
        //        header.AddCell(filters);
        //        header.AddCell(_filters);
        //        header.AddCell(createdBy);
        //        header.AddCell(_createdBy);
        //        header.AddCell(caseStat);
        //        header.AddCell(_caseStat);
        //        document.Add(header);
        //        PdfPTable table = new PdfPTable(8);
        //        table.TotalWidth = 550f;
        //        table.LockedWidth = true;
        //        float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 2f, 1f, 1f, 1f };
        //        table.SetWidths(widths);
        //        table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
        //        table.SpacingAfter = 30f;
        //        PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell1.HorizontalAlignment = 1;
        //        cell1.VerticalAlignment = 1;
        //        cell1.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell1.FixedHeight = 30f;
        //        table.AddCell(cell1);

        //        PdfPCell cell2 = new PdfPCell(new Phrase("CUSTOMER ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell2.HorizontalAlignment = 1;
        //        cell2.VerticalAlignment = 1;
        //        cell2.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell2.FixedHeight = 30f;
        //        table.AddCell(cell2);
        //        PdfPCell cell3 = new PdfPCell(new Phrase("CUSTOMER TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell3.HorizontalAlignment = 1;
        //        cell3.VerticalAlignment = 1;
        //        cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell3.FixedHeight = 30f;
        //        table.AddCell(cell3);
        //        PdfPCell cell4 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell4.HorizontalAlignment = 1;
        //        cell4.VerticalAlignment = 1;
        //        cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell4.FixedHeight = 30f;
        //        table.AddCell(cell4);
        //        PdfPCell cell5 = new PdfPCell(new Phrase("CASE STATUS", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell5.HorizontalAlignment = 1;
        //        cell5.VerticalAlignment = 1;
        //        cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell5.FixedHeight = 30f;
        //        table.AddCell(cell5);
        //        PdfPCell cell6 = new PdfPCell(new Phrase("COMPANY CODE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell6.HorizontalAlignment = 1;
        //        cell6.VerticalAlignment = 1;
        //        cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell6.FixedHeight = 30f;
        //        table.AddCell(cell6);
        //        PdfPCell cell7 = new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell7.HorizontalAlignment = 1;
        //        cell7.VerticalAlignment = 1;
        //        cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell7.FixedHeight = 30f;
        //        table.AddCell(cell7);
        //        PdfPCell cell8 = new PdfPCell(new Phrase("CREATION DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
        //        cell8.HorizontalAlignment = 1;
        //        cell8.VerticalAlignment = 1;
        //        cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
        //        cell8.FixedHeight = 30f;
        //        table.AddCell(cell8);
        //        for (int i = 0; i < downloadModel.Data.Count; i++)
        //        {
        //            table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].CustomerID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].CustomerType, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].Status, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].CompanyCode.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].CreatedBy, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
        //            table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

        //        }
        //        document.Add(table);




        //        document.Add(new Paragraph("\n"));
        //        iTextSharp.text.pdf.draw.LineSeparator line1 = new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1);
        //        document.Add(new Chunk(line1));

        //        PdfPTable footer2 = new PdfPTable(1);
        //        footer2.TotalWidth = 550f;
        //        footer2.LockedWidth = true;
        //        footer2.DefaultCell.Border = 0;
        //        footer2.AddCell("Computer generated report; hence no signature is required. ");
        //        footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
        //        document.Add(footer2);

        //        PdfContentByte content = writer.DirectContent;
        //        Rectangle rectangle = new Rectangle(document.PageSize);
        //        rectangle.Left += document.LeftMargin;
        //        rectangle.Right -= document.RightMargin;
        //        rectangle.Top -= document.TopMargin;
        //        rectangle.Bottom += document.BottomMargin;
        //        content.SetColorStroke(GrayColor.BLACK);
        //        content.Rectangle(rectangle.Left, rectangle.Bottom, rectangle.Width, rectangle.Height);
        //        content.Stroke();


        //        document.Close();


        //        byte[] data = memoryStream.ToArray();


        //        var result = data.ToString();




        //        List<CaseReportListModel> list = new List<CaseReportListModel>();
        //        var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Case_Report_" + DateTime.Now.Ticks, data);
        //        if (file != null)
        //        {
        //            return file;
        //        }
        //        else
        //        {
        //            var model = new ReportLogSearchModel();
        //            model.StartDate = Convert.ToDateTime(startDate);
        //            model.EndDate = Convert.ToDateTime(endDate);
        //            var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
        //                        select new { Id = (int)d, Name = d.ToString() };
        //            model.CaseStatusList = new SelectList(items, "Id", "Name");
        //            var clientId = _clientHandler.GetClientId();
        //            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
        //                                                   select new SelectListItem
        //                                                   {
        //                                                       Value = Convert.ToString(s.Id),
        //                                                       Text = s.FName + " " + s.LName.ToString()
        //                                                   };
        //            model.Users = new SelectList(userList, "Value", "Text");

        //            return View("CaseReport", model);
        //        }
        //    }
        //}


        [HttpGet("Report/ExportCaseReport")]
        public async Task<IActionResult> ExportCaseReport(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string caseStatusChange, string riskLevel, bool isPDF)
        {
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var clientId = _clientHandler.GetClientId();

            var userId = _clientHandler.GetUserId();


            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            else if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if (cust_type == "SHAREHOLDER")
            {
                cust_type = "S";
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
            //List<CaseReportListModel> abc = new List<CaseReportListModel>();
            List<CaseReportListModel> abc = new List<CaseReportListModel>();
            if (searchValue != "" && searchValue != null)
            {
                abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportListBySearch(new CaseReportRequestDTO
                {
                    //User = userID,
                    StartDate = startDate,
                    EndDate = endDate,
                    caseStatus = caseStatus,
                    Cust_type = cust_type,
                    // UpdatedByUserId = updatedByUserID,
                    ClientId = _clientHandler.GetClientId(),
                    SearchValue = searchValue,
                    User = createdBy.ToString(),
                    matchscore = matchScore,
                    riskLevel = riskLevel

                }));
            }
            else
            {
                abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportList(new CaseReportRequestDTO
                {
                    //User = userID,
                    StartDate = startDate,
                    EndDate = endDate,
                    caseStatus = caseStatus,
                    Cust_type = cust_type,
                    // UpdatedByUserId = updatedByUserID,
                    ClientId = _clientHandler.GetClientId(),
                    User = createdBy.ToString(),
                    matchscore = matchScore,
                    riskLevel = riskLevel

                }));

            }
        
            //if (searchValue != "" && searchValue != null)
            //{
            //    abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseManagementSearchValueReportList(new CaseReportRequestDTO()
            //    {
            //        User = userId.ToString(),
            //        StartDate = startDate,
            //        EndDate = endDate,
            //        Cust_type = cust_type,
            //        ClientId = _clientHandler.GetClientId(),
            //        matchscore = matchScore,
            //        createdBy = createdBy,
            //        caseStatus = caseStatus,
            //        riskLevel = riskLevel,
            //        usergroupName = _UserGroupModel.Name,
            //        SearchValue=searchValue,
            //    }));
            //}
            //else
            //{
            //    abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseManagementReportList(new CaseReportRequestDTO()
            //    {
            //        User = userId.ToString(),
            //        StartDate = startDate,
            //        EndDate = endDate,
            //        Cust_type = cust_type,
            //        ClientId = _clientHandler.GetClientId(),
            //        matchscore = matchScore,
            //        createdBy = createdBy,
            //        caseStatus = caseStatus,
            //        riskLevel = riskLevel,
            //        usergroupName = _UserGroupModel.Name
            //    }));
            //}
            int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            downloadModel.Data = abc;
            downloadModel.TotalRows = abc.Count;
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());

            #region FiltersCheck
            string filter = "";
            if (userId != 0)
            {
                filter += "User ";
            }


            if (cust_type != "0")
            {
                if (filter == "")
                {
                    filter += "Customer Type";
                    ;
                }
                else
                {
                    filter += ", Customer Type";
                }
            }
            #endregion
            #region status
            //string caseStatus = "";
            //switch (status)
            //{
            //    case "0":
            //        caseStatus = "Pending";
            //        break;
            //    case "1":
            //        caseStatus = "Assigned";
            //        break;
            //    case "2":
            //        caseStatus = "Approved";
            //        break;
            //    case "3":
            //        caseStatus = "Rejected";
            //        break;
            //    case "4":
            //        caseStatus = "Closed";
            //        break;
            //    case "5":
            //        caseStatus = "Auto Approved";
            //        break;
            //    case "6":
            //        caseStatus = "Pending Case Created From Daily Scheduler";
            //        break;
            //    case "10":
            //        caseStatus = "All";
            //        break;
            //    default:
            //        caseStatus = "";
            //        break;
            //}
            #endregion

            //Excel Export
            if (!isPDF)
            {
                CaseManagementReportExcelModel excelModel = new CaseManagementReportExcelModel();
                List<CaseManagementReportExcelModel> excelData = new List<CaseManagementReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               :   Case Report" + "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n" +
                                          "Filters Applied  :   " + filter;
                excelModel.Details = details;

                excelData = (from res in abc
                             select new CaseManagementReportExcelModel
                             {
                                 CustomerId = res.CustomerID,
                                 CreationDate = Convert.ToDateTime(res.CreatedOn).ToString("dd/MM/yyyy HH:mm:ss"),
                                 UpdationDate = Convert.ToDateTime(res.UpdatedOn).ToString("dd/MM/yyyy HH:mm:ss"),
                                 CustomerType = res.CustomerType == "I" ? "Individual" : "Corporate",
                                 CustomerName = res.FirstName + " " + res.LastName,
                                 CaseStatusChangeReason = res.CaseChangeStatus,
                                 ScreeningScore = res.MatchScore,

                                 RiskScore =
                                 !string.IsNullOrEmpty(res.CustomerType == "I"
                                     ? res.Individual_final_risk_score
                                     : res.corporate_final_risk_score)
                                 ? (
                                     ((res.CustomerType == "I"
                                         ? res.Individual_final_risk_score
                                         : res.corporate_final_risk_score) ?? "")
                                     .ToLower().Contains("high")
                                     && ((res.CustomerType == "I"
                                         ? res.Individual_Risk_Override
                                         : res.Corporate_Risk_Override) ?? "")
                                     .ToLower() == "override"
                                     ? "High(O)"
                                     : ((res.CustomerType == "I"
                                         ? res.Individual_final_risk_score
                                         : res.corporate_final_risk_score) ?? "")
                                     .Replace(" Risk", "")
                                   )
                                 : "",

                                 CreatedBy = res.CreatedUser,
                                 CaseStatus = res.CaseStatus

                             }).ToList();

                excelData.Add(excelModel);

                return new ExcelResult<CaseManagementReportExcelModel>((excelData), "Case Report", "Case_Report_" + DateTime.Now.Ticks);
            }

            //var result = await _viewRenderService.RenderToStringAsync("Report/CaseReportDownload", downloadModel);

            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

            string body = string.Empty;
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
                if (logos != null)
                {
                    string url = logos;
                    Image tif = Image.GetInstance(url);
                    tif.ScalePercent(1f);
                    tif.SpacingBefore = 20f;
                    logo.AddCell(tif);
                    document.Add(logo);
                }



                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                //header.DefaultCell.ExtraParagraphSpace= 1;
                PdfPCell hd = new PdfPCell(new Phrase("Report               :   Case Report"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                PdfPCell dateRange = new PdfPCell(new Phrase("Date Range       :   " + startDate + "  to  " + endDate));
                PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                PdfPCell filters = new PdfPCell(new Phrase("Filters Applied   :   " + filter));
                PdfPCell _filters = new PdfPCell(new Phrase("\n"));
                //PdfPCell createdBy = new PdfPCell(new Phrase(   "Created by        :   "+clientData.CreatedBy));
                //PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));

                //hd.HorizontalAlignment = Element.ALIGN_LEFT;
                //hd.FixedHeight = 20f;
                //hd.VerticalAlignment = 1;
                hd.Border = 0;
                dateRange.Border = 0;
                filters.Border = 0;
                //createdBy.Border = 0;

                _hd.Border = 0;
                _dateRange.Border = 0;
                _filters.Border = 0;
                //_createdBy.Border = 0;

                header.AddCell(hd);
                header.AddCell(_hd);
                header.AddCell(dateRange);
                header.AddCell(_dateRange);
                header.AddCell(filters);
                header.AddCell(_filters);
                //header.AddCell(createdBy);
                //header.AddCell(_createdBy);

                document.Add(header);
                PdfPTable table = new PdfPTable(9);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 2f, 1f, 1f, 1f, 1f };
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
                PdfPCell cell3 = new PdfPCell(new Phrase("CREATED DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CUSTOMER TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("STATUS", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("RISK SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell8.FixedHeight = 30f;
                table.AddCell(cell8);
                PdfPCell cell9 = new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell9.HorizontalAlignment = 1;
                cell9.VerticalAlignment = 1;
                cell9.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell9.FixedHeight = 30f;
                table.AddCell(cell9);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerType, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CaseStatus, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    var riskValue = downloadModel.Data[i].CustomerType == "I"
                    ? downloadModel.Data[i].Individual_final_risk_score
                    : downloadModel.Data[i].corporate_final_risk_score;

                    table.AddCell(new Phrase(
                        riskValue?.ToString() ?? "",
                        new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)
                    ));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedBy, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));


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
                footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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




                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Case_Report_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else
                {
                    var model = new ReportLogSearchModel();
                    model.StartDate = Convert.ToDateTime(startDate);
                    model.EndDate = Convert.ToDateTime(endDate);
                    var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                                select new { Id = (int)d, Name = d.ToString() };
                    model.CaseStatusList = new SelectList(items, "Id", "Name");

                    IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                           select new SelectListItem
                                                           {
                                                               Value = Convert.ToString(s.Id),
                                                               Text = s.FName + " " + s.LName.ToString()
                                                           };
                    model.Users = new SelectList(userList, "Value", "Text");

                    return View("CaseReport", model);
                }
            }
        }

        [HttpGet("Report/ExportCaseManagementReport")]
        public async Task<IActionResult> ExportCaseManagementReport(string startDate, string endDate,string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string caseStatusChange, string riskLevel, bool isPDF)
        {
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var clientId = _clientHandler.GetClientId();

            var userId = _clientHandler.GetUserId();


            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            else if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if (cust_type == "SHAREHOLDER")
            {
                cust_type = "S";
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
            //List<CaseReportListModel> abc = new List<CaseReportListModel>();
            List<CaseModel> abc = new List<CaseModel>();
            if (searchValue != "" && searchValue != null)
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _UserGroupModel.Name, clientId));

            }
            else
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, _UserGroupModel.Name, clientId));

            }
            //if (searchValue != "" && searchValue != null)
            //{
            //    abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseManagementSearchValueReportList(new CaseReportRequestDTO()
            //    {
            //        User = userId.ToString(),
            //        StartDate = startDate,
            //        EndDate = endDate,
            //        Cust_type = cust_type,
            //        ClientId = _clientHandler.GetClientId(),
            //        matchscore = matchScore,
            //        createdBy = createdBy,
            //        caseStatus = caseStatus,
            //        riskLevel = riskLevel,
            //        usergroupName = _UserGroupModel.Name,
            //        SearchValue=searchValue,
            //    }));
            //}
            //else
            //{
            //    abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseManagementReportList(new CaseReportRequestDTO()
            //    {
            //        User = userId.ToString(),
            //        StartDate = startDate,
            //        EndDate = endDate,
            //        Cust_type = cust_type,
            //        ClientId = _clientHandler.GetClientId(),
            //        matchscore = matchScore,
            //        createdBy = createdBy,
            //        caseStatus = caseStatus,
            //        riskLevel = riskLevel,
            //        usergroupName = _UserGroupModel.Name
            //    }));
            //}
            int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            downloadModel.Data1 = abc;
            downloadModel.TotalRows = abc.Count;
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());

            #region FiltersCheck
            string filter = "";
            if (userId != 0)
            {
                filter += "User ";
            }
            
            
            if (cust_type != "0")
            {
                if (filter == "")
                {
                    filter += "Customer Type";
                    ;
                }
                else
                {
                    filter += ", Customer Type";
                }
            }
            #endregion
            #region status
            //string caseStatus = "";
            //switch (status)
            //{
            //    case "0":
            //        caseStatus = "Pending";
            //        break;
            //    case "1":
            //        caseStatus = "Assigned";
            //        break;
            //    case "2":
            //        caseStatus = "Approved";
            //        break;
            //    case "3":
            //        caseStatus = "Rejected";
            //        break;
            //    case "4":
            //        caseStatus = "Closed";
            //        break;
            //    case "5":
            //        caseStatus = "Auto Approved";
            //        break;
            //    case "6":
            //        caseStatus = "Pending Case Created From Daily Scheduler";
            //        break;
            //    case "10":
            //        caseStatus = "All";
            //        break;
            //    default:
            //        caseStatus = "";
            //        break;
            //}
            #endregion

            //Excel Export
            if (!isPDF)
            {
                CaseManagementReportExcelModel excelModel = new CaseManagementReportExcelModel();
                List<CaseManagementReportExcelModel> excelData =new List<CaseManagementReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               :   Case Report" + "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n" +
                                          "Filters Applied  :   " + filter;
                excelModel.Details = details;

                excelData = (from res in abc
                             select new CaseManagementReportExcelModel
                             {
                                 CustomerId = res.CustomerId,
                                 CreationDate = res.CreatedOn.ToString("dd/MM/yyyy HH:mm:ss"),
                                 UpdationDate = Convert.ToDateTime(res.UpdatedOnDB).ToString("dd/MM/yyyy HH:mm:ss"),
                                 CustomerType = res.CustomerType == "I" ? "Individual" : "Corporate",
                                 CustomerName = res.FirstName + " " + res.LastName,
                                 CaseStatusChangeReason = res.CaseChangeStatus,
                                 ScreeningScore = res.MatchScore,

                                 RiskScore =
                                 !string.IsNullOrEmpty(res.CustomerType == "I"
                                     ? res.Individual_final_risk_score
                                     : res.corporate_final_risk_score)
                                 ? (
                                     ((res.CustomerType == "I"
                                         ? res.Individual_final_risk_score
                                         : res.corporate_final_risk_score) ?? "")
                                     .ToLower().Contains("high")
                                     && ((res.CustomerType == "I"
                                         ? res.Individual_Risk_Override
                                         : res.Corporate_Risk_Override) ?? "")
                                     .ToLower() == "override"
                                     ? "High(O)"
                                     : ((res.CustomerType == "I"
                                         ? res.Individual_final_risk_score
                                         : res.corporate_final_risk_score) ?? "")
                                     .Replace(" Risk", "")
                                   )
                                 : "",

                                 CreatedBy = res.CreatedUser,
                                 CaseStatus = res.CaseStatus

                             }).ToList();

                excelData.Add(excelModel);

                return new ExcelResult<CaseManagementReportExcelModel>((excelData), "Case Report", "Case_Report_" + DateTime.Now.Ticks);
            }

            //var result = await _viewRenderService.RenderToStringAsync("Report/CaseReportDownload", downloadModel);

            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

            string body = string.Empty;
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
                if (logos != null)
                {
                    string url = logos;
                    Image tif = Image.GetInstance(url);
                    tif.ScalePercent(1f);
                    tif.SpacingBefore = 20f;
                    logo.AddCell(tif);
                    document.Add(logo);
                }
                


                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                //header.DefaultCell.ExtraParagraphSpace= 1;
                PdfPCell hd = new PdfPCell(new Phrase(          "Report               :   Case Report"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                PdfPCell dateRange = new PdfPCell(new Phrase(   "Date Range       :   "+startDate + "  to  " +endDate));
                PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                PdfPCell filters = new PdfPCell(new Phrase(     "Filters Applied   :   "+filter));
                PdfPCell _filters = new PdfPCell(new Phrase("\n"));
                //PdfPCell createdBy = new PdfPCell(new Phrase(   "Created by        :   "+clientData.CreatedBy));
                //PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));

                //hd.HorizontalAlignment = Element.ALIGN_LEFT;
                //hd.FixedHeight = 20f;
                //hd.VerticalAlignment = 1;
                hd.Border = 0;
                dateRange.Border= 0;
                filters.Border = 0;
                //createdBy.Border = 0;
                
                _hd.Border = 0;
                _dateRange.Border = 0;
                _filters.Border = 0;
                //_createdBy.Border = 0;
                
                header.AddCell(hd);
                header.AddCell(_hd);
                header.AddCell(dateRange);
                header.AddCell(_dateRange);
                header.AddCell(filters);
                header.AddCell(_filters);
                //header.AddCell(createdBy);
                //header.AddCell(_createdBy);
                
                document.Add(header);
                PdfPTable table = new PdfPTable(9);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 2f, 1f, 1f, 1f,1f };
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
                PdfPCell cell3 = new PdfPCell(new Phrase("CREATED DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CUSTOMER TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("STATUS", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("RISK SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell8.FixedHeight = 30f;
                table.AddCell(cell8);
                PdfPCell cell9= new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell9.HorizontalAlignment = 1;
                cell9.VerticalAlignment = 1;
                cell9.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell9.FixedHeight = 30f;
                table.AddCell(cell9);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerType, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CaseStatus, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    var riskValue = downloadModel.Data[i].CustomerType == "I"
                    ? downloadModel.Data[i].Individual_final_risk_score
                    : downloadModel.Data[i].corporate_final_risk_score;

                                    table.AddCell(new Phrase(
                                        riskValue?.ToString() ?? "",
                                        new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)
                                    ));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedBy, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    

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
                footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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




                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Case_Report_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else
                {
                    var model = new ReportLogSearchModel();
                    model.StartDate = Convert.ToDateTime(startDate);
                    model.EndDate = Convert.ToDateTime(endDate);
                    var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                                select new { Id = (int)d, Name = d.ToString() };
                    model.CaseStatusList = new SelectList(items, "Id", "Name");
                    
                    IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                           select new SelectListItem
                                                           {
                                                               Value = Convert.ToString(s.Id),
                                                               Text = s.FName + " " + s.LName.ToString()
                                                           };
                    model.Users = new SelectList(userList, "Value", "Text");

                    return View("CaseReport", model);
                }
            }
        }
        [HttpGet("Report/ExportCompletedCasesReport")]
        public async Task<IActionResult> ExportCompletedCasesReport(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string caseStatusChange, string riskLevel, bool isPDF)
        {
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var clientId = _clientHandler.GetClientId();

            var userId = _clientHandler.GetUserId();
            List<CaseModel> abc = new List<CaseModel>();

            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            else if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if (cust_type == "SHAREHOLDER")
            {
                cust_type = "S";
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
            //List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCompletedCaseReportList(new CaseReportRequestDTO()
            //{
            //    User = userId.ToString(),
            //    StartDate = startDate,
            //    EndDate = endDate,
            //    Cust_type = cust_type,
            //    ClientId = _clientHandler.GetClientId()
            //}));
            int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            downloadModel.Data1 = abc;
            downloadModel.TotalRows = abc.Count;
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());

            #region FiltersCheck
            string filter = "";
            if (userId != 0)
            {
                filter += "User ";
            }


            if (cust_type != "0")
            {
                if (filter == "")
                {
                    filter += "Customer Type";
                    ;
                }
                else
                {
                    filter += ", Customer Type";
                }
            }
            #endregion
            #region status
            //string caseStatus = "";
            //switch (status)
            //{
            //    case "0":
            //        caseStatus = "Pending";
            //        break;
            //    case "1":
            //        caseStatus = "Assigned";
            //        break;
            //    case "2":
            //        caseStatus = "Approved";
            //        break;
            //    case "3":
            //        caseStatus = "Rejected";
            //        break;
            //    case "4":
            //        caseStatus = "Closed";
            //        break;
            //    case "5":
            //        caseStatus = "Auto Approved";
            //        break;
            //    case "6":
            //        caseStatus = "Pending Case Created From Daily Scheduler";
            //        break;
            //    case "10":
            //        caseStatus = "All";
            //        break;
            //    default:
            //        caseStatus = "";
            //        break;
            //}
            #endregion

            //Excel Export
            if (!isPDF)
            {
                CaseManagementReportExcelModel excelModel = new CaseManagementReportExcelModel();
                List<CaseManagementReportExcelModel> excelData = new List<CaseManagementReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               :   Case Report" + "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n" +
                                          "Filters Applied  :   " + filter;
                excelModel.Details = details;

                excelData = (from res in abc
                             select new CaseManagementReportExcelModel
                             {

                                 CustomerId = res.CustomerId,
                                 CreationDate = res.CreatedOn.ToString("dd/MM/yyyy HH:mm:ss"),
                                 UpdationDate = Convert.ToDateTime(res.UpdatedOnDB).ToString("dd/MM/yyyy HH:mm:ss"),
                                 CustomerType = res.CustomerType == "I" ? "Individual" : "Corporate",
                                 CustomerName = res.FirstName + " " + res.LastName,
                                 CaseStatusChangeReason = res.CaseChangeStatus,
                                 ScreeningScore = res.MatchScore,

                                 RiskScore =
                                 !string.IsNullOrEmpty(res.CustomerType == "I"
                                     ? res.Individual_final_risk_score
                                     : res.corporate_final_risk_score)
                                 ? (
                                     ((res.CustomerType == "I"
                                         ? res.Individual_final_risk_score
                                         : res.corporate_final_risk_score) ?? "")
                                     .ToLower().Contains("high")
                                     && ((res.CustomerType == "I"
                                         ? res.Individual_Risk_Override
                                         : res.Corporate_Risk_Override) ?? "")
                                     .ToLower() == "override"
                                     ? "High(O)"
                                     : ((res.CustomerType == "I"
                                         ? res.Individual_final_risk_score
                                         : res.corporate_final_risk_score) ?? "")
                                     .Replace(" Risk", "")
                                   )
                                 : "",

                                 CreatedBy = res.CreatedUser,
                                 CaseStatus = res.CaseStatus


                             }).ToList();
                excelData.Add(excelModel);

                return new ExcelResult<CaseManagementReportExcelModel>((excelData), "Case Report", "Case_Report_" + DateTime.Now.Ticks);
            }

            //var result = await _viewRenderService.RenderToStringAsync("Report/CaseReportDownload", downloadModel);

            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

            string body = string.Empty;
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
                if (logos != null)
                {
                    string url = logos;
                    Image tif = Image.GetInstance(url);
                    tif.ScalePercent(1f);
                    tif.SpacingBefore = 20f;
                    logo.AddCell(tif);
                    document.Add(logo);
                }



                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                //header.DefaultCell.ExtraParagraphSpace= 1;
                PdfPCell hd = new PdfPCell(new Phrase("Report               :   Case Report"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                PdfPCell dateRange = new PdfPCell(new Phrase("Date Range       :   " + startDate + "  to  " + endDate));
                PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                PdfPCell filters = new PdfPCell(new Phrase("Filters Applied   :   " + filter));
                PdfPCell _filters = new PdfPCell(new Phrase("\n"));
                //PdfPCell createdBy = new PdfPCell(new Phrase("Created by        :   " + clientData.CreatedBy));
                //PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));

                //hd.HorizontalAlignment = Element.ALIGN_LEFT;
                //hd.FixedHeight = 20f;
                //hd.VerticalAlignment = 1;
                hd.Border = 0;
                dateRange.Border = 0;
                filters.Border = 0;
                //createdBy.Border = 0;

                _hd.Border = 0;
                _dateRange.Border = 0;
                _filters.Border = 0;
                //_createdBy.Border = 0;

                header.AddCell(hd);
                header.AddCell(_hd);
                header.AddCell(dateRange);
                header.AddCell(_dateRange);
                header.AddCell(filters);
                header.AddCell(_filters);
                //header.AddCell(createdBy);
                //header.AddCell(_createdBy);

                document.Add(header);
                PdfPTable table = new PdfPTable(9);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 2f, 1f, 1f, 1f, 1f };
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
                PdfPCell cell3 = new PdfPCell(new Phrase("CREATED DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CUSTOMER TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("STATUS", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                PdfPCell cell8 = new PdfPCell(new Phrase("RISK SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell8.HorizontalAlignment = 1;
                cell8.VerticalAlignment = 1;
                cell8.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell8.FixedHeight = 30f;
                table.AddCell(cell8);
                PdfPCell cell9 = new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell9.HorizontalAlignment = 1;
                cell9.VerticalAlignment = 1;
                cell9.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell9.FixedHeight = 30f;
                table.AddCell(cell9);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerType, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Nationality, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CaseStatus, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    var riskValue = downloadModel.Data[i].CustomerType == "I"
                    ? downloadModel.Data[i].Individual_final_risk_score
                    : downloadModel.Data[i].corporate_final_risk_score;

                    table.AddCell(new Phrase(
                        riskValue?.ToString() ?? "",
                        new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)
                    ));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedBy, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));


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
                footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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




                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Case_Report_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else
                {
                    var model = new ReportLogSearchModel();
                    model.StartDate = Convert.ToDateTime(startDate);
                    model.EndDate = Convert.ToDateTime(endDate);
                    var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                                select new { Id = (int)d, Name = d.ToString() };
                    model.CaseStatusList = new SelectList(items, "Id", "Name");
                    
                    IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                           select new SelectListItem
                                                           {
                                                               Value = Convert.ToString(s.Id),
                                                               Text = s.FName + " " + s.LName.ToString()
                                                           };
                    model.Users = new SelectList(userList, "Value", "Text");

                    return View("CaseReport", model);
                }
            }
        }

        [HttpGet("Report/ExportkycCaseReport")]
        public async Task<IActionResult> ExportkycCaseReport(string userID, string updatedByUserID, string startDate, string endDate, string status, string cust_type, bool isPDF)
        {
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            else if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if (cust_type == "SHAREHOLDER")
            {
                cust_type = "S";
            }
            
            List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetKycReportList(new CaseReportRequestDTO()
            {
                User = userID,
                StartDate = startDate,
                EndDate = endDate,
                Cust_type = cust_type,
                Status = status,
                ClientId = _clientHandler.GetClientId()
            }));
            int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            CaseReportDownloadModel downloadModel = new CaseReportDownloadModel();
            downloadModel.Data = abc;
            downloadModel.TotalRows = abc.Count;
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());

            #region FiltersCheck
            string filter = "";
            if (userID != "0")
            {
                filter += "User ";
            }

            if (updatedByUserID != "0")
            {
                if (filter == "")
                {
                    filter += "Updated By";
                }
                else
                {
                    filter += ", Updated By";
                }
            }
            if (cust_type != "0")
            {
                if (filter == "")
                {
                    filter += "Customer Type";
                    ;
                }
                else
                {
                    filter += ", Customer Type";
                }
            }
            #endregion
            #region status
            string caseStatus = "";
            switch (status)
            {
                case "0":
                    caseStatus = "Pending";
                    break;
                case "1":
                    caseStatus = "Assigned";
                    break;
                case "2":
                    caseStatus = "Approved";
                    break;
                case "3":
                    caseStatus = "Rejected";
                    break;
                case "4":
                    caseStatus = "Closed";
                    break;
                case "5":
                    caseStatus = "Auto Approved";
                    break;
                case "6":
                    caseStatus = "Pending Case Created From Daily Scheduler";
                    break;
                case "10":
                    caseStatus = "All";
                    break;
                default:
                    caseStatus = "";
                    break;
            }
            #endregion

            //Excel Export
            if (!isPDF)
            {
                CaseKycReportExcelModel excelModel = new CaseKycReportExcelModel();
                List<CaseKycReportExcelModel> excelData = new List<CaseKycReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               :   Kyc Report" + "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n" +
                                          "Filters Applied  :   " + filter + "\r\n" + "Created by        :   " + clientData.ClientName + "\r\n" +
                                          "Case Status       :   " + caseStatus;
                excelModel.Details = details;

                excelData = (from res in abc
                             select new CaseKycReportExcelModel
                             {

                                 CustomerId = res.CustomerID,
                                 CustomerType = res.CustomerType,
                                 CustomerName = res.CustomerName,
                                 CaseStatus = res.Status,
                                 Dateofbirth = res.dob,
                                 Nationality = res.Nationality,
                                 Placeofbirth=res.PlaceOfBirth,
                                 Residencestatus=res.ResidenceStatus,
                                 MaritalStatus= res.MaritalStatus,
                                 IdType=res.CustomerIdType,
                                 IdNumber=res.CustomerIdNumber,
                                 Idexpirydate=res.IdExpdate,
                                 SourceofIncome=res.Sourceofincome,
                                 OccupationType=res.OccupatinTypeTxt,
                                 producttype=res.ProductName,
                                 Deliverychannel=res.DeliveryChannelName,
                                 Modeofpayment=res.Modeofpayment,
                                 BankaccountNo=res.BankAccountNo,
                                 Bankname=res.BankAccountName,
                                 Bankbranch=res.BankAccountBranch,
                                 employeername=res.EmployerName,
                                 employeeraddress=res.EmployerAddress,
                                 residenceaddress=res.ResidenceAddress,
                                 emailaddress=res.Email,
                                 MobileNo=res.Mobile,
                                 EntityTypeTxt=res.EntityTypeTxt,
                                 City=res.City,
                                 Emirate=res.Emirate,
                                 Country=res.Country,
                                 POBox=res.POBox,
                                 CorporateWebsite=res.CorporateWebsite,
                                 LicenseNumber=res.CustomerIdNumber,
                                 LicenseIssueDate=res.LicenseIssueDate,
                                 LicenseIssuingAuthority=res.LicenseIssuingAuthority,
                                 LicenseExpiryDate=res.LicenseExpiryDate,
                                 PlaceofIssue=res.PlaceofIssue,
                                 LicenseType=res.LicenseTypeTxt,
                                 BusinessType=res.BusinessType,
                                 VATRegistrationNumber=res.VATRegistrationNumber,
                                 Thershold=res.Threshold,
                                 Remarks=res.Remarks,

                                 CompanyCode = res.CompanyCode,
                                 CreatedBy = res.CreatedBy,
                                 CreationDate = res.CreatedOn,

                             }).ToList();
                excelData.Add(excelModel);

                return new ExcelResult<CaseKycReportExcelModel>((excelData), "Kyc Report", "Kyc_Report_" + DateTime.Now.Ticks);
            }

            //var result = await _viewRenderService.RenderToStringAsync("Report/CaseReportDownload", downloadModel);

            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

            string body = string.Empty;
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
                //string url = logos;
                //Image tif = Image.GetInstance(url);
                //tif.ScalePercent(1f);
                //tif.SpacingBefore = 20f;
                //logo.AddCell(tif);
                //document.Add(logo);


                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                //header.DefaultCell.ExtraParagraphSpace= 1;
                PdfPCell hd = new PdfPCell(new Phrase("Report               :   Kyc Report"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));
                PdfPCell dateRange = new PdfPCell(new Phrase("Date Range       :   " + startDate + "  to  " + endDate));
                PdfPCell _dateRange = new PdfPCell(new Phrase("\n"));
                PdfPCell filters = new PdfPCell(new Phrase("Filters Applied   :   " + filter));
                PdfPCell _filters = new PdfPCell(new Phrase("\n"));
                PdfPCell createdBy = new PdfPCell(new Phrase("Created by        :   " + clientData.ClientName));
                PdfPCell _createdBy = new PdfPCell(new Phrase("\n"));
                PdfPCell caseStat = new PdfPCell(new Phrase("Case Status      :   " + caseStatus));
                PdfPCell _caseStat = new PdfPCell(new Phrase("\n"));

                //hd.HorizontalAlignment = Element.ALIGN_LEFT;
                //hd.FixedHeight = 20f;
                //hd.VerticalAlignment = 1;
                hd.Border = 0;
                dateRange.Border = 0;
                filters.Border = 0;
                createdBy.Border = 0;
                caseStat.Border = 0;
                _hd.Border = 0;
                _dateRange.Border = 0;
                _filters.Border = 0;
                _createdBy.Border = 0;
                _caseStat.Border = 0;
                header.AddCell(hd);
                header.AddCell(_hd);
                header.AddCell(dateRange);
                header.AddCell(_dateRange);
                header.AddCell(filters);
                header.AddCell(_filters);
                header.AddCell(createdBy);
                header.AddCell(_createdBy);
                header.AddCell(caseStat);
                header.AddCell(_caseStat);
                document.Add(header);
                PdfPTable table = new PdfPTable(7);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 1f, 1f, 1.5f, 2f, 1f, 1f };
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
                PdfPCell cell3 = new PdfPCell(new Phrase("CUSTOMER TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("CUSTOMER NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("CASE STATUS", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("CREATION DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerID, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerType, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Status, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedBy, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

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
                footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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




                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Kyc_Report_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else
                {
                    var model = new ReportLogSearchModel();
                    model.StartDate = Convert.ToDateTime(startDate);
                    model.EndDate = Convert.ToDateTime(endDate);
                    var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                                select new { Id = (int)d, Name = d.ToString() };
                    model.CaseStatusList = new SelectList(items, "Id", "Name");
                    var clientId = _clientHandler.GetClientId();
                    IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                           select new SelectListItem
                                                           {
                                                               Value = Convert.ToString(s.Id),
                                                               Text = s.FName + " " + s.LName.ToString()
                                                           };
                    model.Users = new SelectList(userList, "Value", "Text");

                    return View("KycReport", model);
                }
            }
        }

        public ActionResult ViewIndividualCaseDetailNew(string id, int type)
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

        public ActionResult ViewIndividualCaseDetail(string id, int type)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            int Id = _customerCaseService.GetCaseId(id);//returns id of the customercase table instead of caseid
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
            var dob = model.Case.DOB;
            var createddated = model.Case.CreatedOn;
            string dobText;

            HttpContext.Session.SetString("CorporateId", _CustomerCaseDTO.CustomerId);
            HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path + HttpContext.Request.QueryString);

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
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            model.CaseComments = _mapper.Map<List<CaseModel>>(_caseCommentService.GetAllByCase(Id));

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

        public ActionResult ViewIndividualCaseDetail_PDF(string id, int type)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            int Id = _customerCaseService.GetCaseId(id);
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
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
            var appBaseUrl = MyHttpContext.AppBaseUrl;
            model.Url = appBaseUrl;
            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(Id);
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
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            model.CaseComments = _mapper.Map<List<CaseModel>>(_caseCommentService.GetAllByCase(Id));

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
                return RedirectToAction("PageNotFound", "Error");
            }

            return View(model);
        }

        public ActionResult ViewShareholderCaseDetails(string id, int type)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            int Id = _customerCaseService.GetCaseId(id);//returns id of the customercase table instead of caseid
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
            var dob = model.Case.DOB;
            var createddated = model.Case.CreatedOn;
            string dobText;

            var returnUrl = HttpContext.Session.GetString("ReturnUrl");
            ViewBag.ReturnUrl = returnUrl;

            HttpContext.Session.SetString("ShareholderReturnUrl", HttpContext.Request.Path + HttpContext.Request.QueryString);

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
        
        public ActionResult ViewShareholderCaseDetails_PDF(string id, int type)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            int Id = _customerCaseService.GetCaseId(id);//returns id of the customercase table instead of caseid
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
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
            var appBaseUrl = MyHttpContext.AppBaseUrl;
            model.Url = appBaseUrl;
            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(Id);
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

            return View("ViewShareholderCaseDetails_PDF", model);


        }
        
        private void MapRiskValues(List<RiskTypeCategoryDTO> categories, List<ReportDataDTO> reportData)
        {
            foreach (var category in categories)
            {
                foreach (var riskType in category.RiskTypes)
                {
                    var match = reportData.FirstOrDefault(x =>
                        x.lov_type_category_id == category.Id &&
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
        public class CustomerReportExcelModel
        {
            public int Id { get; set; }



            [IncludeInReport(Order = 1)]
            [Display(Name = "Customer Id")]
            public string CustomerId { get; set; }

            [IncludeInReport(Order = 2)]
            [Display(Name = "Customer Type")]
            public string CustomerType { get; set; }

            [IncludeInReport(Order = 3)]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "Status")]
            public string CaseStatus { get; set; }

            [IncludeInReport(Order = 5)]
            [Display(Name = "Created By")]
            public string CreatedBy { get; set; }
            [IncludeInReport(Order = 6)]
            [Display(Name = "Created On")]
            public string CreationDate { get; set; }

            [IncludeInReport(Order = 7)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }
        }
        public class CaseReportExcelModel
        {
            public int Id { get; set; }

            [IncludeInReport(Order = 1)]
            [Display(Name = "Customer Id")]
            public string CustomerId { get; set; }

            [IncludeInReport(Order = 2)]
            [Display(Name = "Type")]
            public string CustomerType { get; set; }

            [IncludeInReport(Order = 3)]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "Status")]
            public string CaseStatus { get; set; }

            
            
            [IncludeInReport(Order = 5)]
            [Display(Name = "Company Code")]
            public string CompanyCode { get; set; }

            [IncludeInReport(Order = 6)]
            public string CreatedBy { get; set; }

            [IncludeInReport(Order = 7)]
            [Display(Name = "Created On")]
            public string CreationDate { get; set; }
            [IncludeInReport(Order = 8)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }
        }

        public class CaseManagementReportExcelModel
        {
            public int Id { get; set; }

            [IncludeInReport(Order = 1)]
            [Display(Name = "Customer Id")]
            public string CustomerId { get; set; }
            [IncludeInReport(Order = 2)]
            [Display(Name = "Created On")]
            public string CreationDate { get; set; }
            [IncludeInReport(Order = 3)]
            [Display(Name = "Updated On")]
            public string UpdationDate { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "Customer Type")]
            public string CustomerType { get; set; }

            [IncludeInReport(Order = 5)]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; }
            [IncludeInReport(Order = 6)]
            [Display(Name = "DataSets")]
            public string CaseStatusChangeReason { get; set; }

            [IncludeInReport(Order = 7)]
            [Display(Name = "Screening Score")]
            public int ScreeningScore { get; set; }

            [IncludeInReport(Order = 8)]
            [Display(Name = "Risk Score")]
            public string RiskScore { get; set; }
            [IncludeInReport(Order = 9)]
            [Display(Name = "User")]
            public string CreatedBy { get; set; }

            [IncludeInReport(Order = 10)]
            [Display(Name = "Status")]
            public string CaseStatus { get; set; }

            [IncludeInReport(Order = 11)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }
        }

        public class ClientReportExcelModel
        {
            public int Id { get; set; }

            [IncludeInReport(Order = 1)]
            [Display(Name = "Client Id")]
            public int ClientId { get; set; }

            [IncludeInReport(Order = 2)]
            [Display(Name = "Client Name")]
            public string ClientName { get; set; }

            [IncludeInReport(Order = 3)]
            [Display(Name = "Count")]
            public int SearchCount { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }


        }
        public class SchedulerlogsReportExcelModel
        {
            public int Id { get; set; }

            [IncludeInReport(Order = 1)]
            [Display(Name = "Source")]
            public string Source { get; set; }

            [IncludeInReport(Order = 2)]
            [Display(Name = "Total Hits")]
            public int TotalHits { get; set; }

            [IncludeInReport(Order = 3)]
            [Display(Name = "Total Records")]
            public int TotalRecords { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "createdOn")]
            public string createdOn { get; set; }

            [IncludeInReport(Order = 5)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }


        }


        public class CaseKycReportExcelModel
        {
            public int Id { get; set; }

            [IncludeInReport(Order = 1)]
            [Display(Name = "Customer Id")]
            public string CustomerId { get; set; }

            [IncludeInReport(Order = 2)]
            [Display(Name = "Type")]
            public string CustomerType { get; set; }

            [IncludeInReport(Order = 3)]
            [Display(Name = "Customer Name")]
            public string CustomerName { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "Status")]
            public string CaseStatus { get; set; }

            [IncludeInReport(Order = 5)]
            [Display(Name = "Dateofbirth")]
            public string Dateofbirth { get; set; }

            [IncludeInReport(Order = 6)]
            [Display(Name = "Nationality")]
            public string Nationality { get; set; }

            [IncludeInReport(Order = 7)]
            [Display(Name = "Place of Birth")]
            public string Placeofbirth { get; set; }

            [IncludeInReport(Order = 8)]
            [Display(Name = "Residence Status")]
            public string Residencestatus { get; set; }

            [IncludeInReport(Order = 9)]
            [Display(Name = "Marital Status")]
            public string MaritalStatus { get; set; }

            [IncludeInReport(Order = 10)]
            [Display(Name = "Id Type")]
            public string IdType { get; set; }

            [IncludeInReport(Order = 11)]
            [Display(Name = "Id Number")]
            public string IdNumber { get; set; }



            [IncludeInReport(Order = 12)]
            [Display(Name = "Id Expiry Date")]
            public string Idexpirydate { get; set; }

            [IncludeInReport(Order = 13)]
            [Display(Name = "Source of Income")]
            public string SourceofIncome { get; set; }

            [IncludeInReport(Order = 14)]
            [Display(Name = "Occupation Type")]
            public string OccupationType { get; set; }

            [IncludeInReport(Order = 15)]
            [Display(Name = "Prdouct Type")]
            public string producttype { get; set; }

            [IncludeInReport(Order = 16)]
            [Display(Name = "Delivery Channel")]
            public string Deliverychannel { get; set; }

            [IncludeInReport(Order = 17)]
            [Display(Name = "Mode of Payment")]
            public string Modeofpayment { get; set; }

            [IncludeInReport(Order = 18)]
            [Display(Name = "Bank Account No")]
            public string BankaccountNo { get; set; }

            [IncludeInReport(Order = 19)]
            [Display(Name = "Bank Name")]
            public string Bankname { get; set; }

            [IncludeInReport(Order = 18)]
            [Display(Name = "Bank Branch")]
            public string Bankbranch { get; set; }

            [IncludeInReport(Order = 19)]
            [Display(Name = "Employeer Name")]
            public string employeername { get; set; }

            [IncludeInReport(Order = 20)]
            [Display(Name = "Employeer Address")]
            public string employeeraddress { get; set; }

            [IncludeInReport(Order = 21)]
            [Display(Name = "Residence Address")]
            public string residenceaddress { get; set; }

            [IncludeInReport(Order = 22)]
            [Display(Name = "Email Address")]
            public string emailaddress { get; set; }

            [IncludeInReport(Order = 23)]
            [Display(Name = "Mob No")]
            public string MobileNo { get; set; }

            [IncludeInReport(Order = 24)]
            [Display(Name = "Type of Entity/ Legal Status ")]
            public string EntityTypeTxt { get; set; }
            [IncludeInReport(Order = 25)]
            [Display(Name = "City")]
            public string City { get; set; }
            [IncludeInReport(Order = 26)]
            [Display(Name = "Emirate")]
            public string Emirate { get; set; }
            [IncludeInReport(Order = 27)]
            [Display(Name = "Country")]
            public string Country { get; set; }
            [IncludeInReport(Order = 28)]
            [Display(Name = "P.O. Box")]
            public string POBox { get; set; }
            [IncludeInReport(Order = 29)]
            [Display(Name = "Corporate Website")]
            public string CorporateWebsite { get; set; }
            [IncludeInReport(Order = 30)]
            [Display(Name = "License Number")]
            public string LicenseNumber { get; set; }
            [IncludeInReport(Order = 31)]
            [Display(Name = "License Issue Date")]
            public string LicenseIssueDate { get; set; }
            [IncludeInReport(Order = 32)]
            [Display(Name = "License Issuing Authority")]
            public string LicenseIssuingAuthority { get; set; }
            [IncludeInReport(Order = 33)]
            [Display(Name = "License Expiry Date")]
            public string LicenseExpiryDate { get; set; }
            [IncludeInReport(Order = 34)]
            [Display(Name = "Place of Issue")]
            public string PlaceofIssue { get; set; }
            [IncludeInReport(Order = 35)]
            [Display(Name = "License Type")]
            public string LicenseType { get; set; }
            [IncludeInReport(Order = 36)]
            [Display(Name = "Business Activity")]
            public string BusinessType { get; set; }
            [IncludeInReport(Order = 37)]
            [Display(Name = "VAT Registration Number")]
            public string VATRegistrationNumber { get; set; }

            [IncludeInReport(Order = 38)]
            [Display(Name = "Thershold")]
            public int Thershold { get; set; }

            [IncludeInReport(Order = 39)]
            [Display(Name = "Remarks")]
            public string Remarks { get; set; }

            [IncludeInReport(Order = 40)]
            [Display(Name = "Company Code")]
            public string CompanyCode { get; set; }

            [IncludeInReport(Order = 40)]
            public string CreatedBy { get; set; }

            [IncludeInReport(Order = 41)]
            [Display(Name = "Created On")]
            public string CreationDate { get; set; }
            [IncludeInReport(Order = 42)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }
        }
        public class ScreeningDatabaselogsReportExcelModel
        {
            public int Id { get; set; }

            [IncludeInReport(Order = 1)]
            [Display(Name = "Updated Date")]
            public string UpdatedDate { get; set; }

            [IncludeInReport(Order = 2)]
            [Display(Name = "Individual")]
            public int Individual { get; set; }

            [IncludeInReport(Order = 3)]
            [Display(Name = "Corporate")]
            public int Corporate { get; set; }

            [IncludeInReport(Order = 4)]
            [Display(Name = "Deleted")]
            public int Deleted { get; set; }

            [IncludeInReport(Order = 5)]
            [Display(Name = "Report Details")]
            public string Details { get; set; }


        }
        public IActionResult UserPasswordLogs()
        {
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now;
            model.EndDate = System.DateTime.Now;
            return View(model);
        }
        [HttpPost("Report/PasswordList")]
        public JsonResult PasswordList(DataTableModel model)
        {
            var clientId = _clientHandler.GetUserId();
            List<UserPasswordLogModel> list = _mapper.Map<List<UserPasswordLogModel>>(_userService.GetAllPasswordLogs(model.StartDate.ToString(), model.EndDate.ToString(), clientId));

            if (!string.IsNullOrEmpty(model.search.value))
            {
                list = list.Where(m => m.FullName.ToLower().Contains(model.search.value.ToLower())
                || m.UserName.ToLower().Contains(model.search.value.ToLower())).ToList();
            }

            var data = list.Skip(model.start).Take(model.length).ToList();
            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = list.Count,//totalResultsCount,
                recordsFiltered = list.Count,//filteredResultsCount,
                data = data,
            });
        }

        [HttpPost("Report/rollbackCase")]
        public JsonResult rollbackCase(string id, int type)
        {
            Console.WriteLine($"Roll back {id}");
            var comment="";
            CaseProcessModel model = new CaseProcessModel();

            int Id = _customerCaseService.GetCaseId(id);
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);

            _CustomerCaseDTO.Status = 0;
            _CustomerCaseDTO.RollbackCount += 1;
            _CustomerCaseDTO.IsMatched = 1;

            _customerCaseService.Update(_CustomerCaseDTO);

            

            if (type == 4) {
                
                var res = _customerMasterService.UpdateWhiteList(_CustomerCaseDTO.CustomerMasterId, "NO");

                comment = $"Rolled back from 'Whitelisted'";
            }
            else {
                comment = $"Rolled back from '{(type == 0 ? "Approved" : "Rejected")}'";
            }
          

            CaseCommentModel commentModel = new CaseCommentModel
            {
                CaseId = Id,
                Comment = comment,
                CreatedBy = _clientHandler.GetUserId()
            };

            _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));

            return Json(model);
        }

        [HttpGet("Report/KycReport")]
        public IActionResult KycReport(int type)
        {
            var clientId = _clientHandler.GetUserId();
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");

            var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                        select new { Id = (int)d, Name = d.ToString() };
            model.CaseStatusList = new SelectList(items, "Id", "Name");
            model.CaseStatus = "10"; //TODO: Remove/Update the default value
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            if (type == 1)
            {
                model.StartDate = new DateTime();
                model.CaseStatus = "0";
            }
            else if (type == 2)
            {
                model.StartDate = new DateTime();
                model.CaseStatus = "5";
            }
            else if (type == 3)
            {
                model.StartDate = new DateTime();
                model.CaseStatus = "3";
            }
            return View(model);
        }
        [HttpPost("Report/KycReportCustompagination")]
        public JsonResult CustomKycPagination(DataTableModel model, string userID, string startDate,
    string endDate, string cust_type, string status, int nomatch, int tdomesticpep,
    int tforeignpep, int tadversemedia, int pdomesticpep, int pforeignpep,
    int padversemedia, string pepdeclaration, string idstatus, string value,
    int orderColumn = 0, string orderDirection = "asc") // Added sorting parameters
        {
            
                // Handle default values
                if (string.IsNullOrEmpty(endDate))
                {
                    endDate = DateTime.Now.ToString("yyyy-MM-dd");
                }

                if (string.IsNullOrEmpty(pepdeclaration))
                {
                    pepdeclaration = null;
                }

                // Handle document expiry date ranges
                DateTime expiryStartDate = DateTime.Now;
                DateTime expiryEndDate = DateTime.Now;

                if (idstatus == "1") // Expired
                {
                    expiryStartDate = DateTime.Parse("1900-01-01");
                }
                else if (idstatus == "3") // 3 Months to expiry
                {
                    expiryEndDate = expiryStartDate.AddMonths(3);
                }
                else if (idstatus == "6") // 6 Months to expiry
                {
                    expiryEndDate = expiryStartDate.AddMonths(6);
                }
                else if (idstatus == "7") // More than 6 months to expiry
                {
                    expiryEndDate = expiryStartDate.AddMonths(6);
                }

                // Get base data from service
                var reportData = _mapper.Map<List<CaseReportListModel>>(_reportService.GetKycReportList(
                    new CaseReportRequestDTO()
                    {
                        User = userID,
                        StartDate = startDate,
                        EndDate = endDate,
                        Cust_type = cust_type,
                        Status = status,
                        ClientId = _clientHandler.GetClientId(),
                        NoMatch = nomatch,
                        TrueDomesticpep = tdomesticpep,
                        TrueAdverseMedia = tforeignpep,
                        TrueForeignpep = tadversemedia,
                        PartialDomesticpep = pdomesticpep,
                        PartialForeignpep = pforeignpep,
                        Partialadversemedia = padversemedia,
                        idstatus = idstatus,
                        expiryStartDate = expiryStartDate,
                        expiryEndDate = expiryEndDate,
                        pepfrmclients = pepdeclaration,
                        SearchValue = value
                    }));

                // Apply sorting
                IOrderedEnumerable<CaseReportListModel> sortedData;
                switch (orderColumn)
                {
                    case 0: // Customer ID
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.CustomerID) :
                            reportData.OrderByDescending(x => x.CustomerID);
                        break;
                    case 1: // Customer Type
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.CustomerType) :
                            reportData.OrderByDescending(x => x.CustomerType);
                        break;
                    case 2: // Customer Name
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.CustomerName) :
                            reportData.OrderByDescending(x => x.CustomerName);
                        break;
                    case 3: // Case Status
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.Status) :
                            reportData.OrderByDescending(x => x.Status);
                        break;
                    case 4: // Created By
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.CreatedBy) :
                            reportData.OrderByDescending(x => x.CreatedBy);
                        break;
                    case 5: // Date
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.CreatedOn) :
                            reportData.OrderByDescending(x => x.CreatedOn);
                        break;
                    default: // Default sort by Customer ID
                        sortedData = orderDirection == "asc" ?
                            reportData.OrderBy(x => x.CustomerID) :
                            reportData.OrderByDescending(x => x.CustomerID);
                        break;
                }

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(value))
                {
                    var searchValue = value.ToLower();
                    sortedData = (IOrderedEnumerable<CaseReportListModel>)sortedData
                        .Where(x => (x.CustomerName?.ToLower()?.Contains(searchValue) ?? false) ||
                                   (x.CustomerID?.ToLower()?.Contains(searchValue) ?? false) ||
                                   (x.CustomerType?.ToLower()?.Contains(searchValue) ?? false) ||
                                   (x.Status?.ToLower()?.Contains(searchValue) ?? false) ||
                                   (x.CreatedBy?.ToLower()?.Contains(searchValue) ?? false));
                }

                // Apply pagination
                var totalRecords = sortedData.Count();
                var paginatedData = sortedData
                    .Skip(model.start)
                    .Take(model.length)
                    .ToList();

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = totalRecords,
                    recordsFiltered = totalRecords,
                    data = paginatedData,
                    allRows = sortedData.ToList() // For client-side operations
                });
         
            
        }
        public ActionResult ViewIndividualKYCReport(string id, int type)
        {
            var clientId = _clientHandler.GetUserId();
            KycIndividualModel model = new KycIndividualModel();
            var list = _mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId));
            model.CountryList = new SelectList(list, "Name", "Name");
            model = _mapper.Map<KycIndividualModel>(_customerMasterService.GetAll(id));
            int Id = _customerCaseService.GetCaseId(id);
            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(Id);
            model.CaseDocumentsL = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

            if (model == null)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = "With this id there is no data";
                error.module = "ViewIndividualKYCReport_R";
                error.comments = "Customer id was not found or not there";
                error.status_code = 404;
                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                return RedirectToAction("PageNotFound", "Error");
            }


            return View(model);
        }
        public ActionResult ViewCorporateKYCReport(string id, int type)
        {
            CorporateKycModel model = new CorporateKycModel();

            model = _mapper.Map<CorporateKycModel>(_customerMasterService.GetAllCorporate(id));
            model.GroupEntity = _mapper.Map<List<GroupEntity>>(_customerMasterService.GetAllGroupEntities(id));
            model.Partners = _mapper.Map<List<PersonDetails>>(_customerMasterService.GetAllPartners(id));
            model.SeniorManagements = _mapper.Map<List<PersonDetails>>(_customerMasterService.GetAllSeniorManagement(id));
            model.AuthorisedSignatories = _mapper.Map<List<PersonDetails>>(_customerMasterService.GetAllSignatories(id));

            int Id = _customerCaseService.GetCaseId(id);

            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(Id);
            model.CaseDocumentsL = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

            foreach (var item in model.GroupEntity)
            {
                int groupId = _customerCaseService.GetCaseId(item.cust_ref_id);

                List<CaseDocumentDTO> groupcaseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(groupId);
                item.CaseDocumentsL = _mapper.Map<List<CaseDocumentModel>>(groupcaseDocumentbyId);

            }

            foreach (var item in model.Partners)
            {
                int partnersId = _customerCaseService.GetCaseId(item.cust_id_gen);

                List<CaseDocumentDTO> partnerscaseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(partnersId);
                item.CaseDocumentsL = _mapper.Map<List<CaseDocumentModel>>(partnerscaseDocumentbyId);

            }

            foreach (var item in model.SeniorManagements)
            {
                int seniorId = _customerCaseService.GetCaseId(item.cust_id_gen);

                List<CaseDocumentDTO> seniorcaseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(seniorId);
                item.CaseDocumentsL = _mapper.Map<List<CaseDocumentModel>>(seniorcaseDocumentbyId);

            }

            foreach (var item in model.AuthorisedSignatories)
            {
                int authorisedId = _customerCaseService.GetCaseId(item.cust_id_gen);

                List<CaseDocumentDTO> authorisedcaseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(authorisedId);
                item.CaseDocumentsL = _mapper.Map<List<CaseDocumentModel>>(authorisedcaseDocumentbyId);

            }

            if (model == null)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = "With this id there is no data";
                error.module = "ViewCorporateKYCReport_R";
                error.comments = "Customer id was not found or not there";
                error.status_code = 404;
                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                return RedirectToAction("PageNotFound", "Error");
            }

            return View(model);
        }

        //Transaction Screening 

        //Transaction Screening



        [HttpGet("/transactionscreening")]
        public IActionResult TransactionScreeningReport()
        {
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            return View(model);
        }

        [HttpPost("/Report/TransactionCaseReportCustompagination")]
        public JsonResult TransactionCaseReportCustompagination(DataTableModel model, string startDate, string endDate)
        {


            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }

            if (startDate == null)
            {
                startDate = System.DateTime.Now.ToString();
            }


            PendingTransactionRequestDTO req = new PendingTransactionRequestDTO();
            req.StartDate = startDate;
            req.EndDate = endDate;
            req.ClientId = _clientHandler.GetUserId();
            List<PendingTransactionsModel> abc = _mapper.Map<List<PendingTransactionsModel>>(_transactionScreeningService.GetAllTransactions(req));


            int totalcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.tranrefno.ToLower().Contains(model.search.value.ToLower())).ToList();
            }
            int filteredcount = abc.Count;
            var data = abc.Skip(model.start).Take(model.length).ToList();
            var response = Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = totalcount,//totalResultsCount,
                recordsFiltered = filteredcount,//filteredResultsCount,
                data = data,
            });

            return response;





            //return null;
        }
        [HttpGet("/Report/ViewTransactionCaseDetails/{TranRefNo}")]
        public async Task<ActionResult> ViewTransactionCaseDetails(string TranRefNo)
        {
            var clientId = _clientHandler.GetClientId();
            TransactionCaseProcessReportModel model = new TransactionCaseProcessReportModel();
            model.Case = new List<TranScreenCaseModel>();

            List<TranScreenDTO> _TranCaseDTO = _transactionScreeningService.GetTranCaseByRefNo(TranRefNo);
            model.Case = _mapper.Map<List<TranScreenCaseModel>>(_TranCaseDTO);
            model.RefNo = TranRefNo;
            List<TransactionCaseDocumentDTO> caseDocumentbyId = _transactionScreeningService.GetCaseDocumentByTranRefNo(TranRefNo);
            model.CaseDocuments = _mapper.Map<List<TransactionCaseDocumentModel>>(caseDocumentbyId);
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();

            var id = _clientHandler.GetUserId();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            var result = await _clientHandler.PostAsync(new { tranrefno = TranRefNo }, ScreeningService.GETBYTRANREFNO);
            if (!string.IsNullOrEmpty(result))
            {
                List<TransactionMatchDetails> jsonList = JsonConvert.DeserializeObject<List<TransactionMatchDetails>>(result);
                model.MatchDataList = jsonList;
            }
            return View(model);
        }
        [HttpGet("Report/GetClientSearchLog")]
        public IActionResult GetClientSearchLog() 
        {
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now;
            model.EndDate = System.DateTime.Now;
            return View(model);
        
        }

        public JsonResult ClientReportCustompagination(DataTableModel model, string startDate,string endDate)
        {
             var clientid = _customerCaseService.GetAllClients();
            var matchrecordsList = new List<clientSearch>();

            foreach (var client in clientid) 
            {
                var clientcounts = new clientSearch();
                var cb = _mapper.Map<int>(_reportService.GetClientCountReportList(new CaseReportRequestDTO()
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ClientId = client.ClientId
                }));
                clientcounts.client_id = client.ClientId;
                clientcounts.client_name = client.ClientName;
                clientcounts.count = cb;

                matchrecordsList.Add(clientcounts);
            }
            if (!string.IsNullOrEmpty(model.search.value))
            {
                matchrecordsList = matchrecordsList.Where(m => m.count.ToString().Contains(model.search.value.ToLower())).ToList();
            }

            var data = Sort(matchrecordsList, model.columns[model.order[0].column].data ?? "createdOn", model.order[0].dir ?? "dec")

        .Skip(model.start)

        .Take(model.length)

        .ToList();


            return Json(new
            {
                // this is what datatables wants sending back
                model.draw,
                recordsTotal = matchrecordsList.Count,//totalResultsCount,
                recordsFiltered = matchrecordsList.Count,//filteredResultsCount,
                data = data
            });
        }

        [HttpGet("Report/ClientExportCaseReport")]
        public async Task<IActionResult> ClientExportCaseReport(string startDate, string endDate, bool isPDF)
        {
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }

            //else
            //{
            //    cust_type = " ";
            //}
            var clientid = _customerCaseService.GetAllClients();
            var matchrecordsList = new List<clientSearch>();

            foreach (var client in clientid)
            {
                var clientcounts = new clientSearch();
                var cb = _mapper.Map<int>(_reportService.GetClientCountReportList(new CaseReportRequestDTO()
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    ClientId = client.ClientId
                }));
                clientcounts.client_id = client.ClientId;
                clientcounts.client_name = client.ClientName;
                clientcounts.count = cb;

                matchrecordsList.Add(clientcounts);
            }
            
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());

            
         
          

            //Excel Export
            if (!isPDF)
            {
                ClientReportExcelModel excelModel = new ClientReportExcelModel();
                List<ClientReportExcelModel> excelData = new List<ClientReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               :   Client Search count Report" + "\r\n" + "Date Range       :   " + startDate + "  to  " + endDate + "\r\n";
                excelModel.Details = details;

                excelData = (from res in matchrecordsList
                             select new ClientReportExcelModel
                             {

                                 ClientId = res.client_id,
                                 ClientName = res.client_name,
                                 SearchCount = res.count,
                                 

                             }).ToList();
                excelData.Add(excelModel);

                return new ExcelResult<ClientReportExcelModel>((excelData), "Client Search Count Report", "Client_SearchCount_Report_" + DateTime.Now.Ticks);
            }



            return null;        
                
            
        }


        [HttpGet("Report/SchedulerLogsExportReport")]
        public async Task<IActionResult> SchedulerLogsExportReport(bool isPDF, string startDate = null, string endDate = null)
        {



            var clientId = _clientHandler.GetClientId();
            List<DigiSchedulerLogModel> abc;
            
            // Use date-filtered method if dates are provided, otherwise use the original method
            if (!string.IsNullOrEmpty(startDate) && !string.IsNullOrEmpty(endDate))
            {
                abc = _mapper.Map<List<DigiSchedulerLogModel>>(_reportService.GetDigiSchedulerList(clientId, startDate, endDate));
            }
            else
            {
                abc = _mapper.Map<List<DigiSchedulerLogModel>>(_reportService.GetDigiSchedulerList(clientId));
            }
                
               
                

                
            

            





            //Excel Export
            if (!isPDF)
            {
                SchedulerlogsReportExcelModel excelModel = new SchedulerlogsReportExcelModel();
                List<SchedulerlogsReportExcelModel> excelData = new List<SchedulerlogsReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               :   Scheduler Log reports";
                excelModel.Details = details;

                excelData = (from res in abc
                             select new SchedulerlogsReportExcelModel
                             {

                                 Source = res.Source,
                                 TotalHits = res.TotalHits,
                                 TotalRecords = res.TotalRecords,
                                 createdOn=res.CreatedOn

                             }).ToList();
                excelData.Add(excelModel);

                return new ExcelResult<SchedulerlogsReportExcelModel>((excelData), "Scheduler Log Report", "scheduler_log_report_" + DateTime.Now.Ticks);
            }

            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());
            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

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
                PdfPCell compname = new PdfPCell(new Phrase(companyName?.ToString() ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 17, Font.BOLD)));
                compname.FixedHeight = 40f;
                compname.VerticalAlignment = 1;
                compname.HorizontalAlignment = 1;
                compname.Border = 0;
                logo.AddCell(compname);
                if (logos != null)
                {
                    try {
                        string url = logos;
                        Image tif = Image.GetInstance(url);
                        tif.ScalePercent(1f);
                        tif.SpacingBefore = 20f;
                        logo.AddCell(tif);
                    } catch {
                        logo.AddCell(new Phrase(""));
                    }
                }
                else
                {
                    logo.AddCell(new Phrase(""));
                }
                document.Add(logo);

                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                PdfPCell hd = new PdfPCell(new Phrase("Report               :   Scheduler Log Report"));
                hd.Border = 0;
                header.AddCell(hd);
                document.Add(header);

                PdfPTable table = new PdfPTable(5);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 2f, 1f, 1f, 1.5f };
                table.SetWidths(widths);
                table.HorizontalAlignment = 1;
                table.SpacingAfter = 30f;

                string[] headers = { "#", "Source", "Total Hits", "Total Records", "Created On" };
                foreach (var hText in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(hText, new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell.HorizontalAlignment = 1;
                    cell.VerticalAlignment = 1;
                    cell.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell.FixedHeight = 30f;
                    table.AddCell(cell);
                }

                for (int i = 0; i < abc.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(abc[i].Source?.ToString() ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(abc[i].TotalHits.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(abc[i].TotalRecords.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    
                    string createdOnStr = abc[i].CreatedOn != null ? Convert.ToDateTime(abc[i].CreatedOn).ToString("yyyy-MM-dd HH:mm") : "";
                    table.AddCell(new Phrase(createdOnStr, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
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
                footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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

                byte[] dataBytes = memoryStream.ToArray();
                var result = dataBytes.ToString();

                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, (int)OperationType.PDF, "Scheduler_Log_Report_" + DateTime.Now.Ticks, dataBytes);
                if (file != null)
                {
                    return file;
                }
            }

            return null;


        }

        [HttpGet("Report/GetScreeningDatabaseLogs")]
        public IActionResult GetScreeningDatabaseLogs()
        {
            var model = new ReportLogSearchModel();
            model.StartDate = System.DateTime.Now.AddDays(-7);
            //model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            return View(model);

        }
        public JsonResult ScreeningDatabaseReportCustompagination(DataTableModel model,string startDate,string endDate,int orderColumn = 0,string orderDirection = "desc")
       {
            try
            {
                List<ScreeningDatabaseLogsModel> abc = _mapper.Map<List<ScreeningDatabaseLogsModel>>(
                    _reportService.GetScreeningDatabaseLogs(new CaseReportRequestDTO()
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }));

                // ?? Search
                if (!string.IsNullOrEmpty(model.search?.value))
                {
                    string search = model.search.value.ToLower();

                    abc = abc.Where(m =>
                        m.Deleted.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.Individual.ToString().Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        m.Corporate.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // ? Sorting (like your reference API)
                IEnumerable<ScreeningDatabaseLogsModel> sortedData;

                switch (orderColumn)
                {
                    case 0: // Updated Date
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.UpdatedDate)
                            : abc.OrderByDescending(x => x.UpdatedDate);
                        break;

                    case 1: // Individual
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.Individual)
                            : abc.OrderByDescending(x => x.Individual);
                        break;

                    case 2: // Corporate
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.Corporate)
                            : abc.OrderByDescending(x => x.Corporate);
                        break;

                    case 3: // Deleted
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.Deleted)
                            : abc.OrderByDescending(x => x.Deleted);
                        break;

                    default:
                        sortedData = abc.OrderByDescending(x => x.UpdatedDate);
                        break;
                }

                int recordsTotal = abc.Count;
                int recordsFiltered = abc.Count;

                var query = sortedData.Skip(model.start);
                if (model.length > 0)
                {
                    query = query.Take(model.length);
                }
                var pagedData = query.ToList();
                // ? Pagination
                //var pagedData = sortedData
                //    .Skip(model.start)
                //    .Take(model.length)
                //    .ToList();

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<ScreeningDatabaseLogsModel>(),
                    error = ex.Message + " | " + ex.InnerException?.Message
                });
            }
        }
        //public JsonResult ScreeningDatabaseReportCustompagination(DataTableModel model, string startDate, string endDate)
        //{
        //    List<ScreeningDatabaseLogsModel> abc = _mapper.Map<List<ScreeningDatabaseLogsModel>>(_reportService.GetScreeningDatabaseLogs(new CaseReportRequestDTO()
        //    {

        //        StartDate = startDate,
        //        EndDate = endDate
        //    }));
        //    if (!string.IsNullOrEmpty(model.search.value))
        //    {
        //        abc = abc.Where(m => m.Deleted.ToString().Contains(model.search.value.ToLower()) || m.Individual.ToString().Contains(model.search.value.ToLower())).ToList();
        //    }

        //    var data = Sort(abc, model.columns[model.order[0].column].data ?? "createdOn", model.order[0].dir ?? "dec")

        //.Skip(model.start)

        //.Take(model.length)

        //.ToList();


        //    return Json(new
        //    {
        //        // this is what datatables wants sending back
        //        model.draw,
        //        recordsTotal = abc.Count,//totalResultsCount,
        //        recordsFiltered = abc.Count,//filteredResultsCount,
        //        data = data
        //    });
        //}

        //[HttpGet("Report/GetDatasetUpdateLogs")]
        //public IActionResult GetDatasetUpdateLogs()
        //{
        //    var model = new ReportLogSearchModel();
        //    model.StartDate = System.DateTime.Now.AddDays(-7);
        //    model.EndDate = System.DateTime.Now;

        //    // Get client ID from session
        //    var clientId = HttpContext.Session.GetString("sessClientId")?.ParseInt() ?? 0;

        //    // Get client contract start date (using client created date)
        //    DateTime clientContractStartDate = DateTime.MinValue;
        //    if (clientId > 0)
        //    {
        //        var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
        //        if (clientDetails != null && clientDetails.CreatedOn.HasValue)
        //        {
        //            clientContractStartDate = clientDetails.CreatedOn.Value;
        //        }
        //    }

        //    // Get Individual and Corporate names by matching inserted date and created on date
        //    if (clientId > 0)
        //    {
        //        // Get all customers for this client
        //        var customers = _customerMasterService.GetDetailsBySearch("", "", clientId);
                
        //        // Collect Individual and Corporate names
        //        foreach (var customer in customers)
        //        {
        //            if (customer.CustomerType.Equals("Individual", StringComparison.OrdinalIgnoreCase))
        //            {
        //                model.IndividualNames.Add($"{customer.FirstName} {customer.LastName}".Trim());
        //            }
        //            else if (customer.CustomerType.Equals("Corporate", StringComparison.OrdinalIgnoreCase))
        //            {
        //                // For corporates, use CustomerId as the name/identifier
        //                // Format: "CustomerId (ReferenceID)" if ReferenceID is available
        //                string corporateName = customer.CustomerId;
        //                if (!string.IsNullOrEmpty(customer.CustomerReferenceID))
        //                {
        //                    corporateName += $" ({customer.CustomerReferenceID})";
        //                }
        //                model.CorporateNames.Add(corporateName);
        //            }
        //        }
        //    }

        //    model.ClientContractStartDate = clientContractStartDate;

        //    return View(model);
        //}


        [HttpGet("Report/DatasetUpdateLogs")]
        public IActionResult GetDatasetUpdateLogs(int type, string schedulerTrackerId, string option)
        {
            
            var model = new ReportLogSearchModel();
            //model.StartDate = System.DateTime.Now.AddYears(-1);
            model.StartDate = System.DateTime.Now.AddDays(-7);
            model.EndDate = System.DateTime.Now;
            
            return View(model);
        }
        public JsonResult DatasetUpdateLogsCustompagination(DataTableModel model, string startDate, string endDate, string datasets, int orderColumn = 0, string orderDirection = "desc")
        {
            try
            {
                List<DatasetUpdateLogsModel> abc = _mapper.Map<List<DatasetUpdateLogsModel>>(
                    _reportService.GetDatasetUpdateLogs(new CaseReportRequestDTO()
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        Datasets = datasets
                    }));

                // Get client ID from session
                var clientId = _clientHandler.GetClientId();

                // Get client contract start date and filter data
                if (clientId > 0)
                {
                    var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
                    if (clientDetails != null && clientDetails.ApplicationStartDate.HasValue)
                    {
                        DateTime clientContractStartDate = clientDetails.ApplicationStartDate.Value;
                        
                        // Filter out records before the client's contract start date
                        abc = abc.Where(log => 
                            { 
                                if (string.IsNullOrEmpty(log.UpdatedDate))
                                    return false;
                                
                                try
                                {
                                    DateTime logDate = DateTime.Parse(log.UpdatedDate);
                                    return logDate >= clientContractStartDate;
                                }
                                catch
                                {
                                    return false; // If date parsing fails, exclude the record
                                }
                            }
                        ).ToList();
                    }
                }

                // Search
                if (!string.IsNullOrEmpty(model.search?.value))
                {
                    string search = model.search.value.ToLower();

                    abc = abc.Where(m =>
                        (m.Datasets != null && m.Datasets.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Delta != null && m.Delta.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)) ||
                        (m.Cumulative != null && m.Cumulative.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)) 
                    ).ToList();
                }

                // Sorting
                IEnumerable<DatasetUpdateLogsModel> sortedData;

                switch (orderColumn)
                {
                    case 0: // Updated Date
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.UpdatedDate)
                            : abc.OrderByDescending(x => x.UpdatedDate);
                        break;

                    case 1: // Datasets
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.Datasets)
                            : abc.OrderByDescending(x => x.Datasets);
                        break;

                    case 2: // Delta
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.Delta)
                            : abc.OrderByDescending(x => x.Delta);
                        break;

                    case 3: // Humiliated
                        sortedData = orderDirection == "asc"
                            ? abc.OrderBy(x => x.Cumulative)
                            : abc.OrderByDescending(x => x.Cumulative);
                        break;

                    

                    default:
                        sortedData = abc.OrderByDescending(x => x.UpdatedDate);
                        break;
                }

                int recordsTotal = abc.Count;
                int recordsFiltered = abc.Count;

                var query = sortedData.Skip(model.start);
                if (model.length > 0)
                {
                    query = query.Take(model.length);
                }
                var pagedData = query.ToList();

                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = recordsTotal,
                    recordsFiltered = recordsFiltered,
                    data = pagedData
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    draw = model.draw,
                    recordsTotal = 0,
                    recordsFiltered = 0,
                    data = new List<DatasetUpdateLogsModel>(),
                    error = ex.Message + " | " + ex.InnerException?.Message
                });
            }
        }


        [HttpGet("Report/ScreeningDatabaseLogsExportReport")]
        public async Task<IActionResult> ScreeningDatabaseLogsExportReport(string startDate, string endDate, bool IsPDF)
        {



            var clientId = _clientHandler.GetClientId();




            List<ScreeningDatabaseLogsModel> abc = _mapper.Map<List<ScreeningDatabaseLogsModel>>(_reportService.GetScreeningDatabaseLogs(new CaseReportRequestDTO()
            {

                StartDate = startDate,
                EndDate = endDate
            }));
            int fileType = IsPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            ScreeningDatabaseReportDownloadModel downloadModel = new ScreeningDatabaseReportDownloadModel();
            downloadModel.Data = abc;
            downloadModel.TotalRows = abc.Count;
            var clientData = _customerCaseService.GetClientDetailsByID(_clientHandler.GetClientId());


            //Excel Export
            if (!IsPDF)
            {
                ScreeningDatabaselogsReportExcelModel excelModel = new ScreeningDatabaselogsReportExcelModel();
                List<ScreeningDatabaselogsReportExcelModel> excelData = new List<ScreeningDatabaselogsReportExcelModel>();
                //List<CaseReportExcelModel> excelData1 = new List<CaseReportExcelModel>();
                string details = "Report               : Screening Database Log reports";
                excelModel.Details = details;

                excelData = (from res in abc
                             select new ScreeningDatabaselogsReportExcelModel
                             {

                                 UpdatedDate = DateTime.Parse(res.UpdatedDate).ToString("yyyy-MM-dd"),
                                 Individual = res.Individual,
                                 Corporate = res.Corporate,
                                 Deleted = res.Deleted

                             }).ToList();
                excelData.Add(excelModel);

                return new ExcelResult<ScreeningDatabaselogsReportExcelModel>((excelData), "Screening Database Log Report", "screening_database_log_report_" + DateTime.Now.Ticks);
            }
            var logos = "wwwroot/img/" + clientData.DocumentFileName;
            var companyName = clientData.ClientName;

            string body = string.Empty;
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
                if (logos != null)
                {
                    string url = logos;
                    Image tif = Image.GetInstance(url);
                    tif.ScalePercent(1f);
                    tif.SpacingBefore = 20f;
                    logo.AddCell(tif);
                    document.Add(logo);
                }



                PdfPTable header = new PdfPTable(1);
                header.TotalWidth = 550f;
                header.LockedWidth = true;
                header.HorizontalAlignment = Element.ALIGN_LEFT;//0=Left, 1=Centre, 2=Right
                header.SpacingAfter = 30f;
                header.DefaultCell.Border = 0;
                //header.DefaultCell.ExtraParagraphSpace= 1;
                PdfPCell hd = new PdfPCell(new Phrase("Report               :   Screening Database Logs Report"));
                PdfPCell _hd = new PdfPCell(new Phrase("\n"));


                //hd.HorizontalAlignment = Element.ALIGN_LEFT;
                //hd.FixedHeight = 20f;
                //hd.VerticalAlignment = 1;
                hd.Border = 0;

                _hd.Border = 0;

                document.Add(header);
                PdfPTable table = new PdfPTable(5);
                table.TotalWidth = 550f;
                table.LockedWidth = true;
                float[] widths = new float[] { 0.5f, 2f, 1f, 1f, 1f };
                table.SetWidths(widths);
                table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                table.SpacingAfter = 30f;
                PdfPCell cell1 = new PdfPCell(new Phrase("#", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell1.HorizontalAlignment = 1;
                cell1.VerticalAlignment = 1;
                cell1.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                cell1.FixedHeight = 30f;
                table.AddCell(cell1);
                //PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                //cell2.HorizontalAlignment = 1;
                //cell2.VerticalAlignment = 1;
                //cell2.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                //cell2.FixedHeight = 30f;
                //table.AddCell(cell2);
                PdfPCell cell2 = new PdfPCell(new Phrase("Updated Date", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell2.HorizontalAlignment = 1;
                cell2.VerticalAlignment = 1;
                cell2.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                cell2.FixedHeight = 30f;
                table.AddCell(cell2);
                PdfPCell cell3 = new PdfPCell(new Phrase("Individual", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell3.HorizontalAlignment = 1;
                cell3.VerticalAlignment = 1;
                cell3.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                cell3.FixedHeight = 30f;
                table.AddCell(cell3);
                PdfPCell cell4 = new PdfPCell(new Phrase("Corporate", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell4.HorizontalAlignment = 1;
                cell4.VerticalAlignment = 1;
                cell4.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                cell4.FixedHeight = 30f;
                table.AddCell(cell4);
                PdfPCell cell5 = new PdfPCell(new Phrase("Deleted", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                for (int i = 0; i < downloadModel.Data.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(DateTime.Parse(downloadModel.Data[i].UpdatedDate).ToString("yyyy-MM-dd"), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Individual.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Corporate.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.Data[i].Deleted.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));

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
                footer2.AddCell(new Phrase("Date of Extraction  :   " + DateTime.Now));
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




                List<CaseReportListModel> list = new List<CaseReportListModel>();
                var file = _exportService.ExportDataWithHeader<CaseReportListModel>(list, result, fileType, "Screening_Database_log_Report_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }



                return null;


            }





        }

        [HttpGet("GetNamesByCreatedDate")]
        public IActionResult GetNamesByCreatedDate(string date,string type,string category)
        {
            try
            {

                var result = _freeSourceRepository.GetRecordsByCreatedDate(date,type, category);

                if (result != null && result.Count > 0)
                    return Ok(result);

                return Ok(new List<NAMELIST>()); // Return empty list instead of NotFound

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}
