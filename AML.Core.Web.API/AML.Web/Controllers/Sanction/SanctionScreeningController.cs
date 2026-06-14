
using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.Repository;
using AML.Core.Service.CustomerCategory;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.Sanction;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Report;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.Sanction;
using AML.ViewModel.ViewModels.User;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using iTextSharp.text;
using iTextSharp.text.pdf;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.Controllers.Sanction
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class SanctionScreeningController : Controller
    {
        private IMapper _mapper;
        private ICountryService _countryService;
        private IScreeningService _screeningService;
        private IHttpClientHandler _clientHandler;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private ICustomerScreeningService _customerScreeningService;
        private ICustomerCaseService _customerCaseService;
        private ICustomerCategoryService _customerCategoryService;
        private int clientId = 0;
        public SanctionScreeningController(IMapper mapper, ICountryService countryService, IScreeningService screeningService, IHttpClientHandler clientHandler, IViewRenderService viewRenderService, IExportDataService exportService, ICustomerScreeningService customerScreeningService, ICustomerCaseService customerCaseService, ICustomerCategoryService customerCategoryService)
        {
            _mapper = mapper;
            _countryService = countryService;
            _screeningService = screeningService;
            _clientHandler = clientHandler;
            _viewRenderService = viewRenderService;
            _exportService = exportService;
            _customerScreeningService = customerScreeningService;
            _customerCaseService = customerCaseService;
            clientId = clientHandler.GetClientId();
            _customerCategoryService = customerCategoryService;
        }
        public IActionResult Index()
        {
            ScreeningSearchModel model = new ScreeningSearchModel();
            model.CustomerCategories = new SelectList(
   _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
       .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
       .ToList(),
   "Code",
   "Name"
);
            model.SelectionProperties = new[] { "Exact Match", "Partial Match", "Phonetic Match" };
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            return View(model);
        }

        [HttpPost]
        public IActionResult SearchList(ScreeningSearchModel model)
        {
            string response = string.Empty;

            model.clientId = _clientHandler.GetClientId();
            var searchType = "E";//changing from P to F as in front end no options showing
            if (model.SelectionProperty != null)
            {
                var selection = String.Concat(model.SelectionProperty.Where(c => !Char.IsWhiteSpace(c))).ToLower();
                if (selection == "exactmatch")
                    searchType = "E";
                else if (selection == "partialmatch")
                    searchType = "P";
                else if (selection == "phoneticmatch")
                    searchType = "F";
            }

            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.DOB,
                customerfullname = model.Name,
                customernationality = model.Nationality,
                searchtype = searchType,
                customertype=model.customerType
            }, ScreeningService.BACKLIST_SCREENING).Result;
            List<ApiResultModel> apiResultModel = new List<ApiResultModel>();
            if (!string.IsNullOrEmpty(data))
            {
                 apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                    response = AMLUtility.FormatJsonToPlainText(data);
                    model.DataList = apiResultModel;
                    dynamic mdob;
                    try
                    {
                        mdob = Convert.ToDateTime(apiResultModel[0].matchdob);
                    }
                    catch
                    {
                        mdob = Convert.ToDateTime(DateTime.Now);
                    }
                    //Insert Logs
                    _customerScreeningService.InsertSanctionScreeningLogs(new DTO.DTO.Sanction.SanctionScreeningLogDTO {
                        CustomerName = model.Name,
                        Nationality = model.Nationality,
                        DOB = Convert.ToDateTime(model.DOB),
                        SearchType = searchType,
                        CustomerType=model.customerType,
                        MatchName = apiResultModel[0].matchname,
                        MatchScore = Convert.ToInt32(apiResultModel[0].matchscore),
                        MatchUID = apiResultModel[0].matchuid,
                        MatchCategory = apiResultModel[0].matchcategory,
                        MatchType = apiResultModel[0].status == "D" ? apiResultModel[0].matchtype + "-Former" : apiResultModel[0].matchtype,
                        MatchNationality = apiResultModel[0].matchnationality,
                        MatchIDNum = apiResultModel[0].matchidnumber,
                        MatchDOB = mdob,
                        CreatedBy = _clientHandler.GetUserId(),
                        ClientId = _clientHandler.GetClientId()
                });
            }
            
            model.SearchLogs = _mapper.Map<List<SanctionScreeningLogModel>>(_customerScreeningService.GetSanctionScreeningLogs(model.Name,model.Nationality,model.DOB,model.customerType,clientId));

            model.CaseLogs = _mapper.Map<List<CaseModel>>(_customerScreeningService.GetCaseLogs(model.Name, model.Nationality, model.DOB, model.customerType, clientId));


            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.CustomerCategories = new SelectList(
              _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
                  .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
                  .ToList(),
              "Code",
              "Name"
            );
            return View("Index",model);
        }

        [HttpGet]
        public async Task<IActionResult> DownloadPDF(ScreeningSearchModel model)
        {
            string response = string.Empty;
            var searchType = "F";
            if (model.SelectionProperty != null)
            {
                var selection = String.Concat(model.SelectionProperty.Where(c => !Char.IsWhiteSpace(c))).ToLower();
                if (selection == "exactmatch")
                    searchType = "E";
                else if (selection == "partialmatch")
                    searchType = "P";
                else if (selection == "phoneticmatch")
                    searchType = "F";
            }

            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = model.Name,
                customernationality = model.Nationality,
                searchtype = searchType
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(data))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                
                int fileType = (int)OperationType.PDF;
                var result = await _viewRenderService.RenderToStringAsync("SanctionScreening/ExportSearchList", apiResultModel);
                var file = _exportService.ExportData<ApiResultModel>(apiResultModel, result, fileType, "Sanction_Screening_" + DateTime.Now.Ticks);
                if (file != null)
                {
                    return file;
                }
            }
            return Json(response);
        }
        [HttpPost]
        public FileResult DownloadPDFs([FromForm] ScreeningSearchModel model)
        {
            var clientId = _clientHandler.GetClientId();
            var userName = _clientHandler.GetUserId();
            var currentTab = model.activeTable ?? "table1";
            var clientData = _customerCaseService.GetClientDetailsByID(clientId);
            var logos = "wwwroot/img/" + (clientData?.DocumentFileName ?? "");
            var companyName = clientData?.ClientName ?? "Search";

            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                Document document = new Document(PageSize.A4.Rotate(), 15, 15, 15, 15);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                // Logo and Company Name
                PdfPTable logoTable = new PdfPTable(2);
                logoTable.WidthPercentage = 100;
                logoTable.SetWidths(new float[] { 3f, 1f });
                logoTable.DefaultCell.Border = 0;

                PdfPCell compNameCell = new PdfPCell(new Phrase(companyName, new Font(Font.FontFamily.TIMES_ROMAN, 16, Font.BOLD)));
                compNameCell.Border = 0;
                compNameCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                logoTable.AddCell(compNameCell);

                try {
                    Image logoImg = Image.GetInstance(logos);
                    logoImg.ScaleToFit(100, 50);
                    PdfPCell logoCell = new PdfPCell(logoImg);
                    logoCell.Border = 0;
                    logoCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                    logoTable.AddCell(logoCell);
                } catch {
                    logoTable.AddCell(new PdfPCell(new Phrase("")) { Border = 0 });
                }
                document.Add(logoTable);
                document.Add(new Paragraph("\n"));

                // Report Header
                string reportTitle = "Search Results Report";
                if (currentTab == "table2") reportTitle = "Search History Report";
                else if (currentTab == "table3") reportTitle = "Existing Case Matches Report";

                PdfPTable header = new PdfPTable(1);
                header.WidthPercentage = 100;
                header.DefaultCell.Border = 0;
                header.AddCell(new PdfPCell(new Phrase("Report Name: " + reportTitle, new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD))) { Border = 0 });
                header.AddCell(new PdfPCell(new Phrase("Extraction Date: " + DateTime.Now.ToString("dd/MM/yyyy HH:mm"), new Font(Font.FontFamily.TIMES_ROMAN, 10))) { Border = 0 });
                header.AddCell(new PdfPCell(new Phrase("Extracted By: " + userName, new Font(Font.FontFamily.TIMES_ROMAN, 10))) { Border = 0 });
                
                string filters = $"Name: {model.Name}, Nationality: {model.Nationality ?? "All"}, DOB: {model.DOB ?? "All"}";
                header.AddCell(new PdfPCell(new Phrase("Filters Applied: " + filters, new Font(Font.FontFamily.TIMES_ROMAN, 10))) { Border = 0 });
                document.Add(header);
                document.Add(new Paragraph("\n"));

                PdfPTable table;
                if (currentTab == "table1")
                {
                    // Case 1: Search Results
                    var searchType = "F"; // Standardize to Phonetic/Fuzzy as per SearchList logic
                    string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                    {
                        customerdob = model.DOB, // Already Y-m-d from frontend
                        customerfullname = model.Name,
                        customernationality = model.Nationality,
                        searchtype = searchType
                    }, ScreeningService.BACKLIST_SCREENING).Result;

                    List<ApiResultModel> results = !string.IsNullOrEmpty(data) ? JsonConvert.DeserializeObject<List<ApiResultModel>>(data) : new List<ApiResultModel>();

                    table = new PdfPTable(9);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 0.5f, 2f, 0.8f, 1f, 1.5f, 1f, 1.5f, 1.5f, 1.5f });
                    
                    string[] headers = { "#", "NAME", "SCORE", "UID", "CATEGORY", "TYPE", "NATIONALITY", "ID NUMBER", "DOB" };
                    foreach (var h in headers) {
                        table.AddCell(new PdfPCell(new Phrase(h, new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD))) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    for (int i = 0; i < results.Count; i++) {
                        table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchname ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchscore ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchuid ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchcategory ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase((results[i].matchtype ?? "") + (results[i].status == "D" ? "-former" : ""), new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchnationality ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchidnumber ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(results[i].matchdob ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                    }
                }
                else if (currentTab == "table2")
                {
                    // Case 2: Search History
                    var logs = _customerScreeningService.GetSanctionScreeningLogs(model.Name, model.Nationality, model.DOB, model.customerType, clientId);
                    
                    table = new PdfPTable(8);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 0.5f, 1.5f, 1.5f, 1f, 1.5f, 1.5f, 1f, 1.5f });

                    string[] headers = { "#", "SEARCHED ON", "DATASETS", "CUST TYPE", "CUST NAME", "NATIONALITY", "DOB", "SCORE" };
                    foreach (var h in headers) {
                        table.AddCell(new PdfPCell(new Phrase(h, new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD))) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    for (int i = 0; i < logs.Count; i++) {
                        table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(logs[i].CreatedOn.ToString("dd/MM/yyyy HH:mm"), new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(logs[i].MatchCategory ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9))); // Used as Datasets in UI
                        table.AddCell(new Phrase(logs[i].CustomerType ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(logs[i].CustomerName ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(logs[i].Nationality ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(logs[i].DOB.ToString("dd/MM/yyyy"), new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(logs[i].MatchScore.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 9)));
                    }
                }
                else
                {
                    // Case 3: Existing Case Match
                    var cases = _customerScreeningService.GetCaseLogs(model.Name, model.Nationality, model.DOB, model.customerType, clientId);
                    
                    table = new PdfPTable(10);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 0.5f, 1.2f, 1.2f, 1.2f, 1f, 1.5f, 1f, 0.8f, 1f, 1f });

                    string[] headers = { "#", "CUST ID", "CREATED", "UPDATED", "TYPE", "NAME", "SCORE", "RISK", "USER", "STATUS" };
                    foreach (var h in headers) {
                        table.AddCell(new PdfPCell(new Phrase(h, new Font(Font.FontFamily.TIMES_ROMAN, 8, Font.BOLD))) { BackgroundColor = BaseColor.LIGHT_GRAY, HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    for (int i = 0; i < cases.Count; i++) {
                        string customerName = (cases[i].FirstName + " " + (string.IsNullOrEmpty(cases[i].MiddleName) ? "" : cases[i].MiddleName + " ") + cases[i].LastName).Trim();
                        
                        table.AddCell(new Phrase((i + 1).ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].CustomerId ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].CreatedOn ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].UpdatedOn ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].CustomerType ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(customerName, new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].MatchScore.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].Individual_final_risk_score ?? cases[i].corporate_final_risk_score ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].CreatedUser ?? cases[i].CreatedBy.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                        table.AddCell(new Phrase(cases[i].Status.ToString(), new Font(Font.FontFamily.TIMES_ROMAN, 8)));
                    }
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
                document.Add(new Phrase("Computer generated report; hence no signature is required.", new Font(Font.FontFamily.TIMES_ROMAN, 8, Font.ITALIC)));
                
                document.Close();
                byte[] datas = memoryStream.ToArray();

                return File(datas, "application/pdf", $"{reportTitle.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
            }
        }

        [HttpGet]
        public async Task<IActionResult> SanctionScreening_PDF(string name, string nationality, string dob, string customerType, string selectedColumns, string orientation)
        {
            var model = new ScreeningSearchModel
            {
                Name = name,
                Nationality = nationality,
                DOB = dob,
                customerType = customerType,
                clientId = clientId
            };

            var searchType = "F"; 

            // 1. Fetch search results (Tab 1)
            string dataValue = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.DOB,
                customerfullname = model.Name,
                customernationality = model.Nationality,
                searchtype = searchType,
                customertype = model.customerType
            }, ScreeningService.BACKLIST_SCREENING).Result;

            if (!string.IsNullOrEmpty(dataValue))
            {
                model.DataList = JsonConvert.DeserializeObject<List<ApiResultModel>>(dataValue);
            }

            // 2. Fetch search history (Tab 2)
            model.SearchLogs = _mapper.Map<List<SanctionScreeningLogModel>>(_customerScreeningService.GetSanctionScreeningLogs(model.Name, model.Nationality, model.DOB, model.customerType, clientId));

            // 3. Fetch case logs (Tab 3)
            model.CaseLogs = _mapper.Map<List<CaseModel>>(_customerScreeningService.GetCaseLogs(model.Name, model.Nationality, model.DOB, model.customerType, clientId));

            ViewBag.SelectedColumns = selectedColumns;
            ViewBag.Orientation = orientation ?? "portrait";
            return View("SanctionScreening_PDF", model);
        }

        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> ConsolidatedExportExcel(string name, string nationality, string dob, string customerType, string selectedColumns = null, string orientation = "portrait")
        {
            var searchType = "F";
            var clientId = _clientHandler.GetClientId();

            // 1. Fetch Search Results
            string dataValue = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = dob,
                customerfullname = name,
                customernationality = nationality,
                searchtype = searchType,
                customertype = customerType
            }, ScreeningService.BACKLIST_SCREENING).Result;

            var dataList = !string.IsNullOrEmpty(dataValue) 
                ? JsonConvert.DeserializeObject<List<ApiResultModel>>(dataValue) 
                : new List<ApiResultModel>();

            // 2. Fetch Search History
            var searchLogs = _mapper.Map<List<SanctionScreeningLogModel>>(_customerScreeningService.GetSanctionScreeningLogs(name, nationality, dob, customerType, clientId));

            // 3. Fetch Case Logs
            var caseLogs = _mapper.Map<List<CaseModel>>(_customerScreeningService.GetCaseLogs(name, nationality, dob, customerType, clientId));

            using (var package = new ExcelPackage())
            {
                // Styling Helper
                Action<ExcelRange> applyHeaderStyle = (range) => {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0xE9, 0xEF, 0xFD));
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Color.SetColor(System.Drawing.Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                };

                // Tab 1: Search Results
                var sheet1 = package.Workbook.Worksheets.Add("Search Results");
                var colMap1 = new List<(string id, string label)> {
                    ("matchtype", "Type"),
                    ("matchcategory", "Category"),
                    ("matchname", "Name"),
                    ("matchnationality", "Nationality"),
                    ("matchdob", "DOB"),
                    ("matchscore", "Score"),
                    ("matchuid", "UID"),
                    ("matchidnumber", "ID Number")
                };

                var selectedCols1 = string.IsNullOrEmpty(selectedColumns) 
                    ? colMap1.Select(x => x.id).ToList() 
                    : selectedColumns.Split(',').ToList();
                
                var activeCols1 = colMap1.Where(x => selectedCols1.Contains(x.id)).ToList();

                for (int i = 0; i < activeCols1.Count; i++) sheet1.Cells[1, i + 1].Value = activeCols1[i].label;
                if (activeCols1.Count > 0) applyHeaderStyle(sheet1.Cells[1, 1, 1, activeCols1.Count]);

                int row1 = 2;
                foreach (var item in dataList)
                {
                    for (int i = 0; i < activeCols1.Count; i++)
                    {
                        var colId = activeCols1[i].id;
                        var cell = sheet1.Cells[row1, i + 1];
                        if (colId == "matchtype") cell.Value = (item.matchtype ?? "") + (item.status == "D" ? "-former" : "");
                        else if (colId == "matchcategory") cell.Value = item.matchcategory;
                        else if (colId == "matchname") cell.Value = item.matchname;
                        else if (colId == "matchnationality") cell.Value = item.matchnationality;
                        else if (colId == "matchdob") cell.Value = item.matchdob;
                        else if (colId == "matchscore") cell.Value = item.matchscore;
                        else if (colId == "matchuid") cell.Value = item.matchuid;
                        else if (colId == "matchidnumber") cell.Value = item.matchidnumber;
                    }
                    row1++;
                }
                if (row1 > 2) { sheet1.Cells[sheet1.Dimension.Address].AutoFitColumns(); sheet1.View.FreezePanes(2, 1); }

                // Tab 2: Search History
                var sheet2 = package.Workbook.Worksheets.Add("Search History");
                string[] h2 = { "Searched On", "Datasets", "Cust Type", "Name", "Nationality", "DOB", "Score", "User" };
                for (int i = 0; i < h2.Length; i++) sheet2.Cells[1, i + 1].Value = h2[i];
                applyHeaderStyle(sheet2.Cells[1, 1, 1, h2.Length]);

                int row2 = 2;
                foreach (var log in searchLogs)
                {
                    sheet2.Cells[row2, 1].Value = log.CreatedOn.ToString("dd/MM/yyyy HH:mm");
                    sheet2.Cells[row2, 2].Value = log.MatchCategory;
                    sheet2.Cells[row2, 3].Value = log.CustomerType;
                    sheet2.Cells[row2, 4].Value = log.CustomerName;
                    sheet2.Cells[row2, 5].Value = log.Nationality;
                    sheet2.Cells[row2, 6].Value = log.DOB.ToString("dd/MM/yyyy");
                    sheet2.Cells[row2, 7].Value = log.MatchScore;
                    sheet2.Cells[row2, 8].Value = log.CreatedUser;
                    row2++;
                }
                if (row2 > 2) { sheet2.Cells[row2-1, 1, row2-1, h2.Length].AutoFitColumns(); sheet2.View.FreezePanes(2, 1); }

                // Tab 3: Existing Case Match
                var sheet3 = package.Workbook.Worksheets.Add("Case Logs");
                string[] h3 = { "Cust ID", "Created", "Updated", "Type", "Name", "Score", "Risk", "User", "Status" };
                for (int i = 0; i < h3.Length; i++) sheet3.Cells[1, i + 1].Value = h3[i];
                applyHeaderStyle(sheet3.Cells[1, 1, 1, h3.Length]);

                int row3 = 2;
                foreach (var c in caseLogs)
                {
                    sheet3.Cells[row3, 1].Value = c.CustomerId;
                    sheet3.Cells[row3, 2].Value = c.CreatedOn;
                    sheet3.Cells[row3, 3].Value = c.UpdatedOnDB;
                    sheet3.Cells[row3, 4].Value = c.CustomerType;
                    sheet3.Cells[row3, 5].Value = $"{c.FirstName} {c.LastName}".Trim();
                    sheet3.Cells[row3, 6].Value = c.MatchScore;
                    sheet3.Cells[row3, 7].Value = c.RiskScore;
                    sheet3.Cells[row3, 8].Value = c.CreatedUser;
                    sheet3.Cells[row3, 9].Value = c.CaseStatus;
                    row3++;
                }
                if (row3 > 2) { sheet3.Cells[row3-1, 1, row3-1, h3.Length].AutoFitColumns(); sheet3.View.FreezePanes(2, 1); }

                var fileBytes = package.GetAsByteArray();
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"UAEIEC_Consolidated_Report_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
            }
        }
    }
}

