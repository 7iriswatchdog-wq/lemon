using AML.Core.ServiceContract.ProliferationFinance;
using AML.DTO.DTO.ProliferationFinance;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.CaseComment;
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

        public ProliferationFinanceController(
            IProliferationFinanceService proliferationFinanceService, 
            IHttpContextAccessor httpContextAccessor,
            ICaseDocumentService caseDocumentService,
            ICaseCommentService caseCommentService)
        {
            _proliferationFinanceService = proliferationFinanceService;
            _httpContextAccessor = httpContextAccessor;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
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
                        var worksheet = package.Workbook.Worksheets[0];
                        if (worksheet.Dimension == null)
                             return Json(new { success = false, message = "Spreadsheet is empty" });

                        var rowCount = worksheet.Dimension.Rows;
                        summary.Total = rowCount - 1; 

                        for (int row = 2; row <= rowCount; row++)
                        {
                            try
                            {
                                var customerId = worksheet.Cells[row, 1].Value?.ToString();
                                var category = worksheet.Cells[row, 2].Value?.ToString();
                                
                                if (string.IsNullOrEmpty(customerId) && string.IsNullOrEmpty(category)) continue; 

                                if (string.IsNullOrEmpty(customerId) || string.IsNullOrEmpty(category))
                                {
                                    summary.Failed++;
                                    summary.Errors.Add(new BulkError { Row = row, Message = "Customer ID or Category is missing" });
                                    continue;
                                }

                                if (category.Equals("Chemical", StringComparison.OrdinalIgnoreCase))
                                {
                                    var searchDto = new ProliferationFinanceCaseDTO
                                    {
                                        CustomerType = "Chemical",
                                        CorporateId = customerId,
                                        HsCode = worksheet.Cells[row, 3].Value?.ToString(),
                                        CasNumber = worksheet.Cells[row, 4].Value?.ToString(),
                                        Eccn = worksheet.Cells[row, 5].Value?.ToString(),
                                        ChemicalName = worksheet.Cells[row, 6].Value?.ToString(),
                                        SynonymName = worksheet.Cells[row, 7].Value?.ToString(),
                                        CreatedBy = userId,
                                        CreatedOn = DateTime.Now,
                                        StatusReason = worksheet.Cells[row, 9].Value?.ToString()
                                    };

                                    var searchResponse = _proliferationFinanceService.SearchChemicals(searchDto);
                                    if (searchResponse.Status == 200 && searchResponse.Result != null && searchResponse.Result.Count > 0)
                                    {
                                        summary.Matches++;
                                        searchDto.Score = "100";
                                        // We no longer overwrite searchDto.ChemicalName, HsCode, etc. with bestMatch data.
                                        // The user wants the original data from the spreadsheet to be saved.

                                        var createResponse = _proliferationFinanceService.CreateCase(searchDto);
                                        if (createResponse.Status == 200 && createResponse.Result > 0)
                                        {
                                            summary.Success++;
                                            summary.Results.Add(new BulkRowResult
                                            {
                                                Row = row,
                                                CorporateId = customerId,
                                                Category = category,
                                                Score = searchDto.Score,
                                                CaseId = createResponse.Result,
                                                Message = "Match Found"
                                            });
                                        }
                                        else summary.Failed++;
                                    }
                                    else
                                    {
                                        summary.Success++;
                                        summary.Results.Add(new BulkRowResult
                                        {
                                            Row = row,
                                            CorporateId = customerId,
                                            Category = category,
                                            Score = "0",
                                            Message = "No Match"
                                        });
                                    }
                                }
                                else if (category.Equals("Non-Chemical", StringComparison.OrdinalIgnoreCase))
                                {
                                    var keyword = worksheet.Cells[row, 8].Value?.ToString();
                                    if (string.IsNullOrEmpty(keyword))
                                    {
                                        summary.Failed++;
                                        summary.Errors.Add(new BulkError { Row = row, Message = "Keyword is missing for Non-Chemical" });
                                        continue;
                                    }

                                    var searchResponse = _proliferationFinanceService.SearchNonChemical(keyword);
                                    if (searchResponse.Status == 200 && !string.IsNullOrEmpty(searchResponse.Result))
                                    {
                                        summary.Matches++;
                                        var caseDto = new ProliferationFinanceCaseDTO
                                        {
                                            CustomerType = "Non-Chemical",
                                            CorporateId = customerId,
                                            ChemicalName = keyword, // Map keyword to ChemicalName
                                            SearchHitDetails = searchResponse.Result, // Matched paragraph goes here
                                            Score = "100",
                                            CreatedBy = userId,
                                            CreatedOn = DateTime.Now,
                                            StatusReason = worksheet.Cells[row, 9].Value?.ToString()
                                        };
                                    var createResponse = _proliferationFinanceService.CreateCase(caseDto);
                                    if (createResponse.Status == 200)
                                    {
                                        summary.Success++;
                                        summary.Results.Add(new BulkRowResult
                                        {
                                            Row = row,
                                            CorporateId = customerId,
                                            Category = category,
                                            Score = caseDto.Score,
                                            CaseId = createResponse.Result,
                                            Message = "Case created"
                                        });
                                    }
                                    else summary.Failed++;
                                }
                                else
                                {
                                    summary.Success++;
                                    summary.Results.Add(new BulkRowResult
                                    {
                                        Row = row,
                                        CorporateId = customerId,
                                        Category = category,
                                        Score = "0",
                                        Message = "No match found"
                                    });
                                }
                            }
                            else if (category.Equals("Non-Chemical", StringComparison.OrdinalIgnoreCase))
                            {
                                var keyword = worksheet.Cells[row, 8].Value?.ToString();
                                if (string.IsNullOrEmpty(keyword))
                                {
                                    summary.Failed++;
                                    summary.Errors.Add(new BulkError { Row = row, Message = "Keyword is missing for Non-Chemical" });
                                    continue;
                                }

                                var searchResponse = _proliferationFinanceService.SearchNonChemical(keyword);
                                var caseDto = new ProliferationFinanceCaseDTO
                                {
                                    CustomerType = "Non-Chemical",
                                    CorporateId = customerId,
                                    SynonymName = keyword,
                                    ChemicalName = searchResponse.Result,
                                    Score = !string.IsNullOrEmpty(searchResponse.Result) ? "100" : "0",
                                    StatusReason = !string.IsNullOrEmpty(searchResponse.Result) ? "Match found in document scan" : "No match found in document scan",
                                    CreatedBy = userId,
                                    CreatedOn = DateTime.Now,
                                    Type = "Corporate"
                                };

                                var createResponse = _proliferationFinanceService.CreateCase(caseDto);
                                if (createResponse.Status == 200)
                                {
                                    summary.Success++;
                                    if (!string.IsNullOrEmpty(searchResponse.Result)) summary.Matches++;
                                    
                                    summary.Results.Add(new BulkRowResult
                                    {
                                        Row = row,
                                        CorporateId = customerId,
                                        Category = category,
                                        Score = caseDto.Score,
                                        CaseId = createResponse.Result,
                                        Message = !string.IsNullOrEmpty(searchResponse.Result) ? "Match Found" : "No Match"
                                    });
                                }
                                else summary.Failed++;
                            }
                            else
                            {
                                summary.Failed++;
                                summary.Errors.Add(new BulkError { Row = row, Message = "Invalid Category. Use 'Chemical' or 'Non-Chemical'" });
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
                xl.Cells[1, 2].Value = "Category (Chemical/Non-Chemical)";
                xl.Cells[1, 3].Value = "HS Code";
                xl.Cells[1, 4].Value = "CAS Number";
                xl.Cells[1, 5].Value = "ECCN";
                xl.Cells[1, 6].Value = "Chemical Name";
                xl.Cells[1, 7].Value = "Synonym Name";
                xl.Cells[1, 8].Value = "Search Keyword (Non-Chemical Only)";
                xl.Cells[1, 9].Value = "Remarks";

                using (var range = xl.Cells[1, 1, 1, 9])
                {
                    range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(219, 234, 254));
                    range.Style.Font.Bold = true;
                }

                xl.Column(1).Width = 15;
                xl.Column(2).Width = 25;
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
        public string Category { get; set; }
        public string Score { get; set; }
        public int CaseId { get; set; }
        public string Message { get; set; }
    }

    public class BulkError
    {
        public int Row { get; set; }
        public string Message { get; set; }
    }
}
