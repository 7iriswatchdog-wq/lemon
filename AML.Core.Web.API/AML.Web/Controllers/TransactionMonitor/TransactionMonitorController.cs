using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.TransactionMonitor;
using AML.DTO.DTO.TransactionMonitor;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.TransactionMonitor;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Drawing.Charts;
using Fingers10.ExcelExport.ActionResults;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NToastNotify;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AML.Web.Controllers.TransactionMonitor
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class TransactionMonitorController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private ITransactionMonitorService _transacitonMonitorService;
        private ICountryService _countryService;

        private IExportDataService _exportService;

        private string baseC6URL = string.Empty;
        private string _c6Username;
        private string culture = CultureInfo.CurrentCulture.Name;
        private ICommonService _commonService;
        private string baseURL = string.Empty;
        public TransactionMonitorController(IMapper mapper,
            IToastNotification toastNotification, ICountryService countryService,
            IHttpClientHandler clientHandler,
            Microsoft.Extensions.Configuration.IConfiguration configuration, ICommonService commonService,
            IFileUploader fileUploader, IExportDataService exportService, ITransactionMonitorService transacitonMonitorService)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _commonService = commonService;
            _exportService = exportService;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;

            baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            _c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
            _transacitonMonitorService = transacitonMonitorService;
        }

        [Route("/tms")]
        public IActionResult Index()
        {
            return View();
        }
        //anjaly

        public IActionResult TransactionRules()
        {
            var userid = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();
            TMSRulesView model = new TMSRulesView();
            List<TMSNewRulesModel> all = _mapper.Map<List<TMSNewRulesModel>>(_transacitonMonitorService.GetNewTMSRules(clientId,userid));
            model.TMSRulesViewModel = all;
            return View(model);
        }
        [HttpPost("/tmscase/updaterule/{rdata}")]
        public ActionResult UpdateStatus(int rdata)
        {

            if (rdata != 0)
            {
                TMSNewRulesModel model = _mapper.Map<TMSNewRulesModel>(_transacitonMonitorService.GetNewTMSRules(rdata));

                if (model.TMSRuleIsActive == 1)
                {
                    model.TMSRuleIsActive = 0;
                }
                else
                {
                    model.TMSRuleIsActive = 1;
                }
                var result = _transacitonMonitorService.Update(_mapper.Map<TMSRulesMasterDTO>(model));
            }
            return RedirectToAction("TransactionRules", "TransactionMonitor");
        }

        [Route("/tmscase")]
        public IActionResult TMSCase()
        {
            return View();
        }
        [HttpPost("/tmscase/casemanagement")]
        public JsonResult CaseManagement(DataTableModel model)
        {
            var clientId = _clientHandler.GetClientId();
            dynamic list = _transacitonMonitorService.GetAllTMSCasefordashboard(clientId);
            List<TMSCaseDTO> abc = _mapper.Map<List<TMSCaseDTO>>(list);
            int totalcount = abc.Count;
            int filteredcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                //abc = abc.Where(m => m.Name.ToLower().Contains(model.search.value.ToLower())
                //|| m.Description.ToLower().Contains(model.search.value.ToLower()) || m.Code.ToLower().Contains(model.search.value.ToLower())).ToList();
                abc = abc.Where(m =>
                (m.TranRefno?.ToLower().Contains(model.search.value.ToLower()) ?? false) ||
                (m.CustomerId?.ToLower().Contains(model.search.value.ToLower()) ?? false) ||
                (m.CustomerName?.ToLower().Contains(model.search.value.ToLower()) ?? false)||
                (m.RuleViolated?.ToLower().Contains(model.search.value.ToLower()) ?? false) ||
                (m.StatusDescription?.ToLower().Contains(model.search.value.ToLower()) ?? false)
            ).ToList();

             
            }
            //            var data = Sort(abc, model.columns[model.order[0].column].data ?? "tranRefno", model.order[0].dir ?? "dec")

            //.Skip(model.start)

            //.Take(model.length)

            //.ToList();

            string sortColumn = "tranRefno";
            string sortDirection = "desc";

            if (model.order != null && model.order.Count > 0 &&
                model.columns != null &&
                model.order[0].column >= 0 &&
                model.order[0].column < model.columns.Count &&
                model.columns[model.order[0].column] != null)
            {
                sortColumn = model.columns[model.order[0].column].data ?? "tranRefno";
                sortDirection = model.order[0].dir ?? "desc";
            }

            var data = Sort(abc, sortColumn, sortDirection)
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

        //public List<T> Sort<T>(List<T> input, string property, string dir)

        //{

        //    var type = typeof(T);

        //    var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

        //    return dir == "asc"

        //        ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()

        //        : input.OrderByDescending(p => sortProperty.GetValue(p, null)).ToList();

        //}

        public List<T> Sort<T>(List<T> input, string property, string dir)
        {
            var type = typeof(T);
            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            if (sortProperty == null) return input;

            if (property.Equals("createdOn", StringComparison.OrdinalIgnoreCase))
            {
                return dir == "asc"
                    ? input.OrderBy(p => DateTime.TryParse(sortProperty.GetValue(p, null)?.ToString(), out var dt) ? dt : DateTime.MinValue).ToList()
                    : input.OrderByDescending(p => DateTime.TryParse(sortProperty.GetValue(p, null)?.ToString(), out var dt) ? dt : DateTime.MinValue).ToList();
            }

            return dir == "asc"
                ? input.OrderBy(p => sortProperty.GetValue(p, null)).ToList()
                : input.OrderByDescending(p => sortProperty.GetValue(p, null)).ToList();
        }

        //[HttpPost("/tmscase/casemanagement")]
        //public JsonResult CaseManagement(DataTableModel model)
        //{
        //    var clientId = _clientHandler.GetClientId();

        //    // Step 1: Get all case data
        //    var allData = _mapper.Map<List<TMSCaseDTO>>(_transacitonMonitorService.GetAllTMSCasefordashboard(clientId));

        //    // Step 2: Apply search
        //    if (!string.IsNullOrWhiteSpace(model.search?.value))
        //    {
        //        var searchValue = model.search.value.ToLower();
        //        allData = allData.Where(m =>
        //            (!string.IsNullOrEmpty(m.TranRefno) && m.TranRefno.ToLower().Contains(searchValue)) ||
        //            (!string.IsNullOrEmpty(m.CustomerId) && m.CustomerId.ToLower().Contains(searchValue)) ||
        //            (!string.IsNullOrEmpty(m.CustomerName) && m.CustomerName.ToLower().Contains(searchValue)) ||
        //            (!string.IsNullOrEmpty(m.RuleViolated) && m.RuleViolated.ToLower().Contains(searchValue)) ||
        //            (!string.IsNullOrEmpty(m.StatusDescription) && m.StatusDescription.ToLower().Contains(searchValue))
        //        ).ToList();
        //    }

        //    var filteredCount = allData.Count;

        //    // Step 3: Apply sorting
        //    var sortColumn = model.columns[model.order[0].column].data ?? "TranRefno";
        //    var sortDir = model.order[0].dir?.ToLower() ?? "asc";

        //    var sortedData = Sort(allData, sortColumn, sortDir);

        //    // Step 4: Apply paging
        //    var pagedData = sortedData
        //        .Skip(model.start)
        //        .Take(model.length)
        //        .ToList();

        //    // Step 5: Return result
        //    return Json(new
        //    {
        //        draw = model.draw,
        //        recordsTotal = allData.Count,
        //        recordsFiltered = filteredCount,
        //        data = pagedData
        //    });
        //}


        //private List<T> Sort<T>(List<T> data, string sortBy, string direction)
        //{
        //    if (string.IsNullOrWhiteSpace(sortBy))
        //        return data;

        //    var prop = typeof(T).GetProperty(sortBy, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        //    if (prop == null)
        //        return data;

        //    return direction == "asc"
        //        ? data.OrderBy(x => prop.GetValue(x, null)).ToList()
        //        : data.OrderByDescending(x => prop.GetValue(x, null)).ToList();
        //}





        public IActionResult TMSNewCaseReport(ReportLogSearchModel model)
        {
            model.StartDate = System.DateTime.Now.AddDays(-365);
            model.EndDate = System.DateTime.Now;
            model.TransactionStartDate = System.DateTime.Now.AddDays(-365);
            model.TransactionEndDate = System.DateTime.Now;
            return View(model);
        }

        [HttpPost("/tmscasereport/casemanagement")]
        public JsonResult TMSCaseManagementReport(DataTableModel model, string startDate, string endDate, string cust_type, string status, string txnStartDate, string txnEndDate)
        {
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }

            if (txnEndDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            dynamic list = _transacitonMonitorService.GetAllTMSCaseReport(new TMSCaseReportRequestDTO()
            {
                StartDate = startDate,
                EndDate = endDate,
                TxnStartDate = txnStartDate,
                TxnEndDate = txnEndDate,
                Status = status
            });
            List<TMSCaseDTO> abc = _mapper.Map<List<TMSCaseDTO>>(list);
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

        //[HttpPost("/CaseReportCustompagination")]
        //public JsonResult CustomPagination(DataTableModel model, string createdByUserID, string updatedByUserID, string startDate, string endDate, string cust_type, string status)
        //{
        //    if (endDate == null)
        //    {
        //        endDate = System.DateTime.Now.ToString();
        //    }
        //    if (cust_type == null)
        //    {
        //        cust_type = "";
        //    }
        //    List<CaseReportListModel> abc = _mapper.Map<List<CaseReportListModel>>(_reportService.GetCaseReportList(new CaseReportRequestDTO()
        //    {
        //        CreatedByUserId = createdByUserID,
        //        UpdatedByUserId = updatedByUserID,
        //        StartDate = startDate,
        //        EndDate = endDate,
        //        Cust_type = cust_type,
        //        Status = status
        //    }));
        //    if (!string.IsNullOrEmpty(model.search.value))
        //    {
        //        abc = abc.Where(m => m.CustomerName.ToLower().Contains(model.search.value.ToLower()) || m.CustomerID.ToLower().Contains(model.search.value.ToLower())
        //        || m.CustomerType.ToLower().Contains(model.search.value.ToLower()) || m.Status.ToLower().Contains(model.search.value.ToLower())
        //        || m.CreatedBy.ToLower().Contains(model.search.value.ToLower())).ToList();
        //    }

        //    var data = abc.Skip(model.start).Take(model.length).ToList();
        //    return Json(new
        //    {
        //        // this is what datatables wants sending back
        //        model.draw,
        //        recordsTotal = abc.Count,//totalResultsCount,
        //        recordsFiltered = abc.Count,//filteredResultsCount,
        //        data = data
        //    });
        //}


        [HttpGet("/tmscasereport/caseprocess/{tranRefno}")]
        public async Task<ActionResult> TMSCaseReportProcess(string tranRefno)
        {
            TMSCase model = _mapper.Map<TMSCase>(_transacitonMonitorService.GetTMSCaseDetailsById(tranRefno));


            List<TMSCaseNewModel> abc = _mapper.Map<List<TMSCaseNewModel>>(_transacitonMonitorService.GetTMSCasehitDetailsById(model.Id));
            model.TMSCaseNewModels = abc;
            return View(model);
        }

        [HttpGet("/tmscase/caseprocess/{tranRefno}")]
        public async Task<ActionResult> TMSCaseProcess(string tranRefno)
        {
            TMSCase model = _mapper.Map<TMSCase>(_transacitonMonitorService.GetTMSCaseDetailsById(tranRefno));
            string RemitterId = model.CustomerId;
            string BeneficiaryId = model.BeneficiaryId;
            List<TMSCaseNewModel> lastrecord = _mapper.Map<List<TMSCaseNewModel>>(_transacitonMonitorService.GetAllLastTransactionByRemitterId(RemitterId));
            model.TMSCaseViewModel = lastrecord;
            List<TMSCaseNewModel> lastbeneficiaryrecord = _mapper.Map<List<TMSCaseNewModel>>(_transacitonMonitorService.GetAllLastTransactionByBeneficiaryId(BeneficiaryId));
            model.TMSBeneficiaryViewModel = lastbeneficiaryrecord;
            List<TMSCaseNewModel> exclusiverecord = _mapper.Map<List<TMSCaseNewModel>>(_transacitonMonitorService.GetAllExculsiveTransactionBetweenRemitterBeneficiary(RemitterId, BeneficiaryId));
            model.TMSCaseExclusiveModel = exclusiverecord;
            //For matching details
            List<TMSCaseNewModel> abc = _mapper.Map<List<TMSCaseNewModel>>(_transacitonMonitorService.GetTMSCasehitDetailsById(model.Id));
            model.TMSCaseNewModels = abc;
            return View(model);
        }

        [HttpPost("/tmscase/caseprocess")]
        public JsonResult CaseProcess(TMSCase model)
        {
            string returnMsg = null;
            bool isSuccess = true;
            model.UpdatedBy = _clientHandler.GetUserId();

            var result = _transacitonMonitorService.UpdateNewTmsCase(_mapper.Map<TMSCaseDTO>(model));
            var response = "";
            if (result.Status == 200)
            {
                _toastNotification.AddSuccessToastMessage("Case Updated Successfully.");

            }
            else
            {
                _toastNotification.AddErrorToastMessage("Case Update Failed.");

            }
            return Json(model);
        }



        [HttpPost("/tms/custompagination")]
        public JsonResult CustomPagination(DataTableModel model)
        {
            dynamic list = _transacitonMonitorService.GetAll();
            List<TMSCaseModelDTO> abc = _mapper.Map<List<TMSCaseModelDTO>>(list);
            int totalcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
                || m.CustomerId.ToString().ToLower().Contains(model.search.value.ToLower())
                || m.CustomerName.ToLower().Contains(model.search.value.ToLower())
                || m.CreatedOn.ToLower().Contains(model.search.value.ToLower())
                || m.RuleViolatedText.ToLower().Contains(model.search.value.ToLower())
                || m.UpdatedStatus.ToLower().Contains(model.search.value.ToLower())).ToList();
            }
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

        [HttpGet("/tms/process/{CaseId}")]
        public async Task<ActionResult> Process(int CaseId)
        {

            TMSCaseModel model = _mapper.Map<TMSCaseModel>(_transacitonMonitorService.GetCaseDetailsById(CaseId));
            var rule = "";
            for (var i = 0; i < model.RuleViolated.Count; i++)
            {
                rule += model.RuleViolated[i].RuleName;
                if (i != model.RuleViolated.Count - 1)
                { rule += ","; }
            }
            model.RuleViolatedText = rule;
            return View(model);
        }
        [HttpPost("/tms/process")]
        public JsonResult Process(TMSCaseModel model)
        {
            string returnMsg = null;
            bool isSuccess = true;
            model.UpdatedBy = _clientHandler.GetUserId();
            if (model.Status == 0)
            {
                model.UpdatedStatus = "Pending";
            }
            else if (model.Status == 1)
            {
                model.UpdatedStatus = "Approved";
            }
            else if (model.Status == 2)
            {
                model.UpdatedStatus = "Rejected";
            }
            var result = _transacitonMonitorService.UpdateTmsCase(_mapper.Map<TMSCaseModelDTO>(model), culture);
            var response = "";
            if (result.Status == 200)
            {
                _toastNotification.AddSuccessToastMessage("Case Updated Successfully.");
                response = "Case Updated Successfully.";
            }
            else
            {
                _toastNotification.AddErrorToastMessage("Case Update Failed.");
                response = "Case Update Failed.";
            }
            return Json(response);
        }
        public IActionResult TMSReport()
        {
            TMSReportModel model = new TMSReportModel();
            model.FromDt = System.DateTime.Now.AddDays(-7);
            model.ToDt = System.DateTime.Now;
            return View(model);
        }
        public JsonResult TMSReportCustomPagination(DataTableModel model, string fromDate, string toDate)
        {
            List<TMSCaseModel> tmsReports = _mapper.Map<List<TMSCaseModel>>(_transacitonMonitorService.GetTMSBetweenDates(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate)).Result);
            int totalcount = tmsReports.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                tmsReports = tmsReports.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
                || m.CustomerId.ToString().ToLower().Contains(model.search.value.ToLower())
                || m.CustomerName.ToLower().Contains(model.search.value.ToLower())
                || m.CreatedOn.ToLower().Contains(model.search.value.ToLower())
                || m.RuleViolatedText.ToLower().Contains(model.search.value.ToLower())
                || m.UpdatedStatus.ToLower().Contains(model.search.value.ToLower())).ToList();
            }
            int filteredcount = tmsReports.Count;
            var data = tmsReports.Skip(model.start).Take(model.length).ToList();
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
        public ActionResult ViewTMSReport(int id)
        {
            TMSCaseModel model = _mapper.Map<TMSCaseModel>(_transacitonMonitorService.GetCaseDetailsById(id));
            return View(model);
        }
        [HttpGet("TransactionMonitor/ExportTMSCaseReport")]
        public async Task<IActionResult> ExportCaseReport(string startDate, string endDate, bool isPDF)
        {
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            List<TMSCaseModel> abc = _mapper.Map<List<TMSCaseModel>>(_transacitonMonitorService.GetTMSBetweenDates(Convert.ToDateTime(startDate), Convert.ToDateTime(endDate)).Result);
            int fileType = isPDF ? (int)OperationType.PDF : (int)OperationType.Excel;
            TMSReportModel downloadModel = new TMSReportModel();
            downloadModel.TMSCaseModels = abc;
            downloadModel.TotalRows = abc.Count;
            var companyName = HttpContext.Session.GetString("SessCompanyName");
            //Excel Export
            if (!isPDF)
            {
                var excelData = (from res in abc
                                 select new TMSCaseExcelModel
                                 {
                                     CreatedOn = res.CreatedOn,
                                     CustomerId = res.CustomerId,
                                     CustomerName = res.CustomerName,
                                     RuleViolatedText = res.RuleViolatedText,
                                     UpdatedStatus = res.UpdatedStatus
                                 }).ToList();
                return new ExcelResult<TMSCaseExcelModel>(excelData, "tmscasereport", "TMS_Case_Report_" + DateTime.Now.Ticks);
            }
            //var result = await _viewRenderService.RenderToStringAsync("Report/CaseReportDownload", downloadModel);
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
                string url = "wwwroot/img/logo.png";
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
                PdfPCell hd = new PdfPCell(new Phrase("TMS Case Report"));
                hd.HorizontalAlignment = 1;
                hd.FixedHeight = 20f;
                hd.VerticalAlignment = 1;
                hd.Border = 0;
                header.AddCell(hd);
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
                PdfPCell cell2 = new PdfPCell(new Phrase("CREATION DATE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
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
                PdfPCell cell5 = new PdfPCell(new Phrase("RULE VIOLATED", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell5.HorizontalAlignment = 1;
                cell5.VerticalAlignment = 1;
                cell5.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell5.FixedHeight = 30f;
                table.AddCell(cell5);
                PdfPCell cell6 = new PdfPCell(new Phrase("CASE STATUS", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell6.HorizontalAlignment = 1;
                cell6.VerticalAlignment = 1;
                cell6.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell6.FixedHeight = 30f;
                table.AddCell(cell6);
                PdfPCell cell7 = new PdfPCell(new Phrase("CREATED BY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                cell7.HorizontalAlignment = 1;
                cell7.VerticalAlignment = 1;
                cell7.BackgroundColor = BaseColor.LIGHT_GRAY;
                cell7.FixedHeight = 30f;
                table.AddCell(cell7);
                for (int i = 0; i < downloadModel.TMSCaseModels.Count; i++)
                {
                    table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.TMSCaseModels[i].CreatedOn, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.TMSCaseModels[i].CustomerId, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.TMSCaseModels[i].CustomerName, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.TMSCaseModels[i].RuleViolatedText, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.TMSCaseModels[i].UpdatedStatus, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
                    table.AddCell(new Phrase(downloadModel.TMSCaseModels[i].UpdatedByUSer, new Font(Font.FontFamily.TIMES_ROMAN, 9, Font.NORMAL)));
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




                List<TMSCaseExcelModel> list = new List<TMSCaseExcelModel>();
                var file = _exportService.ExportDataWithHeader<TMSCaseExcelModel>(list, result, fileType, "TMS_Case_Report_" + DateTime.Now.Ticks, data);
                if (file != null)
                {
                    return file;
                }
                else { return file; }
                //else
                //{
                //    var model = new ReportLogSearchModel();
                //    model.StartDate = Convert.ToDateTime(startDate);
                //    model.EndDate = Convert.ToDateTime(endDate);
                //    var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                //                select new { Id = (int)d, Name = d.ToString() };
                //    model.CaseStatusList = new SelectList(items, "Id", "Name");
                //    IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll())
                //                                           select new SelectListItem
                //                                           {
                //                                               Value = Convert.ToString(s.Id),
                //                                               Text = s.FName + " " + s.LName.ToString()
                //                                           };
                //    model.Users = new SelectList(userList, "Value", "Text");

                //    return View("CaseReport", model);
                //}
            }
        }
        [Route("/transaction-monitor-config-master")]
        public async Task<ActionResult> RulesConfig()
        {
            TMSRIskConfigurationMasterModel model = new TMSRIskConfigurationMasterModel();

            TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
            if (token.status == 400)
            {
                //_toastNotification.AddErrorToastMessage("User is not authorized for using this module.");
                model.userAuthorised = false;
            }
            else
            {
                model.userAuthorised = true;
                //var riskCategoriesAll = _transacitonMonitorService.GetAllTMSMasterCategories("en");
                var riskCategoriesAll = _transacitonMonitorService.GetAllTMSMasterCategories(culture);

                model.RiskCategories = new SelectList(riskCategoriesAll, "LovRiskCategoryCode", "LovRiskCategory");
            }
            return View(model);
        }
        public JsonResult GetRiskCategoryTypes(string riskCategoryID)
        {
            dynamic riskCategoryTypes = _transacitonMonitorService.GetRiskCategoryTypes(riskCategoryID, culture);
            return Json(new SelectList(riskCategoryTypes, "LovTypeCategoryId", "LovCategoryType"));
        }
        public JsonResult GetRiskTypes(string riskTypeCategoryID)
        {
            var riskTypes = _transacitonMonitorService.GetRiskTypes(riskTypeCategoryID, culture);
            return Json(new SelectList(riskTypes, "LovTypeId", "LovTypeName"));
        }
        [HttpPost]
        public JsonResult CustomPaginationRiskTypeCategory(DataTableModel model, string riskCategoryID)
        {
            List<TMSTypeCategoryDTO> abc = _transacitonMonitorService.GetRiskCategoryType(riskCategoryID, culture);
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
        public JsonResult CustomPaginationRiskType(DataTableModel model, string RiskTypeCategoryID)
        {
            List<TMSMasterModel> abc = _mapper.Map<List<TMSMasterModel>>(_transacitonMonitorService.GetRiskType(RiskTypeCategoryID, culture));
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
        [HttpPost("/TransactionMonitor/custompaginationRiskItems")]
        public JsonResult CustomPaginationRiskItems(DataTableModel model, string riskTypeID)
        {
            List<TMSMasterModel> abc = _mapper.Map<List<TMSMasterModel>>(_transacitonMonitorService.GetRiskItems(Convert.ToInt32(riskTypeID), culture));
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
        public IActionResult ViewRules()
        {
            TMSMasterModel model = new TMSMasterModel();
            List<TMSRulesViewModel> all = _mapper.Map<List<TMSRulesViewModel>>(_transacitonMonitorService.GetAllRules());
            model.TMSRulesViewModel = all;
            return View(model);
        }

        public IActionResult TMSRules()
        {

            TMSNewRulesModel model = new TMSNewRulesModel();
            model.TMSRuleNames = new SelectList(_mapper.Map<List<TMSNewRulesNamesModel>>(_transacitonMonitorService.GetRuleNames()), "TMSRuleID", "TMSRuleName");
            model.TMSHeaderMapScreennames = new SelectList(_mapper.Map<List<TMSFieldsNamesmodel>>(_transacitonMonitorService.GetFieldsnames()), "TMSHeaderName", "TMSHeaderMapScreenname");
            model.TMSCustType = new SelectList(_mapper.Map<List<TMSCustomerTypeModel>>(_transacitonMonitorService.GetCustomTypedrpdown()), "TMSCustTypeId", "TMSCustType");
            model.TMSTranType = new SelectList(_mapper.Map<List<TMSTranType>>(_transacitonMonitorService.GetTranTypedrpdown()), "TranTypeCode", "TranTypeName");
            return View(model);
        }

        [HttpPost("/TransactionMonitor/TMSRules")]
        public JsonResult TMSRules(TMSNewRulesModel superObject)
        {


            var count_tr = superObject.TMSRuleParameters.Count;


            //superObject.random_gen_id = random_id;
            superObject.TMSRuleIsActive = 1;
            superObject.TMSRuleCreatedBy = _clientHandler.GetUserId();
            superObject.TMSRuleCreatedOn = DateTime.Now;
            superObject.Client_Id = _clientHandler.GetClientId();
            var result = _transacitonMonitorService.InsertTMSRulesMaster(_mapper.Map<TMSRulesMasterDTO>(superObject));

            var ruleid = result.Result;

            TMSRulesDetailsModel pers = new TMSRulesDetailsModel();


            for (var i = 0; i < count_tr; i++)
            {
                pers.TMSRuleDetParamType = superObject.TMSRuleParameters[i].TMSRuleDetParamType;
                pers.TMSRuleDetOperator = superObject.TMSRuleParameters[i].TMSRuleDetOperator;
                pers.TMSRuleDetCompareTo = superObject.TMSRuleParameters[i].TMSRuleDetCompareTo;
                pers.TMSRuleDetCompareValue = superObject.TMSRuleParameters[i].TMSRuleDetCompareValue;
                pers.TMSRuleDetForSame = superObject.TMSRuleParameters[i].TMSRuleDetForSame;
                pers.TMSRuleDetTimeFrameType = superObject.TMSRuleParameters[i].TMSRuleDetTimeFrameType;
                pers.TMSRuleDetTimeFrameVal = superObject.TMSRuleParameters[i].TMSRuleDetTimeFrameVal;
                pers.TMSRuleDetValue = superObject.TMSRuleParameters[i].TMSRuleDetValue;
                pers.TMSRuleDetCreatedBy = _clientHandler.GetUserId();
                pers.TMSRuleDetAutoID = (i + 1).ToString();
                pers.TMSRuleDetMasterID = ruleid;
                pers.TMSRuleDetSource = superObject.TMSRuleParameters[i].TMSRuleDetSource;
                pers.TMSRuleDetSourceAggr = superObject.TMSRuleParameters[i].TMSRuleDetSourceAggr;
                pers.TMSRuleDetCompareTimeStart = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeStart;
                pers.TMSRuleDetCompareTimeStartType = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeStartType;
                pers.TMSRuleDetCompareTimeEnd = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeEnd;
                pers.TMSRuleDetCompareTimeEndType = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeEndType;
                pers.TMSRuleDetTarget = superObject.TMSRuleParameters[i].TMSRuleDetTarget;
                pers.TMSRuleDetDescription = superObject.TMSRuleParameters[i].TMSRuleDetDescription;
                pers.TMSRuleDetTargetAggr = superObject.TMSRuleParameters[i].TMSRuleDetTargetAggr;
                pers.TMSOperatorGRP = superObject.TMSRuleParameters[i].TMSOperatorGRP;
                pers.TMSRuleDetTimeFrameSource = superObject.TMSRuleParameters[i].TMSRuleDetTimeFrameSource;
                pers.TMSRuleDetPercentValue = superObject.TMSRuleParameters[i].TMSRuleDetPercentValue;

                pers.TMSRuleDetSourceIfSource = superObject.TMSRuleParameters[i].TMSRuleDetSourceIfSource;
                pers.TMSRuleDetSourceIfSourceAggr = superObject.TMSRuleParameters[i].TMSRuleDetSourceIfSourceAggr;
                pers.TMSRuleDetCompareFieldIfSource = superObject.TMSRuleParameters[i].TMSRuleDetCompareFieldIfSource;
                pers.TMSRuleDetCompareFieldIfSourceAggr = superObject.TMSRuleParameters[i].TMSRuleDetCompareFieldIfSourceAggr;
                pers.TMSRuleDetSourceIfValue = superObject.TMSRuleParameters[i].TMSRuleDetSourceIfValue;
                pers.TMSRuleDetCompareFieldIfValue = superObject.TMSRuleParameters[i].TMSRuleDetCompareFieldIfValue;
                pers.TMSRuleDetSourceIfOperator = superObject.TMSRuleParameters[i].TMSRuleDetSourceIfOperator;
                pers.TMSRuleDetCompareFieldIfOperator = superObject.TMSRuleParameters[i].TMSRuleDetCompareFieldIfOperator;
                pers.Client_Id =_clientHandler.GetClientId();

                var result1 = _transacitonMonitorService.InsertTMSRulesDetails(_mapper.Map<TMSRulesDetailsDTO>(pers));
            }



            ViewBag.Message = "Rule Has Been saved successfully.";

            _toastNotification.AddSuccessToastMessage("Rule Has Been saved successfully.");

            Dictionary<String, string> myAjaxResult = new Dictionary<string, string>();
            myAjaxResult.Add("status_code", "200");
            myAjaxResult.Add("msg", "Rule Has been saved");

            return Json(myAjaxResult);
        }

        //public IActionResult EditTMSRules()
        //{

        //    TMSNewRulesModel model = new TMSNewRulesModel();
        //    return View("TMSRules", model);

        //}

        //[HttpPost]
        //public IActionResult EditTMSRules(int id)
        //{

        //    //TMSNewRulesModel model = new TMSNewRulesModel();

        //    List<TMSRulesDetailsModel> all = _mapper.Map<List<TMSRulesDetailsModel>>(_transacitonMonitorService.GetAllRuleparamters(id));


        //    return Ok(all);


        //    //return View("EditTMSRules");

        //}

        [HttpPost("/TransactionMonitor/TMSTestRule")]
        public IActionResult TMSTestRule(TMSNewRulesModel superObject)
        {
            var count_tr = superObject.TMSRuleParameters.Count;



            TMSRulesDetailsModel pers = new TMSRulesDetailsModel();

            List<TMSNewMasterDTO> MasterHits = new List<TMSNewMasterDTO>();

            for (var i = 0; i < count_tr; i++)
            {
                pers.TMSRuleDetParamType = superObject.TMSRuleParameters[i].TMSRuleDetParamType;
                pers.TMSRuleDetOperator = superObject.TMSRuleParameters[i].TMSRuleDetOperator;
                pers.TMSRuleDetCompareTo = superObject.TMSRuleParameters[i].TMSRuleDetCompareTo;
                pers.TMSRuleDetCompareValue = superObject.TMSRuleParameters[i].TMSRuleDetCompareValue;
                pers.TMSRuleDetForSame = superObject.TMSRuleParameters[i].TMSRuleDetForSame;
                pers.TMSRuleDetTimeFrameType = superObject.TMSRuleParameters[i].TMSRuleDetTimeFrameType;
                pers.TMSRuleDetTimeFrameVal = superObject.TMSRuleParameters[i].TMSRuleDetTimeFrameVal;

                int intCompareValue;

                bool success = int.TryParse(superObject.TMSRuleParameters[i].TMSRuleDetCompareValue, out intCompareValue);
                if (success)
                {
                    pers.TMSRuleDetDynamicCompareValue = intCompareValue;
                }
                else
                {
                    pers.TMSRuleDetDynamicCompareValue = superObject.TMSRuleParameters[i].TMSRuleDetCompareValue;
                }


                pers.TMSRuleDetValue = superObject.TMSRuleParameters[i].TMSRuleDetValue;
                pers.TMSRuleDetCreatedBy = _clientHandler.GetUserId();
                pers.TMSRuleDetAutoID = (i + 1).ToString();
                pers.TMSRuleDetSource = superObject.TMSRuleParameters[i].TMSRuleDetSource;
                pers.TMSRuleDetSourceAggr = superObject.TMSRuleParameters[i].TMSRuleDetSourceAggr;
                pers.TMSRuleDetCompareTimeStart = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeStart;
                pers.TMSRuleDetCompareTimeStartType = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeStartType;
                pers.TMSRuleDetCompareTimeEnd = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeEnd;
                pers.TMSRuleDetCompareTimeEndType = superObject.TMSRuleParameters[i].TMSRuleDetCompareTimeEndType;
                pers.TMSRuleDetTarget = superObject.TMSRuleParameters[i].TMSRuleDetTarget;
                pers.TMSRuleDetDescription = superObject.TMSRuleParameters[i].TMSRuleDetDescription;
                pers.TMSRuleDetTargetAggr = superObject.TMSRuleParameters[i].TMSRuleDetTargetAggr;
                pers.TMSRuleDetTimeFrameSource = superObject.TMSRuleParameters[i].TMSRuleDetTimeFrameSource;
                pers.TMSRuleDetPercentValue = superObject.TMSRuleParameters[i].TMSRuleDetPercentValue;
                pers.TMSRuleDetCompareIsPercent = superObject.TMSRuleParameters[i].TMSRuleDetCompareIsPercent;
            }

            var TMSRule = superObject;

            var DTOobj = _mapper.Map<TMSRulesMasterDTO>(TMSRule);

            var Transactions = _transacitonMonitorService.GetAllTransactionsForTMS();

            bool ruleHit = DTOobj.TMSRuleParameters[0].TMSOperatorGRP == "OR" ? false : true;



            var ruleid = _transacitonMonitorService.GetTMSDraftLogRuleId();

         
            foreach (var ruleParam in DTOobj.TMSRuleParameters)
            {

                int intCompareValue;

                bool success = int.TryParse(ruleParam.TMSRuleDetCompareValue, out intCompareValue);
                if (success)
                {
                    ruleParam.TMSRuleDetDynamicCompareValue = intCompareValue;
                }
                else
                {
                    ruleParam.TMSRuleDetDynamicCompareValue = ruleParam.TMSRuleDetCompareValue;
                }


                //(bool isRuleHit, List<TMSNewMasterDTO> hits) = Builder.CheckRule(ruleParam, Transactions);

                (bool isRuleHit, List<TMSNewMasterDTO> hits) = Builder.CheckRule(ruleParam, MasterHits.Count() > 0 ? MasterHits : Transactions, ruleParam.TMSRuleDetCustomCheckerString, ruleParam.TMSRuleDetCustomGetterString);
                

                Console.WriteLine($"Rule Hit? {isRuleHit}, {hits?.Count} hits returned");

                if (isRuleHit)
                {
                    MasterHits = hits;
                }

                if (ruleParam.TMSOperatorGRP == "AND") ruleHit = isRuleHit && ruleHit;
                if (ruleParam.TMSOperatorGRP == "OR") ruleHit = isRuleHit || ruleHit;



                TMSDraftLogModel draftlog = new TMSDraftLogModel();
                draftlog.TMSRuleId = ruleid.Result + 1;
                draftlog.TMSRuleName = superObject.TMSRuleName;
                draftlog.TMSScore = superObject.TMSRuleScore;
                draftlog.TMSRuleDescription = superObject.TMSRuleDescription;
                draftlog.TMSRuleparameter = ruleParam.TMSRuleDetParamType;
                draftlog.TMSRuleParameterDescription = ruleParam.TMSRuleDetDescription;
                draftlog.TMSRuleOperator = ruleParam.TMSOperatorGRP;
                Encoding u8 = Encoding.UTF8;
                // byte[] dataAsBytes = hit.SelectMany(s =>
                // System.Text.Encoding.UTF8.GetBytes(s + Environment.NewLine)).ToArray();

                //draftlog.TMSResult = dataAsBytes;
                //draftlog.Status = 1;
                draftlog.TMSCreatedBy = _clientHandler.GetUserId();
                draftlog.client_id = _clientHandler.GetClientId();
                //YourObject objectToSerialize = new YourObject();
                draftlog.TMSResult = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(MasterHits));

                var draftlogresult = _transacitonMonitorService.InsertTMSDraftLog(_mapper.Map<TMSDraftLogModelDTO>(draftlog));



            }

            if (ruleHit && MasterHits.Count != 0)
            {

                Dictionary<String, object> myAjaxResult = new Dictionary<string, object>();
                myAjaxResult.Add("status_code", "200");
                myAjaxResult.Add("msg", "Hits Has been found");
                myAjaxResult.Add("info", MasterHits);
                //List list = new List();
                //list.Add("status_code", "200");

                return Json(myAjaxResult);


            }
            else
            {
                _toastNotification.AddErrorToastMessage("There Are  No Hits For these rules.");
                Dictionary<String, object> myAjaxResult = new Dictionary<string, object>();

                myAjaxResult.Add("status_code", "0");
                myAjaxResult.Add("msg", "Hits Not Found");


                return Json(myAjaxResult);
                //return Ok(hit);
            }






            return View("TMSRules", superObject);
        }


        public IActionResult ViewTMSDraftLog()
        {

            var userid = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();

            List<TMSDraftLogModel> model = _mapper.Map<List<TMSDraftLogModel>>(_transacitonMonitorService.GetAllTMSDraftlog(clientId,userid));

            var count_tr = model.Count();

            List<DataListModel> list = new List<DataListModel>();

            for (var i = 0; i < count_tr; i++)
            {
                List<DataListModel> tMSDraftLogs = _mapper.Map<List<DataListModel>>(_transacitonMonitorService.GetAllTMSDraftlogID(model[i].TMSRuleId));

                model[i].DataList = tMSDraftLogs;
                // Removed BinaryFormatter usage
                /*
                using (var ms = new MemoryStream(model[i].TMSResult))
                {
                    // Newtonsoft.Json replacement:
                    // var obj = JsonConvert.DeserializeObject<List<TMSNewMasterDTO>>(Encoding.UTF8.GetString(model[i].TMSResult));
                }
                */
            }


                return View(model);
            

        }


        [HttpPost("/TransactionMonitor/TmsResults")]
        public IActionResult TmsResults(int id)
        {
            //return Ok(id);
            //List<TMSDraftLogModel> model = _mapper.Map<List<TMSDraftLogModel>>(_transacitonMonitorService.GetAllTMSDraftlog());

            DataListModel model = new DataListModel();
            // List<TMSNewMasterDTO>
            TMSDraftLogModel tMSDraftLogs = _mapper.Map<TMSDraftLogModel>(_transacitonMonitorService.GetAllTMSDraftlogResults(id));
                model.TMSResults= tMSDraftLogs.TMSResult;
                string jsonResult = Encoding.UTF8.GetString(model.TMSResults);
                object obj = JsonConvert.DeserializeObject(jsonResult);

                Dictionary<String, object> myAjaxResult = new Dictionary<string, object>();
                myAjaxResult.Add("status_code", "200");
                myAjaxResult.Add("msg", "Hits Has been found");
                myAjaxResult.Add("info", obj);

                return Json(myAjaxResult);

            return View();

        }

        

        //[HttpGet]
        //public IActionResult DeleteTMSRules()
        //{
        //    TMSNewRulesModel model = new TMSNewRulesModel();
        //    model.TMSRuleNames = new SelectList(_mapper.Map<List<TMSNewRulesNamesModel>>(_transacitonMonitorService.GetRuleNames()), "TMSRuleID", "TMSRuleName");
        //    return View("TMSRules", model);
        //}

        //[HttpPost("/TransactionMonitor/TMSRulesdetails")]
        //public IActionResult TMSRulesdetails(int id)
        //{
        //    List<TMSRulesDetailsModel> all = _mapper.Map<List<TMSRulesDetailsModel>>(_transacitonMonitorService.GetAllRuleparamters(id));
        //    return Ok(all);

        //}

        //[HttpPost("/TransactionMonitor/DeleteTMSRules")]
        //public IActionResult DeleteTMSRules(int ruleid)
        //{
        //    var result = _transacitonMonitorService.UpdateTMSRuleMasterStatus(ruleid);
        //    return View("TMSRules", ruleid);

        //}
    }
    
    }

