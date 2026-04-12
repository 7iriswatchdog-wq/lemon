using AML.Core.Common.StaticResource;
using AML.Core.Service.Kyc;
using AML.Core.Service.Risk;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.UserGroup;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.ProliferationFinance;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using NToastNotify;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Fingers10.ExcelExport.ActionResults;
using AML.Web.CustomFilters;

namespace AML.Web.Controllers.ProliferationFinance
{
    public class ProliferationFinanceController : Controller
    {
        private readonly IProliferationFinanceService _proliferationFinanceService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICaseDocumentService _caseDocumentService;
        private readonly ICaseCommentService _caseCommentService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IExportDataService _exportDataService;
        private readonly ICustomerCaseService _customerCaseService;
        private readonly IHttpClientHandler _clientHandler;
        private readonly IMapper _mapper;

        private RiskAPIController _riskAPIController;
        private readonly IToastNotification _toastNotification;
        private IKycService _kycService;
        private string culture = CultureInfo.CurrentCulture.Name;
        
        private ILovMasterService _lovMasterService;
        private IUserGroupService _UserGroupService;
        private IRiskService _riskService;

        public ProliferationFinanceController(
            IProliferationFinanceService proliferationFinanceService, 
            IHttpContextAccessor httpContextAccessor,
            ICaseDocumentService caseDocumentService,
            ICaseCommentService caseCommentService,
            IViewRenderService viewRenderService,
            IExportDataService exportDataService,
            ICustomerCaseService customerCaseService,
            IHttpClientHandler clientHandler,
             IKycService kycService,
            IMapper mapper,
            IToastNotification toastNotification, RiskAPIController riskAPIController, ILovMasterService lovMasterService, IRiskService RiskService, IUserGroupService userGroupService)
        {
            _proliferationFinanceService = proliferationFinanceService;
            _httpContextAccessor = httpContextAccessor;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
            _viewRenderService = viewRenderService;
            _exportDataService = exportDataService;
            _customerCaseService = customerCaseService;
            _clientHandler = clientHandler;
            _mapper = mapper;
            _kycService = kycService;
            _toastNotification = toastNotification;
            _riskAPIController = riskAPIController;
            _lovMasterService = lovMasterService;
            _riskService = RiskService;
            _UserGroupService = userGroupService;
        }

        public IActionResult CaseCreation()
        {
            var model = new ProliferationFinanceModel();
            return View(model);
        }

        public IActionResult CaseManager()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAllCases(DateTime? startDate, DateTime? endDate, string customerType, string status)
        {
            try
            {
                var response = _proliferationFinanceService.GetAllCases(_clientHandler.GetClientId());
                if (response.Status == 200)
                {
                    var data = response.Result;

                    // Apply filters in memory for now if service doesn't support it directly
                    if (startDate.HasValue)
                        data = data.FindAll(x => x.CreatedOn >= startDate.Value);
                    
                    if (endDate.HasValue)
                        data = data.FindAll(x => x.CreatedOn <= endDate.Value.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(customerType) && customerType != "0")
                    {
                        if (customerType == "Chemical")
                        {
                            data = data.FindAll(x => 
                                (!string.IsNullOrEmpty(x.ChemicalName) && x.ChemicalName != "-" && x.ChemicalName != "N/A") ||
                                (!string.IsNullOrEmpty(x.HsCode) && x.HsCode != "-" && x.HsCode != "N/A") ||
                                (!string.IsNullOrEmpty(x.CasNumber) && x.CasNumber != "-" && x.CasNumber != "N/A") ||
                                (!string.IsNullOrEmpty(x.Eccn) && x.Eccn != "-" && x.Eccn != "N/A") ||
                                (!string.IsNullOrEmpty(x.SynonymName) && x.SynonymName != "-" && x.SynonymName != "N/A")
                            );
                        }
                        else if (customerType == "Non-Chemical")
                        {
                            data = data.FindAll(x => !string.IsNullOrEmpty(x.SearchHitDetails) && x.SearchHitDetails.Trim() != "");
                        }
                    }

                    if (!string.IsNullOrEmpty(status) && status != "0")
                        data = data.FindAll(x => x.Status == status);

                    // Filter out cases submitted to senior management for regular users
                    var GroupId = _clientHandler.GetGroupId();
                    var _UserGroupModel = _mapper.Map<AML.ViewModel.ViewModels.UserGroup.UserGroupModel>(_UserGroupService.GetDetails(GroupId));
                    if (_UserGroupModel.Name != "Senior Management")
                    {
                        data = data.FindAll(x => x.Status != "Submit to Senior Management");
                    }

                    return Json(new { success = true, data = data });
                }
                return Json(new { success = false, message = "Failed to fetch cases." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("/ProliferationFinance/ExportPFCaseReport")]
        public async Task<IActionResult> ExportPFCaseReport(DateTime? startDate, DateTime? endDate, string customerType, string status, string searchValue, bool isPDF)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();
                var response = _proliferationFinanceService.GetAllCases(clientId);
                if (response.Status != 200) return BadRequest("Failed to fetch data");

                var data = response.Result;

                // Filtering alignment with GetAllCases
                if (startDate.HasValue) data = data.FindAll(x => x.CreatedOn >= startDate.Value);
                if (endDate.HasValue) data = data.FindAll(x => x.CreatedOn <= endDate.Value.AddDays(1).AddSeconds(-1));
                // Filtering alignment with GetAllCases (Datasets)
                if (!string.IsNullOrEmpty(customerType) && customerType != "0")
                {
                    if (customerType == "Chemical")
                    {
                        data = data.FindAll(x => 
                            (!string.IsNullOrEmpty(x.ChemicalName) && x.ChemicalName != "-" && x.ChemicalName != "N/A") ||
                            (!string.IsNullOrEmpty(x.HsCode) && x.HsCode != "-" && x.HsCode != "N/A") ||
                            (!string.IsNullOrEmpty(x.CasNumber) && x.CasNumber != "-" && x.CasNumber != "N/A") ||
                            (!string.IsNullOrEmpty(x.Eccn) && x.Eccn != "-" && x.Eccn != "N/A") ||
                            (!string.IsNullOrEmpty(x.SynonymName) && x.SynonymName != "-" && x.SynonymName != "N/A")
                        );
                    }
                    else if (customerType == "Non-Chemical")
                    {
                        data = data.FindAll(x => !string.IsNullOrEmpty(x.SearchHitDetails) && x.SearchHitDetails.Trim() != "");
                    }
                }

                if (!string.IsNullOrEmpty(status) && status != "0") data = data.FindAll(x => x.Status == status);

                // Search Value filtering
                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    data = data.FindAll(x =>
                        (x.CorporateId != null && x.CorporateId.ToLower().Contains(searchValue)) ||
                        (x.CompanyName != null && x.CompanyName.ToLower().Contains(searchValue)) ||
                        (x.ChemicalName != null && x.ChemicalName.ToLower().Contains(searchValue)) ||
                        (x.HsCode != null && x.HsCode.ToLower().Contains(searchValue)) ||
                        (x.CasNumber != null && x.CasNumber.ToLower().Contains(searchValue)) ||
                        (x.Id.ToString().Contains(searchValue))
                    );
                }

                // Service Group Filter
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<AML.ViewModel.ViewModels.UserGroup.UserGroupModel>(_UserGroupService.GetDetails(GroupId));
                if (_UserGroupModel.Name != "Senior Management")
                {
                    data = data.FindAll(x => x.Status != "Submit to Senior Management");
                }

                if (!isPDF)
                {
                    var excelData = data.Select(x => new PFReportExcelModel
                    {
                        CaseId = "PF-" + x.Id,
                        CreatedDate = x.CreatedOn.ToString("dd/MM/yyyy HH:mm:ss"),
                        CorporateId = x.CorporateId,
                        CompanyName = x.CompanyName,
                        CustomerType = x.CustomerType,
                        ChemicalName = x.ChemicalName,
                        HsCode = x.HsCode,
                        CasNumber = x.CasNumber,
                        Status = x.Status,
                        Remarks = x.StatusReason
                    }).ToList();

                    string typeLabel = (customerType == "0" || string.IsNullOrEmpty(customerType)) ? "All" : customerType;
                    string statusLabel = (status == "0" || string.IsNullOrEmpty(status)) ? "All" : status;
                    string filterStr = $"Dataset: {typeLabel}, Status: {statusLabel}";
                    if (!string.IsNullOrEmpty(searchValue)) filterStr += $", Search: {searchValue}";

                    var headerModel = new PFReportExcelModel
                    {
                        Details = $"Report               :   Proliferation Finance Case Report\r\nDate Range       :   {startDate?.ToString("dd/MM/yyyy") ?? "All"} to {endDate?.ToString("dd/MM/yyyy") ?? "All"}\r\nFilters Applied  :   {filterStr}"
                    };
                    excelData.Insert(0, headerModel);

                    return new ExcelResult<PFReportExcelModel>(excelData, "Proliferation Finance Report", "PF_Case_Report_" + DateTime.Now.Ticks);
                }

                // PDF Export using iTextSharp
                var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
                var logos = "wwwroot/img/" + clientDetails.DocumentFileName;
                var clientName = clientDetails.ClientName;

                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 15, 15, 15, 15);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // Logo & Header
                    PdfPTable logoTable = new PdfPTable(2);
                    logoTable.WidthPercentage = 100;
                    logoTable.SetWidths(new float[] { 3f, 1f });
                    
                    PdfPCell nameCell = new PdfPCell(new Phrase(clientName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 18, iTextSharp.text.Font.BOLD)));
                    nameCell.Border = 0;
                    nameCell.VerticalAlignment = Element.ALIGN_MIDDLE;
                    logoTable.AddCell(nameCell);

