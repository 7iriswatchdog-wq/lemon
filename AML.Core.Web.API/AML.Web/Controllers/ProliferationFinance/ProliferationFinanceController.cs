using AML.Core.ServiceContract.ProliferationFinance;
using AML.DTO.DTO.ProliferationFinance;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.CaseComment;
using AML.Web.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;
using System.Threading.Tasks;
using System.Drawing;

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

        public ProliferationFinanceController(
            IProliferationFinanceService proliferationFinanceService, 
            IHttpContextAccessor httpContextAccessor,
            ICaseDocumentService caseDocumentService,
            ICaseCommentService caseCommentService,
            IViewRenderService viewRenderService,
            IExportDataService exportDataService)
        {
            _proliferationFinanceService = proliferationFinanceService;
            _httpContextAccessor = httpContextAccessor;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
            _viewRenderService = viewRenderService;
            _exportDataService = exportDataService;
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
        public IActionResult GetAllCases(DateTime? startDate, DateTime? endDate, string customerType)
        {
            try
            {
                var response = _proliferationFinanceService.GetAllCases();
                if (response.Status == 200)
                {
                    var data = response.Result;

                    // Apply filters in memory for now if service doesn't support it directly
                    if (startDate.HasValue)
                        data = data.FindAll(x => x.CreatedOn >= startDate.Value);
                    
                    if (endDate.HasValue)
                        data = data.FindAll(x => x.CreatedOn <= endDate.Value.AddDays(1).AddSeconds(-1));

                    if (!string.IsNullOrEmpty(customerType) && customerType != "0")
                        data = data.FindAll(x => x.CustomerType == customerType);

                    return Json(new { success = true, data = data });
                }
                return Json(new { success = false, message = "Failed to fetch cases." });
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
                        // Fill all fields from the database record for full transparency
                        searchDto.MatchedChemicalName = match.ChemicalName;
                        searchDto.HsCode = match.HsCode;
                        searchDto.CasNumber = match.CasNumber;
                        searchDto.Eccn = match.Eccn;
                        searchDto.SynonymName = match.SynonymName;
                        
                        matchSources.Add("UAE Control List");
                    }
                }

                // 3. PDF Scan (triggered ONLY by Chemical Name)
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

                // 4. Update Case Metadata based on results
                if (dbMatch || pdfMatch)
                {
                    searchDto.Score = "100";
                    searchDto.StatusReason = "Match found in: " + string.Join(" & ", matchSources);
                }
                else
                {
                    searchDto.Score = "0";
                    searchDto.StatusReason = "No match found in UAE Control List or Document Scan";
                }

                // 5. Direct Case Creation
                var createResponse = _proliferationFinanceService.CreateCase(searchDto);
                if (createResponse.Status == 200 && createResponse.Result > 0)
                {
                    return Json(new { 
                        success = true, 
                        caseId = createResponse.Result, 
                        caseRefId = "PF-" + createResponse.Result,
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

                // For Chemical cases, we might want to re-fetch potential hits from UAE Goods
                // For Non-Chemical, the "ChemicalName" field already stores the matched paragraph(s)
                return View(model);
            }
            catch (Exception)
            {
                return RedirectToAction("CaseManager");
            }
        }

        [HttpPost]
        public IActionResult UpdateStatus(int caseId, string status)
        {
            try
            {
                var response = _proliferationFinanceService.UpdateCaseStatus(caseId, status);
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
                var response = _proliferationFinanceService.UpdateCaseRemarks(caseId, remarks);
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
                    CreatedOn = DateTime.Now.ToString("dd/MM/yyyy")
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

        [HttpPost]
        public IActionResult SaveSearchHits([FromBody] SearchHitsRequest request)
        {
            try
            {
                var jsonHits = Newtonsoft.Json.JsonConvert.SerializeObject(request.Hits);
                var response = _proliferationFinanceService.UpdateSearchHits(request.CaseId, jsonHits);
                return Json(new { success = response.Status == 200, message = response.Message });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet("/ProliferationFinance/Details/{id}")]
        public IActionResult Details(int id)
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
                var pdfBytes = _exportDataService.HtmlToPDF(html, null);
                
                string fileName = $"PF_MatchReport_{model.CorporateId}_{DateTime.Now:yyyyMMdd}.pdf";
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
                var response = _caseCommentService.GetAllByCase(caseId);
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
                                            
                                            // Fill all fields from the database record for full transparency
                                            caseDto.MatchedChemicalName = match.ChemicalName;
                                            caseDto.HsCode = match.HsCode;
                                            caseDto.CasNumber = match.CasNumber;
                                            caseDto.Eccn = match.Eccn;
                                            caseDto.SynonymName = match.SynonymName;
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
                                        caseDto.StatusReason = "Match found in: " + string.Join(" & ", matchSources);
                                        if (!string.IsNullOrEmpty(remarks)) caseDto.StatusReason += " | Remarks: " + remarks;

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
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(219, 234, 254));
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
    }

    public class SearchHitsRequest
    {
        public int CaseId { get; set; }
        public List<SearchHitDecision> Hits { get; set; }
    }

    public class SearchHitDecision
    {
        public int Index { get; set; }
        public string SearchType { get; set; }
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
