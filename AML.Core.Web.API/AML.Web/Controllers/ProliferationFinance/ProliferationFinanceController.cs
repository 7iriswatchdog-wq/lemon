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
            IToastNotification toastNotification, RiskAPIController riskAPIController, ILovMasterService lovMasterService, IRiskService RiskService)
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

                    if (!string.IsNullOrEmpty(status) && status != "0")
                        data = data.FindAll(x => x.Status == status);

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

                var mongoData = _proliferationFinanceService.GetMongoSearchResults(id);
                if (mongoData != null) model.MongoHits = mongoData.Hits;
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
                    CreatedOn = DateTime.Now.ToString("dd/MM/yyyy")
                };
                _caseCommentService.Create(commentDto);
                
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
                if (request == null || request.CaseId == 0) return Json(new { success = false, message = "Invalid request" });

                foreach (var hit in request.Hits)
                {
                    if (!string.IsNullOrEmpty(hit.Decision))
                    {
                        _proliferationFinanceService.UpdateMongoHitDecision(request.CaseId, hit.Index, hit.Decision, hit.Remarks);
                    }
                }

                int Id = _customerCaseService.GetCaseId(request.corporateId);

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);

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

                if (previousStatus == 2)
                {
                    int newStatus = hasMatchRecords ? 0 : 5;

                    _CustomerCaseDTO.Status = newStatus;
                }

                _customerCaseService.Update(_CustomerCaseDTO);
                var comments = new List<(string Comment, string CommentType)>
                {
                    ("Search hits have been reviewed and saved", "Proliferation Finance")
                };

                foreach (var item in comments)
                {
                    var remarkModel = new CaseCommentModel
                    {
                        CaseId = Id,
                        Comment = item.Comment,
                        CommentType = item.CommentType,
                        CreatedBy = _clientHandler.GetUserId()
                    };

                    _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
                }

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