                    if (System.IO.File.Exists(logos))
                    {
                        iTextSharp.text.Image logoImg = iTextSharp.text.Image.GetInstance(logos);
                        logoImg.ScaleToFit(100, 50);
                        PdfPCell imgCell = new PdfPCell(logoImg);
                        imgCell.Border = 0;
                        imgCell.HorizontalAlignment = Element.ALIGN_RIGHT;
                        logoTable.AddCell(imgCell);
                    }
                    else logoTable.AddCell("");

                    document.Add(logoTable);
                    document.Add(new Paragraph("\n"));

                    // Report Metadata
                    string typeLabel = (customerType == "0" || string.IsNullOrEmpty(customerType)) ? "All" : customerType;
                    string statusLabel = (status == "0" || string.IsNullOrEmpty(status)) ? "All" : status;
                    string filterStr = $"Dataset: {typeLabel}, Status: {statusLabel}";
                    if (!string.IsNullOrEmpty(searchValue)) filterStr += $", Search: {searchValue}";

                    document.Add(new Phrase("Report               :   Proliferation Finance Case Report\n", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11)));
                    document.Add(new Phrase($"Date Range       :   {startDate?.ToString("dd/MM/yyyy") ?? "All"} to {endDate?.ToString("dd/MM/yyyy") ?? "All"}\n", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11)));
                    document.Add(new Phrase($"Filters Applied  :   {filterStr}\n", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 11)));
                    document.Add(new Paragraph("\n"));

                    // Data Table
                    PdfPTable table = new PdfPTable(8);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 0.4f, 0.8f, 1.2f, 1f, 1.8f, 1.8f, 0.6f, 0.8f });

                    string[] headers = { "#", "CASE ID", "CREATED DATE", "CORPORATE ID", "COMPANY NAME", "PRODUCT / CHEMICAL", "SCORE", "STATUS" };
                    foreach (var h in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(h, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 10, iTextSharp.text.Font.BOLD)));
                        cell.BackgroundColor = BaseColor.LIGHT_GRAY;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.Padding = 5;
                        table.AddCell(cell);
                    }

                    int count = 1;
                    foreach (var item in data)
                    {
                        table.AddCell(new Phrase(count++.ToString(), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase("PF-" + item.Id, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(item.CreatedOn.ToString("dd/MM/yyyy HH:mm"), new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(item.CorporateId, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(item.CompanyName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(item.ChemicalName, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(item.Score ?? "0", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                        table.AddCell(new Phrase(item.Status, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                    }

                    document.Add(table);

                    // Footer
                    document.Add(new Paragraph("\n"));
                    document.Add(new iTextSharp.text.pdf.draw.LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_LEFT, 1));
                    document.Add(new Phrase("Computer generated report; hence no signature is required.\n", new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));
                    document.Add(new Phrase("Date of Extraction  :   " + DateTime.Now, new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.TIMES_ROMAN, 9)));

                    document.Close();
                    return File(ms.ToArray(), "application/pdf", $"PF_Case_Report_{DateTime.Now:yyyyMMdd}.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating report: " + ex.Message);
            }
        }

        [HttpGet("/ProliferationFinance/CaseManager_PDF")]
        public IActionResult CaseManager_PDF(DateTime? startDate, DateTime? endDate, string customerType, string status, string searchValue)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();
                var response = _proliferationFinanceService.GetAllCases(clientId);
                if (response.Status != 200) return BadRequest("Failed to fetch data");

                var data = response.Result;

                // Standardized filtering
                if (startDate.HasValue) data = data.FindAll(x => x.CreatedOn >= startDate.Value);
                if (endDate.HasValue) data = data.FindAll(x => x.CreatedOn <= endDate.Value.AddDays(1).AddSeconds(-1));
                
                if (!string.IsNullOrEmpty(customerType) && customerType != "0")
                {
                    if (customerType == "Chemical")
                    {
                        data = data.FindAll(x => 
                            (!string.IsNullOrEmpty(x.ChemicalName) && x.ChemicalName != "-" && x.ChemicalName != "N/A") ||
                            (!string.IsNullOrEmpty(x.HsCode) && x.HsCode != "-" && x.HsCode != "N/A") ||
                            (!string.IsNullOrEmpty(x.CasNumber) && x.CasNumber != "-" && x.CasNumber != "N/A") ||
                            (!string.IsNullOrEmpty(x.Eccn) && x.Eccn != "-" && x.Eccn != "N/A") ||
                            (!string.IsNullOrEmpty(x.SynonymName) && x.SynonymName != "-" && x.SynonymName != "N/A")
                        );
                    }
                    else if (customerType == "Non-Chemical")
                    {
                        data = data.FindAll(x => !string.IsNullOrEmpty(x.SearchHitDetails) && x.SearchHitDetails.Trim() != "");
                    }
                }

                if (!string.IsNullOrEmpty(status) && status != "0") data = data.FindAll(x => x.Status == status);

                if (!string.IsNullOrEmpty(searchValue))
                {
                    searchValue = searchValue.ToLower();
                    data = data.FindAll(x =>
                        (x.CorporateId != null && x.CorporateId.ToLower().Contains(searchValue)) ||
                        (x.CompanyName != null && x.CompanyName.ToLower().Contains(searchValue)) ||
                        (x.ChemicalName != null && x.ChemicalName.ToLower().Contains(searchValue)) ||
                        (x.HsCode != null && x.HsCode.ToLower().Contains(searchValue)) ||
                        (x.CasNumber != null && x.CasNumber.ToLower().Contains(searchValue)) ||
                        (x.Id.ToString().Contains(searchValue))
                    );
                }

                // Service Group Filter
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<AML.ViewModel.ViewModels.UserGroup.UserGroupModel>(_UserGroupService.GetDetails(GroupId));
                if (_UserGroupModel.Name != "Senior Management")
                {
                    data = data.FindAll(x => x.Status != "Submit to Senior Management");
                }

                ViewBag.ClientDetails = _customerCaseService.GetClientDetailsByID(clientId);
                ViewBag.StartDate = startDate;
                ViewBag.EndDate = endDate;
                ViewBag.CustomerType = customerType;
                ViewBag.Status = status;

                return View("CaseManager_PDF", data);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating report: " + ex.Message);
            }
        }


        [HttpPost]
        public IActionResult SearchChemicalsOnly([FromBody] ProliferationFinanceModel model)
        {
            try
            {
                if (model.CustomerType == "Chemical")
                {
                    var searchDto = new ProliferationFinanceCaseDTO
                    {
                        HsCode = model.HsCode,
                        CasNumber = model.CasNumber,
                        Eccn = model.Eccn,
                        ChemicalName = model.ChemicalName,
                        SynonymName = model.SynonymName
                    };
                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                    if (searchResponse.Status == 200)
                    {
                        return Json(new { success = true, results = searchResponse.Result });
                    }
                    return Json(new { success = false, message = searchResponse.Message });
                }
                else
                {
                    // For non-chemical, search in PDF
                    var pdfResponse = _proliferationFinanceService.SearchNonChemical(model.SearchKeyword ?? model.ChemicalName);
                    if (pdfResponse.Status == 200 && !string.IsNullOrEmpty(pdfResponse.Result))
                    {
                        // Wrap the single paragraph result in a list as expected by the frontend
                        return Json(new { success = true, results = new List<string> { pdfResponse.Result } });
                    }
                    return Json(new { success = true, results = new List<string>() });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult SearchChemicals([FromBody] ProliferationFinanceModel model)
        {
            try
            {
                int userId = Convert.ToInt32(_httpContextAccessor.HttpContext.Session.GetString("SessUserId"));
                
                var searchDto = new ProliferationFinanceCaseDTO
                {
                    CustomerType = "Goods", // Consolidated Type
                    ClientId = _clientHandler.GetClientId(),
                    CorporateId = string.IsNullOrWhiteSpace(model.CorporateId) ? null : model.CorporateId,
                    CompanyName = string.IsNullOrWhiteSpace(model.CompanyName) ? null : model.CompanyName,
                    HsCode = string.IsNullOrWhiteSpace(model.HsCode) ? null : model.HsCode,
                    CasNumber = string.IsNullOrWhiteSpace(model.CasNumber) ? null : model.CasNumber,
                    Eccn = string.IsNullOrWhiteSpace(model.Eccn) ? null : model.Eccn,
                    ChemicalName = string.IsNullOrWhiteSpace(model.ChemicalName) ? null : model.ChemicalName,
                    SynonymName = string.IsNullOrWhiteSpace(model.SynonymName) ? null : model.SynonymName,
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now,
                    Type = "Corporate"
                };

                // 1. Determine Search Context
                bool hasExactFields = !string.IsNullOrWhiteSpace(model.HsCode) || !string.IsNullOrWhiteSpace(model.CasNumber) || !string.IsNullOrWhiteSpace(model.Eccn);
                bool hasSynonym = !string.IsNullOrWhiteSpace(model.SynonymName);
                bool hasChemicalName = !string.IsNullOrWhiteSpace(model.ChemicalName);

                bool dbMatch = false;
                bool pdfMatch = false;
                List<string> matchSources = new List<string>();

                // 2. Database Search (triggered by any field)
                if (hasExactFields || hasSynonym || hasChemicalName)
                {
                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                    if (searchResponse.Status == 200 && searchResponse.Result != null && searchResponse.Result.Count > 0)
                    {
                        dbMatch = true;
                        var match = searchResponse.Result[0];
                        // Only set the flag and source; do NOT store the matched name in the case record to preserve user-input integrity
                        
                        matchSources.Add("UAE Control List");
                    }
                }

                // 3. PDF Scan (triggered ONLY by Product Name)
                if (hasChemicalName)
                {
                    var pdfResponse = _proliferationFinanceService.SearchNonChemical(model.ChemicalName);
                    if (!string.IsNullOrEmpty(pdfResponse.Result))
                    {
                        pdfMatch = true;
                        searchDto.SearchHitDetails = pdfResponse.Result; // Store matched paragraphs
                        matchSources.Add("Document Scan");
                    }
                }

                // 4. Validation: Prevent Case Creation if NO match found
                if (!dbMatch && !pdfMatch)
                {
                    return Json(new { 
                        success = false, 
                        isNoMatch = true,
                        message = "No matches found in UAE Control List or Document Scan. Case will not be created." 
                    });
                }

                // 5. Update Case Metadata based on results (Remove system-generated match comments)
                searchDto.Score = "100";
                searchDto.StatusReason = ""; // Kept empty as per user request

                // 6. Direct Case Creation
                var createResponse = _proliferationFinanceService.CreateCase(searchDto);
                if (createResponse.Status == 200 && createResponse.Result > 0)
                {
                    int caseId = createResponse.Result;

                    int Id = _customerCaseService.GetCaseId(model.CorporateId);
                    CaseCommentModel remarkModel = new CaseCommentModel();
                    remarkModel.CaseId = Convert.ToInt32(Id);
                    remarkModel.Comment = "PF Case is Created"; // ? FIXED
                    remarkModel.CommentType = "Proliferation Case Creation";
                    remarkModel.CreatedBy = _clientHandler.GetUserId();

                    var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));

                    // 7. Sync Initial Search Results to MongoDB
                    try
                    {
                        var chemMatches = _proliferationFinanceService.SearchChemicals(searchDto).Result;
                        var pdfMatches = _proliferationFinanceService.SearchNonChemical(model.ChemicalName).Result;
                        _proliferationFinanceService.SyncSearchResultsToMongo(caseId, model.ChemicalName, chemMatches, pdfMatches);
                    }
                    catch (Exception) { /* Log error but don't fail case creation */ }

                    return Json(new { 
                        success = true, 
                        caseId = caseId, 
                        caseRefId = "PF-" + caseId,
                        score = searchDto.Score,
                        message = "Case created successfully." 
                    });
                }

                return Json(new { success = false, message = createResponse.Message ?? "Failed to create case." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet("/ProliferationFinance/Process/{id}")]
        public IActionResult Process(int id)
        {
            try
            {
                var caseDetails = _proliferationFinanceService.GetCaseById(id);
                if (caseDetails == null) return RedirectToAction("CaseManager");

                var model = new ProliferationFinanceModel
                {
                    CaseId = caseDetails.Id,
                    CustomerType = caseDetails.CustomerType,
                    CorporateId = caseDetails.CorporateId,
                    CompanyName = caseDetails.CompanyName,
                    HsCode = caseDetails.HsCode,
                    CasNumber = caseDetails.CasNumber,
                    Eccn = caseDetails.Eccn,
                    ChemicalName = caseDetails.ChemicalName,
                    SynonymName = caseDetails.SynonymName,
                    Status = caseDetails.Status,
                    CreatedOn = caseDetails.CreatedOn,
                    UpdatedOn = caseDetails.UpdatedOn,
                    Score = caseDetails.Score ?? "0",
                    StatusReason = caseDetails.StatusReason,
                    MatchedChemicalName = caseDetails.MatchedChemicalName,
                    SearchHitDetails = caseDetails.SearchHitDetails
                };

                // For Chemical/Goods cases, re-fetch ALL potential hits from UAE Control List
                if (model.CustomerType == "Goods" || model.CustomerType == "Chemical")
                {
                    var searchDto = new AML.DTO.DTO.ProliferationFinance.ProliferationFinanceCaseDTO
                    {
                        ClientId = _clientHandler.GetClientId(),
                        ChemicalName = model.ChemicalName,
                        HsCode = model.HsCode,
                        CasNumber = model.CasNumber,
                        Eccn = model.Eccn,
                        SynonymName = model.SynonymName
                    };

                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                    if (searchResponse.Status == 200 && searchResponse.Result != null)
                    {
                        // Deduplicate chemical matches by composite key
                        model.MatchedChemicals = searchResponse.Result
                            .GroupBy(c => new { 
                                Name = (c.ChemicalName ?? "").Trim().ToLower(), 
                                Hs = (c.HsCode ?? "").Trim().ToLower(), 
                                Cas = (c.CasNumber ?? "").Trim().ToLower(), 
                                Ec = (c.Eccn ?? "").Trim().ToLower() 
                            })
                            .Select(g => g.First())
                            .ToList();
                    }
                }

                // Deduplicate PDF snippets
                if (!string.IsNullOrEmpty(model.SearchHitDetails))
                {
                    var snippets = model.SearchHitDetails.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var uniqueSnippets = snippets
                        .Select(s => s.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    model.SearchHitDetails = string.Join("\n\n", uniqueSnippets);
                }

                // 1. Sync fresh results to MongoDB (Detection of new matches)
                _proliferationFinanceService.SyncSearchResultsToMongo(id, model.ChemicalName, model.MatchedChemicals, model.SearchHitDetails);

                // 2. Load from MongoDB to get any previously saved decisions/remarks
                var mongoData = _proliferationFinanceService.GetMongoSearchResults(id);
                if (mongoData != null)
                {
                    model.MongoHits = mongoData.Hits;
                }

                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<AML.ViewModel.ViewModels.UserGroup.UserGroupModel>(_UserGroupService.GetDetails(GroupId));
                model.UserGroupName = _UserGroupModel.Name;

                model.Comments = _caseCommentService.GetAllByCase(id);

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("CaseManager");
            }
        }

        [HttpGet("/ProliferationFinance/Process_PDF/{id}")]
        public IActionResult Process_PDF(int id)
        {
            try
            {
                var caseDetails = _proliferationFinanceService.GetCaseById(id);
                if (caseDetails == null) return RedirectToAction("CaseManager");

                var model = new ProliferationFinanceModel
                {
                    CaseId = caseDetails.Id,
                    CustomerType = caseDetails.CustomerType,
                    CorporateId = caseDetails.CorporateId,
                    CompanyName = caseDetails.CompanyName,
                    HsCode = caseDetails.HsCode,
                    CasNumber = caseDetails.CasNumber,
                    Eccn = caseDetails.Eccn,
                    ChemicalName = caseDetails.ChemicalName,
                    SynonymName = caseDetails.SynonymName,
                    Status = caseDetails.Status,
                    CreatedOn = caseDetails.CreatedOn,
                    UpdatedOn = caseDetails.UpdatedOn,
                    Score = caseDetails.Score ?? "0",
                    StatusReason = caseDetails.StatusReason,
                    MatchedChemicalName = caseDetails.MatchedChemicalName,
                    SearchHitDetails = caseDetails.SearchHitDetails
                };

                // For Chemical/Goods cases, re-fetch ALL potential hits from UAE Control List
                if (model.CustomerType == "Goods" || model.CustomerType == "Chemical")
                {
                    var searchDto = new AML.DTO.DTO.ProliferationFinance.ProliferationFinanceCaseDTO
                    {
                        ClientId = _clientHandler.GetClientId(),
                        ChemicalName = model.ChemicalName,
                        HsCode = model.HsCode,
                        CasNumber = model.CasNumber,
                        Eccn = model.Eccn,
                        SynonymName = model.SynonymName
                    };

                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                    if (searchResponse.Status == 200 && searchResponse.Result != null)
                    {
                        // Deduplicate chemical matches by composite key
                        model.MatchedChemicals = searchResponse.Result
                            .GroupBy(c => new { 
                                Name = (c.ChemicalName ?? "").Trim().ToLower(), 
                                Hs = (c.HsCode ?? "").Trim().ToLower(), 
                                Cas = (c.CasNumber ?? "").Trim().ToLower(), 
                                Ec = (c.Eccn ?? "").Trim().ToLower() 
                            })
                            .Select(g => g.First())
                            .ToList();
                    }
                }

                // Deduplicate PDF snippets
                if (!string.IsNullOrEmpty(model.SearchHitDetails))
                {
                    var snippets = model.SearchHitDetails.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var uniqueSnippets = snippets
                        .Select(s => s.Trim())
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .ToArray();
                    model.SearchHitDetails = string.Join("\n\n", uniqueSnippets);
                }

                // Load from MongoDB to get any previously saved decisions/remarks
                var mongoData = _proliferationFinanceService.GetMongoSearchResults(id);
                if (mongoData != null)
                {
                    model.MongoHits = mongoData.Hits;
                }

                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<AML.ViewModel.ViewModels.UserGroup.UserGroupModel>(_UserGroupService.GetDetails(GroupId));
                model.UserGroupName = _UserGroupModel.Name;

                model.Comments = _caseCommentService.GetAllByCase(id);

                return View("Process_PDF", model);
            }
            catch (Exception)
            {
                return RedirectToAction("CaseManager");
            }
        }

        [HttpGet("/ProliferationFinance/DownloadProcessPDF/{id}")]
        public async Task<IActionResult> DownloadProcessPDF(int id)
        {
            try
            {
                var caseDetails = _proliferationFinanceService.GetCaseById(id);
                if (caseDetails == null) return NotFound();

                var model = new ProliferationFinanceModel
                {
                    CaseId = caseDetails.Id,
                    CustomerType = caseDetails.CustomerType,
                    CorporateId = caseDetails.CorporateId,
                    CompanyName = caseDetails.CompanyName,
                    HsCode = caseDetails.HsCode,
                    CasNumber = caseDetails.CasNumber,
                    Eccn = caseDetails.Eccn,
                    ChemicalName = caseDetails.ChemicalName,
                    SynonymName = caseDetails.SynonymName,
                    Status = caseDetails.Status,
                    CreatedOn = caseDetails.CreatedOn,
                    UpdatedOn = caseDetails.UpdatedOn,
                    Score = caseDetails.Score ?? "0",
                    StatusReason = caseDetails.StatusReason,
                    MatchedChemicalName = caseDetails.MatchedChemicalName,
                    SearchHitDetails = caseDetails.SearchHitDetails
                };

                if (model.CustomerType == "Goods" || model.CustomerType == "Chemical")
                {
                    var searchDto = new AML.DTO.DTO.ProliferationFinance.ProliferationFinanceCaseDTO
                    {
                        ChemicalName = model.ChemicalName,
                        HsCode = model.HsCode,
                        CasNumber = model.CasNumber,
                        Eccn = model.Eccn,
                        SynonymName = model.SynonymName
                    };

                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                    if (searchResponse.Status == 200 && searchResponse.Result != null)
                    {
                        model.MatchedChemicals = searchResponse.Result
                            .GroupBy(c => new { 
                                Name = (c.ChemicalName ?? "").Trim().ToLower(), 
                                Hs = (c.HsCode ?? "").Trim().ToLower(), 
                                Cas = (c.CasNumber ?? "").Trim().ToLower(), 
                                Ec = (c.Eccn ?? "").Trim().ToLower() 
                            })
                            .Select(g => g.First())
                            .ToList();
                    }
                }

                if (!string.IsNullOrEmpty(model.SearchHitDetails))
                {
                    var snippets = model.SearchHitDetails.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries);
                    var uniqueSnippets = snippets.Select(s => s.Trim()).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
                    model.SearchHitDetails = string.Join("\n\n", uniqueSnippets);
                }

                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<AML.ViewModel.ViewModels.UserGroup.UserGroupModel>(_UserGroupService.GetDetails(GroupId));
                model.UserGroupName = _UserGroupModel.Name;

                model.Comments = _caseCommentService.GetAllByCase(id);

                string html = await _viewRenderService.RenderToStringAsync("ProliferationFinance/Process_PDF", model);
                var pdfBytes = _exportDataService.HtmlToPDFforChecklistLogs(html);
                string fileName = $"PF_ProcessReport_{model.CorporateId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating PDF: " + ex.Message);
            }
        }


                [HttpPost]
        public IActionResult UpdateStatus(int caseId, string status)
        {
            try
            {
                var response = _proliferationFinanceService.UpdateCaseStatus(caseId, status);
                
                var userIdStr = _httpContextAccessor.HttpContext.Session.GetString("SessUserId");
                int.TryParse(userIdStr, out int userId);
                var commentDto = new AML.DTO.DTO.CaseComment.CaseCommentDTO
                {
                    CaseId = caseId.ToString(),
                    Comment = "Case Status has been updated to " + status,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy HH:mm:ss"),
                    CommentType = status
                };
                _caseCommentService.CreateProliferationCaseComments(commentDto);

                return Json(new { success = response.Status == 200, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UpdateRemarks(int caseId, string remarks)
        {
            try
            {
                var userId = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("SessUserId") ?? "0");
                
                // 1. Update the main status reason for quick reference
                var response = _proliferationFinanceService.UpdateCaseRemarks(caseId, remarks);

                // 2. Add to Case Comment history table
                var commentDto = new CaseCommentDTO
                {
                    CaseId = caseId.ToString(),
                    Comment = remarks,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy"),
                    CommentType = "Comments"

                };
                _caseCommentService.CreateProliferationCaseComments(commentDto);
                
                return Json(new { success = response.Status == 200, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetDetails(int caseId)
        {
            try
            {
                var caseDetails = _proliferationFinanceService.GetCaseById(caseId);
                return Json(new { success = caseDetails != null, data = caseDetails });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public IActionResult UploadDocument(int caseId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return Json(new { success = false, message = "No file uploaded." });

                var userId = int.Parse(_httpContextAccessor.HttpContext.Session.GetString("SessUserId") ?? "0");
                
                // Simplified upload logic - saving to a temporary path or using CaseDocumentService
                // Assuming CaseDocumentService has a standard way to handle uploads
                var documentDto = new CaseDocumentDTO
                {
                    CaseId = caseId.ToString(),
                    DocumentName = file.FileName,
                    DocumentFileName = file.FileName,
                    CreatedBy = userId,
                    CreatedOn = DateTime.UtcNow.AddHours(4).ToString("dd/MM/yyyy")
                };

                // In a real scenario, we'd save the file to disk here
                var result = _caseDocumentService.Create(documentDto);
                
                return Json(new { success = result.Status == 200, message = result.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("/ProliferationFinance/Details/{id}")]
        public IActionResult Details(int id, int? chemicalId = null)
        {
            try
            {
                var caseDetails = _proliferationFinanceService.GetCaseById(id);
                if (caseDetails == null) return RedirectToAction("CaseManager");

                var model = new ProliferationFinanceModel
                {
                    CaseId = caseDetails.Id,
                    CustomerType = caseDetails.CustomerType,
                    CorporateId = caseDetails.CorporateId,
                    CompanyName = caseDetails.CompanyName,
                    HsCode = caseDetails.HsCode,
                    CasNumber = caseDetails.CasNumber,
                    Eccn = caseDetails.Eccn,
                    ChemicalName = caseDetails.ChemicalName,
                    SynonymName = caseDetails.SynonymName,
                    Status = caseDetails.Status,
                    CreatedOn = caseDetails.CreatedOn,
                    UpdatedOn = caseDetails.UpdatedOn,
                    Score = caseDetails.Score ?? "0",
                    StatusReason = caseDetails.StatusReason,
                    MatchedChemicalName = caseDetails.MatchedChemicalName,
                    SearchHitDetails = caseDetails.SearchHitDetails
                };

                // If a specific chemical was selected from search results, override the case default fields
                if (chemicalId.HasValue && chemicalId.Value > 0)
                {
                    var chem = _proliferationFinanceService.GetChemicalById(chemicalId.Value);
                    if (chem != null)
                    {
                        model.MatchedChemicalName = chem.ChemicalName;
                        model.HsCode = chem.HsCode;
                        model.CasNumber = chem.CasNumber;
                        model.Eccn = chem.Eccn;
                        model.SynonymName = chem.SynonymName;
                    }
                }
                else if (model.CustomerType == "Goods" || model.CustomerType == "Chemical")
                {
                    // Re-fetch ALL potential hits to display in a list if requested
                    var searchDto = new AML.DTO.DTO.ProliferationFinance.ProliferationFinanceCaseDTO
                    {
                        ClientId = _clientHandler.GetClientId(),
                        ChemicalName = model.ChemicalName,
                        HsCode = model.HsCode,
                        CasNumber = model.CasNumber,
                        Eccn = model.Eccn,
                        SynonymName = model.SynonymName
                    };

                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                    if (searchResponse.Status == 200 && searchResponse.Result != null)
                    {
                        model.MatchedChemicals = searchResponse.Result
                            .GroupBy(c => new { 
                                Name = (c.ChemicalName ?? "").Trim().ToLower(), 
                                Hs = (c.HsCode ?? "").Trim().ToLower(), 
                                Cas = (c.CasNumber ?? "").Trim().ToLower(), 
                                Ec = (c.Eccn ?? "").Trim().ToLower() 
                            })
                            .Select(g => g.First())
                            .ToList();
                    }
                }

                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("CaseManager");
            }
        }

        [HttpGet("/ProliferationFinance/DownloadDetails/{id}")]
        public async Task<IActionResult> DownloadDetails(int id)
        {
            try
            {
                var caseDetails = _proliferationFinanceService.GetCaseById(id);
                if (caseDetails == null) return NotFound();

                var model = new ProliferationFinanceModel
                {
                    CaseId = caseDetails.Id,
                    CustomerType = caseDetails.CustomerType,
                    CorporateId = caseDetails.CorporateId,
                    CompanyName = caseDetails.CompanyName,
                    HsCode = caseDetails.HsCode,
                    CasNumber = caseDetails.CasNumber,
                    Eccn = caseDetails.Eccn,
                    ChemicalName = caseDetails.ChemicalName,
                    SynonymName = caseDetails.SynonymName,
                    Status = caseDetails.Status,
                    CreatedOn = caseDetails.CreatedOn,
                    UpdatedOn = caseDetails.UpdatedOn,
                    Score = caseDetails.Score ?? "0",
                    StatusReason = caseDetails.StatusReason,
                    MatchedChemicalName = caseDetails.MatchedChemicalName,
                    SearchHitDetails = caseDetails.SearchHitDetails
                };

                string html = await _viewRenderService.RenderToStringAsync("ProliferationFinance/_DetailsReport", model);
                var pdfBytes = _exportDataService.HtmlToPDFforChecklistLogs(html);
                
                string fileName = $"PF_MatchReport_{model.CorporateId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating PDF: " + ex.Message);
            }
        }

        [HttpPost("/ProliferationFinance/DownloadGoodsSearchPDF")]
        public async Task<IActionResult> DownloadGoodsSearchPDF([FromForm] ProliferationFinanceModel model)
        {
            try
            {
                // Assign sensible defaults for non-DB cases
                model.CreatedOn = DateTime.Now;
                if (string.IsNullOrEmpty(model.CustomerType)) model.CustomerType = "Chemical";
                
                string html = await _viewRenderService.RenderToStringAsync("ProliferationFinance/GoodsSeach_PDF", model);
                var pdfBytes = _exportDataService.HtmlToPDFforChecklistLogs(html);

                string fileName = $"GoodsSearch_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating PDF: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult GetComments(int caseId)
        {
            try
            {
                var response = _caseCommentService.GetAllProliferationByCase(caseId);
                return Json(new { success = response != null, data = response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> BulkUploadCases(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, message = "Please select an Excel file" });

            var summary = new BulkUploadSummary();
            var userIdStr = HttpContext.Session.GetString("SessUserId") ?? "0";
            int userId = int.TryParse(userIdStr, out var u) ? u : 0;

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        if (package.Workbook.Worksheets.Count == 0)
                            return Json(new { success = false, message = "The Excel file contains no worksheets." });

                        var worksheet = package.Workbook.Worksheets[1]; // EPPlus is 1-indexed
                        if (worksheet.Dimension == null)
                             return Json(new { success = false, message = "Spreadsheet is empty" });

                        var rowCount = worksheet.Dimension.Rows;
                        summary.Total = rowCount - 1; 

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var customerId = worksheet.Cells[row, 1].Value?.ToString();
                                var companyName = worksheet.Cells[row, 2].Value?.ToString();

                                // Fetch company name if missing but customerId is present
                                if (string.IsNullOrEmpty(companyName) && !string.IsNullOrEmpty(customerId))
                                {
                                    try
                                    {
                                        var clientId = _clientHandler.GetClientId();
                                        var companyData = _customerCaseService.GetCompanyCode(customerId, clientId, "C");
                                        if (companyData != null && companyData.Count > 0)
                                        {
                                            companyName = companyData[0].FirstName ?? companyData[0].CompanyName;
                                        }
                                    }
                                    catch (Exception) { /* Fallback to null if service call fails */ }
                                }
                                
                                // Chemical fields
                                var hsCode = worksheet.Cells[row, 3].Value?.ToString();
                                var casNumber = worksheet.Cells[row, 4].Value?.ToString();
                                var eccn = worksheet.Cells[row, 5].Value?.ToString();
                                var chemicalName = worksheet.Cells[row, 6].Value?.ToString();
                                var synonymName = worksheet.Cells[row, 7].Value?.ToString();
                                
                                // Non-Chemical field
                                var searchKeyword = worksheet.Cells[row, 8].Value?.ToString();
                                var remarks = worksheet.Cells[row, 9].Value?.ToString();

                                if (string.IsNullOrEmpty(customerId)) continue;

                                bool hasChemicalFields = !string.IsNullOrEmpty(hsCode) || !string.IsNullOrEmpty(casNumber) || 
                                                       !string.IsNullOrEmpty(eccn) || !string.IsNullOrEmpty(chemicalName) || 
                                                       !string.IsNullOrEmpty(synonymName);
                                
                                bool hasNonChemicalFields = !string.IsNullOrEmpty(searchKeyword) || !string.IsNullOrEmpty(chemicalName);

                                if (hasChemicalFields || hasNonChemicalFields)
                                {
                                    var caseDto = new ProliferationFinanceCaseDTO
                                    {
                                        CustomerType = "Goods", // Consolidated type
                                        ClientId = _clientHandler.GetClientId(),
                                        CorporateId = customerId,
                                        CompanyName = companyName,
                                        HsCode = hsCode,
                                        CasNumber = casNumber,
                                        Eccn = eccn,
                                        ChemicalName = chemicalName,
                                        SynonymName = synonymName,
                                        CreatedBy = userId,
                                        CreatedOn = DateTime.Now,
                                        Type = "Corporate"
                                    };

                                    bool dbMatch = false;
                                    bool pdfMatch = false;
                                    List<string> matchSources = new List<string>();

                                    // 1. Database Search
                                    if (hasChemicalFields)
                                    {
                                        var dbResponse = _proliferationFinanceService.SearchChemicals(caseDto);
                                        if (dbResponse.Status == 200 && dbResponse.Result != null && dbResponse.Result.Count > 0)
                                        {
                                            dbMatch = true;
                                            matchSources.Add("UAE Control List");
                                            var match = dbResponse.Result[0];
                                            
                                            // Only set the source; do NOT store the matched name in the case record
                                        }
                                    }

                                    // 2. Document Scan
                                    if (hasNonChemicalFields)
                                    {
                                        string keyword = !string.IsNullOrEmpty(searchKeyword) ? searchKeyword : chemicalName;
                                        var pdfResponse = _proliferationFinanceService.SearchNonChemical(keyword);
                                        if (pdfResponse.Status == 200 && !string.IsNullOrEmpty(pdfResponse.Result))
                                        {
                                            pdfMatch = true;
                                            caseDto.SearchHitDetails = pdfResponse.Result;
                                            matchSources.Add("Document Scan");
                                            if (string.IsNullOrEmpty(caseDto.ChemicalName)) caseDto.ChemicalName = keyword;
                                        }
                                    }

                                    // 3. Conditional Case Creation (Only on match)
                                    if (dbMatch || pdfMatch)
                                    {
                                        summary.Matches++;
                                        caseDto.Score = "100";
                                        caseDto.StatusReason = !string.IsNullOrEmpty(remarks) ? remarks : ""; // Only keep user remarks

                                        var createResponse = _proliferationFinanceService.CreateCase(caseDto);
                                        if (createResponse.Status == 200)
                                        {
                                            summary.Success++;
                                            summary.Results.Add(new BulkRowResult
                                            {
                                                Row = row,
                                                CorporateId = customerId,
                                                CaseId = createResponse.Result,
                                                Message = "Case Created (Match Found)"
                                            });
                                        }
                                        else summary.Failed++;
                                    }
                                    else
                                    {
                                        summary.Results.Add(new BulkRowResult
                                        {
                                            Row = row,
                                            CorporateId = customerId,
                                            Message = "No Match - No Case Created"
                                        });
                                    }
                                }
                                else
                                {
                                    summary.Failed++;
                                    summary.Errors.Add(new BulkError { Row = row, Message = "Missing searchable data (HS Code, Chemical Name, etc.)" });
                                }
                            }
                            catch (Exception ex)
                            {
                                summary.Failed++;
                                summary.Errors.Add(new BulkError { Row = row, Message = ex.Message });
                            }
                        }
                    }
                }
                return Json(new { success = true, summary = summary });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error processing file: " + ex.Message });
            }
        }

        [HttpGet("/ProliferationFinance/DownloadBulkTemplate")]
        public IActionResult DownloadBulkTemplate()
        {
            using (var package = new ExcelPackage())
            {
                var xl = package.Workbook.Worksheets.Add("Bulk Upload Template");
                xl.Cells[1, 1].Value = "Customer ID";
                xl.Cells[1, 2].Value = "Company Name (Optional)";
                xl.Cells[1, 3].Value = "HS Code";
                xl.Cells[1, 4].Value = "CAS Number";
                xl.Cells[1, 5].Value = "ECCN";
                xl.Cells[1, 6].Value = "Chemical Name";
                xl.Cells[1, 7].Value = "Synonym Name";
                xl.Cells[1, 8].Value = "Search Keyword (Non-Chemical)";
                xl.Cells[1, 9].Value = "Remarks";

                using (var range = xl.Cells[1, 1, 1, 9])
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(219, 234, 254));
                    range.Style.Font.Bold = true;
                }

                xl.Column(1).Width = 15;
                xl.Column(2).Width = 25;
                xl.Column(3).Width = 15;
                xl.Column(6).Width = 25;
                xl.Column(8).Width = 35;
                xl.Column(9).Width = 20;

                var fileContents = package.GetAsByteArray();
                return File(fileContents, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ProliferationFinance_BulkUpload_Template.xlsx");
            }
        }

        [HttpPost("/ProliferationFinance/SaveSearchHits")]
        public IActionResult SaveSearchHits([FromBody] SearchHitsRequest request)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();
                var userId = _clientHandler.GetUserId();
                if (request == null || request.CaseId == 0) return Json(new { success = false, message = "Invalid request" });
                dynamic modelrisk = null;
                int Id = _customerCaseService.GetCaseId(request.corporateId);
                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
                if (request.Hits == null || request.Hits.Count == 0) return Json(new { success = true, message = "No hits to save" });
                foreach (var hit in request.Hits)
                {
                    if (hit.Decision != "")
                    {
                        _proliferationFinanceService.UpdateMongoHitDecision(request.CaseId, hit.Index, hit.Decision, hit.Remarks);
                        ProliferationFinanceCaseDTO result = _proliferationFinanceService.GetVersionAllCases(request.CaseId, request.corporateId, clientId);

                        CorporateKycDTO corpModel = new CorporateKycDTO();
                        KycIndividualDTO imodel = new KycIndividualDTO();
                        RiskModel _riskmodel = new RiskModel();
                        if (_CustomerCaseDTO.CustomerType == "C")
                        {
                            int riskid = _riskService.GetRiskIdByCustomercode(request.corporateId, _CustomerCaseDTO.CustomerType).Result;
                            if (riskid != 0)
                            {

                                _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, clientId);


                                modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporateByCID(request.corporateId).Result);

                                MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                                foreach (var category in _riskmodel.RiskTypeCategoryDTO)
                                {
                                    foreach (var riskType in category.RiskTypes)
                                    {
                                        if (!string.IsNullOrEmpty(riskType.ItemTxt))
                                        {
                                            switch (riskType.RiskType?.ToLower()) // or riskType.Code (better if available)
                                            {
                                                case "legal status of the entity ":
                                                    corpModel.EntityTypeTxt = riskType.ItemTxt;
                                                    break;

                                                case "nature of business":
                                                    corpModel.BusinessType = riskType.ItemTxt;
                                                    break;

                                                case "country of incorporation":
                                                    corpModel.PlaceofIncorporation = riskType.ItemTxt;
                                                    break;
                                                case "nationality partner 1":
                                                    corpModel.Partners[0].Nationality = riskType.ItemTxt;
                                                    break;
                                                case "nationality partner 2":
                                                    corpModel.Partners[1].Nationality = riskType.ItemTxt;
                                                    break;
                                                case "nationality partner 3":
                                                    corpModel.Partners[2].Nationality = riskType.ItemTxt;
                                                    break;

                                                case "does the company have any subsidiary, affiliate, branch or group/holding company in fatf listed high risk monitored jurisdiction?":
                                                    corpModel.FATF = riskType.ItemTxt;
                                                    break;

                                                case "product":
                                                    corpModel.ProductName = riskType.ItemTxt;
                                                    break;

                                                case "if more than one product(put the riskiest product)":
                                                    corpModel.HighestRiskProduct = riskType.ItemTxt;
                                                    break;

                                                case "delivery channel":
                                                    corpModel.DeliveryChannelName = riskType.ItemTxt;
                                                    break;
                                                case "is there any domestic pep match on the owners / bod/senior management / related parties names?":
                                                    corpModel.Domesticpep = riskType.ItemTxt;
                                                    break;
                                                case "is there any foreign pep match on the owners / bod/senior management/ related parties names?":
                                                    corpModel.ForeignPep = riskType.ItemTxt;
                                                    break;
                                                case "are there any red flags noticed against the company/ owners / bod/senior management/ related parties names?":
                                                    corpModel.RedFlags = riskType.ItemTxt;
                                                    break;
                                                case "is there any owners / bod/senior management names categorized as very high networth individual?":
                                                    corpModel.HighNetworkIndividual = riskType.ItemTxt;
                                                    break;
                                                case "is there a sanction match against other than uae local list or unsc consolidated list on the company, owner/partners/bod, senior management / related parties names?":
                                                    corpModel.SanctionMatch = riskType.ItemTxt;
                                                    break;
                                                case "is there a sanction match against uae local list or unsc consolidated list on the company, owner/partners/bod, senior management / related parties names?":
                                                    corpModel.UAEORUNSC = riskType.ItemTxt;
                                                    break;
                                                
                                                case "mode of payment":
                                                    corpModel.Modeofpayment = riskType.ItemTxt;
                                                    break;
                                                case "dual use goods match":
                                                    corpModel.DualUseGoods = riskType.ItemTxt;
                                                    break;

                                                default:
                                                    // Optional: log unmatched value
                                                    break;
                                            }
                                        }
                                    }
                                }

                            }
                            if (result.Version > 1)
                            {
                                if (hit.Decision == "True Match" || hit.Decision == "Potential Match")
                                {
                                    corpModel.MoreDualUseGoods = "Yes";
                                }
                                else
                                {
                                    corpModel.MoreDualUseGoods = "No";
                                }
                            }
                            else
                            {
                                if (hit.Decision == "True Match" || hit.Decision == "Potential Match")
                                {
                                    corpModel.DualUseGoods = "Yes";
                                }
                                else
                                {
                                    corpModel.DualUseGoods = "No";
                                }
                            }

                            //var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientId));

                            //if (result != null)
                            //{
                            Console.WriteLine("Generate risk");


                            
                            //model.IsPeP = isPep;
                            var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(corpModel), "C", culture, clientId);
                            if (str1.Result == null)
                            {
                                return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                            }

                            var spStr1 = str1.Result.Split('Ø');
                            var entlovId = GetSafely(spStr1, 2);
                            var buslovId = GetSafely(spStr1, 4);
                            var incorplovId = GetSafely(spStr1, 3);
                            var productlovId = GetSafely(spStr1, 11);
                            var deliverylovId = GetSafely(spStr1, 12);
                            var nationality1lovId = GetSafely(spStr1, 6);
                            var nationality2lovId = GetSafely(spStr1, 7);
                            var nationality3lovId = GetSafely(spStr1, 8);
                            var nationality4lovId = GetSafely(spStr1, 9);
                            var nationality5lovId = GetSafely(spStr1, 10);
                            var modeofpaymentlovId = GetSafely(spStr1, 16);
                            var domesticpeplovId = GetSafely(spStr1,18);
                            var foreignlovId = GetSafely(spStr1,20);
                            var redflagslovId = GetSafely(spStr1,22);
                            var sanctionlovId = GetSafely(spStr1,24);
                            var UAEORUNSClovId = GetSafely(spStr1,26);
                            var corpfaftlovId = GetSafely(spStr1,27);
                            var highestriskproductlovId = GetSafely(spStr1,29);
                            var veryhighnetworkIdlovId = GetSafely(spStr1, 31);

                            var dualusegoodslovId = GetSafely(spStr1, 32);
                            var MoredualusegoodslovId = GetSafely(spStr1, 34);

                            var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(corpModel), "C", culture, clientId);

                            if (str.Result == null)
                            {
                                return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                            }
                            var spStr = str.Result.Split('Ø');
                            var entId = GetSafely(spStr, 2);
                            var busId = GetSafely(spStr, 4);
                            var incorpId = GetSafely(spStr, 3);
                            var productId = GetSafely(spStr, 11);
                            var deliveryId = GetSafely(spStr, 12);
                            var nationality1Id = GetSafely(spStr, 6);
                            var nationality2Id = GetSafely(spStr, 7);
                            var nationality3Id = GetSafely(spStr, 8);
                            var nationality4Id = GetSafely(spStr, 9);
                            var nationality5Id = GetSafely(spStr, 10);
                            var modeofpaymentId = GetSafely(spStr, 16);
                            var domesticpepId = GetSafely(spStr, 18);
                            var foreignId = GetSafely(spStr, 20);
                            var redflagsId = GetSafely(spStr, 22);
                            var sanctionId = GetSafely(spStr, 24);
                            var UAEORUNSCId = GetSafely(spStr, 26);
                            var corpfaftId = GetSafely(spStr, 27);
                            var highestriskproductId = GetSafely(spStr, 29);
                            var veryhighnetworkId = GetSafely(spStr, 31);
                            var dualusegoodsId = GetSafely(spStr, 32);
                            var MoredualusegoodsId = GetSafely(spStr, 34);


                            RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                            riskModel.CustomerId = request.corporateId;
                            riskModel.CustomerName = request.FirstName;
                            //riskModel.MainNationality = modelrisk?.pl;
                            riskModel.ClientId = _clientHandler.GetClientId();
                            riskModel.CreatedBy = _clientHandler.GetUserId();


                            riskModel.RiskCategory = "C";


                            var riskTypeList = new List<RiskTypeListModel>();

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
                            if (dualusegoodsId != "0")
                            {
                                var riskType20 = new RiskTypeListModel();
                                riskType20.Id = Convert.ToString(dualusegoodslovId);
                                var riskItem20 = new RiskItemListModel();
                                riskItem20.Id = dualusegoodsId.ToString();//Convert.ToString(1);
                                var riskItemList20 = new List<RiskItemListModel>();
                                riskItemList20.Add(riskItem20);
                                riskType20.RiskItemList = riskItemList20;
                                riskTypeList.Add(riskType20);
                            }
                            if (MoredualusegoodsId != "0")
                            {
                                var riskType21 = new RiskTypeListModel();
                                riskType21.Id = Convert.ToString(MoredualusegoodslovId);
                                var riskItem21 = new RiskItemListModel();
                                riskItem21.Id = MoredualusegoodsId.ToString();//Convert.ToString(1);
                                var riskItemList21 = new List<RiskItemListModel>();
                                riskItemList21.Add(riskItem21);
                                riskType21.RiskItemList = riskItemList21;
                                riskTypeList.Add(riskType21);
                            }

                            //for legal status of entity start

                            //for mode of payment end

                            riskModel.RiskTypeList = riskTypeList;
                            var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                            var xyz = riskResult;

                            //}
                        }
                        else
                        {
                            int riskid = _riskService.GetRiskIdByCustomercode(request.corporateId, _CustomerCaseDTO.CustomerType).Result;
                            if (riskid != 0)
                            {

                                _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, clientId);


                                modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividualByCID(request.corporateId).Result);

                                MapRiskValues(_riskmodel.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                                foreach (var category in _riskmodel.RiskTypeCategoryDTO)
                                {
                                    foreach (var riskType in category.RiskTypes)
                                    {
                                        if (!string.IsNullOrEmpty(riskType.ItemTxt))
                                        {
                                            switch (riskType.RiskType?.ToLower()) // or riskType.Code (better if available)
                                            {
                                                case "profession":
                                                    imodel.OccupatinTypeTxt = riskType.ItemTxt;
                                                    break;
                                                case "residence country":
                                                    imodel.ResidenceStatus = riskType.ItemTxt;
                                                    break;
                                                case "nationality":
                                                    imodel.Nationality = riskType.ItemTxt;
                                                    break;
                                                //case "Second Nationality (if applicable)":
                                                //    imodel.Parnter = riskType.ItemTxt;
                                                //    break;
                                                case "product, service & activity":
                                                    imodel.ProductName = riskType.ItemTxt;
                                                    break;
                                                case "if more than one product(put the riskiest product)":
                                                    imodel.HighestRiskProduct = riskType.ItemTxt;
                                                    break;
                                                case "delivery channel":
                                                    imodel.DeliveryChannelName = riskType.ItemTxt;
                                                    break;
                                                case "is the customer a domestic pep or related close associate of domestic pep?":
                                                    imodel.Domesticpep = riskType.ItemTxt;
                                                    break;
                                                case "is the customer a foreign pep or related close associate of foreign pep?":
                                                    imodel.ForeignPep = riskType.ItemTxt;
                                                    break;
                                                case "are there any red flags noticed against the customer or related close associate?":
                                                    imodel.RedFlags = riskType.ItemTxt;
                                                    break;
                                                case "is the customer categorized as very high networth individual?":
                                                    imodel.HighNetworkIndividual = riskType.ItemTxt;
                                                    break;
                                                case "is the customer a sanction match against other than uae local list or unsc consolidated?":
                                                    imodel.SanctionMatch = riskType.ItemTxt;
                                                    break;
                                                case "is the customer a sanction match against uae local list or unsc consolidated?":
                                                    imodel.UAEORUNSC = riskType.ItemTxt;
                                                    break;

                                                case "mode of payment":
                                                    imodel.ModeOfPayment = riskType.ItemTxt;
                                                    break;
                                                case "dual use goods match":
                                                    corpModel.DualUseGoods = riskType.ItemTxt;
                                                    break;
                                            }
                                        }
                                    }
                                }

                            }
                            if (result.Version > 1)
                            {
                                if (hit.Decision == "True Match" || hit.Decision == "Potential Match")
                                {
                                    imodel.MoreDualUseGoods = "Yes";
                                }
                                else
                                {
                                    imodel.MoreDualUseGoods = "No";
                                }
                            }
                            else
                            {
                                if (hit.Decision == "True Match" || hit.Decision == "Potential Match")
                                {
                                    imodel.DualUseGoods = "Yes";
                                }
                                else
                                {
                                    imodel.DualUseGoods = "No";
                                }
                            }
                            

                            //var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientId));

                            //if (result != null)
                            //{
                            Console.WriteLine("Generate risk");



                            //model.IsPeP = isPep;
                            var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(imodel), corpModel, "I", culture, clientId);
                            if (str1.Result == null)
                            {
                                
                                return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });

                            }
                            var spStr1 = str1.Result.Split('Ø');
                            var proflovId = GetSafely(spStr1, 0);
                            var natlovId = GetSafely(spStr1, 1);
                            var reslovId = GetSafely(spStr1, 5);
                            //var IspeplovId = GetSafely(spStr1, 13);
                            var IndprodlovId = GetSafely(spStr1, 13);
                            var InddelilovId = GetSafely(spStr1, 14);
                            var IndmodeofpaymentlovId = GetSafely(spStr1, 15);
                            var domesticpeplovId = GetSafely(spStr1, 17);
                            var foreignlovId = GetSafely(spStr1, 19);
                            var redflagslovId = GetSafely(spStr1, 21);
                            var sanctionlovId = GetSafely(spStr1, 23);
                            var UAEORUNSClovId = GetSafely(spStr1, 25);
                            var highestriskproductlovId = GetSafely(spStr1, 28);
                            var veryhighnetworkIdlovId = GetSafely(spStr1, 30);
                            var dualusegoodslovId = GetSafely(spStr1, 33);
                            var MoredualusegoodslovId = GetSafely(spStr1, 35);

                            var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(imodel), corpModel, "I", culture,clientId);
                            if (str.Result == null)
                            {
                                
                                return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                            }
                            var spStr = str.Result.Split('Ø');
                            var profId = GetSafely(spStr, 0);
                            var natId = GetSafely(spStr, 1);
                            var resId = GetSafely(spStr, 5);
                            //var IspepId = GetSafely(spStr, 13);
                            var IndprodId = GetSafely(spStr, 13);
                            var InddeliId = GetSafely(spStr, 14);
                            var Indmodeofpaymentid = GetSafely(spStr, 15);
                            var domesticpepId = GetSafely(spStr, 17);
                            var foreignId = GetSafely(spStr, 19);
                            var redflagsId = GetSafely(spStr, 21);
                            var sanctionId = GetSafely(spStr, 23);
                            var UAEORUNSCId = GetSafely(spStr, 25);
                            var highestriskproductId = GetSafely(spStr, 28);
                            var veryhighnetworkId = GetSafely(spStr, 30);
                            var dualusegoodsId = GetSafely(spStr, 33);
                            var MoredualusegoodsId = GetSafely(spStr, 35);
                            RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                            riskModel.CustomerId = request.corporateId;
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
                                riskType8.Id = Convert.ToString(MoredualusegoodslovId);
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

                        }
                    }
                    


                    
                }
                

                int previousStatus = _CustomerCaseDTO.Status;

                var result1 = _clientHandler.PostAsync(new { caseid = Id.ToString() }, ScreeningService.GETBYCASEID).Result;

                // Declare jsonList outside
                List<DataListModel> jsonList = new List<DataListModel>();
                bool hasMatchRecords = false;

                if (!string.IsNullOrEmpty(result1))
                {
                    jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result1);
                    hasMatchRecords = jsonList != null && jsonList.Any();
                }

                // default first time

                if (previousStatus == 2 || previousStatus == 3)
                {
                    int newStatus = hasMatchRecords ? 0 : 5;

                    _CustomerCaseDTO.Status = newStatus;
                }

                _customerCaseService.Update(_CustomerCaseDTO);
                var comments = new List<(string Comment, string CommentType)>
                {
                    ("Risk Parameter Has Been Updated", "Risk Parameters(By Proliferation Finance)"),
                    ((request.Hits != null && request.Hits.Count > 0 ? request.Hits[0].Decision : "Details") +" Found And Saved", "Proliferation Finance")
                    
                };

                foreach (var item in comments)
                {
                    var remarkModel = new CaseCommentModel
                    {
                        CaseId = Id,
                        Comment = item.Comment,
                        CommentType = item.CommentType,
                        CreatedBy = _clientHandler.GetUserId(),

                    };

                    _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
                }

                var commentDto = new CaseCommentDTO
                {
                    CaseId = request.CaseId.ToString(),
                    Comment = "Search Results Has been Updated",
                    CreatedBy = userId,
                    CreatedOn = DateTime.Now.ToString("dd/MM/yyyy"),
                    CommentType = "Search Results"
                };
                _caseCommentService.CreateProliferationCaseComments(commentDto);

                return Json(new { success = true });
            }
            
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
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

        private string GetSafely(string[] segments, int index)
        {
            if (segments == null || index < 0 || segments.Length <= index) return "0";
            return segments[index] ?? "0";
        }
    }

    public class SearchHitsRequest
    {
        public int CaseId { get; set; }

        public string corporateId { get; set; }

        public string FirstName { get; set; }
        public List<SearchHitDecision> Hits { get; set; }
    }

    public class SearchHitDecision
    {
        public int Index { get; set; }
        public string SearchType { get; set; }
        public string Decision { get; set; }
        public string Remarks { get; set; }
    }

    public class BulkUploadSummary
    {
        public int Total { get; set; }
        public int Success { get; set; }
        public int Matches { get; set; }
        public int Failed { get; set; }
        public List<BulkError> Errors { get; set; } = new List<BulkError>();
        public List<BulkRowResult> Results { get; set; } = new List<BulkRowResult>();
    }

    public class BulkRowResult
    {
        public int Row { get; set; }
        public string CorporateId { get; set; }
        public int CaseId { get; set; }
        public string Message { get; set; }
    }

    public class BulkError
    {
        public int Row { get; set; }
        public string Message { get; set; }
    }
}




