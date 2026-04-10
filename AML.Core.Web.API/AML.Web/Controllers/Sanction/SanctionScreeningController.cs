
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
                        MatchType = apiResultModel[0].matchtype,
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
            var companyName = clientData?.ClientName ?? "WatchDog";

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
                        table.AddCell(new Phrase(results[i].matchtype ?? "", new Font(Font.FontFamily.TIMES_ROMAN, 9)));
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

    }
}
