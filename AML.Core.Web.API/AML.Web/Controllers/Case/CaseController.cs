using Amazon.Auth.AccessControlPolicy;
using Amazon.Runtime;
using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Authentication;
using AML.Core.DataContract.Enum;
using AML.Core.Repository;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.RepositoryContract.FreeSource;
using AML.Core.Service.CaseStudio;
using AML.Core.Service.CustomerScreening;
using AML.Core.Service.UserAccess;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.CaseComment;
using AML.Core.ServiceContract.CaseDocument;
using AML.Core.ServiceContract.CaseStudio;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.Country;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.Core.ServiceContract.CustomerMaster;
using AML.Core.ServiceContract.CustomerScreening;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.ServiceContract.Kyc;
using AML.Core.ServiceContract.LovMaster;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.UserAccess;
using AML.Core.ServiceContract.UserGroup;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.CaseComment;
using AML.DTO.DTO.CaseDocument;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.FreeSource;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.ProliferationFinance;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.CaseAssignment;
using AML.ViewModel.ViewModels.CaseComment;
using AML.ViewModel.ViewModels.CaseDetail;
using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CaseProcess;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.Common;
using AML.ViewModel.ViewModels.Corporate;
using AML.ViewModel.ViewModels.CorporateDetail;
using AML.ViewModel.ViewModels.Country;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.CustomerCategory;
using AML.ViewModel.ViewModels.DataTable;
using AML.ViewModel.ViewModels.IdentityType;
using AML.ViewModel.ViewModels.Kyc;
using AML.ViewModel.ViewModels.ProliferationFinance;
using AML.ViewModel.ViewModels.Report;
using AML.ViewModel.ViewModels.Risk;
using AML.ViewModel.ViewModels.RiskAPI;
using AML.ViewModel.ViewModels.RiskV2;
using AML.ViewModel.ViewModels.Sanction;
using AML.ViewModel.ViewModels.User;
using AML.ViewModel.ViewModels.UserGroup;
using AML.Web.CustomFilters;
using AML.Web.Helper;
using AutoMapper;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Drawing.Wordprocessing;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.collection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using NLog;
using NToastNotify;
using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Logical;
using OfficeOpenXml.Style;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static AML.Core.Service.Common.CommonService;
using static AML.DTO.DTO.FreeSource.BlackListMongoDTO;
using static AML.DTO.DTO.FreeSource.CaseLogsMongoDTO;
using static Microsoft.AspNetCore.Razor.Language.TagHelperMetadata;
using ApiResultModel = AML.DTO.DTO.CustomerCase.ApiResultModel;
using Document = iTextSharp.text.Document;
using Font = iTextSharp.text.Font;
using Formatting = Newtonsoft.Json.Formatting;
using Image = iTextSharp.text.Image;
using PageSize = iTextSharp.text.PageSize;
using Paragraph = iTextSharp.text.Paragraph;
using Rectangle = iTextSharp.text.Rectangle;


namespace AML.Web.Controllers.Case
{
    [SessionAuthorize]
    [ServiceFilter(typeof(PageAuthorizeAttribute))]
    public class CaseController : Controller
    {
        private IMapper _mapper;
        private readonly IToastNotification _toastNotification;
        private IHttpClientHandler _clientHandler;
        private IConfiguration _configuration;
        private ICustomerCaseService _customerCaseService;
        private ICustomerScreeningService _customerScreeningService;
        private ICustomerMasterService _customerMasterService;
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
        private string pdfbaseURL = string.Empty;
        private string baseC6URL = string.Empty;
        private string _c6Username;
        private int checkThreshold = 0;
        private RiskAPIController _riskAPIController;
        private IViewRenderService _viewRenderService;
        private IExportDataService _exportService;
        private IFreeSourceRepository _freeSourceRepository;
        private ICustomerMasterRepository _customerMasterRepository;
        private string culture = CultureInfo.CurrentCulture.Name;
        private IKycService _kycService;
        private ILovMasterService _lovMasterService;
        private IRiskService _riskService;
        private IUserGroupService _UserGroupService;
        IUserGroupRightService _userGroupRightService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICaseStudioService _caseStudioService;


        private readonly Logger log = LogManager.GetCurrentClassLogger();

        public CaseController(IMapper mapper,
            IToastNotification toastNotification, ICountryService countryService, ICaseDocumentService caseDocumentService, ICustomerMasterService customerMasterService, IHttpClientFactory httpClientFactory,
            IHttpClientHandler clientHandler, ICustomerCategoryService customerCategoryService, ICaseCommentService caseCommentService, IUserGroupService UserGroupService, IUserGroupRightService userGroupRightService,
            IConfiguration configuration, IIdentityTypeService idTypeService, IUserService userService, ICaseAssignmentService caseAssignmentService, ICommonService commonService, IKycService kycService, ILovMasterService lovMasterService, IRiskService RiskService,
            ICustomerCaseService CustomerCaseService, ICustomerScreeningService CustomerScreeningService, IFileUploader fileUploader, IExportDataService exportService, IViewRenderService viewRenderService, RiskAPIController riskAPIController, IFreeSourceRepository freeSourceRepository,
            ICaseStudioService caseStudioService, ICustomerMasterRepository customerMasterRepository)
        {
            _mapper = mapper;
            _toastNotification = toastNotification;
            _clientHandler = clientHandler;
            _configuration = configuration;
            _customerCaseService = CustomerCaseService;
            _customerScreeningService = CustomerScreeningService;
            _customerMasterService = customerMasterService;
            _countryService = countryService;
            _idTypeService = idTypeService;
            _customerCategoryService = customerCategoryService;
            _userService = userService;
            _caseDocumentService = caseDocumentService;
            _caseCommentService = caseCommentService;
            _caseAssignmentService = caseAssignmentService;
            _fileUploader = fileUploader;
            _commonService = commonService;
            _riskAPIController = riskAPIController;
            baseURL = configuration.GetSection("AMLBaseApiUrl").GetSection("BaseUrl").Value;
            pdfbaseURL = configuration.GetSection("C6BaseApiUrl").GetSection("PDFbaseUrl").Value;
            _kycService = kycService;
            var clientId = _clientHandler.GetClientId();
            var clientDetails = _customerCaseService.GetClientDetailsByID(clientId);
            _c6Username = clientDetails?.C6Username;
            checkThreshold = clientDetails?.Threshold ?? 0;
            baseC6URL = clientDetails?.C6BaseUrl;
            //baseC6URL = configuration.GetSection("C6BaseApiUrl").GetSection("BaseUrl").Value;
            //_c6Username = _configuration.GetSection("C6BaseApiUrl:Username").Value;
            //checkThreshold = configuration.GetSection("C6BaseApiUrl").GetSection("Threshold").Value.ParseInt();
            _viewRenderService = viewRenderService;
            _exportService = exportService;
            _freeSourceRepository = freeSourceRepository;
            _customerMasterRepository = customerMasterRepository;
            _riskService = RiskService;
            _lovMasterService = lovMasterService;
            _UserGroupService = UserGroupService;
            _userGroupRightService = userGroupRightService;
            _httpClientFactory = httpClientFactory;
            _caseStudioService = caseStudioService;
        }

        [HttpGet("/case")]
        public ActionResult Index()
        {

            ReportPageViewModel model = new ReportPageViewModel
            {
                ReportData = new ReportLogSearchModel()  // ✅ FIX
            };
            var clientId = _clientHandler.GetClientId();
            model.ReportData.StartDate = System.DateTime.Now.AddYears(-1);
            //model.StartDate = System.DateTime.Now.AddDays(-7);
            model.ReportData.EndDate = System.DateTime.Now;
            //model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.ReportData.CustomerCategories = new SelectList(
    _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
        .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
        .ToList(),
    "Code",
    "Name"
);
            

            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            model.ReportData.UserGroupName = _UserGroupModel.Name;

            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.ReportData.Users = new SelectList(userList, "Value", "Text");
            var items = from CaseStatus d in Enum.GetValues(typeof(CaseStatus))
                        select new
                        {
                            Id = (int)d,
                            Name = Regex.Replace(d.ToString(), "(\\B[A-Z])", " $1")
                        };

            model.ReportData.CaseStatusList = new SelectList(items, "Id", "Name");
            
           



            return View(model);
        }

        [HttpGet("/case/studio")]
        public async Task<ActionResult> Studio(string triggerId)
        {
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            if (!string.IsNullOrEmpty(triggerId))
            {
                var dto = _customerCaseService.GetCaseFullDetailsByCustId(triggerId);
                if (dto != null)
                {
                    // Manually trigger screening
                    await _commonService.CustomerScreeningCall(dto, baseURL, baseC6URL, dto.MatchCategory, "", dto.Threshold, triggerId);
                    
                    // Manually trigger risk if it's a main entity
                    if (string.IsNullOrEmpty(dto.ParentID))
                    {
                        var node = new CaseStudioNode 
                        { 
                            FirstName = dto.FirstName, 
                            MiddleName = dto.MiddleName, 
                            LastName = dto.LastName,
                            Nationality = dto.Nationality,
                            Type = dto.CustomerType == "C" ? "Corporate" : "Individual",
                            IsRoot = true
                        };
                        await _caseStudioService.PerformDetailedRiskAssessmentAsync(node, clientId, userId, triggerId);
                    }
                    ViewBag.TriggerMessage = "Manual trigger successful for " + triggerId;
                }
            }
            var model = new CorporateScreeningModel();
            var CustomerType = "C";

            model.ClientId = clientId;
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, clientId)), "ProductName", "ProductName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.BusinessTypeList = new SelectList(_mapper.Map<List<BusinessNature>>(_kycService.GetBusinessType(culture, CustomerType, clientId)), "BusinessName", "BusinessName");
            model.EntityType = new SelectList(_mapper.Map<List<LegalStatusModel>>(_kycService.GetLegalStatus(culture, CustomerType, clientId)), "LegalStatus", "LegalStatus");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
            model.ProfessionalList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, "I", clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId)), "Code", "Name");
            model.RegistrationAuthorityList = new SelectList(new List<SelectListItem> { 
                new SelectListItem { Text = "DED", Value = "DED" }, 
                new SelectListItem { Text = "Free Zone", Value = "Free Zone" },
                new SelectListItem { Text = "DIFC", Value = "DIFC" },
                new SelectListItem { Text = "ADGM", Value = "ADGM" }
            }, "Value", "Text");
            
            model.GenderList = new SelectList(new List<SelectListItem> { 
                new SelectListItem { Text = "Male", Value = "Male" }, 
                new SelectListItem { Text = "Female", Value = "Female" }
            }, "Value", "Text");

            model.CounterPartyList = new SelectList(new List<SelectListItem> { 
                new SelectListItem { Text = "Insurer", Value = "Insurer" }, 
                new SelectListItem { Text = "Bank", Value = "Bank" },
                new SelectListItem { Text = "Developer", Value = "Developer" }
            }, "Value", "Text");

            model.GoldenVisaList = new SelectList(new List<SelectListItem> { 
                new SelectListItem { Text = "Yes", Value = "Yes" }, 
                new SelectListItem { Text = "No", Value = "No" }
            }, "Value", "Text");

            model.EmployerIndustryList = new SelectList(new List<SelectListItem>());
            model.EmployerSectorList = new SelectList(new List<SelectListItem>());
            
            model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Customer Risk");
            model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Product Risk");
            model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Delivery Channel Risk");
            model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Mode of Payment");
            
            var codes = _customerCaseService.GetCodesByClientID(clientId);
            ViewBag.ScreeningSources = codes.Select(x => new { Value = x.ccName, Text = x.ccName }).ToList();

            return View(model);
        }

        [HttpPost("/case/studio/batch-create")]
        public async Task<IActionResult> BatchCreate([FromBody] CaseStudioPayload payload)
        {
            if (payload == null || payload.Nodes == null)
            {
                return BadRequest("Invalid payload");
            }

            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            // Phase 0: Sanitization & Dedup (Hardening)
            var rootNode = payload.Nodes.FirstOrDefault(n => n.IsRoot);
            string dedupKey = null;

            if (rootNode != null)
            {
                if (!InputGuards.IsSafeName(rootNode.FirstName))
                {
                    return BadRequest($"First Name / Entity Name '{rootNode.FirstName}' contains prohibited characters or sequences.");
                }
                if (!InputGuards.IsSafeName(rootNode.LastName))
                {
                    return BadRequest($"Last Name '{rootNode.LastName}' contains prohibited characters or sequences.");
                }
                if (!InputGuards.IsSafeName(rootNode.Name))
                {
                    return BadRequest($"Display Name '{rootNode.Name}' contains prohibited characters or sequences.");
                }

                // Distinguish between Creation and Update for deduping
                if (rootNode.Id != null && !rootNode.Id.StartsWith("S_") && !rootNode.Id.StartsWith("ext-"))
                {
                    // Existing node - use ID to prevent concurrent updates to the same case
                    dedupKey = $"caseUpdate:{clientId}:{rootNode.Id}";
                }
                else
                {
                    // New node - use identity fields to prevent duplicates
                    dedupKey = $"caseCreate:{clientId}:{(rootNode.PassportId ?? rootNode.EmiratesIdNumber ?? "")}:{(rootNode.FirstName ?? "")}:{(rootNode.LastName ?? "")}:{(rootNode.Dob ?? "")}";
                }

                if (CaseCreationDedup.TryRegister(dedupKey))
                {
                    return Conflict("A similar case creation is already in progress. Please wait.");
                }
            }

            try
            {
                var result = await _caseStudioService.ProcessHierarchyAsync(payload, clientId, userId, baseURL, baseC6URL);
                if (result != null && result.Success)
                {
                    if (!string.IsNullOrEmpty(payload.DraftId))
                    {
                        await _caseStudioService.DeleteCaseDraftAsync(clientId, userId, payload.DraftId);
                    }
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                log.Error(ex, "Batch create failed in controller");
                return StatusCode(500, new { success = false, message = "An unexpected error occurred during batch creation.", error = ex.Message });
            }
            finally
            {
                if (!string.IsNullOrEmpty(dedupKey))
                {
                    CaseCreationDedup.Release(dedupKey);
                }
            }
        }

        [HttpPost("/case/studio/draft")]
        public async Task<IActionResult> SaveCaseDraft([FromBody] CaseStudioPayload payload)
        {
            if (payload == null || payload.Nodes == null)
            {
                return BadRequest(new { success = false, message = "Invalid payload" });
            }
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var draftId = await _caseStudioService.SaveCaseDraftAsync(clientId, userId, payload);
            if (!string.IsNullOrEmpty(draftId))
            {
                return Ok(new { success = true, message = "Draft saved successfully.", draftId = draftId });
            }
            return StatusCode(500, new { success = false, message = "Failed to save draft." });
        }

        [HttpGet("/case/studio/drafts")]
        public async Task<IActionResult> GetCaseDraftSummaries()
        {
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var drafts = await _caseStudioService.GetCaseDraftSummariesAsync(clientId, userId);
            return Ok(new { success = true, drafts = drafts });
        }

        [HttpGet("/case/studio/draft/{draftId}")]
        public async Task<IActionResult> GetCaseDraft(string draftId)
        {
            if (string.IsNullOrEmpty(draftId)) return BadRequest("Draft ID is required");
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var draft = await _caseStudioService.GetCaseDraftAsync(clientId, userId, draftId);
            if (draft != null)
            {
                return Ok(new { success = true, draft = draft });
            }
            return Ok(new { success = false, message = "No draft found." });
        }

        [HttpDelete("/case/studio/draft/{draftId}")]
        public async Task<IActionResult> DeleteCaseDraft(string draftId)
        {
            if (string.IsNullOrEmpty(draftId)) return BadRequest("Draft ID is required");
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var success = await _caseStudioService.DeleteCaseDraftAsync(clientId, userId, draftId);
            if (success)
            {
                return Ok(new { success = true, message = "Draft deleted successfully." });
            }
            return StatusCode(500, new { success = false, message = "Failed to delete draft." });
        }

        [HttpPost("/case/studio/draft/{draftId}/rename")]
        public async Task<IActionResult> RenameCaseDraft(string draftId, [FromBody] CaseStudioPayload payload)
        {
            if (string.IsNullOrEmpty(draftId) || payload == null || string.IsNullOrEmpty(payload.DraftName))
            {
                return BadRequest("Draft ID and new Draft Name are required.");
            }
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var success = await _caseStudioService.RenameCaseDraftAsync(clientId, userId, draftId, payload.DraftName);
            if (success)
            {
                return Ok(new { success = true, message = "Draft renamed successfully." });
            }
            return StatusCode(500, new { success = false, message = "Failed to rename draft." });
        }

        [HttpGet("/case/studio/fetch-hierarchy")]
        public IActionResult FetchHierarchy(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required");

            var clientID = HttpContext.Session.GetString("SessClientId").ParseInt();
            var clientInfo = _customerCaseService.GetCustomerCodeprefixByclient(clientID);
            string clientPrefix = clientInfo?.Prefix ?? "NAT";

            // 1. Normalize ID (prefixed NAT for numeric lookups)
            string normalizedId = CaseStudioHelper.NormalizeCustomerId(id, clientPrefix);
            var mainEntity = _customerCaseService.GetCaseFullDetailsByCustId(normalizedId);
            if (mainEntity == null)
            {
                mainEntity = _customerCaseService.GetCaseFullDetailsByCaseId(id.ParseInt());
                if (mainEntity == null) return NotFound("Entity not found");
            }

            var hierarchy = new List<CustomerCaseDTO>();
            var entitiesToProcess = new Queue<CustomerCaseDTO>();

            // Phase 1: Identify "Core" entities (Initial entity + Group members)
            string groupId = mainEntity.GroupId;
            
            // Fallback: If GroupId is missing from search result, try to fetch it directly
            if (string.IsNullOrEmpty(groupId))
            {
                var fullDetails = _customerCaseService.GetCaseFullDetailsByCustId(mainEntity.CustomerId);
                groupId = fullDetails?.GroupId;
            }

            if (!string.IsNullOrEmpty(groupId))
            {
                var groupMembers = _customerCaseService.GetCasesByGroupId(groupId);
                foreach (var member in groupMembers)
                {
                    if (!hierarchy.Any(h => h.CustomerId == member.CustomerId))
                    {
                        // Handle legacy/numeric links by prefixing
                        member.CompanyCode = CaseStudioHelper.NormalizeCustomerId(member.CompanyCode, clientPrefix);
                        member.ParentID = CaseStudioHelper.NormalizeCustomerId(member.ParentID, clientPrefix);

                        CaseStudioHelper.HydrateNode(member); 
                        hierarchy.Add(member);
                        entitiesToProcess.Enqueue(member);
                    }
                }
            }

            // Ensure main search entity is always in the processing queue
            if (!hierarchy.Any(h => h.CustomerId == mainEntity.CustomerId))
            {
                // Handle legacy/numeric links for main entity
                mainEntity.CompanyCode = CaseStudioHelper.NormalizeCustomerId(mainEntity.CompanyCode, clientPrefix);
                mainEntity.ParentID = CaseStudioHelper.NormalizeCustomerId(mainEntity.ParentID, clientPrefix);

                CaseStudioHelper.HydrateNode(mainEntity);
                hierarchy.Add(mainEntity);
                entitiesToProcess.Enqueue(mainEntity);
            }

            // Phase 2: Recursive Fetch (All levels)
            var processedIds = new HashSet<string>();
            while (entitiesToProcess.Any())
            {
                var current = entitiesToProcess.Dequeue();
                if (processedIds.Contains(current.CustomerId)) continue;
                processedIds.Add(current.CustomerId);

                // 1. Fetch by CompanyCode (Corporate Shareholders)
                var shareholders = _customerCaseService.GetShareHoldersByCompanyCode(current.CustomerId);
                if (shareholders != null)
                {
                    foreach (var s in shareholders)
                    {
                        // Normalize numeric IDs in shareholder results
                        s.CompanyCode = CaseStudioHelper.NormalizeCustomerId(s.CompanyCode, clientPrefix);
                        s.ParentID = CaseStudioHelper.NormalizeCustomerId(s.ParentID, clientPrefix);

                        CaseStudioHelper.HydrateNode(s);
                        if (!hierarchy.Any(h => h.CustomerId == s.CustomerId))
                        {
                            hierarchy.Add(s);
                            entitiesToProcess.Enqueue(s);
                        }
                    }
                }

                // 2. Fetch by ParentID (Related Individuals/Nested)
                var children = _customerCaseService.GetShareHoldersByParentCode(current.CustomerId);
                if (children != null)
                {
                    foreach (var c in children)
                    {
                        // Normalize numeric IDs in children results
                        c.CompanyCode = CaseStudioHelper.NormalizeCustomerId(c.CompanyCode, clientPrefix);
                        c.ParentID = CaseStudioHelper.NormalizeCustomerId(c.ParentID, clientPrefix);

                        CaseStudioHelper.HydrateNode(c);
                        if (!hierarchy.Any(h => h.CustomerId == c.CustomerId))
                        {
                            hierarchy.Add(c);
                            entitiesToProcess.Enqueue(c);
                        }
                    }
                }
            }

            // Phase 3: Final Ordering (Root/Main entity first for canvas stability)
            var sortedHierarchy = hierarchy
                .OrderByDescending(h => h.CustomerId == id || h.CustomerId == mainEntity.CustomerId)
                .ThenBy(h => !string.IsNullOrEmpty(h.CompanyCode) || !string.IsNullOrEmpty(h.ParentID)) // Roots first
                .ToList();

            CaseStudioHelper.CalculateHierarchyMetadata(sortedHierarchy, mainEntity.CustomerId);

            // Fetch attachments for the main entity
            List<CaseDocumentDTO> attachments = new List<CaseDocumentDTO>();
            try {
                attachments = _caseDocumentService.GetCaseDocumentByCaseId(mainEntity.Id);
            } catch (Exception ex) { log.Warn(ex, "Failed to fetch attachments for hierarchy root"); }

            return Ok(new { entities = sortedHierarchy, attachments });
        }


        [HttpGet("/case/studio/fetch-party")]
        public IActionResult FetchParty(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("ID is required");

            var entity = _customerCaseService.GetCaseFullDetailsByCustId(id);
            int finalCaseId = 0;

            if (entity == null)
            {
                // Try fetching by CIF/CaseId if direct ID fails
                if (int.TryParse(id, out finalCaseId))
                {
                    entity = _customerCaseService.GetCaseFullDetailsByCaseId(finalCaseId);
                }
                
                if (entity == null) return NotFound("Entity not found");
            }
            else 
            {
                // Get the actual case ID from the entity if fetched by CustId
                finalCaseId = entity.Id;
            }

            // Fetch attachments
            List<CaseDocumentDTO> attachments = new List<CaseDocumentDTO>();
            if (finalCaseId > 0)
            {
                try {
                    attachments = _caseDocumentService.GetCaseDocumentByCaseId(finalCaseId);
                } catch (Exception ex) { log.Warn(ex, "Failed to fetch attachments for Studio"); }
            }

            // Standardize entity hydration
            CaseStudioHelper.HydrateNode(entity);
            CaseStudioHelper.HydrateGroupMetadata(entity, entity.GroupId, entity.GroupRisk, entity.GroupEntityof);

            return Ok(new { entity, attachments });
        }

        [HttpPost("/case/custompagination")]
        //ToDo
        public JsonResult CustomPagination(DataTableModel model, string startDate, string endDate, string cust_type,string searchValue,int createdBy,string matchScore,int caseStatus,string caseStatusChange,string riskLevel, string includingDuplicate)
        {
            
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var clientId = _clientHandler.GetClientId();
            
            
             var userId = _clientHandler.GetUserId();
            

            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            List<CaseModel> abc = new List<CaseModel>();
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if(cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
            }
            
                if(riskLevel == "1")
                {
                    riskLevel = "Low Risk";
                }else if (riskLevel == "2")
                {
                    riskLevel = "Medium Risk";
                }else if(riskLevel == "3")
                {
                    riskLevel = "High Risk";
                }
                else if (riskLevel == "4")
                {
                    riskLevel = "Unclassified";
                }
            
            if (caseStatusChange == "0")
            {
                caseStatusChange = null;
            }
            
            if (searchValue != "" && searchValue != null)
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _UserGroupModel.Name, clientId,includingDuplicate));

            }
            else
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore,createdBy,caseStatus,riskLevel, _UserGroupModel.Name,clientId,includingDuplicate));

            }
                int totalcount = abc.Count;
            //if (!string.IsNullOrEmpty(model.search.value))
            //{
            //    var words = model.search.value.Trim().Split(' ');
            //    foreach (var item in words)
            //    {
            //        abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
            //    || m.FirstName.ToLower().Contains(item.ToLower())
            //    || m.LastName.ToLower().Contains(item.ToLower())
            //    || m.MiddleName.ToLower().Contains(item.ToLower())
            //    || m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
            //    ).ToList();
            //    }
            //    //abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
            //    //|| m.FirstName.ToLower().Contains(model.search.value.ToLower())
            //    //|| m.LastName.ToLower().Contains(model.search.value.ToLower())
            //    //|| m.MiddleName.ToLower().Contains(model.search.value.ToLower()) 
            //    //|| m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
            //    //).ToList();
            //}
            int filteredcount = abc.Count;
            //var data = abc.Skip(model.start).Take(model.length).ToList();

            var sortColumn = model?.order?.Any() == true
    ? model.columns[model.order[0].column].data ?? "updatedOnSortKey"
    : "updatedOnSortKey";

            var sortDir = model?.order?.Any() == true
                ? model.order[0].dir ?? "desc"
                : "desc";

            var data = SortData(abc, sortColumn, sortDir)
                        .Skip(model.start)
                        .Take(model.length)
                        .ToList();

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


        public List<T> Sort<T>(List<T> input, string property, string dir)
        {

            var type = typeof(T);

            var sortProperty = type.GetProperty(property, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

            return dir == "asc"

                ? input.OrderBy(p => sortProperty?.GetValue(p, null)).ToList()

                : input.OrderByDescending(p => sortProperty?.GetValue(p, null)).ToList();

        }


        [HttpGet("/case/create")]
        public ActionResult Create()
        {
            CaseModel model = new CaseModel();
            model.CreatedBy = _clientHandler.GetUserId();
            var CustomerType = "I";
            var clientId = _clientHandler.GetClientId();
            model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId)), "Name", "Name");
            model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId)), "Code", "Name");
            model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.TypeId = CustomerType;
            model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(clientId)), "ccName", "ccName");
            model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Customer Risk");
            model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Product Risk");
            model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Delivery Channel Risk");
            model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, clientId, "Mode of Payment");
            model.ProfessionalList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ResidentialStatusList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_residence_status(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, clientId)), "DeliveryChannelName", "DeliveryChannelName");
            model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, clientId)), "ProductName", "ProductName");
            model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, clientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
            //for (int i = 0; i < model.CodesTable.Count; i++)
            //{
            //    var val = model.CodesTable[i].ccName;
            //    model.CodeNames.Add(val);
            //    model.IsChecked.Add(false);
            //}
            model.ClientId = _clientHandler.GetClientId();
            model.itemId = (int)ItemType.clientcase;
            model.branchId = _clientHandler.GetBranchId();

            if (TempData.TryGetValue("UploadedExcelData", out var uploadedData) && uploadedData != null)
            {
                // Deserialize Excel data
                model.ExcelUploadedData = JsonConvert.DeserializeObject<List<CustomerExcelData>>(uploadedData.ToString());

                // Only show modal if there is actual data
                if (model.ExcelUploadedData.Any())
                {
                    model.ShowExcelUploadModal = true;
                }

                // No need to keep TempData unless you plan to use it again
            }
            model.IsCaseCreated = TempData["IsCaseCreated"] != null && (bool)TempData["IsCaseCreated"];
            model.CaseRefId = TempData["CaseRefId"]?.ToString();

            return View(model);
        }

        [HttpPost]
        public JsonResult AutoCompleteCustomer(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return Json(new List<object>());
            var customers = _customerCaseService.GetCustomerMasterByCodePrefix(prefix);
            if (customers == null) return Json(new List<object>());
            
            var query = prefix.Trim().ToLower();
            
            // Prioritize Exact Matches (by CIF, CustomerId or Ref)
            var exactMatches = customers.Where(c => 
                (c.CIFNumber != null && c.CIFNumber.ToLower() == query) || 
                (c.CustomerId != null && c.CustomerId.ToLower() == query) ||
                (c.CustomerReferenceID != null && c.CustomerReferenceID.ToLower() == query)
            ).ToList();

            // Then add recent matches to fill up to 3 results
            var recentMatches = customers
                .Where(c => !exactMatches.Any(e => e.Id == c.Id))
                .OrderByDescending(c => c.Id)
                .Take(Math.Max(0, 3 - exactMatches.Count))
                .ToList();

            var finalResults = exactMatches.Concat(recentMatches).Take(3);
            return Json(finalResults);
        }

        private async void SendScreendedMailAsync(string body, CaseModel model, CustomerCaseDTO x)
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
                    cell1.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell1.FixedHeight = 30f;
                    table.AddCell(cell1);
                    PdfPCell cell2 = new PdfPCell(new Phrase("ID", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell2.HorizontalAlignment = 1;
                    cell2.VerticalAlignment = 1;
                    cell2.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell2.FixedHeight = 30f;
                    table.AddCell(cell2);
                    PdfPCell cell3 = new PdfPCell(new Phrase("TYPE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell3.HorizontalAlignment = 1;
                    cell3.VerticalAlignment = 1;
                    cell3.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell3.FixedHeight = 30f;
                    table.AddCell(cell3);
                    PdfPCell cell4 = new PdfPCell(new Phrase("CATEGORY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell4.HorizontalAlignment = 1;
                    cell4.VerticalAlignment = 1;
                    cell4.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell4.FixedHeight = 30f;
                    table.AddCell(cell4);
                    PdfPCell cell5 = new PdfPCell(new Phrase("NAME", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell5.HorizontalAlignment = 1;
                    cell5.VerticalAlignment = 1;
                    cell5.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell5.FixedHeight = 30f;
                    table.AddCell(cell5);
                    PdfPCell cell6 = new PdfPCell(new Phrase("SCORE", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell6.HorizontalAlignment = 1;
                    cell6.VerticalAlignment = 1;
                    cell6.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell6.FixedHeight = 30f;
                    table.AddCell(cell6);
                    PdfPCell cell7 = new PdfPCell(new Phrase("NATIONALITY", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell7.HorizontalAlignment = 1;
                    cell7.VerticalAlignment = 1;
                    cell7.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
                    cell7.FixedHeight = 30f;
                    table.AddCell(cell7);
                    PdfPCell cell8 = new PdfPCell(new Phrase("DOB", new Font(Font.FontFamily.TIMES_ROMAN, 11, Font.NORMAL)));
                    cell8.HorizontalAlignment = 1;
                    cell8.VerticalAlignment = 1;
                    cell8.BackgroundColor = new BaseColor(System.Drawing.Color.LightGray);
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
                error.description = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                error.module = "Sending_email";
                error.comments = "Sending email Error";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                //Console.Error.WriteLine(ex);


            }
        }

        [HttpPost("/case/create")]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.RateLimiting.EnableRateLimiting("screening-tight")]
        public async Task<ActionResult> Create(CaseModel model)
        {
            // A4: enforce tenant isolation — ClientId always comes from session, never the form
            model.ClientId = _clientHandler.GetClientId();

            // A6: basic input validation on the customer name fields
            if (!AML.Web.Helper.InputGuards.IsSafeName(model.FirstName) ||
                !AML.Web.Helper.InputGuards.IsSafeName(model.MiddleName) ||
                !AML.Web.Helper.InputGuards.IsSafeName(model.LastName))
            {
                _toastNotification.AddErrorToastMessage("Customer name contains characters that aren't allowed.");
                return View(model);
            }

            // R4: short-window double-submit suppression
            var idemKey = $"caseCreate:{model.ClientId}:{(model.PassportId ?? model.CustomerIdNumber ?? "")}:{(model.FirstName ?? "")}:{(model.LastName ?? "")}:{(model.DOB?.ToString() ?? "")}";
            if (AML.Web.Helper.CaseCreationDedup.TryRegister(idemKey))
            {
                _toastNotification.AddInfoToastMessage("Duplicate submission suppressed (the same case was just created).");
                return View(model);
            }

            try
            {
                model.Nationalities = new SelectList(_mapper.Map<List<CountryModel>>(_countryService.GetAll(model.ClientId)), "Name", "Name");
                model.IdTypes = new SelectList(_mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(model.ClientId)), "Code", "Name");
                model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
                model.IsDelete = 0;
                var CustomerType = "I";
                model.CreatedBy = _clientHandler.GetUserId();
                model.CustomerId = "0";
                model.CodesTable = new SelectList(_mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(model.ClientId)), "ccName", "ccName");
                model.CustomerRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Customer Risk");
                model.ProductRiskCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Product Risk");
                model.DeliveryChannelCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Delivery Channel Risk");
                model.ModeOfPaymentCategory = _customerCaseService.GetRiskCategoryStatus(CustomerType, model.ClientId, "Mode of Payment");
                model.ProfessionalList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetProfessionalStatus(culture, CustomerType, model.ClientId)), "DeliveryChannelName", "DeliveryChannelName");
                model.ResidentialStatusList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_residence_status(culture, model.ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
                model.DeliveryChannelList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.GetAllDeliveryChannel(culture, CustomerType, model.ClientId)), "DeliveryChannelName", "DeliveryChannelName");
                model.ProductTypeList = new SelectList(_mapper.Map<List<ProductType>>(_kycService.GetAllProduct(culture, CustomerType, model.ClientId)), "ProductName", "ProductName");
                model.ModeofpaymentList = new SelectList(_mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(_kycService.get_all_mode_of_payment(culture, model.ClientId, CustomerType)), "DeliveryChannelName", "DeliveryChannelName");
                //model.CodesTable = _mapper.Map<List<CodesTableModel>>(_customerCaseService.GetCodesByClientID(model.ClientId));

                string CallC6Screening = string.Empty;

                CallC6Screening = _configuration["CallC6Screening"];
                checkThreshold = model.Threshold;

                TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                if (token.status == 400)
                {
                    _toastNotification.AddErrorToastMessage("User is not authorized for screening");
                }
                else
                {
                    //if (ModelState.IsValid)
                    //{
                        try
                        {
                        List<string> selectedScreeningOptions = new List<string>();
                        model.CustomerType = "I";
                        model.MatchCategory = "INDIVIDUAL";
                        model.Type = "Individual";
                        model.Version = 1;
                        CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(model);
                        bool IsSanction = false;
                                    int count = 0;
                                    for (int i = 0; i < model.CodeNames.Count; i++)
                                    {
                                        if (model.IsChecked[i] == true)
                                        {
                                            count++;
                                            selectedScreeningOptions.Add(model.CodeNames[i]);
                                            if (model.CodeNames[i] == "Sanction")
                                            {
                                                log.Debug("Only sanction was true");
                                                IsSanction = true;
                                            }
                                        }
                                    }

                                    for (int i = 0; i < model.CodeNames.Count; i++)
                                    {
                                        if (model.CodeNames[i] == "PEP" && model.IsChecked[i] == true) { _ccDTO.IsPep = true; continue; }
                                        if (model.CodeNames[i] == "Sanction" && model.IsChecked[i] == true) { _ccDTO.IsSan = true; continue; }
                                        if (model.CodeNames[i] == "Reputational Risk Exposure" && model.IsChecked[i] == true) { _ccDTO.IsRre = true; continue; }
                                        if (model.CodeNames[i] == "Insolvency (UK & Ireland)" && model.IsChecked[i] == true) { _ccDTO.IsIns = true; continue; }
                                        if (model.CodeNames[i] == "Disqualified Director (UK Only)" && model.IsChecked[i] == true) { _ccDTO.IsDd = true; continue; }
                                        if (model.CodeNames[i] == "Profile of Interest" && model.IsChecked[i] == true) { _ccDTO.IsPoi = true; continue; }
                                        if (model.CodeNames[i] == "Regulatory Enforcement List" && model.IsChecked[i] == true) { _ccDTO.IsRel = true; continue; }
                                    }

                        //var customerCodeprefix = _mapper.Map<ClientMasterDTO>(_customerCaseService.GetCustomerCodeprefixByclient(model.ClientId));
                        //_ccDTO.customerCodeprefix = customerCodeprefix.Prefix;
                        //var result = _customerCaseService.CreatePrefix(_ccDTO);
                        _ccDTO.ScreeningOptions = string.Join(", ", selectedScreeningOptions);
                        
                        var result = _customerCaseService.Create(_ccDTO);
                        string parsedCustId = result.Result;
                        if (!string.IsNullOrEmpty(parsedCustId))
                        {
                            if (parsedCustId.Contains("Ø")) parsedCustId = parsedCustId.Split('Ø')[1];
                            else if (parsedCustId.Contains("??")) parsedCustId = parsedCustId.Split(new string[] { "??" }, StringSplitOptions.None)[1];
                            else if (parsedCustId.Contains("A~")) parsedCustId = parsedCustId.Split(new string[] { "A~" }, StringSplitOptions.None)[1];
                        }
                        _ccDTO.CustomerId = parsedCustId;

                            Console.WriteLine(result.Result);





                            //Document upload 
                            CaseDocumentModel _caseDoc = new CaseDocumentModel();
                            _caseDoc.CaseId = _customerCaseService.GetCaseId(_ccDTO.CustomerId).ToString();
                            if (model.CaseDocumentsL != null)
                            {
                                foreach (var item in model.CaseDocumentsL)
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
                                        _caseDoc.CustomerId = _ccDTO.CustomerId;
                                        var resp = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(_caseDoc));
                                    }
                                }
                            }
                            
                            CaseCommentModel remarkModel = new CaseCommentModel();
                            remarkModel.CaseId = Convert.ToInt32(_caseDoc.CaseId);
                            remarkModel.Comment = "Case is Created"; // ✅ FIXED
                            remarkModel.CommentType = "Individual Screening";
                            remarkModel.CreatedBy = _clientHandler.GetUserId();
                            remarkModel.CustomerId = _ccDTO.CustomerId;
                            var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));

                            //foreach (var item in model.CodesTable)
                            //{
                            //    model.CodeNames.Add(item.Value);
                            //}
                            //for (int i = 0; i < model.CodesTable.Count; i++)
                            //{
                            //    var val = model.CodesTable[i].ccName;
                            //    model.CodeNames.Add(val);
                            //}
                            


                            if (count == 1 && IsSanction == true && CallC6Screening == "N")
                            {
                                log.Debug("Only Sanction Screening searching from mongo db(internal watchlist)");
                                string data = "";
                                try
                                {
                                    string response = string.Empty;
                                    var searchType = "F";
                                    data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
                                    {
                                        customerdob = model.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                                        customerfullname = string.Concat(_ccDTO.FirstName, " ", _ccDTO.MiddleName, " ", _ccDTO.LastName),
                                        customernationality = model.Nationality,
                                        searchtype = searchType
                                    }, ScreeningService.BACKLIST_SCREENING).Result;

                                    log.Debug(data);

                                }
                                catch (Exception ex)
                                {
                                    log.Debug(ex);
                                }


                                if (!string.IsNullOrEmpty(data))
                                {
                                    List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                                    //response = AMLUtility.FormatJsonToPlainText(data);
                                    log.Debug("$got apiresults:", apiResultModel.Count());

                                    log.Debug("After checking from interanl watch list");

                                    UpdateSanctionRecords(_ccDTO, apiResultModel, parsedCustId);

                                    string body = string.Empty;
                                    using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                                    {
                                        body = reader.ReadToEnd();
                                    }
                                    ;

                                    //var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, result.Result);

                                    if (_ccDTO.sendMail == 1)
                                    {
                                        await Task.Run(() => SendScreendedMailAsync(body, model, _ccDTO));
                                    }

                                    if (_ccDTO.IsMatched == 1 && _ccDTO.MatchScore >= checkThreshold)
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        //_toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                                    }
                                    else if (_ccDTO.IsMatched == 0 && _ccDTO.MatchScore == 0)
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        model.IsMatched = 2;
                                        var appBaseUrl = MyHttpContext.AppBaseUrl;
                                        // body = body.Replace("{baseUrl}", appBaseUrl);
                                        //model.Url = appBaseUrl;
                                        //model.createdUserName = HttpContext.Session.GetString("SessUsername");
                                        //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));
                                        //return View(model);
                                    }
                                    else if (_ccDTO.MatchScore >= 0 && _ccDTO.MatchScore < checkThreshold)
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        _ccDTO.ApiResultsjson = _ccDTO.ApiResultsjson.Where((source, index) => index < 100).ToList();
                                        model.ApiResultJson = _ccDTO.ApiResultsjson;
                                        //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", _ccDTO.FirstName, " ", _ccDTO.LastName, ". \n"));

                                        //return View(model);
                                    }
                                    else
                                    {
                                        model.CaseRefId = _ccDTO.CustomerId;
                                        model.IsCaseCreated = true;
                                        //_toastNotification.AddSuccessToastMessage("Customer Approved");
                                    }//      _toastNotification.AddWarningToastMessage("Customer blocked!");
                                }
                                else
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    //_toastNotification.AddWarningToastMessage("Customer blocked! ");
                                }

                                
                            }
                            else if (result.Status == StaticResource.SuccessStatusCode)
                            {
                                if (CallC6Screening == "Y")
                                {
                                    {
                                        _ccDTO.IsPep = true;
                                        _ccDTO.IsSan = true;
                                    }
                                }
                                string body = string.Empty;
                                using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                                {
                                    body = reader.ReadToEnd();
                                }
                                ;

                                var x = await _commonService.CustomerScreeningCall(_ccDTO, baseURL, baseC6URL, "INDIVIDUAL", body, model.Threshold, parsedCustId);

                                if (x.sendMail == 1)
                                {
                                    await Task.Run(() => SendScreendedMailAsync(body, model, x));
                                }
                                if (x.IsMatched == 1 && x.MatchScore >= checkThreshold)
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                   // _toastNotification.AddWarningToastMessage("Customer blocked, Case created");
                                }
                                else if (x.IsMatched == 0 && x.MatchScore == 0)
                                {

                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    model.IsMatched = 2;
                                    var appBaseUrl = MyHttpContext.AppBaseUrl;
                                    body = body.Replace("{baseUrl}", appBaseUrl);
                                    model.Url = appBaseUrl;
                                    model.createdUserName = HttpContext.Session.GetString("SessUsername");
                                    //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as there was no match found for ", x.FirstName, " ", x.LastName, ". \n"));
                                    
                                }
                                else if (x.MatchScore >= 0 && x.MatchScore < checkThreshold)
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    x.ApiResultsjson = x.ApiResultsjson.Where((source, index) => index < 100).ToList();
                                    model.ApiResultJson = x.ApiResultsjson;
                                    //_toastNotification.AddInfoToastMessage(string.Concat("Customer Approved as the matchscore is less than threshold for ", x.FirstName, " ", x.LastName, ". \n"));

                                    
                                }
                                else
                                {
                                    model.CaseRefId = _ccDTO.CustomerId;
                                    model.IsCaseCreated = true;
                                    //_toastNotification.AddSuccessToastMessage("Customer Approved");
                                }
                            }
                            else
                            {
                                _toastNotification.AddErrorToastMessage("Customer creation failed");
                                return View(model);
                            }
                            CorporateKycDTO corpModel = new CorporateKycDTO();
                           
                            //To check if risk assessment is enabled for the client.
                            //var results = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(model.ClientId));
                            //if (results != null)
                            //{
                                var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, model.ClientId);
                                if (str1.Result == null)
                                {
                                    _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                    return View(model);
                                }
                                // The SP returns 36 Ø-delimited tokens. Earlier versions of the codebase
                                // hit IndexOutOfRange whenever the connection charset was mis-configured
                                // (Ø became Ã˜) - guard against any truncated/missing tokens with a helper
                                // so this never panics again.
                                var spStr1 = str1.Result.Split('Ø');
                                if (spStr1.Length < 16)
                                {
                                    log.Warn($"GetRiskLovId returned {spStr1.Length} tokens (expected 36). Raw='{str1.Result}'");
                                    _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                    return View(model);
                                }
                                string TokenAt(string[] arr, int idx) => (idx >= 0 && idx < arr.Length) ? arr[idx] : "0";
                                var proflovId          = TokenAt(spStr1, 0);
                                var natlovId           = TokenAt(spStr1, 1);
                                var reslovId           = TokenAt(spStr1, 5);
                                var IndprodlovId       = TokenAt(spStr1, 13);
                                var InddelilovId       = TokenAt(spStr1, 14);
                                var IndmodeofpaymentlovId = TokenAt(spStr1, 15);
                                var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(model), corpModel, "I", culture, model.ClientId);
                                if (str.Result == null)
                                {
                                    _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                    return View(model);
                                }
                                var spStr = str.Result.Split('Ø');
                                if (spStr.Length < 16)
                                {
                                    log.Warn($"GetRiskTypeId returned {spStr.Length} tokens (expected 36). Raw='{str.Result}'");
                                    _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");
                                    return View(model);
                                }
                                var profId             = TokenAt(spStr, 0);
                                var natId              = TokenAt(spStr, 1);
                                var resId              = TokenAt(spStr, 5);
                                var IndprodId          = TokenAt(spStr, 13);
                                var InddeliId          = TokenAt(spStr, 14);
                                var Indmodeofpaymentid = TokenAt(spStr, 15);
                                RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                                riskModel.CustomerId = _ccDTO.CustomerId;
                                riskModel.CustomerName = model.FirstName;
                                riskModel.CaseVersion = model.Version;

                                riskModel.ClientId = _clientHandler.GetClientId();
                                riskModel.CreatedBy = _clientHandler.GetUserId();
                                if (model.Nationality == "0")
                                {
                                    riskModel.MainNationality = "";
                                }
                                else
                                {
                                    riskModel.MainNationality = model.Nationality;
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
                                //if (IspepId != "0")
                                //{
                                //    var riskType1 = new RiskTypeListModel();
                                //    riskType1.Id = Convert.ToString(IspeplovId);
                                //    var riskItem1 = new RiskItemListModel();
                                //    riskItem1.Id = IspepId.ToString();
                                //    var riskItemList1 = new List<RiskItemListModel>();
                                //    riskItemList1.Add(riskItem1);
                                //    riskType1.RiskItemList = riskItemList1;
                                //    riskTypeList.Add(riskType1);
                                //}
                                //for pep end
                                if (Indmodeofpaymentid != "0")
                                {
                                    var riskType8 = new RiskTypeListModel();
                                    riskType8.Id = Convert.ToString(IndmodeofpaymentlovId);
                                    var riskItem8 = new RiskItemListModel();
                                    riskItem8.Id = Indmodeofpaymentid.ToString();//Convert.ToString(1);
                                    var riskItemList8 = new List<RiskItemListModel>();
                                    riskItemList8.Add(riskItem8);
                                    riskType8.RiskItemList = riskItemList8;
                                    riskTypeList.Add(riskType8);
                                }


                                riskModel.RiskTypeList = riskTypeList;
                                var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                                var xyz = riskResult;

                                Console.WriteLine($"Risk assessment result: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");
                                return View(model);
                            //}
                            //return RedirectToAction("Create");
                        }
                        catch (Exception ex)
                        {
                            //var error = ex.Message;
                            log.Debug(ex.Message);
                            ErrorLogDTO error = new ErrorLogDTO();
                            error.created_on = DateTime.Now;
                            error.createdBy = _clientHandler.GetUserId();
                            error.description = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                            error.module = "Screening_I";
                            error.comments = "API call while screening";
                            error.status_code = 404;

                            var result = _commonService.createErrorlog(error);

                            _toastNotification.AddErrorToastMessage(error.description);

                            Console.Error.WriteLine(ex);

                            return RedirectToAction("Create");


                        }
                        return RedirectToAction("Create");
                    }
                //}
            }
            catch (Exception ex)
            {
                log.Debug(ex.Message);

                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                error.module = "Screening_I";
                error.comments = "API call Error while creating token";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Create");
            }
            finally
            {
                CaseCreationDedup.Release(idemKey);
            }
            return View(model);
        }

        private void UpdateSanctionRecords(CustomerCaseDTO _CustomerCaseDTO, List<ApiResultModel> apiResp, string cid)
        {


            string id;
            if (_CustomerCaseDTO.Id == 0 && cid != "")
            {
                id = cid;
            }
            else
            {
                id = _CustomerCaseDTO.Id.ToString();
            }

            _CustomerCaseDTO.Id = _customerCaseService.GetCaseId(id);
            bool isRecordCreated = false;
            (bool exists, List<NAMELIST> response, List<NAMELIST> response2) IsBlackListed = _freeSourceRepository.SearchNameList(_CustomerCaseDTO.FirstName, false, _CustomerCaseDTO.ClientId);

            _CustomerCaseDTO.Status = 5;

            if (true) { _CustomerCaseDTO.IsMatched = ((apiResp.IsNotNullOrEmpty() && IsBlackListed.exists) || apiResp.Count > 0) ? 1 : 0; }
            if (_CustomerCaseDTO.IsWhiteListed == "YES") { _CustomerCaseDTO.Source = "WHITELIST"; }



            if (true)
            {

                _CustomerCaseDTO.MatchScore = IsBlackListed.exists ? 100 : Convert.ToInt32(apiResp.FirstOrDefault().matchscore);
                _CustomerCaseDTO.SourceUniqueId = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : Convert.ToString(apiResp.FirstOrDefault().matchuid);
                _CustomerCaseDTO.Status = _CustomerCaseDTO.MatchScore.IsNotNullOrEmpty() ? (_CustomerCaseDTO.MatchScore < checkThreshold ? 5 : 0) : 5;
                _CustomerCaseDTO.Source = IsBlackListed.exists ? IsBlackListed.response[0].TYPE : apiResp.FirstOrDefault().matchtype;

                _CustomerCaseDTO.ApiResultsjson = apiResp;

                var matchrecordsList = new List<MatchRecordsDTO>();
                var stat = 0;
                if (!IsBlackListed.exists)
                {
                    log.Debug($"Get C6 results below threshold ({checkThreshold}) for CaseLogUnderThreshold");

                    foreach (var item in apiResp)
                    {

                        var matchrecords = new MatchRecordsDTO();
                        if (item.IsNotNullOrEmpty())
                        {
                            matchrecords.MATCHUID = item.matchuid.ToString();
                            matchrecords.MATCHTYPE = item.matchtype;// "KYC6";
                            matchrecords.MATCHCATEGORY = item.matchcategory;
                            matchrecords.MATCHNAME = item.matchname.ToUpper();
                            matchrecords.MATCHSCORE = Convert.ToInt32(item.matchscore);
                            //matchrecords.MATCHNATIONALITY = item.nationality.IsNotNullOrEmpty() ? item.nationality.FirstOrDefault().ToString() : string.Empty;
                            matchrecords.MATCHNATIONALITY = item.nationality.IsNotNullOrEmpty() ? item.nationality.ToString() : string.Empty;
                            matchrecords.MATCHIDNO = item.matchidnumber != null ? item.matchidnumber.ToString() : String.Empty;
                            //matchrecords.MATCHDOB = item.matchdob != null ? item.matchdob.FirstOrDefault().ToString() : "";
                            matchrecords.MATCHDOB = item.matchdob != null ? item.matchdob.ToString() : "";

                            if (Convert.ToInt32(item.matchscore) >= checkThreshold)
                            {
                                matchrecordsList.Add(matchrecords);
                                stat++;
                            }

                            //if (Convert.ToInt32(item.matchscore) < checkThreshold)
                            //{
                            //    matchrecordsList.Add(matchrecords);
                            //    stat = 1;
                            //}
                            //else
                            //{
                            //    stat = 0;
                            //}
                        }
                    }

                    log.Debug($"Got {matchrecordsList.Count} results");
                }
                else
                {
                    stat = 0;
                }

                if (stat >= 1)
                {
                    CASELOG modelCaseLog = new CASELOG
                    {
                        CASEID = _CustomerCaseDTO.Id.ToString(),
                        MATCHRECORDS = matchrecordsList
                    };

                    log.Debug("Add case log under threshold to MongoDB");
                    _freeSourceRepository.InsertCaseLog(modelCaseLog);
                }
                try
                {

                    _CustomerCaseDTO.sendMail = 1;
                    //var emailSent = await SendHtmlFormattedEmail("Risk creation Alert", emailBody);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                    //throw ex;
                }
            }

            log.Debug("Update customer case table with details after screening");

            _customerCaseService.Update(_CustomerCaseDTO);
            /*if(_CustomerCaseDTO.ApiResultsjson.Count>0)
            {
                isRecordCreated = true;
              
            }
            return isRecordCreated;*/
            //throw new NotImplementedException();
        }

        // Rebuild trigger: force watch rebuild and reload DTO column mappings
        [HttpGet("/case/Process/{CaseId}")]
        [AML.Web.CustomFilters.TenantOwned(AML.Web.CustomFilters.TenantResource.Case, "CaseId")]
        public async Task<ActionResult> Process(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
               
                model.Case = new CaseModel();

                

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
                if (_CustomerCaseDTO == null)
                {
                    return RedirectToAction("Index");
                }
                HttpContext.Session.SetString("CorporateId", _CustomerCaseDTO.CustomerId);
                HttpContext.Session.SetString("CorporateCaseId", CaseId.ToString());

                HttpContext.Session.SetString("ReturnUrl", HttpContext.Request.Path + HttpContext.Request.QueryString);

                var dualMatchStatus = _customerCaseService.GetDualGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.DualGoodsMatchStatus = dualMatchStatus;
                var militaryMatchStatus = _customerCaseService.GetMilitaryGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.MilitaryGoodsMatchStatus = militaryMatchStatus;
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel?.Name ?? "System";
                if (model.Case.UserGroupName.Contains("Compliance") && _CustomerCaseDTO.Status == 4) { model.IsReadOnly = true; ViewBag.HideChatbot = true; }
                var dob = model.Case.DOB;
                var createddated = model.Case.CreatedOn;
                string dobText;

                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM" || dob == "1/1/1900 12:00:00 AM")
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
                string RiskactionName;
                string CreatecontrollerName;
                if (model.Case.CustomerType == "I")
                {
                     RiskactionName = "risk";
                     CreatecontrollerName = "Create";
                }
                else
                {
                     RiskactionName = "risk";
                     CreatecontrollerName = "RiskAssessmentForCorpCustomer";
                }
                string sessionId = this.HttpContext.Session.GetString("SessID");
                var riskCreation = _userGroupRightService.CheckUserRightExixts(RiskactionName, CreatecontrollerName, userId, GroupId, sessionId);
                model.RiskCreation = riskCreation.Result;
                var actionRights = new Dictionary<string, string>();
                var actionsToCheck = new List<string>
                {
                    "Approve",
                    "Reject",
                    "Senior Management",
                    "On Hold",
                    "Whitelist"
                    //"SaveSearchResult"
                };
                foreach (var action in actionsToCheck)
                {
                    // Call the service to check if user has the right
                    var serviceResponse = _userGroupRightService.CheckNameuserrightExists(
                        "case",
                        "close",// Controller
                        userId,
                        GroupId,
                        sessionId ,
                        action// Action to check
                    );

                    // If user has the right, store the actual action name, else "1"
                    actionRights[action] = (serviceResponse?.Result ?? false).ToString().ToLower();
                }

                model.ActionRights = actionRights;

                

           
                var CommentCase = _userGroupRightService.CheckUserRightExixts("case","comment", userId, GroupId, sessionId);
                model.CommentCase = CommentCase.Result;

                var DocumentCase = _userGroupRightService.CheckUserRightExixts("case","document",  userId, GroupId, sessionId);
                model.DocumentsCase = DocumentCase.Result;

                var TransferCase = _userGroupRightService.CheckUserRightExixts("case","assign", userId, GroupId, sessionId);
                model.TransferCase = TransferCase.Result;

                var savesearch = _userGroupRightService.CheckUserRightExixts("case", "saveRemark", userId, GroupId, sessionId);
                model.SaveSearResult = savesearch.Result;

                List<CaseDocumentModel> caseDocumentbyId = _mapper.Map<List<CaseDocumentModel>>(_caseDocumentService.GetCaseDocumentByCaseId(CaseId));
                model.CaseDocuments = caseDocumentbyId;

                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);

                // Group Entity Visibility logic: Aggregate group members and their related parties
                if (!string.IsNullOrEmpty(_CustomerCaseDTO.GroupId))
                {
                    var groupMembers = _customerCaseService.GetCasesByGroupId(_CustomerCaseDTO.GroupId);
                    bool anyUnclassified = false;
                    List<string> riskLevels = new List<string>();

                    if (groupMembers != null)
                    {
                        foreach (var member in groupMembers)
                        {
                        // Check if member risk is unclassified
                        string memberRisk = !string.IsNullOrEmpty(member.Individual_final_risk_score) 
                                            ? member.Individual_final_risk_score 
                                            : member.corporate_final_risk_score;

                        if (string.IsNullOrEmpty(memberRisk) || memberRisk.Equals("Unclassified", StringComparison.OrdinalIgnoreCase))
                        {
                            anyUnclassified = true;
                        }
                        else
                        {
                            riskLevels.Add(memberRisk);
                        }

                        if (member.CustomerId != _CustomerCaseDTO.CustomerId)
                        {
                            // Add the group member itself as a root-level peer in the related parties list
                            if (!_CustomerCaseshareholders.Any(s => s.CustomerId == member.CustomerId))
                            {
                                _CustomerCaseshareholders.Add(member);
                            }

                                // Add related parties of this group member
                                var targetCompanyCode = !string.IsNullOrEmpty(member.CompanyCode) ? member.CompanyCode : member.CustomerId;
                                var memberRelatedParties = _customerCaseService.GetShareHoldersByCompanyCode(targetCompanyCode);
                                if (memberRelatedParties != null)
                                {
                                    foreach (var rp in memberRelatedParties)
                                    {
                                        if (!_CustomerCaseshareholders.Any(s => s.CustomerId == rp.CustomerId))
                                        {
                                            _CustomerCaseshareholders.Add(rp);
                                        }
                                    }
                                }
                        }
                    }
                }

                    // Calculate Group Risk
                    if (anyUnclassified || riskLevels.Count == 0)
                    {
                        model.Case.GroupRisk = "Unclassified";
                    }
                    else
                    {
                        // Risk Hierarchy: High > Medium > Low (Order can be adjusted based on system definitions)
                        var hierarchy = new List<string> { "High", "Medium", "Low" };
                        model.Case.GroupRisk = riskLevels
                            .OrderBy(r => hierarchy.IndexOf(hierarchy.FirstOrDefault(h => r.Contains(h)) ?? "Low"))
                            .FirstOrDefault() ?? "Unclassified";
                    }
                }

                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                var currentVersion = _CustomerCaseDTO.Version;

                var _CustomerCasesData = _customerCaseService
                    .GetAllVersionCases(_CustomerCaseDTO.CustomerId)
                    .Where(x => x.Version != currentVersion)   // 🔥 exclude current
                    .OrderByDescending(x => x.Version)         // latest first
                    .ToList();

                model.CustomerCaseData = _mapper.Map<List<CaseModel>>(_CustomerCasesData);
                
                

                List<ProliferationFinanceCaseDTO> proliferationData = _customerCaseService.GetProliferationData(_CustomerCaseDTO.CustomerId);
                model.ProliferationFinanceData = _mapper.Map<List<ProliferationFinanceModel>>(proliferationData);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId,_CustomerCaseDTO.CustomerType));
                model.RiskVersionData= _mapper.Map<List<RiskReportModel>>(riskReports);

                //List<PassportDetailsDTO> passportDetails= _customerCaseService.GetPassportdetails(CaseId);
                //model.passportDetails = _mapper.Map<List<PassportDetails>>(passportDetails);
                //model.Case.Id = CaseId;
                //model.DocumentCategories = new SelectList(_mapper.Map<List<DocumentCategoryModel>>(_caseDocumentService.GetAllCategories()), "Id", "Name");
                //model.DocumentTypes = new SelectList(_mapper.Map<List<DocumentTypeModel>>(_caseDocumentService.GetAllTypes()), "Id", "Name");
                var clientId = _clientHandler.GetClientId();
                //IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                //                                       select new SelectListItem
                //                                       {
                //                                           Value = Convert.ToString(s.Id),
                //                                           Text = s.FName + " " + s.LName.ToString()
                //
                //       };

                // Load the latest risk assessment for this customer (mirrors the pattern at line 1628+).
                // Picks up risk records written by Save Search Action, Proliferation Finance,
                // KYC, or any other flow that calls _riskAPIController.KycRiskAssessment(...).
                // Wrapped in try/catch because some legacy risk records have Ø-delimited multi-LOV
                // encoding in lov_type_id that crashes Dapper int deserialization (see RiskController
                // GetRiskDetailsByCIdIndividual for the same issue). On failure we fall back to the
                // empty template so the page still renders with editable dropdowns.
                int riskid = 0;
                try { riskid = _riskService.GetRiskIdByCustomercode(model.Case.CustomerId, model.Case.CustomerType).Result; }
                catch (Exception ex) { Console.WriteLine($"CaseController.Process: GetRiskIdByCustomercode failed for case {CaseId}: {ex.Message}"); }

                bool riskLoaded = false;
                if (riskid != 0)
                {
                    dynamic modelrisk = null;
                    try
                    {

                    if (model.Case.CustomerType == "I")
                    {
                        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);
                        int riskVersion = modelrisk != null ? modelrisk.version : 1;
                        if (riskVersion == 0) riskVersion = 1;

                        model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("I", 1, 0, 0, clientId, riskVersion);
                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        model.MainNationalityTxt = modelrisk.MainNationalityTxt;
                        model.Address = modelrisk.Address;

                        bool hasHighOverride = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                        if (model.FinalRiskScore != null && model.FinalRiskScore.ToLower().Contains("high risk") && hasHighOverride)
                            model.FinalRiskScore = "High Risk (O)";
                    }
                    else
                    {
                        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);
                        int riskVersion = modelrisk != null ? modelrisk.version : 1;
                        if (riskVersion == 0) riskVersion = 1;

                        model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("C", 1, 0, 0, clientId, riskVersion);
                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.FinalRiskScore = modelrisk.RiskAssessmentRating;
                        model.RiskScoreBeforeOverride = modelrisk.RiskAssessmentRatingWithoutOverride;
                        model.RiskAssessmentRating = modelrisk.RiskAssessmentRating;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        model.MainNationalityTxt = modelrisk.CountryOfIncorporationTxt;

                        bool hasHighOverrideCor = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                        if (model.RiskAssessmentRating != null && model.RiskAssessmentRating.ToLower().Contains("high risk") && hasHighOverrideCor)
                        {
                            model.RiskAssessmentRating = "High Risk (O)";
                            model.FinalRiskScore = "High Risk (O)";
                        }
                    }
                    riskLoaded = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Count > 0;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"CaseController.Process: Risk load failed for case {CaseId} (riskid={riskid}): {ex.Message}");
                    }
                }

                if (!riskLoaded)
                {
                    // No risk record yet (or load failed) — fall back to the empty configuration
                    // template so the user still sees the dropdowns and can fill them in manually.
                    model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig(model.Case.CustomerType, 1, 0, 0, clientId);
                }

                for (var i = 0; i < model.RiskTypeCategoryDTO.Count; i++)
                {
                    for (var j = 0; j < model.RiskTypeCategoryDTO[i].RiskTypes.Count; j++)
                    {
                        var items = model.RiskTypeCategoryDTO[i].RiskTypes[j].RiskItems;
                        model.RiskTypeCategoryDTO[i].RiskTypes[j].Items = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_mapper.Map<List<RiskItemsDTO>>(items.ToList()), "Score", "RiskItem");
                    }
                }

                var id = _clientHandler.GetUserId();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                       select new SelectListItem
                                                       {
                                                           Value = Convert.ToString(s.Id),
                                                           Text = s.FName + " " + s.LName.ToString()
                                                       };

                model.Users = new SelectList(userList, "Value", "Text");
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                //log.Debug($"apiresults ({result}) ");
                if (!string.IsNullOrEmpty(result))
                {
                    var token = Newtonsoft.Json.Linq.JToken.Parse(result);
                    if (token is Newtonsoft.Json.Linq.JArray)
                    {
                        var jsonList = token.ToObject<List<DataListModel>>();
                        model.DataList = jsonList ?? new List<DataListModel>();
                        model.WebDataList = new List<WebListModel>();
                    }
                    else
                    {
                        var response = token.ToObject<CaseLogResponseModel>();
                        model.DataList = response?.MatchRecords ?? new List<DataListModel>();
                        model.WebDataList = response?.WebRecords ?? new List<WebListModel>();
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                error.module = "CaseManagement_Process";
                error.comments = "API call Error while Getting data from manogo db";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost("/case/RunComparison/{caseId}")]
        public async Task<JsonResult> RunComparison(int caseId, [FromBody] BulkComparisonRequest request)
        {
            try
            {
                if (request?.Ids == null || request.Ids.Count == 0)
                    return Json(new { success = false, error = "No IDs provided" });

                var _CustomerCaseDTO = _customerCaseService.GetDetails(caseId);
                if (_CustomerCaseDTO == null)
                    return Json(new { success = false, error = "Case not found" });

                TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                if (token == null || token.user == null || string.IsNullOrEmpty(token.user.token))
                    return Json(new { success = false, error = "Unable to authenticate with screening service" });

                bool isIndividual = _CustomerCaseDTO.CustomerType == "I";
                string endpoint = isIndividual
                    ? baseC6URL.TrimEnd('/') + "/" + ScreeningService.C6_PERSONAL_BULKID
                    : baseC6URL.TrimEnd('/') + "/" + ScreeningService.C6_BUSINESS_BULKID;

                log.Info($"RunComparison: caseId={caseId}, isIndividual={isIndividual}, endpoint={endpoint}, ids=[{string.Join(",", request.Ids)}]");

                string responseJson;
                using (var httpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(60) })
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.user.token);

                    var payload = JsonConvert.SerializeObject(new { ids = request.Ids });
                    var content = new System.Net.Http.StringContent(payload, System.Text.Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(endpoint, content);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errBody = await response.Content.ReadAsStringAsync();
                        log.Warn($"RunComparison: C6 API returned {(int)response.StatusCode}: {errBody}");
                        return Json(new { success = false, error = $"API returned {(int)response.StatusCode}: {errBody}" });
                    }

                    responseJson = await response.Content.ReadAsStringAsync();
                    log.Info($"RunComparison: C6 response length={responseJson.Length}");

                    // Extract qrCodes from raw to verify they match the sent IDs
                    try {
                        var parsed = JsonConvert.DeserializeObject<dynamic>(responseJson);
                        var results = parsed?.results;
                        if (results != null) {
                            var qrSample = new System.Text.StringBuilder();
                            int count = 0;
                            foreach (var item in results) {
                                if (count++ >= 5) break;
                                string rawStr = item?.raw?.ToString() ?? "";
                                dynamic rawObj = string.IsNullOrEmpty(rawStr) ? null : JsonConvert.DeserializeObject<dynamic>(rawStr);
                                string qr = rawObj?.qrCode?.ToString() ?? "(null)";
                                string flatId = item?.id?.ToString() ?? "(null)";
                                qrSample.Append($"[{count}: raw.qrCode={qr}, flat.id={flatId}] ");
                            }
                            log.Info($"RunComparison: first 5 entities → {qrSample}");
                        }
                    } catch (Exception logEx) { log.Warn("RunComparison: qrCode sample failed: " + logEx.Message); }
                }

                return Json(new
                {
                    success = true,
                    rawData = responseJson,
                    customerType = isIndividual ? "I" : "C"
                });
            }
            catch (Exception ex)
            {
                log.Error("RunComparison error", ex);
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet("/case/GetGroupRisk/{CaseId}")]
        public JsonResult GetGroupRisk(int CaseId)
        {
            var clientId = _clientHandler.GetClientId();
            var _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
            
            if (_CustomerCaseDTO == null || string.IsNullOrEmpty(_CustomerCaseDTO.GroupId))
            {
                return Json(new { risk = "Unclassified" });
            }

            var groupMembers = _customerCaseService.GetCasesByGroupId(_CustomerCaseDTO.GroupId);
            bool anyUnclassified = false;
            List<string> riskLevels = new List<string>();

            foreach (var member in groupMembers)
            {
                string memberRisk = !string.IsNullOrEmpty(member.Individual_final_risk_score) 
                                    ? member.Individual_final_risk_score 
                                    : member.corporate_final_risk_score;

                if (string.IsNullOrEmpty(memberRisk) || memberRisk.Equals("Unclassified", StringComparison.OrdinalIgnoreCase))
                {
                    anyUnclassified = true;
                }
                else
                {
                    riskLevels.Add(memberRisk);
                }
            }

            string groupRisk = "Unclassified";
            if (!anyUnclassified && riskLevels.Count > 0)
            {
                var hierarchy = new List<string> { "High", "Medium", "Low" };
                groupRisk = riskLevels
                    .OrderBy(r => hierarchy.IndexOf(hierarchy.FirstOrDefault(h => r.Contains(h)) ?? "Low"))
                    .FirstOrDefault() ?? "Unclassified";
            }

            return Json(new { risk = groupRisk });
        }

        [HttpGet("/case/Process_PDF/{CaseId}")]
        public async Task<ActionResult> Process_PDF(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
               
                model.Case = new CaseModel();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);

                var dualMatchStatus = _customerCaseService.GetDualGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.DualGoodsMatchStatus = dualMatchStatus;
                var militaryMatchStatus = _customerCaseService.GetMilitaryGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.MilitaryGoodsMatchStatus = militaryMatchStatus;
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;
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
                
                string sessionId = this.HttpContext.Session.GetString("SessID");
                
                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId,_CustomerCaseDTO.CustomerType));
                model.RiskVersionData= _mapper.Map<List<RiskReportModel>>(riskReports);

                var clientId = _clientHandler.GetClientId();
                RiskModel _riskmodel = new RiskModel();
                int riskid = _riskService.GetRiskIdByCustomercode(model.Case.CustomerId, model.Case.CustomerType).Result;

                if (riskid != 0)
                {
                    dynamic modelrisk = null;

                    if (model.Case.CustomerType == "I")
                    {
                        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);
                        int riskVersion = modelrisk != null ? modelrisk.version : 1;
                        if (riskVersion == 0) riskVersion = 1;

                        model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("I", 1, 0, 0, clientId, riskVersion);

                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;

                        // Check for Override (O)
                        bool hasHighOverride = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                        if (model.FinalRiskScore != null && model.FinalRiskScore.ToLower().Contains("high risk") && hasHighOverride)
                        {
                            model.FinalRiskScore = "High Risk (O)";
                        }
                    }
                    else
                    {
                        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);
                        int riskVersion = modelrisk != null ? modelrisk.version : 1;
                        if (riskVersion == 0) riskVersion = 1;

                        model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport("C", 1, 0, 0, clientId, riskVersion);

                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);

                        model.RiskAssessmentRating = modelrisk.RiskAssessmentRating;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;

                        // Check for Override (O)
                        bool hasHighOverrideCor = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                        if (model.RiskAssessmentRating != null && model.RiskAssessmentRating.ToLower().Contains("high risk") && hasHighOverrideCor)
                        {
                            model.RiskAssessmentRating = "High Risk (O)";
                        }
                    }
                }

                var allComments = _caseCommentService.GetAllByCase(CaseId);
                if (allComments != null)
                {
                    model.CaseComments = allComments.Select(c => new CaseModel
                    {
                        Comments = c.Comment,
                        CreatedUser = c.CreatedUser,
                        CreatedOn = c.CreatedOnDB ?? DateTime.MinValue,
                        MatchType = c.CommentType
                    }).ToList();
                }
                
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
                
                return View("Process_PDF", model);
            }
            catch (Exception ex)
            {
                return RedirectToAction("Index");
            }
        }

        [HttpGet("/case/DownloadProcessPDF/{CaseId}")]
        public async Task<IActionResult> DownloadProcessPDF(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

                model.Case = new CaseModel();
                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);

                var dualMatchStatus = _customerCaseService.GetDualGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.DualGoodsMatchStatus = dualMatchStatus;
                var militaryMatchStatus = _customerCaseService.GetMilitaryGoodsStatus(_CustomerCaseDTO.CustomerId);
                model.MilitaryGoodsMatchStatus = militaryMatchStatus;
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;

                var dob = model.Case.DOB;
                string dobText;
                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM")
                    dobText = "NA";
                else if (DateTime.TryParse(dob, out DateTime parsedDob))
                    dobText = parsedDob.ToString("dd/MM/yyyy");
                else
                    dobText = dob;

                var createdDate = model.Case.CreatedOn;
                model.Case.CreatedOnText = createdDate == DateTime.MinValue ? "NA" : createdDate.ToString("dd/MM/yyyy");
                model.Case.DOB = dobText;

                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByCompanyCode(_CustomerCaseDTO.CustomerId);
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);

                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType));
                model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);

                var clientId = _clientHandler.GetClientId();
                var sessionId = this.HttpContext.Session.GetString("SessID");

                // Risk Assessment
                int riskid = _riskService.GetRiskIdByCustomercode(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType).Result;
                if (riskid != 0)
                {
                    var riskReportVersion = 1;
                    model.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfigReport(_CustomerCaseDTO.CustomerType, 1, 0, 0, clientId, riskReportVersion);

                    if (_CustomerCaseDTO.CustomerType == "I")
                    {
                        dynamic modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividual(riskid).Result);
                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);
                        model.FinalRiskScore = modelrisk.FinalRiskScore;
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        
                        // For Individual, RiskAssessmentRating comes from FinalRiskScore
                        model.RiskAssessmentRating = modelrisk.FinalRiskScore;
                    }
                    else if (_CustomerCaseDTO.CustomerType == "C")
                    {
                        dynamic modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporate(riskid).Result);
                        MapRiskValues(model.RiskTypeCategoryDTO, modelrisk.ReportDataDTO);
                        model.RiskScoreCount = modelrisk.RiskScoreCount;
                        model.RiskScoreSum = modelrisk.RiskScoreSum;
                        model.RiskAssessmentRating = modelrisk.RiskAssessmentRating;
                        model.DateofAssessment = modelrisk.DateofAssessment;
                        // For Corporate, FinalRiskScore is mapped from RiskAssessmentRating
                        model.FinalRiskScore = modelrisk.RiskAssessmentRating;
                    }

                    // Check for Override (O)
                    bool hasHighOverrideCor = model.RiskTypeCategoryDTO != null && model.RiskTypeCategoryDTO.Any(c => c.RiskTypes.Any(r => r.OverrideScore == 3));
                    if (model.RiskAssessmentRating != null && model.RiskAssessmentRating.ToLower().Contains("high risk") && hasHighOverrideCor)
                    {
                        model.RiskAssessmentRating = "High Risk (O)";
                        model.FinalRiskScore = "High Risk (O)";
                    }
                }



                var allComments = _caseCommentService.GetAllByCase(CaseId);
                if (allComments != null)
                {
                    model.CaseComments = allComments.Select(c => new CaseModel
                    {
                        Comments = c.Comment,
                        CreatedUser = c.CreatedUser,
                        CreatedOn = c.CreatedOnDB ?? DateTime.MinValue,
                        MatchType = c.CommentType
                    }).ToList();
                }

                // Screening results

                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }

                // Render view to HTML string then convert to PDF
                string html = await _viewRenderService.RenderToStringAsync("Case/Process_PDF", model);
                byte[] pdfBytes = _exportService.HtmlToPDFforChecklistLogs(html);
                string fileName = $"Case_{_CustomerCaseDTO.CustomerId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest("Error generating PDF: " + ex.Message);
            }
        }


        public IActionResult CreateMainparty(string customerCode, string custtype)
        {
            int Id = _customerCaseService.GetCaseId(customerCode);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(Id);
            _CustomerCaseDTO.CustomerType = custtype;
            _CustomerCaseDTO.ClientId = _clientHandler.GetClientId();
            _CustomerCaseDTO.CreatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.CustomerId = "0";
            _CustomerCaseDTO.CompanyCode = "";
            _CustomerCaseDTO.ParentID = "";
            _CustomerCaseDTO.IsDuplicate = 1;
            _CustomerCaseDTO.Version = 1;

            int previousStatus = _CustomerCaseDTO.Status;

            if (custtype == "I")
            {
                _CustomerCaseDTO.Type = "Individual";
            }
            else
            {
                _CustomerCaseDTO.Type = "Corporate";
            }

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

            if (previousStatus == 2 || previousStatus == 3)
            {
                int newStatus = hasMatchRecords ? 0 : 5;

                _CustomerCaseDTO.Status = newStatus;
            }
            

            // Assign status before creating case


            var result1 = _customerCaseService.Create(_CustomerCaseDTO);
            string parsedResult1 = result1.Result;
            if (!string.IsNullOrEmpty(parsedResult1))
            {
                if (parsedResult1.Contains("Ø")) parsedResult1 = parsedResult1.Split('Ø')[1];
                else if (parsedResult1.Contains("??")) parsedResult1 = parsedResult1.Split(new string[] { "??" }, StringSplitOptions.None)[1];
                else if (parsedResult1.Contains("A~")) parsedResult1 = parsedResult1.Split(new string[] { "A~" }, StringSplitOptions.None)[1];
            }

            int newCaseId = _customerCaseService.GetCaseId(parsedResult1);

            bool isCaseCreated = false;
            string caseRefId = null;

            if (result1 != null)
            {
                List<MatchRecordsDTO> matchrecordsList = jsonList
                    .Select(x => new MatchRecordsDTO
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
                    })
                    .ToList();

                CASELOG modelCaseLog = new CASELOG
                {
                    CASEID = newCaseId.ToString(),
                    MATCHRECORDS = matchrecordsList
                };

                log.Debug("Add case log under threshold to MongoDB");

                _freeSourceRepository.InsertCaseLog(modelCaseLog);

                caseRefId = parsedResult1;
                isCaseCreated = true;
            }

            if (isCaseCreated && !string.IsNullOrEmpty(caseRefId))
            {
                int id = _customerCaseService.GetCaseId(caseRefId);
                

                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = id;
                remarkModel.Comment = "Case is Created"; // ✅ FIXED
                remarkModel.CommentType = "Convert Related Parties to Main Party";
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                remarkModel.CustomerId = caseRefId;

                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));

                int previousId = _customerCaseService.GetCaseId(customerCode);

                CustomerCaseDTO _CustomerCaseDTO1 = _customerCaseService.GetDetails(previousId);
                DateTime createdDate;
                CaseCommentModel remarkModel1 = new CaseCommentModel();
                remarkModel1.CaseId = id;
                var formats = new[]
                {
                    "dd/MM/yyyy HH:mm:ss",
                    "dd-MM-yyyy HH:mm:ss"
                };

                if (DateTime.TryParseExact(
                    _CustomerCaseDTO1.CreatedOn,
                    formats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out createdDate))
                {
                    remarkModel1.Comment = $"Case is Screened on {createdDate:dd/MM/yyyy}";
                }
                remarkModel1.CommentType = "Convert Related Parties to Main Party";
                remarkModel1.CreatedBy = _clientHandler.GetUserId();
                remarkModel1.CustomerId = caseRefId;

                var remarkResult1 = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel1));

                TempData["CaseRefId"] = caseRefId;
                TempData["IsCaseCreated"] = true;
            }

            if (_CustomerCaseDTO.Type == "Individual")
            {
                return Json(new { redirectUrl = Url.Action("Create") });
            }
            else
            {
                return Json(new { redirectUrl = Url.Action("CorporateScreening", "Corporate") });
            }
        }

        [HttpGet("/case/Shareholder/{CaseId}")]
        public async Task<ActionResult> Shareholder(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var userId = _clientHandler.GetUserId();
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

                var corporateId = HttpContext.Session.GetString("CorporateId");
                var corporatecaseId = HttpContext.Session.GetString("CorporateCaseId");
                
                TempData["CorporateId"] = corporateId;
                TempData["CorporateCaseId"] = corporatecaseId;

                

                var returnUrl = HttpContext.Session.GetString("ReturnUrl");
                ViewBag.ReturnUrl = returnUrl;

                HttpContext.Session.SetString("ShareholderReturnUrl", HttpContext.Request.Path + HttpContext.Request.QueryString);

                model.Case = new CaseModel();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
                if (_CustomerCaseDTO == null)
                {
                    return RedirectToAction("Index", "Error"); // Or some other handling
                }
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel?.Name ?? "System";
                var dob = model.Case.DOB;
                var createddated = model.Case.CreatedOn;
                string dobText;

                if (string.IsNullOrWhiteSpace(dob) || dob == "1/1/0001 12:00:00 AM" || dob == "1/1/1900 12:00:00 AM")
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
                string sessionId = this.HttpContext.Session.GetString("SessID");
                var actionRights = new Dictionary<string, string>();
                var actionsToCheck = new List<string>
                {
                    "Approve",
                    "Reject",
                    "Senior Management",
                    "On Hold",
                    "Whitelist"
                    //"SaveSearchResult"
                };
                foreach (var action in actionsToCheck)
                {
                    // Call the service to check if user has the right
                    var serviceResponse = _userGroupRightService.CheckNameuserrightExists(
                        "case",
                        "close",// Controller
                        userId,
                        GroupId,
                        sessionId,
                        action// Action to check
                    );

                    // If user has the right, store the actual action name, else "1"
                    actionRights[action] = (serviceResponse?.Result ?? false).ToString().ToLower();
                }

                model.ActionRights = actionRights;




                var CommentCase = _userGroupRightService.CheckUserRightExixts("case", "comment", userId, GroupId, sessionId);
                model.CommentCase = CommentCase.Result;

                var DocumentCase = _userGroupRightService.CheckUserRightExixts("case", "document", userId, GroupId, sessionId);
                model.DocumentsCase = DocumentCase.Result;

                var TransferCase = _userGroupRightService.CheckUserRightExixts("case", "assign", userId, GroupId, sessionId);
                model.TransferCase = TransferCase.Result;

                var savesearch = _userGroupRightService.CheckUserRightExixts("case", "saveRemark", userId, GroupId, sessionId);
                model.SaveSearResult = savesearch.Result;


                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
                model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);
                List<CustomerCaseDTO> _CustomerCaseshareholders = _customerCaseService.GetShareHoldersByParentCode(_CustomerCaseDTO.CustomerId);
                if (!string.IsNullOrEmpty(_CustomerCaseDTO.GroupId))
                {
                    var groupMembers = _customerCaseService.GetCasesByGroupId(_CustomerCaseDTO.GroupId);
                    if (groupMembers != null)
                    {
                        foreach (var member in groupMembers)
                        {
                            if (!_CustomerCaseshareholders.Any(s => s.CustomerId == member.CustomerId))
                            {
                                _CustomerCaseshareholders.Add(member);
                            }
                        }
                    }
                }
                model.ShareholdersData = _mapper.Map<List<CaseModel>>(_CustomerCaseshareholders);
                List<RiskReportModel> riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(_CustomerCaseDTO.CustomerId, _CustomerCaseDTO.CustomerType));
                model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);
                var currentVersion = _CustomerCaseDTO.Version;

                var _CustomerCasesData = _customerCaseService
                    .GetAllVersionCases(_CustomerCaseDTO.CustomerId)
                    .Where(x => x.Version != currentVersion)   // 🔥 exclude current
                    .OrderByDescending(x => x.Version)         // latest first
                    .ToList();

                model.CustomerCaseData = _mapper.Map<List<CaseModel>>(_CustomerCasesData);

                //List<PassportDetailsDTO> passportDetails= _customerCaseService.GetPassportdetails(CaseId);
                //model.passportDetails = _mapper.Map<List<PassportDetails>>(passportDetails);
                //model.Case.Id = CaseId;
                //model.DocumentCategories = new SelectList(_mapper.Map<List<DocumentCategoryModel>>(_caseDocumentService.GetAllCategories()), "Id", "Name");
                //model.DocumentTypes = new SelectList(_mapper.Map<List<DocumentTypeModel>>(_caseDocumentService.GetAllTypes()), "Id", "Name");
                var clientId = _clientHandler.GetClientId();
                //IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                //                                       select new SelectListItem
                //                                       {
                //                                           Value = Convert.ToString(s.Id),
                //                                           Text = s.FName + " " + s.LName.ToString()
                //
                //       };
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

                var id = _clientHandler.GetUserId();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                       select new SelectListItem
                                                       {
                                                           Value = Convert.ToString(s.Id),
                                                           Text = s.FName + " " + s.LName.ToString()
                                                       };

                model.Users = new SelectList(userList, "Value", "Text");
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                //log.Debug($"apiresults ({result}) ");
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                error.module = "CaseManagement_Process";
                error.comments = "API call Error while Getting data from manogo db";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpGet("/case/Shareholder_PDF/{CaseId}")]
        public async Task<ActionResult> Shareholder_PDF(int CaseId)
        {
            CaseProcessModel model = new CaseProcessModel();
            try
            {
                var BranchId = _clientHandler.GetBranchId();
                var GroupId = _clientHandler.GetGroupId();
                var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

                var corporateId = HttpContext.Session.GetString("CorporateId");
                TempData["CorporateId"] = corporateId;

                model.Case = new CaseModel();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
                model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                model.Case.UserGroupName = _UserGroupModel.Name;
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


                List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(CaseId);
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

                var id = _clientHandler.GetUserId();
                string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
                string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
                IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != id))
                                                       select new SelectListItem
                                                       {
                                                           Value = Convert.ToString(s.Id),
                                                           Text = s.FName + " " + s.LName.ToString()
                                                       };

                model.Users = new SelectList(userList, "Value", "Text");
                var result = _clientHandler.PostAsync(new { caseid = CaseId.ToString() }, ScreeningService.GETBYCASEID).Result;
                if (!string.IsNullOrEmpty(result))
                {
                    List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                    model.DataList = jsonList;
                }
                
                return View("Shareholder_PDF", model);
            }
            catch (Exception ex)
            {
                ErrorLogDTO error = new ErrorLogDTO();
                error.created_on = DateTime.Now;
                error.createdBy = _clientHandler.GetUserId();
                error.description = ex.InnerException?.Message ?? ex.Message;
                error.module = "CaseManagement_Shareholder_PDF";
                error.comments = "API call Error while Getting data from mongo db";
                error.status_code = 404;

                var result = _commonService.createErrorlog(error);

                _toastNotification.AddErrorToastMessage(error.description);

                Console.Error.WriteLine(ex);

                return RedirectToAction("Index");
            }
        }

        private void MapRiskValues(List<RiskTypeCategoryDTO> categories, List<ReportDataDTO> reportData)
        {
            if (reportData == null) return;

            foreach (var category in categories)
            {
                foreach (var riskType in category.RiskTypes)
                {
                    var isCountry = riskType.lov_country_duplicate == 1;
                    var match = reportData.FirstOrDefault(x =>
                        (isCountry || x.lov_type_category_id == category.Id) &&
                        x.lov_type_id == riskType.Id);

                    // SelectList value is the composite "score|override|id" produced by RiskItemsDTO.Score;
                    // the dropdown is highlighted only when the SelectList's selectedValue matches one of those composite values.
                    string selectedComposite = null;

                    if (match != null)
                    {
                        riskType.ItemTxt = match.lov_risk_data;
                        riskType.ItemScore = match.lov_risk_score;
                        riskType.OverrideScore = match.Over_ride_Score;

                        // Stored procedure does not return trans_risk_item_id, so resolve the LOV item by
                        // matching the saved text (lov_risk_data) against RiskItems.RiskItem within this RiskType.
                        // Fall back to a score-based match if the text doesn't line up (legacy data, country dupes).
                        var matchedItem = riskType.RiskItems?.FirstOrDefault(it =>
                            string.Equals(it.RiskItem, match.lov_risk_data, StringComparison.OrdinalIgnoreCase));
                        if (matchedItem == null)
                        {
                            matchedItem = riskType.RiskItems?.FirstOrDefault(it =>
                                int.TryParse(it.RiskScore, out var rs) && rs == match.lov_risk_score);
                        }
                        if (matchedItem != null)
                        {
                            riskType.SelectedItemId = matchedItem.Id;
                            selectedComposite = matchedItem.Score;
                        }
                    }

                    riskType.Items = new SelectList(
                        riskType.RiskItems,
                        "Score",
                        "RiskItem",
                        selectedComposite
                    );
                }
            }
        }

        [HttpGet]
        [Route("Case/process/Reject")]
        public ActionResult Reject()
        {
            return View();
        }

        //[HttpGet]
        //[Route("Case/process/Download")]
        public async Task<FileResult> CallApiMatchuid(string id, string category)
        {
            HttpResponseMessage file = new HttpResponseMessage();


            using (HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                //string url = pdfbaseURL;
                string url = baseC6URL;
                
                TokenRS token = AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                //     string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1ZmQ4NzVjNWVmMmFmYjMxNGNhMWE1YjIiLCJpYXQiOjE2MTI2OTE4MTMsImV4cCI6MTYxMzI5NjYxM30.trsanUcNCINZTa0gkWD_5LfofoHG1aD2wX8tq3XsP8I";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                if (category == "INDIVIDUAL")
                    url = baseC6URL + "personal/" + id;
                else
                    url = baseC6URL + "business/" + id;
                Task<HttpResponseMessage> response = httpClient.GetAsync(url);
                file = response.Result;

            }


            return File(file.Content.ReadAsByteArrayAsync().Result, "application/pdf; charset=utf-8", id + ".pdf");
        }

        [HttpGet]
        [Route("Case/process/Download")]
        public async Task<FileResult> CallApi(string id, string category)
        {
            HttpResponseMessage file = new HttpResponseMessage();


            using (HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                //string url = pdfbaseURL;
                string url = baseC6URL;
            
                TokenRS token =  AMLUtility.CreateC6Token(ScreeningService.C6AUTHENTICATION, baseC6URL, _c6Username);
                //     string accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1ZmQ4NzVjNWVmMmFmYjMxNGNhMWE1YjIiLCJpYXQiOjE2MTI2OTE4MTMsImV4cCI6MTYxMzI5NjYxM30.trsanUcNCINZTa0gkWD_5LfofoHG1aD2wX8tq3XsP8I";
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                if (category == "INDIVIDUAL")
                    url = baseC6URL + "personal/" + id;
                else
                    url = baseC6URL + "business/" + id;
                Task<HttpResponseMessage> response = httpClient.GetAsync(url);
                file = response.Result;

            }


            return File(file.Content.ReadAsByteArrayAsync().Result, "application/pdf; charset=utf-8", id + ".pdf");
        }
        [HttpGet]
        [Route("Case/process/DownloadPdf")]
        public async Task<IActionResult> DownloadPdf(int caseId, int index)
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            model.Case.Id = caseId;
            model.Index = index;
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(caseId);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);

            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(caseId);
            model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

            var clientId = _clientHandler.GetClientId();
            int fileType = (int)OperationType.PDF;
            var ClientId = _clientHandler.GetUserId();
            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != ClientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");
            var result = _clientHandler.PostAsync(new { caseid = caseId.ToString() }, ScreeningService.GETBYCASEID).Result;
            if (!string.IsNullOrEmpty(result))
            {
                List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                model.DataList = jsonList;
            }

            string data = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.Case.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = string.Concat(model.Case.FirstName, " ", model.Case.MiddleName, " ", model.Case.LastName),
                customernationality = model.Case.Nationality,
                searchtype = "F"
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(data))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(data);
                model.apiResultModels = apiResultModel;
            }

            // High-fidelity HTML to PDF conversion
            string html = await _viewRenderService.RenderToStringAsync("Case/GetDetails_PDF", model);
            byte[] pdfBytes = _exportService.HtmlToPDFforChecklistLogs(html);
            string fileName = $"CaseMatchReport_{caseId}.pdf";
            return File(pdfBytes, "application/pdf", fileName);
        }



        [HttpGet]
        [Route("Case/process/Details")]
        public async Task<ActionResult> GetDetailsApi(string id, string category, int CaseId,string Type,string Resourcesid ,string screenType)
        {

            UsersModel umodel = new UsersModel();
            BusinessModel bmodel = new BusinessModel();
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(CaseId);
            string url;
            var returnUrl = "";
            if (screenType == "shareholder")
            {
                returnUrl = HttpContext.Session.GetString("ShareholderReturnUrl");
            }
            else
            {
                returnUrl = HttpContext.Session.GetString("ReturnUrl");
            }


            ViewBag.ReturnUrl = returnUrl;

            
            TokenRS token =  AMLUtility.CreateC6Token("users/authenticate", baseC6URL, _c6Username);
            using (HttpClient httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) })
            {
                if (token == null || token.user == null || string.IsNullOrEmpty(token.user.token))
                {
                    _toastNotification.AddErrorToastMessage("Unable to authenticate with C6 service.");
                    return Redirect(string.IsNullOrEmpty(returnUrl) ? "/case/Process/" + CaseId : returnUrl);
                }

                if (category == "INDIVIDUAL")
                {
                    url = baseC6URL.TrimEnd('/') + "/personal/person/" + Uri.EscapeDataString(id);

                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _toastNotification.AddErrorToastMessage("Person details not found in C6 database (404).");
                        return Redirect(string.IsNullOrEmpty(returnUrl) ? "/case/Process/" + CaseId : returnUrl);
                    }

                    var details = response.Content.ReadAsStringAsync();
                    UsersModel jsonList = JsonConvert.DeserializeObject<UsersModel>(details.Result);
                    umodel = jsonList;
                    umodel.caseid = CaseId;
                    umodel.Type = Type;
                    umodel.Resourcesid = Resourcesid;
                    umodel.Category = category;
                    umodel.MatchUid = id;
                    umodel.CreationDate = _CustomerCaseDTO.CreatedOn;
                    umodel.Case= _mapper.Map<CaseModel>(_CustomerCaseDTO);
                    ViewBag.user = umodel;
                }
                else
                {
                    url = baseC6URL.TrimEnd('/') + "/business/person/" + Uri.EscapeDataString(id);
                    var request = new HttpRequestMessage(HttpMethod.Get, url);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.user.token);
                    HttpResponseMessage response = await httpClient.SendAsync(request);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        _toastNotification.AddErrorToastMessage("Business details not found in C6 database (404).");
                        return Redirect(string.IsNullOrEmpty(returnUrl) ? "/case/Process/" + CaseId : returnUrl);
                    }



                    var details = response.Content.ReadAsStringAsync();
                    BusinessModel jsonList = JsonConvert.DeserializeObject<BusinessModel>(details.Result);
                    bmodel = jsonList;
                    bmodel.caseid = CaseId;
                    bmodel.Type = Type;
                    bmodel.Resourcesid = Resourcesid;
                    bmodel.Category = category;
                    bmodel.MatchUid = id;
                    bmodel.CreationDate = _CustomerCaseDTO.CreatedOn;
                    bmodel.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
                    ViewBag.business = bmodel;

                }



            }
            return View();
        }

        private async void SendCaseTransferedMailAsync(string body, CaseAssignmentModel model)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                var appBaseUrl = MyHttpContext.AppBaseUrl;
                body = body.Replace("{caseID}", model.CaseId.ToString());
                body = body.Replace("{baseUrl}", appBaseUrl);
                body = body.Replace("{Username}", HttpContext.Session.GetString("SessUsername"));

                var emailSent = await _commonService.sendCaseTransferedEmaiLog("Case Transfered Alert", body, model);
            }
        }



        [HttpPost("/case/assign")]
        public async Task<JsonResult> Assign(CaseAssignmentModel model)
        {
            var BranchId = _clientHandler.GetBranchId();
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.UtcNow.AddHours(4);
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(model.CaseId);
            if (_UserGroupModel.Name == "Senior Management") 
            {
                if(_CustomerCaseDTO.MatchScore == 0)
                {
                    _CustomerCaseDTO.Status = 5;
                }
                else
                {
                    _CustomerCaseDTO.Status = 0;
                }

                    _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
                _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.UtcNow.AddHours(4));
                _CustomerCaseDTO.Comments = model.Comment;

                 _customerCaseService.Update(_CustomerCaseDTO);

            }


            
            var result = _caseAssignmentService.Create(_mapper.Map<CaseAssignmentDTO>(model));
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = string.Format("Transferred Case To {0}", model.TransferUser);


            UserModel _UserModel = _mapper.Map<UserModel>(_userService.GetDetails(model.UserId));
            model.Email = _UserModel.UserDetail.Email;
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(@"Views/Risk/CaseTransferedEmailBody.html"))
            {
                body = reader.ReadToEnd();
            }
            ;

            if (model.Comment != "" && model.Comment != null)
            {
                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CustomerId = model.CustomerId;
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }
            CaseCommentModel commentModel = new CaseCommentModel();
            commentModel.CaseId = model.CaseId;
            commentModel.Comment = comment;
            commentModel.CreatedBy = _clientHandler.GetUserId();
            commentModel.CustomerId = model.CustomerId;
            commentModel.CommentType = "Transferred Case Section";
            var commentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));

            await Task.Run(() => SendCaseTransferedMailAsync(body, model));

            return Json(result);
        }
        [HttpPost("/case/onhold")]
        public async Task<JsonResult> OnHold(CaseAssignmentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;

            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            CustomerCaseDTO currentCase = _customerCaseService.GetDetails(model.CaseId);
            if (_UserGroupModel.Name .Contains("Compliance") && currentCase.Status == 4)
            {
                return Json("Edit access restricted for cases submitted to senior management.");
            }
            
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = string.Format("Customer Case Onhold");
            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(model.CaseId);
            _CustomerCaseDTO.Status = 7;
            _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
            _CustomerCaseDTO.Comments = model.Comment;

            var result = _customerCaseService.Update(_CustomerCaseDTO);
            if(_CustomerCaseDTO.Type !="Individual" && _CustomerCaseDTO.Type != "Corporate")
            {
                
                
                int companyId= _customerCaseService.GetCaseId(_CustomerCaseDTO.CompanyCode);
                CustomerCaseDTO _CompanyCustomerCaseDTO = _customerCaseService.GetDetails(companyId);
                _CompanyCustomerCaseDTO.Status = 0;
                _CompanyCustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
                _CompanyCustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
                _CompanyCustomerCaseDTO.Comments = model.Comment;

                var result1 = _customerCaseService.Update(_CompanyCustomerCaseDTO);
                CaseCommentModel CompanycommentModel = new CaseCommentModel();
                CompanycommentModel.CaseId = companyId;
                CompanycommentModel.Comment = _CustomerCaseDTO.FlagType +" case has been on hold";// ✅ FIXED
                CompanycommentModel.CommentType = "Related Parties On Hold";
                CompanycommentModel.CreatedBy = _clientHandler.GetUserId();
                CompanycommentModel.CustomerId = _CustomerCaseDTO.CompanyCode;
                var CompanycommentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(CompanycommentModel));
                if(_CustomerCaseDTO.ParentID != null) 
                {
                    int parentId = _customerCaseService.GetCaseId(_CustomerCaseDTO.ParentID);
                    //CustomerCaseDTO _parentCustomerCaseDTO = _customerCaseService.GetDetails(parentId);
                    //_parentCustomerCaseDTO.Status = 0;
                    //_parentCustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
                    //_parentCustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
                    //_parentCustomerCaseDTO.Comments = model.Comment;

                    var result2 = _customerCaseService.Update(_CompanyCustomerCaseDTO);
                    CaseCommentModel parentIdcommentModel = new CaseCommentModel();
                    parentIdcommentModel.CaseId = parentId;
                    parentIdcommentModel.Comment = _CustomerCaseDTO.FlagType + " case has been on hold";// ✅ FIXED
                    parentIdcommentModel.CommentType = "Related Parties On Hold";
                    parentIdcommentModel.CreatedBy = _clientHandler.GetUserId();
                    parentIdcommentModel.CustomerId = _CustomerCaseDTO.ParentID;
                    var parentIdcommentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(parentIdcommentModel));

                }
            }
            if (model.Comment != "" && model.Comment != null)
            {
                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CommentType = "Hold Remarks";
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                remarkModel.CustomerId = model.CustomerId;
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }
            CaseCommentModel commentModel = new CaseCommentModel();
            commentModel.CaseId = model.CaseId;
            commentModel.Comment = comment;// ✅ FIXED
            commentModel.CommentType = "Hold";
            commentModel.CreatedBy = _clientHandler.GetUserId();
            commentModel.CustomerId = model.CustomerId;
            var commentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));


            return Json(model);
        }
        [HttpPost("/case/document")]
        public JsonResult Document(CaseDocumentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            model.ClientId = _clientHandler.GetClientId();
            if (model.Document != null)
            {
                DocumentsModel _documentsModel = _fileUploader.UploadFile(model.ClientId, ItemType.caseDocument, _clientHandler.GetBranchId(), model.Document);
                model.DocumentFileName = _documentsModel.DocName;
                model.DocumentFullPath = _documentsModel.DocFullPath;
            }
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            model.IssuedDateOnDB = model.IssuedDate;
            model.ExpiryDateOnDB = model.ExpiryDate;
            var result = _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(model));
            _customerCaseService.UpdateCase(model.Id,model.CreatedBy);
            return Json(result);
        }
        [HttpPost("/case/comment")]
        public JsonResult Comment(CaseCommentModel model)
        {
            model.CreatedBy = _clientHandler.GetUserId();
            model.CreatedOn = DateTime.Now;
            model.CustomerId = model.CustomerId;
            var result = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(model));

            return Json(_caseCommentService.GetAllByCustomerId(model.CustomerId));
        }
        [HttpPost("/case/close")]
        public JsonResult Close(CaseCloseModel model)
        {
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            CustomerCaseDTO currentCase = _customerCaseService.GetDetails(model.CaseId);
            if (_UserGroupModel.Name .Contains("Compliance") && currentCase.Status == 4) { return Json("Edit access restricted for cases submitted to senior management."); }

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(model.CaseId);
            _CustomerCaseDTO.Status = model.Action;
            _CustomerCaseDTO.UpdatedBy = _clientHandler.GetUserId();
            _CustomerCaseDTO.UpdatedOn = Convert.ToString(DateTime.Now);
            _CustomerCaseDTO.Comments = model.Comment;
            _CustomerCaseDTO.NoMatch = model.NoMatch;
            _CustomerCaseDTO.TrueDomesticpep = model.TrueDomesticpep;
            _CustomerCaseDTO.TrueForeignpep = model.TrueForeignpep;
            _CustomerCaseDTO.TrueAdverseMedia = model.TrueAdverseMedia;
            _CustomerCaseDTO.PartialDomesticpep = model.PartialDomesticpep;
            _CustomerCaseDTO.PartialForeignpep = model.PartialForeignpep;
            _CustomerCaseDTO.Partialadversemedia = model.Partialadversemedia;
            _CustomerCaseDTO.TrueUAEUNSanction = model.TrueUAEUNSanction;
            _CustomerCaseDTO.TrueOtherSanction = model.TrueOtherSanction;
           

            

            var result = _customerCaseService.Update(_CustomerCaseDTO);

            //Update White List
            if (model.Action == 1)
            {

                var res = _customerMasterService.UpdateWhiteList(_CustomerCaseDTO.CustomerMasterId, "YES");
                _customerMasterService.InsertCustomerWhiteListLogs(_CustomerCaseDTO.CustomerMasterId, _CustomerCaseDTO.CustomerId, _clientHandler.GetUserId(), "YES");
            }
            //else
            //{
            //    var res = _customerMasterService.UpdateWhiteList(_CustomerCaseDTO.CustomerMasterId, "NO");
            //    _customerMasterService.InsertCustomerWhiteListLogs(_CustomerCaseDTO.CustomerMasterId, _CustomerCaseDTO.CustomerId, _clientHandler.GetUserId(), "NO");
            //}

            string response = string.Empty;
            string commenttype = string.Empty;
            if (model.Action == 2)
            {
                response = "Customer case approved.";
                commenttype = "Approved";
            }
            else if (model.Action == 3)
            {
                response = "Customer case rejected.";
                commenttype = "Rejected";
            }
            else if(model.Action == 4)
            {
                response = "Customer case is  forwarded to Senior Management.";
                commenttype = "Senior Management";
            }
            //response = model.Action == 2 ? "Customer case approved." : "Customer case rejected.";
            if (model.Action == 1)
            {

                response = "Customer Case Whitelisted";
                commenttype = "whitelisted";
            }
            //_toastNotification.AddErrorToastMessage(reposne);
            if (model.Comment != "" && model.Comment != null)
            {
                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = model.CaseId;
                remarkModel.Comment = model.Comment;
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                remarkModel.CustomerId = model.CustomerId;
                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }
            CaseCommentModel commentModel = new CaseCommentModel();
            commentModel.CaseId = model.CaseId;
            var userName = HttpContext.Session.GetString("SessUsername");
            var comment = "";
            switch (model.Action)
            {
                case 1:
                    comment = string.Format("Whitelisted Case");
                    break;
                case 2:
                    comment = string.Format("Approved Case");
                    break;
                case 3:
                    comment = string.Format("Rejected Case");
                    break;
                case 4:
                    comment = string.Format("Forwarded to Senior Management");
                    break;
                default:
                    comment = "";
                    break;
            }
            commentModel.Comment = comment;
            commentModel.CreatedBy = _clientHandler.GetUserId();
            commentModel.CommentType = commenttype;
            commentModel.CustomerId = model.CustomerId;
            var commentResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(commentModel));

            if (model.screenType == "Shareholder")
            {
                int id = _customerCaseService.GetCaseId(model.CorporateId);

                CaseCommentModel remarkModel = new CaseCommentModel();
                remarkModel.CaseId = Convert.ToInt32(id);
                remarkModel.Comment = model.FlagType + " Case is Updated"; // ✅ FIXED
                remarkModel.CommentType = model.FlagType;
                remarkModel.CreatedBy = _clientHandler.GetUserId();
                remarkModel.CustomerId = model.CustomerId;

                var remarkResult = _caseCommentService.Create(_mapper.Map<CaseCommentDTO>(remarkModel));
            }


            return Json(response);

        }
        [HttpGet("/case/comment/{CaseId}")]
        public JsonResult Comment(int CaseId, string customerId)
        {
            return Json(_caseCommentService.GetAllByCustomerId(customerId));
        }
        [HttpGet]
        public ActionResult DownloadCaseDocument(string filePath, string fileName)
        {
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);



            return File(fileBytes, "application/force-download", fileName);
        }

        [Route("/case/saveRemark")]
        [HttpPost]
        //[AML.Web.CustomFilters.TenantOwned(AML.Web.CustomFilters.TenantResource.Case, "id")]
        //public JsonResult SaveRemark(int id,string customerid,string type, List<DataListModel> model)
            public JsonResult SaveRemark([FromBody] SaveRemarkRequest request)
        {
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            CustomerCaseDTO currentCase = _customerCaseService.GetDetails(request.Id);
            if (_UserGroupModel.Name .Contains("Compliance") && currentCase.Status == 4)
            {
                return Json("Edit access restricted for cases submitted to senior management.");
            }

            var userid = _clientHandler.GetUserId();
            var clientid = _clientHandler.GetClientId();
            CorporateKycDTO corpModel = new CorporateKycDTO();
            KycIndividualDTO imodel = new KycIndividualDTO();
            CaseModel model1=new CaseModel();
            CorporateDetailsModel corporateDetailsModel = new CorporateDetailsModel();
            var result = _commonService.UpdateCaseRemark(request.Id, request.Model);

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(request.Id);

            string response = string.Empty;
            response = result ? "Remarks Updated" : "Saving Remarks failed";
            _customerCaseService.UpdateCase(request.Id, userid);

            
            if (request.Type == "Individual" || request.Type == "Corporate")
            {
                bool hasOFAC = false;
                bool hasKYC6 = false;
                var rowtype = "";

                /* -------------------------
                   FIRST LOOP
                   Detect match types
                --------------------------*/
                model1.Domesticpep = "No";
                model1.ForeignPep = "No";
                model1.RedFlags = "No";
                model1.SanctionMatch = "No";
                model1.UAEORUNSC = "No";
                model1.HighNetworkIndividual = "No";
                corporateDetailsModel.Domesticpep = "No";
                corporateDetailsModel.SanctionMatch = "No";
                corporateDetailsModel.ForeignPep = "No";
                corporateDetailsModel.RedFlags = "No";
                corporateDetailsModel.UAEORUNSC = "No";
                corporateDetailsModel.HighNetworkIndividual = "No";

                foreach (var row in request.Model)
                {
                    if (row.matchcategory == "INDIVIDUAL")
                    {
                        rowtype = row.matchcategory;
                        // Skip if searchTypes empty
                        if (row.searchTypes == null || !row.searchTypes.Any())
                            continue;

                        bool hasNone = row.searchTypes.Contains("None");

                        // If only None selected → treat as no selection
                        if (hasNone && row.searchTypes.Count == 1)
                            continue;

                        // Remove None if mixed with others
                        if (hasNone)
                        {
                            row.searchTypes = row.searchTypes
                                .Where(x => x != "None")
                                .ToList();
                        }

                        if (!string.IsNullOrWhiteSpace(row.matchtype))
                        {
                            var type1 = row.matchtype?.ToUpper() ?? string.Empty;

                            // Group 1
                            var sanctionTypes = new List<string> { "UN", "OFAC", "UAE IEC LIST", "BL", "CBWL", "INTERNAL","" };
                            if (sanctionTypes.Any(t => type1.Contains(t)))
                            
                                hasOFAC = true;   // You can rename this to hasSanction if needed
                            
                            

                            if (type1.Contains("KYC6"))
                                hasKYC6 = true;
                        }
                        if (hasKYC6 && row.searchTypes.Contains("Domestic PEP"))
                        {
                            model1.Domesticpep = "Yes";
                        }
                        

                        if (hasKYC6 && row.searchTypes.Contains("Foreign PEP"))
                        {
                            model1.ForeignPep = "Yes";
                        }
                        
                        var hasHighRisk = row.searchTypes.Any(x => x == "REL" || x == "RRE" || x == "GRI" || x == "SOE");

                        var hasMediumRisk = row.searchTypes.Any(x =>
                            x == "INS" || x == "DD");

                        if (hasKYC6 && hasHighRisk)
                        {
                            model1.RedFlags = "Yes";
                        }else if (hasKYC6 && hasMediumRisk)
                        {
                            model1.RedFlags = "Might Be";
                        }
                        



                        if ((hasOFAC || hasKYC6) && row.searchTypes.Contains("Other Sanctions"))
                        {
                            model1.SanctionMatch = "Yes";
                        }

                        if ((hasOFAC || hasKYC6) &&
    row.searchTypes.Any(x =>
        x.Trim().Equals("UAE Sanction", StringComparison.OrdinalIgnoreCase) || x.Trim().Equals("UAE Sanctions", StringComparison.OrdinalIgnoreCase) ||
        x.Trim().Equals("UN Sanction", StringComparison.OrdinalIgnoreCase)|| x.Trim().Equals("UN Sanctions", StringComparison.OrdinalIgnoreCase)))
                        {
                            model1.UAEORUNSC = "Yes";
                        }


                        if (hasKYC6 && row.searchTypes.Contains("VHNWI"))
                        {
                            model1.HighNetworkIndividual = "Yes";
                        }
                        
                            
                        if (row.riskAssessments != null && row.riskAssessments.Any())
                        {
                            foreach (var risk in row.riskAssessments)
                            {
                                if (string.IsNullOrWhiteSpace(risk.RiskTypeText) ||
                                    string.IsNullOrWhiteSpace(risk.ItemText))
                                    continue;

                                switch (risk.RiskTypeText?.Trim().ToLower())
                                {
                                    case "profession":
                                        model1.OccupatinTypeTxt = risk.ItemText;
                                        break;
                                    case "nationality":
                                        model1.Nationality = risk.ItemText;
                                        break;
                                    case "Second Nationality (if applicable)":
                                        model1.Nationality = risk.ItemText;
                                        break;
                                    case "residence country":
                                        model1.ResidenceStatus = risk.ItemText;
                                        break;
                                    case "product, service & activity":
                                        model1.ProductName = risk.ItemText;
                                        break;
                                    case "if more than one product(put the riskiest product)":
                                        model1.HighestRiskProduct = risk.ItemText;
                                        break;
                                    case "delivery channel":
                                        model1.DeliveryChannelName = risk.ItemText;
                                        break;

                                    case "mode of payment":
                                        model1.Modeofpayment = risk.ItemText;
                                        break;
                                    case "dual use goods match":
                                        model1.DualUseGoods = risk.ItemText;
                                    break;
                                    case "if more dual use goods match":
                                        model1.MoreDualUseGoods = risk.ItemText;
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {

                        rowtype = row.matchcategory;
                        if (row.searchTypes == null || !row.searchTypes.Any())
                            continue;

                        bool hasNone = row.searchTypes.Contains("None");

                        // If only None selected → treat as no selection
                        if (hasNone && row.searchTypes.Count == 1)
                            continue;

                        // Remove None if mixed with others
                        if (hasNone)
                        {
                            row.searchTypes = row.searchTypes
                                .Where(x => x != "None")
                                .ToList();
                        }

                        if (!string.IsNullOrWhiteSpace(row.matchtype))
                        {
                            var type1 = row.matchtype.ToUpper();

                            var sanctionTypes = new List<string> { "UN", "OFAC", "UAE IEC LIST", "BL", "CBWL", "INTERNAL" };
                            if (sanctionTypes.Any(t => type1.Contains(t)))

                                hasOFAC = true;   // You can rename this to hasSanction if needed


                            if (type1.Contains("KYC6"))
                                hasKYC6 = true;
                        }

                        if (hasKYC6 && row.searchTypes.Contains("Domestic PEP"))
                        {
                            corporateDetailsModel.Domesticpep = "Yes";
                        }
                        

                        if (hasKYC6 && row.searchTypes.Contains("Foreign PEP"))
                        {
                            corporateDetailsModel.ForeignPep = "Yes";
                        }
                       
                        var hasHighRisk = row.searchTypes.Any(x => x == "REL" || x == "RRE" || x == "GRI" || x == "SOE");

                        var hasMediumRisk = row.searchTypes.Any(x =>
                            x == "INS" || x == "DD");

                        if (hasKYC6 && hasHighRisk)
                        {
                            corporateDetailsModel.RedFlags = "Yes";
                        }
                        else if(hasKYC6 && hasMediumRisk) { 

                        
                            corporateDetailsModel.RedFlags = "Might Be";
                        }
                        

                        if ((hasOFAC || hasKYC6) && row.searchTypes.Contains("Other Sanctions"))
                        {
                            corporateDetailsModel.SanctionMatch = "Yes";
                        }
                        

                        if ((hasOFAC || hasKYC6) &&
                            row.searchTypes.Any(x => x == "UAE Sanction" || x == "UN Sanction"))
                        {
                            corporateDetailsModel.UAEORUNSC = "Yes";
                        }
                        

                        if (hasKYC6 && row.searchTypes.Contains("VHNWI"))
                        {
                            corporateDetailsModel.HighNetworkIndividual = "Yes";
                        }
                        

                        

                        if (row.riskAssessments != null && row.riskAssessments.Any())
                        {
                            foreach (var risk in row.riskAssessments)
                            {
                                if (string.IsNullOrWhiteSpace(risk.RiskTypeText) ||
                                    string.IsNullOrWhiteSpace(risk.ItemText))
                                    continue;

                                switch (risk.RiskTypeText?.Trim().ToLower())
                                {
                                    case "legal status of the entity":
                                        corporateDetailsModel.EntityTypeTxt = risk.ItemText;
                                        break;

                                    case "nature of business":
                                        corporateDetailsModel.BusinessType = risk.ItemText;
                                        break;

                                    case "country of incorporation":
                                        corporateDetailsModel.Nationality = risk.ItemText;
                                        break;

                                    case "does the company have any subsidiary, affiliate, branch or group/holding company in fatf listed high risk monitored jurisdiction?":
                                        corporateDetailsModel.FATF = risk.ItemText;
                                        break;

                                    case "product":
                                        corporateDetailsModel.ProductName = risk.ItemText;
                                        break;

                                    case "if more than one product(put the riskiest product)":
                                        corporateDetailsModel.HighestRiskProduct = risk.ItemText;
                                        break;

                                    case "delivery channel":
                                        corporateDetailsModel.DeliveryChannelName = risk.ItemText;
                                        break;

                                    case "mode of payment":
                                        corporateDetailsModel.Modeofpayment = risk.ItemText;
                                        break;
                                    case "dual use goods match":
                                        corporateDetailsModel.DualUseGoods = risk.ItemText;
                                        break;
                                    case "if more dual use goods match":
                                        corporateDetailsModel.MoreDualUseGoods = risk.ItemText;
                                        break;

                                    default:
                                        // Optional: log unmatched value
                                        break;
                                }
                            }
                        }
                    }
                }



                if (rowtype == "INDIVIDUAL")
                {

                    //var results = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientid));
                    //if (results != null)
                    //{
                        var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(model1), corpModel, "I", culture, clientid);
                        if (str1.Result == null)
                        {
                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

                        }
                        var spStr1 = str1.Result.Split('Ø');
                        var proflovId = spStr1[0];
                        var natlovId = spStr1[1];
                        var reslovId = spStr1[5];
                        //var IspeplovId = spStr1[13];
                        var IndprodlovId = spStr1[13];
                        var InddelilovId = spStr1[14];
                        var IndmodeofpaymentlovId = spStr1[15];
                        var domesticpeplovId = spStr1[17];
                        var foreignlovId = spStr1[19];
                        var redflagslovId = spStr1[21];
                        var sanctionlovId = spStr1[23];
                        var UAEORUNSClovId = spStr1[25];
                        var highestriskproductlovId = spStr1[28];
                        var veryhighnetworkIdlovId = spStr1[30];
                    var dualusegoodslovId = GetSafely(spStr1, 33);
                    var MoredualusegoodslovId = GetSafely(spStr1, 35);
                    var militarygoodslovId = GetSafely(spStr1, 37);
                    var MoremilitarygoodslovId = GetSafely(spStr1, 39);

                    var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(model1), corpModel, "I", culture, clientid);
                        if (str.Result == null)
                        {
                            _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

                        }
                        var spStr = str.Result.Split('Ø');
                        var profId = spStr[0];
                        var natId = spStr[1];
                        var resId = spStr[5];
                        //var IspepId = spStr[13];
                        var IndprodId = spStr[13];
                        var InddeliId = spStr[14];
                        var Indmodeofpaymentid = spStr[15];
                        var domesticpepId = spStr[17];
                        var foreignId = spStr[19];
                        var redflagsId = spStr[21];
                        var sanctionId = spStr[23];
                        var UAEORUNSCId = spStr[25];
                        var highestriskproductId = spStr[28];
                        var veryhighnetworkId = spStr[30];
                    var dualusegoodsId = GetSafely(spStr, 33);
                    var MoredualusegoodsId = GetSafely(spStr, 35);
                    var militarygoodsId = GetSafely(spStr, 37);
                    var MoremilitarygoodsId = GetSafely(spStr, 39);

                    RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                        riskModel.CustomerId = request.CustomerId;
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
                            riskType8.Id = Convert.ToString(IndmodeofpaymentlovId);
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
                    if (militarygoodsId != "0" && militarygoodsId != "")
                    {
                        var riskTypeMil = new RiskTypeListModel();
                        riskTypeMil.Id = Convert.ToString(militarygoodslovId);
                        var riskItemMil = new RiskItemListModel();
                        riskItemMil.Id = militarygoodsId.ToString();
                        var riskItemListMil = new List<RiskItemListModel>();
                        riskItemListMil.Add(riskItemMil);
                        riskTypeMil.RiskItemList = riskItemListMil;
                        riskTypeList.Add(riskTypeMil);
                    }
                    if (MoremilitarygoodsId != "0" && MoremilitarygoodsId != "")
                    {
                        var riskTypeMoreMil = new RiskTypeListModel();
                        riskTypeMoreMil.Id = Convert.ToString(MoremilitarygoodslovId);
                        var riskItemMoreMil = new RiskItemListModel();
                        riskItemMoreMil.Id = MoremilitarygoodsId.ToString();
                        var riskItemListMoreMil = new List<RiskItemListModel>();
                        riskItemListMoreMil.Add(riskItemMoreMil);
                        riskTypeMoreMil.RiskItemList = riskItemListMoreMil;
                        riskTypeList.Add(riskTypeMoreMil);
                    }


                    riskModel.RiskTypeList = riskTypeList;
                        var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                        var xyz = riskResult;


                    //}

                }
                else
                {

                    //model.IsPeP = isPep;
                    var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(corporateDetailsModel), "C", culture, clientid);
                    if (str1.Result == null)
                    {


                        _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

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
                    var domesticpeplovId = spStr1[18];
                    var foreignlovId = spStr1[20];
                    var redflagslovId = spStr1[22];
                    var sanctionlovId = spStr1[24];
                    var UAEORUNSClovId = spStr1[26];
                    var corpfaftlovId = spStr1[27];
                    var highestriskproductlovId = spStr1[29];
                    var veryhighnetworkIdlovId = spStr1[31];
                    var dualusegoodslovId = GetSafely(spStr1, 32);
                    var MoredualusegoodslovId = GetSafely(spStr1, 34);
                    var militarygoodslovId = GetSafely(spStr1, 36);
                    var MoremilitarygoodslovId = GetSafely(spStr1, 38);
                    // var IsPeplovId = spStr1[14];

                    var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(corporateDetailsModel), "C", culture, clientid);

                    if (str.Result == null)
                    {
                        _toastNotification.AddWarningToastMessage("Unable to calculate risk due to insufficient data.");

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
                    var domesticpepId = spStr[18];
                    var foreignId = spStr[20];
                    var redflagsId = spStr[22];
                    var sanctionId = spStr[24];
                    var UAEORUNSCId = spStr[26];
                    var corpfaftId = spStr[27];
                    var highestriskproductId = spStr[29];
                    var veryhighnetworkId = spStr[31];
                    var dualusegoodsId = GetSafely(spStr, 32);
                    var MoredualusegoodsId = GetSafely(spStr, 34);
                    var militarygoodsId = GetSafely(spStr, 36);
                    var MoremilitarygoodsId = GetSafely(spStr, 38);

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
                    riskModel.CustomerId = request.CustomerId;
                    riskModel.CustomerName = _CustomerCaseDTO.FirstName;
                    riskModel.MainNationality = _CustomerCaseDTO.Nationality;
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
                        if (corpfaftId != "0")
                        {
                            var riskType19 = new RiskTypeListModel();
                            riskType19.Id = Convert.ToString(corpfaftlovId);
                            var riskItem19 = new RiskItemListModel();
                            riskItem19.Id = corpfaftId.ToString();//Convert.ToString(1);
                            var riskItemList19 = new List<RiskItemListModel>();
                            riskItemList19.Add(riskItem19);
                            riskType19.RiskItemList = riskItemList19;
                            riskTypeList.Add(riskType19);
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
                        //for pep start
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

                        if (militarygoodsId != "0" && militarygoodsId != "")
                        {
                            var riskTypeMil = new RiskTypeListModel();
                            riskTypeMil.Id = Convert.ToString(militarygoodslovId);
                            var riskItemMil = new RiskItemListModel();
                            riskItemMil.Id = militarygoodsId.ToString();
                            var riskItemListMil = new List<RiskItemListModel>();
                            riskItemListMil.Add(riskItemMil);
                            riskTypeMil.RiskItemList = riskItemListMil;
                            riskTypeList.Add(riskTypeMil);
                        }

                        if (MoremilitarygoodsId != "0" && MoremilitarygoodsId != "")
                        {
                            var riskTypeMoreMil = new RiskTypeListModel();
                            riskTypeMoreMil.Id = Convert.ToString(MoremilitarygoodslovId);
                            var riskItemMoreMil = new RiskItemListModel();
                            riskItemMoreMil.Id = MoremilitarygoodsId.ToString();
                            var riskItemListMoreMil = new List<RiskItemListModel>();
                            riskItemListMoreMil.Add(riskItemMoreMil);
                            riskTypeMoreMil.RiskItemList = riskItemListMoreMil;
                            riskTypeList.Add(riskTypeMoreMil);
                        }

                        riskModel.RiskTypeList = riskTypeList;
                        var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                        var xyz = riskResult;
                        Console.WriteLine($"generated risk for customer: {JsonConvert.SerializeObject(riskModel, Formatting.Indented)}");
                        Console.WriteLine($"Finished generating risk for customer: {JsonConvert.SerializeObject(riskResult, Formatting.Indented)}");
                    }

                }
                //// Example: High Risk logic




                ////To check if risk assessment is enabled for the client.


            }
                return Json(response);
        }

        [HttpGet]
        [Route("/case/GetDetails")]
        [AML.Web.CustomFilters.TenantOwned(AML.Web.CustomFilters.TenantResource.Case, "caseId")]
        public async Task<ActionResult> GetDetails(int caseId, int index,string type )
        {
            CaseProcessModel model = new CaseProcessModel();
            model.Case = new CaseModel();
            model.Case.Id = caseId;
            model.Index = index;

            CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetails(caseId);
            model.Case = _mapper.Map<CaseModel>(_CustomerCaseDTO);
            var returnUrl="";
            if (type == "shareholder")
            {
                 returnUrl = HttpContext.Session.GetString("ShareholderReturnUrl");
            }
            else
            {
                returnUrl = HttpContext.Session.GetString("ReturnUrl");
            }


                ViewBag.ReturnUrl = returnUrl;
            
            
            List<CaseDocumentDTO> caseDocumentbyId = _caseDocumentService.GetCaseDocumentByCaseId(caseId);
            model.CaseDocuments = _mapper.Map<List<CaseDocumentModel>>(caseDocumentbyId);

            var clientId = _clientHandler.GetClientId();
            var ClientId = _clientHandler.GetUserId();

            string actionName = this.ControllerContext.RouteData.Values["action"].ToString();
            string controllerName = this.ControllerContext.RouteData.Values["controller"].ToString();
            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAuthorisedUser(controllerName, actionName, clientId).Where(m => m.Id != ClientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.Users = new SelectList(userList, "Value", "Text");

            var result = _clientHandler.PostAsync(new { caseid = caseId.ToString() }, ScreeningService.GETBYCASEID).Result;
            if (!string.IsNullOrEmpty(result))
            {
                List<DataListModel> jsonList = JsonConvert.DeserializeObject<List<DataListModel>>(result);
                model.DataList = jsonList;
            }

            string datas = _clientHandler.PostAsync(new DTO.DTO.Sanction.ScreeningSearchDTO
            {
                customerdob = model.Case.DOB.ParseDBDate("dd MMM yyyy", "01 jan 1970"),
                customerfullname = string.Concat(model.Case.FirstName, " ", model.Case.MiddleName, " ", model.Case.LastName),
                customernationality = model.Case.Nationality,
                searchtype = "F"
            }, ScreeningService.BACKLIST_SCREENING).Result;
            if (!string.IsNullOrEmpty(datas))
            {
                List<ApiResultModel> apiResultModel = JsonConvert.DeserializeObject<List<ApiResultModel>>(datas);
                model.apiResultModels = apiResultModel;
            }

            return View(model);

        }

        public JsonResult Checkduplicatenames(string fullname,string type)
        {
            var clientId = _clientHandler.GetClientId();

            List<CaseModel> abc = new List<CaseModel>();

            abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetDuplicateNames(fullname,type, clientId));

            


            return Json(abc);
        }

        [HttpGet("/CompletedCaseIndex")]
        public ActionResult CompletedCaseIndex()
        {

            ReportPageViewModel model = new ReportPageViewModel
            {
                ReportData = new ReportLogSearchModel()  // ✅ FIX
            };
            var clientId = _clientHandler.GetClientId();
            model.ReportData.StartDate = System.DateTime.Now.AddYears(-1);
            //model.StartDate = System.DateTime.Now.AddDays(-7);
            model.ReportData.EndDate = System.DateTime.Now;
            //model.CustomerCategories = new SelectList(_mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result), "Code", "Name");
            model.ReportData.CustomerCategories = new SelectList(
    _mapper.Map<List<CustomerCategoryModel>>(_customerCategoryService.GetAll().Result)
        .Where(x => x.Name == "INDIVIDUAL" || x.Name == "CORPORATE")
        .ToList(),
    "Code",
    "Name"
);


            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            model.ReportData.UserGroupName = _UserGroupModel.Name;

            IEnumerable<SelectListItem> userList = from s in _mapper.Map<List<UserModel>>(_userService.GetAll(clientId))
                                                   select new SelectListItem
                                                   {
                                                       Value = Convert.ToString(s.Id),
                                                       Text = s.FName + " " + s.LName.ToString()
                                                   };
            model.ReportData.Users = new SelectList(userList, "Value", "Text");
            var items = from CompletedCaseStatus d in Enum.GetValues(typeof(CompletedCaseStatus))
                        select new
                        {
                            Id = (int)d,
                            Name = Regex.Replace(d.ToString(), "(\\B[A-Z])", " $1")
                        };

            model.ReportData.CaseStatusList = new SelectList(items, "Id", "Name");





            return View(model);

            
            


          
        }

        //public static string GetEnumDisplayName(Enum value)
        //{
        //    return value.GetType()
        //        .GetMember(value.ToString())
        //        .First()
        //        .GetCustomAttribute<DisplayAttribute>()?
        //        .GetName() ?? value.ToString();
        //}

        [HttpPost("/case/completedcasescustompagination")]
        //ToDo
        public JsonResult completedcasescustompagination(DataTableModel model, string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string caseStatusChange, string riskLevel, string includingDuplicate)
        {
            var userId = _clientHandler.GetUserId();
            var clientId = _clientHandler.GetClientId();
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            List<CaseModel> abc = new List<CaseModel>();
            if (endDate == null)
            {
                endDate = System.DateTime.Now.ToString();
            }
            if (cust_type == "CORPORATE")
            {
                cust_type = "C";
            }
            else if (cust_type == "INDIVIDUAL")
            {
                cust_type = "I";
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
            else if (riskLevel == "4")
            {
                riskLevel = "Unclassified";
            }
            if (caseStatusChange == "0")
            {
                caseStatusChange = null;
            }
            if (searchValue != "" && searchValue != null)
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, clientId,includingDuplicate));

            }
            else
            {
                abc = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, clientId,includingDuplicate));

            }
            int totalcount = abc.Count;
            if (!string.IsNullOrEmpty(model.search.value))
            {
                var words = model.search.value.Trim().Split(' ');
                foreach (var item in words)
                {
                    abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
                || m.FirstName.ToLower().Contains(item.ToLower())
                || m.LastName.ToLower().Contains(item.ToLower())
                || m.MiddleName.ToLower().Contains(item.ToLower())
                || m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
                ).ToList();
                }
                //abc = abc.Where(m => m.CustomerId.ToLower().Contains(model.search.value.ToLower())
                //|| m.FirstName.ToLower().Contains(model.search.value.ToLower())
                //|| m.LastName.ToLower().Contains(model.search.value.ToLower())
                //|| m.MiddleName.ToLower().Contains(model.search.value.ToLower()) 
                //|| m.CompanyCode.ToString().ToLower().Contains(model.search.value.ToLower())
                //).ToList();
            }
            int filteredcount = abc.Count;
            //var data = abc.Skip(model.start).Take(model.length).ToList();

            var sortColumn = model?.order?.Any() == true
    ? model.columns[model.order[0].column].data ?? "updatedOnSortKey"
    : "updatedOnSortKey";

            var sortDir = model?.order?.Any() == true
                ? model.order[0].dir ?? "desc"
                : "desc";

            var data = SortData(abc, sortColumn, sortDir)
                        .Skip(model.start)
                        .Take(model.length)
                        .ToList();

            //     var data = Sort(abc, model.columns[model.order[0].column].data ?? "createdOn", model.order[0].dir ?? "desc")

            //.Skip(model.start)

            //.Take(model.length)

            //.ToList();

            //      var data = Sort(abc, model.columns[model.order[0].column].data ?? "createdOnDB", model.order[0].dir ?? "dec")

            //.Skip(model.start)

            //.Take(model.length)

            //.ToList();
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

        public ActionResult ViewIndividualCaseDetail(string id, int type)
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

        //[HttpPost]
        //public async Task<IActionResult> UploadMRZ(List<IFormFile> file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //            return Json(new { success = false, message = "No file uploaded" });

        //        string base64String;

        //        using (var ms = new MemoryStream())
        //        {
        //            await file.CopyToAsync(ms);
        //            base64String = Convert.ToBase64String(ms.ToArray());
        //        }

        //        using (var client = new HttpClient())
        //        {
        //            using (var formData = new MultipartFormDataContent())
        //            {
        //                var streamContent = new StreamContent(file.OpenReadStream());
        //                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        //                formData.Add(streamContent, "file", file.FileName);

        //                string apiUrl = $"https://astrid-unpavilioned-pearlene.ngrok-free.dev/process_document";

        //                var response = await client.PostAsync(apiUrl, formData);

        //                var result = await response.Content.ReadAsStringAsync();

        //                return Content(result, "application/json");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }
        //}

        [HttpPost]
        //public async Task<IActionResult> UploadMRZ(List<IFormFile> files)
        //{
        //    try
        //    {
        //        if (files == null || files.Count == 0)
        //            return Json(new { success = false, message = "No file uploaded" });

        //        using (var client = new HttpClient()) {
        //            client.Timeout = TimeSpan.FromMinutes(20);
        //            using (var formData = new MultipartFormDataContent())
        //            {
        //                foreach (var file in files)
        //                {
        //                    if (file.Length > 0)
        //                    {
        //                        var streamContent = new StreamContent(file.OpenReadStream());
        //                        streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);

        //                        // "files" should match API parameter name
        //                        formData.Add(streamContent, "files", file.FileName);
        //                    }
        //                }

        //                string apiUrl = "https://astrid-unpavilioned-pearlene.ngrok-free.dev/process_document";

        //                var response = await client.PostAsync(apiUrl, formData);

        //                var result = await response.Content.ReadAsStringAsync();

        //                return Content(result, "application/json");
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, error = ex.Message });
        //    }
        //}
        public async Task<IActionResult> UploadMRZ(List<IFormFile> files)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return Json(new { success = false, message = "No file uploaded" });

                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromMinutes(20);

                using var formData = new MultipartFormDataContent();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var streamContent = new StreamContent(file.OpenReadStream());
                        streamContent.Headers.ContentType =
                            new MediaTypeHeaderValue(file.ContentType);

                        formData.Add(streamContent, "file", file.FileName);
                    }
                }

                string apiUrl =
                "https://astrid-unpavilioned-pearlene.ngrok-free.dev/process_document";

                var response = await client.PostAsync(apiUrl, formData);

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new
                    {
                        success = false,
                        message = "External API error",
                        status = response.StatusCode
                    });
                }

                var result = await response.Content.ReadAsStringAsync();

                return Content(result, "application/json");
            }
            catch (TaskCanceledException)
            {
                return Json(new
                {
                    success = false,
                    message = "Request timed out while processing document"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult GetCountryNameByIso(string isoCode)
        {
            var clientId = _clientHandler.GetClientId();
            var country = _countryService.GetCountryNameByCode(isoCode, clientId);

            return Json(country?.Name);
        }

        //[HttpPost]
        //public async Task<IActionResult> UploadMRZ(IFormFile file)
        //{
        //    try
        //    {
        //        if (file == null || file.Length == 0)
        //            return BadRequest(new { success = false, message = "No file uploaded" });

        //        var apiKey = "b967cd9398544f0325322c3ad88321f6a2f5227dabaf2ce79b68fce81cfa6861";
        //        var apiUrl = $"https://mrzbk.7iris.ae/extract-mrz?key={apiKey}";

        //        using var client = new HttpClient();
        //        using var content = new MultipartFormDataContent();

        //        using var stream = file.OpenReadStream();
        //        content.Add(new StreamContent(stream), "file", file.FileName);

        //        var response = await client.PostAsync(apiUrl, content);

        //        if (!response.IsSuccessStatusCode)
        //            return BadRequest(new { success = false, message = "MRZ API Failed" });

        //        var result = await response.Content.ReadAsStringAsync();

        //        return Json(new { success = true, data = result });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { success = false, message = ex.Message });
        //    }
        //}

        

        

        
        

    [HttpGet]
    public async Task<IActionResult> DueDiligence_PDF(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string includingDuplicate, string selectedColumns = null, string orientation = "portrait")
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();
        var GroupId = _clientHandler.GetGroupId();
        var _userGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString();
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";
        else if (riskLevel == "4") riskLevel = "Unclassified";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId, includingDuplicate));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId, includingDuplicate));
        }

        ViewBag.SelectedColumns = selectedColumns;
        ViewBag.Orientation = orientation;
        return View("DueDiligence_PDF", cases);
    }

    [HttpGet]
    public async Task<IActionResult> CompletedCases_PDF(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string includingDuplicate, string selectedColumns = null, string orientation = "portrait")
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString();
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";
        else if (riskLevel == "4") riskLevel = "Unclassified";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, clientId, includingDuplicate));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, clientId, includingDuplicate));
        }

        ViewBag.SelectedColumns = selectedColumns;
        ViewBag.Orientation = orientation;
        return View("CompletedCases_PDF", cases);
    }
    [HttpGet]
    public async Task<IActionResult> ExportDueDiligenceExcel(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string includingDuplicate, string selectedColumns = null)
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();
        var GroupId = _clientHandler.GetGroupId();
        var _userGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString("yyyy-MM-dd");
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";
        else if (riskLevel == "4") riskLevel = "Unclassified";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId, includingDuplicate));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAll(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, _userGroupModel.Name, clientId, includingDuplicate));
        }

        var columnMap = new Dictionary<string, (string Header, Func<CaseModel, object> Value)>
        {
            { "CustomerId", ("Customer ID", c => c.CustomerId) },
            { "CreatedOn", ("Created On", c => c.CreatedOn.ToString("dd MMM yyyy HH:mm:ss")) },
            { "UpdatedOnDB", ("Updated On", c => DateTime.TryParse(c.UpdatedOnDB, out var dt) ? dt.ToString("dd MMM yyyy HH:mm:ss") : (c.UpdatedOnDB ?? "-")) },
            { "CustomerType", ("Customer Type", c => c.CustomerType == "I" ? "Individual" : "Corporate") },
            { "CustomerName", ("Customer Name", c => (c.FirstName + " " + c.LastName).Trim()) },
            { "CaseChangeStatus", ("Datasets", c => c.CaseChangeStatus) },
            { "MatchScore", ("Screening Score", c => c.MatchScore) },
            { "riskScore", ("Risk Rating", c => c.Individual_final_risk_score ?? c.corporate_final_risk_score ?? "Unclassified") },
            { "CreatedUser", ("User", c => c.CreatedUser) },
            { "CaseStatus", ("Status", c => FormatExcelStatus(c.CaseStatus)) }
        };

        var selectedCols = string.IsNullOrEmpty(selectedColumns) 
            ? columnMap.Keys.ToList() 
            : selectedColumns.Split(',').ToList();

        using (var package = new ExcelPackage())
        {
            var sheet = package.Workbook.Worksheets.Add("Due Diligence Report");
            int colIndex = 1;
            foreach (var colId in selectedCols)
            {
                if (columnMap.ContainsKey(colId))
                {
                    sheet.Cells[1, colIndex].Value = columnMap[colId].Header;
                    colIndex++;
                }
            }

            using (var range = sheet.Cells[1, 1, 1, Math.Max(1, colIndex - 1)])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0xE9, 0xEF, 0xFD));
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var item in cases)
            {
                colIndex = 1;
                foreach (var colId in selectedCols)
                {
                    if (columnMap.ContainsKey(colId))
                    {
                        sheet.Cells[row, colIndex].Value = columnMap[colId].Value(item);
                        colIndex++;
                    }
                }
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"DueDiligence_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }

    [HttpGet]
    public async Task<IActionResult> ExportCompletedCasesExcel(string startDate, string endDate, string cust_type, string searchValue, int createdBy, string matchScore, int caseStatus, string riskLevel, string includingDuplicate, string selectedColumns = null)
    {
        var userId = _clientHandler.GetUserId();
        var clientId = _clientHandler.GetClientId();

        if (string.IsNullOrEmpty(endDate)) endDate = DateTime.Now.ToString("yyyy-MM-dd");
        if (cust_type == "CORPORATE") cust_type = "C";
        else if (cust_type == "INDIVIDUAL") cust_type = "I";

        if (riskLevel == "1") riskLevel = "Low Risk";
        else if (riskLevel == "2") riskLevel = "Medium Risk";
        else if (riskLevel == "3") riskLevel = "High Risk";
        else if (riskLevel == "4") riskLevel = "Unclassified";

        List<CaseModel> cases = new List<CaseModel>();
        if (!string.IsNullOrEmpty(searchValue))
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedBySearchValue(userId, startDate, endDate, cust_type, searchValue, matchScore, createdBy, caseStatus, riskLevel, clientId, includingDuplicate));
        }
        else
        {
            cases = _mapper.Map<List<CaseModel>>(_customerCaseService.GetAllCompletedCases(userId, startDate, endDate, cust_type, matchScore, createdBy, caseStatus, riskLevel, clientId, includingDuplicate));
        }

        var columnMap = new Dictionary<string, (string Header, Func<CaseModel, object> Value)>
        {
            { "CustomerId", ("Customer ID", c => c.CustomerId) },
            { "CreatedOn", ("Created On", c => c.CreatedOn.ToString("dd MMM yyyy HH:mm:ss")) },
            { "UpdatedOnDB", ("Updated On", c => DateTime.TryParse(c.UpdatedOnDB, out var dt) ? dt.ToString("dd MMM yyyy HH:mm:ss") : (c.UpdatedOnDB ?? "-")) },
            { "CustomerType", ("Customer Type", c => c.CustomerType == "I" ? "Individual" : "Corporate") },
            { "CustomerName", ("Customer Name", c => (c.FirstName + " " + c.LastName).Trim()) },
            { "CaseChangeStatus", ("Datasets", c => c.CaseChangeStatus) },
            { "MatchScore", ("Screening Score", c => c.MatchScore) },
            { "riskScore", ("Risk Rating", c => c.Individual_final_risk_score ?? c.corporate_final_risk_score ?? "Unclassified") },
            { "CreatedUser", ("User", c => c.CreatedUser) },
            { "CaseStatus", ("Status", c => FormatExcelStatus(c.CaseStatus)) }
        };

        var selectedCols = string.IsNullOrEmpty(selectedColumns) 
            ? columnMap.Keys.ToList() 
            : selectedColumns.Split(',').ToList();

        using (var package = new ExcelPackage())
        {
            var sheet = package.Workbook.Worksheets.Add("Completed Cases Report");
            int colIndex = 1;
            foreach (var colId in selectedCols)
            {
                if (columnMap.ContainsKey(colId))
                {
                    sheet.Cells[1, colIndex].Value = columnMap[colId].Header;
                    colIndex++;
                }
            }

            using (var range = sheet.Cells[1, 1, 1, Math.Max(1, colIndex - 1)])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(0xE9, 0xEF, 0xFD));
                range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            }

            int row = 2;
            foreach (var item in cases)
            {
                colIndex = 1;
                foreach (var colId in selectedCols)
                {
                    if (columnMap.ContainsKey(colId))
                    {
                        sheet.Cells[row, colIndex].Value = columnMap[colId].Value(item);
                        colIndex++;
                    }
                }
                row++;
            }

            sheet.Cells[sheet.Dimension.Address].AutoFitColumns();
            return File(package.GetAsByteArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"CompletedCases_Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }

    private string FormatExcelStatus(string status)
    {
        if (string.IsNullOrEmpty(status)) return "";
        if (!status.Contains("| Shareholders:")) return status;

        var parts = status.Split('|');
        var mainStatus = parts[0].Trim();
        var shInfo = parts[1].Replace("Shareholders:", "").Trim();
        
        var metrics = new List<string>();
        var patterns = new Dictionary<string, string> { 
            { "Approved", "AP" }, { "Auto", "A" }, { "Pending", "P" }, 
            { "Rejected", "R" }, { "OnHold", "OH" }, { "Waitlist", "W" }, { "Whitelist", "W" } 
        };

        foreach (var p in patterns)
        {
            var match = System.Text.RegularExpressions.Regex.Match(shInfo, p.Key + @"\s+(\d+)");
            if (match.Success && int.Parse(match.Groups[1].Value) > 0)
            {
                metrics.Add($"{p.Value}:{match.Groups[1].Value}");
            }
        }

        return metrics.Count > 0 ? $"{mainStatus} ({string.Join(", ", metrics)})" : mainStatus;
    }

        [HttpGet]
        public IActionResult GetCaseLatestById(int id,int version)
        {
            try
            {
                var clientId = _clientHandler.GetClientId();

                CustomerCaseDTO _CustomerCaseDTO = _customerCaseService.GetDetailsByVersion(id,version);

                if (_CustomerCaseDTO == null)
                    return NotFound();

                var customerType = _CustomerCaseDTO.CustomerType;
                // ✅ Fetch dropdowns based on customerType
                var nationalities = _mapper.Map<List<CountryModel>>(_countryService.GetAll(clientId))
                                          .Select(x => x.Name).ToList();

                var idTypes = _mapper.Map<List<IdentityTypeModel>>(_idTypeService.GetAll(clientId))
                                     .Select(x => x.Name).ToList();

                var productTypes = _mapper.Map<List<ProductType>>(
                                        _kycService.GetAllProduct(culture, customerType, clientId))
                                        .Select(x => x.ProductName).ToList();

                var deliveryChannels = _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                                        _kycService.GetAllDeliveryChannel(culture, customerType, clientId))
                                        .Select(x => x.DeliveryChannelName).ToList();

                var modeOfPayments = _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                                        _kycService.get_all_mode_of_payment(culture, clientId, customerType))
                                        .Select(x => x.DeliveryChannelName).ToList();
                var ProfessionalList = _mapper.Map<List<ViewModel.ViewModels.Kyc.DeliveryChannel>>(
                                        _kycService.GetProfessionalStatus(culture,customerType, clientId))
                                        .Select(x => x.DeliveryChannelName).ToList();
                var businesstype= _mapper.Map<List<ViewModel.ViewModels.Kyc.BusinessNature>>(
                                        _kycService.GetBusinessType(culture, customerType, clientId))
                                        .Select(x => x.BusinessName).ToList();

                var LegalStatus = _mapper.Map<List<ViewModel.ViewModels.Kyc.LegalStatusModel>>(
                                        _kycService.GetLegalStatus(culture, customerType, clientId))
                                        .Select(x => x.LegalStatus).ToList();


                

                var response = new CaseResponseDto
                {
                    Id=_CustomerCaseDTO.Id,
                    Name = _CustomerCaseDTO.FirstName + " " + _CustomerCaseDTO.LastName,
                    Nationality = _CustomerCaseDTO.Nationality,
                    Dob = _CustomerCaseDTO.DOB.ToString("yyyy-MM-dd"),
                    Gender = _CustomerCaseDTO.Gender,
                    ProductValue = _CustomerCaseDTO.ProductValue,
                   CustomerType = _CustomerCaseDTO.CustomerType,
                   OccupatinTypeTxt= _CustomerCaseDTO.OccupatinTypeTxt,
                   ProductName=_CustomerCaseDTO.ProductName,
                   DeliveryChannelName=_CustomerCaseDTO.DeliveryChannelName,
                   ResidenceStatus=_CustomerCaseDTO.ResidenceStatus,
                   EntityTypeTxt=_CustomerCaseDTO.EntityTypeTxt,
                   BusinessType=_CustomerCaseDTO.BusinessType,
                   Modeofpayment=_CustomerCaseDTO.Modeofpayment,
                   CustomerIdType=_CustomerCaseDTO.CustomerIdType,
                   PassportId=_CustomerCaseDTO.PassportId,
                   PassportExpiryDate=_CustomerCaseDTO.PassportExpiryDate,
                   PassportIssueDate=_CustomerCaseDTO.PassportIssueDate,
                   EmiratesIdNumber=_CustomerCaseDTO.EmiratesIdNumber,
                   EmiratesIdExpiryDate=_CustomerCaseDTO.EmiratesIdExpiryDate,
                   EmiratesIdIssueDate=_CustomerCaseDTO.EmiratesIdIssueDate,
                   CounterPartyName=_CustomerCaseDTO.CounterPartyName,
                   CounterParty=_CustomerCaseDTO.CounterParty,
                   SOWSOFCountry=_CustomerCaseDTO.SOWSOFCountry,
                   CIFNumber=_CustomerCaseDTO.CIFNumber,
                   Employer=_CustomerCaseDTO.Employer,
                   EmployerIndustry=_CustomerCaseDTO.EmployerIndustry,
                   EmployerSector=_CustomerCaseDTO.EmployerSector,
                   GoldenVisa=_CustomerCaseDTO.GoldenVisa,
                    TradeLicenseAuthority=_CustomerCaseDTO.TradeLicenseAuthority,
                    Type=_CustomerCaseDTO.Type,
                    FlagType=_CustomerCaseDTO.FlagType,
                    ProductRefNo=_CustomerCaseDTO.ProductRefNo,
                    PlaceOfBirth=_CustomerCaseDTO.PlaceOfBirth,
                    Version=_CustomerCaseDTO.Version,
                    // ✅ dropdowns
                    Nationalities = nationalities,
                    IdTypes = idTypes,
                    ProductTypes = productTypes,
                    DeliveryChannels = deliveryChannels,
                    ModeOfPayments = modeOfPayments,
                    Professions= ProfessionalList,
                    BusinessTypeList= businesstype,
                    LegalStatusList=LegalStatus

                };

                return Json(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> UpdateAndReplace(CaseModel model)
        {
            model.ClientId = _clientHandler.GetClientId();
            model.UpdatedBy = _clientHandler.GetUserId();
            
            var response = "";
            int newCaseId = 0;
            var submissionReasonComment = "";

            var previousRecord = _customerCaseService.GetDetails(model.Id);

            var previousComments = _caseCommentService.GetAllByCase(model.Id);

            var previousDocuments = _caseDocumentService.GetCaseDocumentByCaseId(model.Id);
            if (model.Mode == "edit" &&
                            !string.IsNullOrWhiteSpace(model.SubmissionReasons))
            {
                 submissionReasonComment =
                    "Submission Reasons: " + model.SubmissionReasons;

                // If "Others" is selected, append custom reason
                if (model.SubmissionReasons
                        .Split(',')
                        .Any(x => x.Trim().Equals(
                            "Others",
                            StringComparison.OrdinalIgnoreCase))
                    && !string.IsNullOrWhiteSpace(model.OtherReason))
                {
                    submissionReasonComment += Environment.NewLine +
                                               "Other Reason: " +
                                               model.OtherReason.Trim();
                }
            }

                if (previousRecord == null)
            {
                return Json("Record not found");
            }
            bool isRiskChanged = false;
            
                isRiskChanged = HasRiskFieldsChanged(previousRecord, model);
            
            
            bool shouldCreateNewCase =
       model.Mode == "replace"
    || (model.Mode == "edit" && isRiskChanged);
            CustomerCaseDTO _ccDTO = _mapper.Map<CustomerCaseDTO>(previousRecord);
            //if (!string.IsNullOrWhiteSpace(model.SelectedVersions))
            //{
            //    // Optional: If multiple versions are sent as comma-separated values
            //    var selectedVersions = model.SelectedVersions
            //                                .Split(',', StringSplitOptions.RemoveEmptyEntries)
            //                                .Select(v => v.Trim())
            //                                .ToList();

            //    // If current model.Version is one of the selected versions, perform update
            //    // (If you do not need this check, you can remove the if statement below)
            //    foreach (var version in selectedVersions)
            //    {
            //        var VersionRecord = _customerCaseService.GetDetailsByCustidVersion(model.CustomerId, Convert.ToInt32(version));
            //        CustomerMasterDTO _mmDTO = _mapper.Map<CustomerMasterDTO>(VersionRecord);
            //        if (model.Mode == "edit")
            //        {
            //            _mmDTO.Address = model.Address;
            //            _mmDTO.ProductName = model.ProductName;
            //            _mmDTO.DeliveryChannelName = model.DeliveryChannelName;
            //            _mmDTO.Modeofpayment = model.Modeofpayment;
            //            _mmDTO.CIFNumber = model.CIFNumber;
            //            _mmDTO.CounterParty = model.CounterParty;
            //            _mmDTO.CounterPartyName = model.CounterPartyName;
            //            _mmDTO.ProductRefNo = model.ProductRefNo;


            //            //_mmDTO.CustomerIdType = model.CustomerIdType;
            //            //_mmDTO.PassportId = model.PassportId;

            //            //_mmDTO.PassportIssueDate = string.IsNullOrWhiteSpace(model.PassportIssueDate)
            //            //    ? null
            //            //    : Convert.ToDateTime(model.PassportIssueDate);

            //            //_mmDTO.PassportExpiryDate = string.IsNullOrWhiteSpace(model.PassportExpiryDate)
            //            //    ? null
            //            //    : Convert.ToDateTime(model.PassportExpiryDate);

            //            //_mmDTO.EmiratesIdNumber = model.EmiratesIdNumber;

            //            //_mmDTO.EmiratesIdIssueDate = string.IsNullOrWhiteSpace(model.EmiratesIdIssueDate)
            //            //    ? null
            //            //    : Convert.ToDateTime(model.EmiratesIdIssueDate);

            //            //_mmDTO.EmiratesIdExpiryDate = string.IsNullOrWhiteSpace(model.EmiratesIdExpiryDate)
            //            //    ? null
            //            //    : Convert.ToDateTime(model.EmiratesIdExpiryDate);

            //            _mmDTO.SOWSOFCountry = model.SOWSOFCountry;
            //            _mmDTO.ResidenceStatus = model.ResidenceStatus;
            //            _mmDTO.OccupatinTypeTxt = model.OccupatinTypeTxt;
            //            _mmDTO.ProductValue = model.ProductValue;
            //            _mmDTO.Employer = model.Employer;
            //            _mmDTO.EmployerIndustry = model.EmployerIndustry;
            //            _mmDTO.EmployerSector = model.EmployerSector;
            //            _mmDTO.GoldenVisa = model.GoldenVisa;
            //            _mmDTO.Tradelicense = model.Tradelicense;
            //            _mmDTO.TradeLicenseAuthority = model.TradeLicenseAuthority;
            //            _mmDTO.TradeLicenseSector = model.TradeLicenseSector;
            //            _mmDTO.BusinessType = model.BusinessType;
            //            _mmDTO.EntityTypeTxt = model.EntityTypeTxt;
            //            _mmDTO.UpdateReason = submissionReasonComment;
                        


            //            response = "Case updated successfully";

            //            _ccDTO.Version = Convert.ToInt32(version);
            //            // Update existing record
            //            _customerMasterService.UpdateCustomerMaster(_mmDTO);
            //            _customerCaseService.UpdateCase(_mmDTO.Id, model.UpdatedBy);

            //            if (isRiskChanged)
            //            {
            //                GenerateKycRiskAssessment(_ccDTO, model);
            //            }
            //        }
            //    }
            //}
            //else
            //{

                
                    var VersionRecord = _customerCaseService.GetDetailsByCustidVersion(model.CustomerId, Convert.ToInt32(_ccDTO.Version));
                    CustomerMasterDTO _mmDTO = _mapper.Map<CustomerMasterDTO>(VersionRecord);
                    if (model.Mode == "edit")
                    {
                        _mmDTO.Address = model.Address;
                        _mmDTO.ProductName = model.ProductName;
                        _mmDTO.DeliveryChannelName = model.DeliveryChannelName;
                        _mmDTO.Modeofpayment = model.Modeofpayment;
                        _mmDTO.CIFNumber = model.CIFNumber;
                        _mmDTO.CounterParty = model.CounterParty;
                        _mmDTO.CounterPartyName = model.CounterPartyName;
                        _mmDTO.ProductRefNo = model.ProductRefNo;


                        //_mmDTO.CustomerIdType = model.CustomerIdType;
                        //_mmDTO.PassportId = model.PassportId;

                        //_mmDTO.PassportIssueDate = string.IsNullOrWhiteSpace(model.PassportIssueDate)
                        //    ? null
                        //    : Convert.ToDateTime(model.PassportIssueDate);

                        //_mmDTO.PassportExpiryDate = string.IsNullOrWhiteSpace(model.PassportExpiryDate)
                        //    ? null
                        //    : Convert.ToDateTime(model.PassportExpiryDate);

                        //_mmDTO.EmiratesIdNumber = model.EmiratesIdNumber;

                        //_mmDTO.EmiratesIdIssueDate = string.IsNullOrWhiteSpace(model.EmiratesIdIssueDate)
                        //    ? null
                        //    : Convert.ToDateTime(model.EmiratesIdIssueDate);

                        //_mmDTO.EmiratesIdExpiryDate = string.IsNullOrWhiteSpace(model.EmiratesIdExpiryDate)
                        //    ? null
                        //    : Convert.ToDateTime(model.EmiratesIdExpiryDate);

                        _mmDTO.SOWSOFCountry = model.SOWSOFCountry;
                        _mmDTO.ResidenceStatus = model.ResidenceStatus;
                        _mmDTO.OccupatinTypeTxt = model.OccupatinTypeTxt;
                        _mmDTO.ProductValue = model.ProductValue;
                        _mmDTO.Employer = model.Employer;
                        _mmDTO.EmployerIndustry = model.EmployerIndustry;
                        _mmDTO.EmployerSector = model.EmployerSector;
                        _mmDTO.GoldenVisa = model.GoldenVisa;
                        _mmDTO.Tradelicense = model.Tradelicense;
                        _mmDTO.TradeLicenseAuthority = model.TradeLicenseAuthority;
                        _mmDTO.TradeLicenseSector = model.TradeLicenseSector;
                        _mmDTO.BusinessType = model.BusinessType;
                        _mmDTO.EntityTypeTxt = model.EntityTypeTxt;
                        _mmDTO.UpdateReason = submissionReasonComment;


                        response = "Case updated successfully";

                        _ccDTO.Version = Convert.ToInt32(_mmDTO.Version);
                        _ccDTO.CustomerId = _mmDTO.CustomerId;
                        newCaseId = _ccDTO.Id;

                        // Update existing record
                        _customerMasterService.UpdateCustomerMaster(_mmDTO);
                        _customerCaseService.UpdateCase(newCaseId, model.UpdatedBy);

                    if (isRiskChanged)
                        {
                            _ccDTO.Status = 0;

                            _customerCaseService.Update(_ccDTO);
                        }



                    response = "Case updated successfully";
                }
                else if (model.Mode == "replace")
                {
                    _ccDTO.FirstName = model.FirstName;
                    _ccDTO.Nationality = model.Nationality;
                    _ccDTO.DOB = Convert.ToDateTime(model.DOB);
                    _ccDTO.Gender = model.Gender;
                    _ccDTO.PlaceOfBirth = model.PlaceOfBirth;
                    _ccDTO.CustomerIdType = model.CustomerIdType;
                    _ccDTO.PassportId = model.PassportId;
                    _ccDTO.PassportIssueDate = string.IsNullOrWhiteSpace(model.PassportIssueDate)
                            ? null
                            : Convert.ToDateTime(model.PassportIssueDate);

                    _ccDTO.PassportExpiryDate = string.IsNullOrWhiteSpace(model.PassportExpiryDate)
                        ? null
                        : Convert.ToDateTime(model.PassportExpiryDate);

                    _ccDTO.EmiratesIdNumber = model.EmiratesIdNumber;

                    _ccDTO.EmiratesIdIssueDate = string.IsNullOrWhiteSpace(model.EmiratesIdIssueDate)
                        ? null
                        : Convert.ToDateTime(model.EmiratesIdIssueDate);

                    _ccDTO.EmiratesIdExpiryDate = string.IsNullOrWhiteSpace(model.EmiratesIdExpiryDate)
                        ? null
                        : Convert.ToDateTime(model.EmiratesIdExpiryDate);

                    _ccDTO.Version = previousRecord.Version + 1;
                    _ccDTO.ReplaceFlag = 1;

                        Console.WriteLine("Before Set: " + _ccDTO.Version);

                        Console.WriteLine("After Set: " + _ccDTO.Version);
                        _ccDTO.ClientId = _clientHandler.GetClientId();
                        _ccDTO.Id = 0;

                        var result = _customerCaseService.Create(_ccDTO);
                        string parsedResult = result.Result;
                        if (!string.IsNullOrEmpty(parsedResult))
                        {
                            if (parsedResult.Contains("Ø")) parsedResult = parsedResult.Split('Ø')[1];
                            else if (parsedResult.Contains("??")) parsedResult = parsedResult.Split(new string[] { "??" }, StringSplitOptions.None)[1];
                            else if (parsedResult.Contains("A~")) parsedResult = parsedResult.Split(new string[] { "A~" }, StringSplitOptions.None)[1];
                        }

                        _ccDTO.CustomerId = parsedResult;
                        newCaseId = _customerCaseService.GetCaseId(parsedResult);
                        var customerType = previousRecord.CustomerType == "I"
               ? "INDIVIDUAL"
               : "CORPORATE";

                        string body = string.Empty;
                        using (StreamReader reader = new StreamReader(@"Views/Risk/RiskEmailBody.html"))
                        {
                            body = reader.ReadToEnd();
                        }

                        // ✅ OLD RESULTS
                        var oldApiResult = _clientHandler
                            .PostAsync(new { caseid = model.Id.ToString() }, ScreeningService.GETBYCASEID)
                            .Result;

                        var oldList = string.IsNullOrEmpty(oldApiResult)
                            ? new List<DataListModel>()
                            : JsonConvert.DeserializeObject<List<DataListModel>>(oldApiResult);

                        var oldMatchRecords = MapToMatchRecords(oldList);

                        // ✅ NEW SCREENING
                        var screeningResponse = await _commonService.CustomerScreeningCall(
                            _ccDTO,
                            baseURL,
                            baseC6URL,
                            customerType,
                            body,
                            model.Threshold,
                            parsedResult
                        );


                        

                        // ✅ Email
                        if (screeningResponse.sendMail == 1)
                        {
                            await Task.Run(() => SendScreendedMailAsync(body, model, screeningResponse));
                        }

                        response = "Case replaced successfully";
                }
                else
                {
                    response = "Invalid mode";
                }

                    if (isRiskChanged)
                    {
                       
                            GenerateKycRiskAssessment(_ccDTO, model);
                    }







                    // Add comments only when a new case has been created
                    if (newCaseId > 0)
                    {
                        string additionalComment = model.Mode?.ToLower() == "edit"
                            ? "This case has been edited"
                            : "This case has been replaced";

                        string additionalCommentType = model.Mode?.ToLower() == "edit"
                            ? "Edit"
                            : "Replace";

                        CaseCommentModel additionalCommentDTO = new CaseCommentModel
                        {
                            CaseId = newCaseId,
                            Comment = additionalComment,
                            CommentType = additionalCommentType,
                            CreatedBy = _clientHandler.GetUserId(),
                            CustomerId = _ccDTO.CustomerId
                        };

                        _caseCommentService.Create(
                            _mapper.Map<CaseCommentDTO>(additionalCommentDTO)
                        );

                        // Save submission reasons only for Edit mode
                        

                            CaseCommentModel submissionReasonCommentDTO =
                                new CaseCommentModel
                                {
                                    CaseId = newCaseId,
                                    Comment = submissionReasonComment,
                                    CommentType = additionalCommentType,
                                    CreatedBy = _clientHandler.GetUserId(),
                                    CustomerId = _ccDTO.CustomerId
                                };

                            _caseCommentService.Create(
                                _mapper.Map<CaseCommentDTO>(
                                    submissionReasonCommentDTO
                                )
                            );
                        
                    }
                
            //}
            


            //if (previousDocuments != null && previousDocuments.Any())
            //{
            //    foreach (var doc in previousDocuments)
            //    {
            //        CaseDocumentModel newDoc = new CaseDocumentModel
            //        {
            //            CaseId = newCaseId.ToString(),
            //            CreatedBy = _clientHandler.GetUserId(),
            //            CreatedOn = DateTime.Now,
            //            ClientId = _clientHandler.GetClientId(),

            //            // Copy existing document details
            //            DocumentFileName = doc.DocumentFileName,
            //            DocumentFullPath = doc.DocumentFullPath,
            //            IssuedDateOnDB = doc.IssuedDateOnDB,
            //            ExpiryDateOnDB = doc.ExpiryDateOnDB,
            //            DocumentName = doc.DocumentName

            //        };

            //        _caseDocumentService.Create(_mapper.Map<CaseDocumentDTO>(newDoc));
            //    }
            //}

            response = model.Mode == "edit"
                    ? "Case updated successfully"
                    : "Case replaced successfully";

            TempData["SuccessMessage"] = response;

            if (model.ActionScreen == "DueDiligence")
            {
                return RedirectToAction("Index", "Case");
            }
            else if (model.ActionScreen == "CompletedCases")
            {
                return RedirectToAction("CompletedCaseIndex", "Case");
            }



            return null;
        }

        private List<MatchRecordsDTO> MapToMatchRecords(List<DataListModel> list)
        {
            return list.Select(x => new MatchRecordsDTO
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
                        x.matchdatasets.Split(',')
                        .Select(d => d.Contains("-")
                            ? d.Split('-')[0].Trim()
                            : d.Trim()))
                    : "--",
                MATCHGENDER = x.matchgender ?? ""
            }).ToList();
        }
        private bool HasRiskFieldsChanged(CustomerCaseDTO oldRecord, CaseModel model)
        {

            string Normalize(string value)
            {
                return string.IsNullOrWhiteSpace(value)
                    ? null
                    : value.Trim();
            }
            if (oldRecord.CustomerType == "C") // Corporate
            {
                return
                    Normalize(oldRecord.EntityTypeTxt) != Normalize(model.EntityTypeTxt) ||
                    Normalize(oldRecord.BusinessType) != Normalize(model.BusinessType) ||
                    Normalize(oldRecord.Nationality) != Normalize(model.Nationality) ||
                    Normalize(oldRecord.ProductName) != Normalize(model.ProductName) ||
                    Normalize(oldRecord.DeliveryChannelName) != Normalize(model.DeliveryChannelName) ||
                    Normalize(oldRecord.Modeofpayment) != Normalize(model.Modeofpayment);
            }
            else // Individual
            {
                return
                    Normalize(oldRecord.OccupatinTypeTxt) != Normalize(model.OccupatinTypeTxt) ||
                    Normalize(oldRecord.Nationality) != Normalize(model.Nationality) ||
                    Normalize(oldRecord.ResidenceStatus) != Normalize(model.ResidenceStatus) ||
                    Normalize(oldRecord.ProductName) != Normalize(model.ProductName) ||
                    Normalize(oldRecord.DeliveryChannelName) != Normalize(model.DeliveryChannelName) ||
                    Normalize(oldRecord.Modeofpayment) != Normalize(model.Modeofpayment);
            }
        }
        public void GenerateKycRiskAssessment(CustomerCaseDTO _ccDTO, CaseModel model)
        {
            dynamic modelrisk = null;
            CorporateKycDTO corpModel = new CorporateKycDTO();
            KycIndividualDTO imodel = new KycIndividualDTO();
            RiskModel _riskmodel = new RiskModel();

            if (_ccDTO.Type == "Individual" || _ccDTO.Type == "Corporate")
            {
                if (_ccDTO.CustomerType == "C")
                {
                    int riskid = _riskService.GetRiskIdByCustomercode(_ccDTO.CustomerId, _ccDTO.CustomerType).Result;
                    if (riskid != 0)
                    {

                        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("C", 1, 0, 0, model.ClientId);


                        modelrisk = _mapper.Map<RiskCorpCustomerModel>(_riskService.GetRiskDetailsOfCorporateByCID(_ccDTO.CustomerId,_ccDTO.Version).Result);

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
                                            corpModel.EntityTypeTxt =
                                                string.IsNullOrWhiteSpace(model.EntityTypeTxt)
                                                    ? riskType.ItemTxt
                                                    : model.EntityTypeTxt;
                                            break;

                                        case "nature of business":

                                            corpModel.BusinessType =
                                                string.IsNullOrWhiteSpace(model.BusinessType)
                                                    ? riskType.ItemTxt
                                                    : model.BusinessType;
                                            break;

                                        case "country of incorporation":

                                            corpModel.PlaceofIncorporation =
                                                string.IsNullOrWhiteSpace(model.Nationality)
                                                    ? riskType.ItemTxt
                                                    : model.Nationality;
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
                                            corpModel.ProductName =
                                               string.IsNullOrWhiteSpace(model.ProductName)
                                                   ? riskType.ItemTxt
                                                   : model.ProductName;

                                            break;

                                        case "if more than one product(put the riskiest product)":
                                            corpModel.HighestRiskProduct =
                                               string.IsNullOrWhiteSpace(model.HighestRiskProduct)
                                                   ? riskType.ItemTxt
                                                   : model.HighestRiskProduct;

                                            break;

                                        case "delivery channel":
                                            corpModel.DeliveryChannelName =
                                               string.IsNullOrWhiteSpace(model.DeliveryChannelName)
                                                   ? riskType.ItemTxt
                                                   : model.DeliveryChannelName;

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
                                            corpModel.Modeofpayment =
                                               string.IsNullOrWhiteSpace(model.Modeofpayment)
                                                   ? riskType.ItemTxt
                                                   : model.Modeofpayment;
                                            break;
                                        case "dual use goods match":
                                            corpModel.DualUseGoods = riskType.ItemTxt;
                                            break;
                                        case "more dual use goods match":
                                            imodel.MoreDualUseGoods = riskType.ItemTxt;
                                            break;

                                        default:
                                            // Optional: log unmatched value
                                            break;
                                    }
                                }
                            }
                        }

                    }


                    //var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientId));

                    //if (result != null)
                    //{
                    Console.WriteLine("Generate risk");



                    //model.IsPeP = isPep;
                    var str1 = _kycService.GetRiskLovId(imodel, _mapper.Map<CorporateKycDTO>(corpModel), "C", culture, model.ClientId);
                    if (str1.Result == null)
                    {
                        //return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                        return;
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
                    var domesticpeplovId = GetSafely(spStr1, 18);
                    var foreignlovId = GetSafely(spStr1, 20);
                    var redflagslovId = GetSafely(spStr1, 22);
                    var sanctionlovId = GetSafely(spStr1, 24);
                    var UAEORUNSClovId = GetSafely(spStr1, 26);
                    var corpfaftlovId = GetSafely(spStr1, 27);
                    var highestriskproductlovId = GetSafely(spStr1, 29);
                    var veryhighnetworkIdlovId = GetSafely(spStr1, 31);

                    var dualusegoodslovId = GetSafely(spStr1, 32);
                    var MoredualusegoodslovId = GetSafely(spStr1, 34);
                    var militarygoodslovId = GetSafely(spStr1, 36);
                    var MoremilitarygoodslovId = GetSafely(spStr1, 38);

                    var str = _kycService.GetRiskTypeId(imodel, _mapper.Map<CorporateKycDTO>(corpModel), "C", culture, model.ClientId);

                    if (str.Result == null)
                    {
                        //return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                        return;
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
                    var militarygoodsId = GetSafely(spStr, 36);
                    var MoremilitarygoodsId = GetSafely(spStr, 38);


                    RiskAPIRequestModel riskModel = new RiskAPIRequestModel();
                    riskModel.CustomerId = _ccDTO.CustomerId;
                    riskModel.CustomerName = _ccDTO.FirstName;
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
                    if (militarygoodsId != "0" && militarygoodsId != "")
                    {
                        var riskTypeMil = new RiskTypeListModel();
                        riskTypeMil.Id = Convert.ToString(militarygoodslovId);
                        var riskItemMil = new RiskItemListModel();
                        riskItemMil.Id = militarygoodsId.ToString();
                        var riskItemListMil = new List<RiskItemListModel>();
                        riskItemListMil.Add(riskItemMil);
                        riskTypeMil.RiskItemList = riskItemListMil;
                        riskTypeList.Add(riskTypeMil);
                    }
                    if (MoremilitarygoodsId != "0" && MoremilitarygoodsId != "")
                    {
                        var riskTypeMoreMil = new RiskTypeListModel();
                        riskTypeMoreMil.Id = Convert.ToString(MoremilitarygoodslovId);
                        var riskItemMoreMil = new RiskItemListModel();
                        riskItemMoreMil.Id = MoremilitarygoodsId.ToString();
                        var riskItemListMoreMil = new List<RiskItemListModel>();
                        riskItemListMoreMil.Add(riskItemMoreMil);
                        riskTypeMoreMil.RiskItemList = riskItemListMoreMil;
                        riskTypeList.Add(riskTypeMoreMil);
                    }

                    //for legal status of entity start

                    //for mode of payment end

                    riskModel.RiskTypeList = riskTypeList;
                    riskModel.CaseVersion = _ccDTO.Version;
                    riskModel.SelectedVersions = model.SelectedVersions;
                    var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                    var xyz = riskResult;

                    //}
                }
                else
                {
                    int riskid = _riskService.GetRiskIdByCustomercode(_ccDTO.CustomerId, _ccDTO.CustomerType).Result;
                    if (riskid != 0)
                    {

                        _riskmodel.RiskTypeCategoryDTO = _lovMasterService.GetAllRiskConfig("I", 1, 0, 0, model.ClientId);


                        modelrisk = _mapper.Map<RiskModel>(_riskService.GetRiskDetailsOfIndividualByCID(_ccDTO.CustomerId,_ccDTO.Version).Result);

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
                                            imodel.OccupatinTypeTxt =
                                               string.IsNullOrWhiteSpace(model.OccupatinTypeTxt)
                                                   ? riskType.ItemTxt
                                                   : model.OccupatinTypeTxt;

                                            break;
                                        case "nationality":

                                            imodel.Nationality =
                                               string.IsNullOrWhiteSpace(model.Nationality)
                                                   ? riskType.ItemTxt
                                                   : model.Nationality;
                                            break;
                                        case "residence country":
                                            imodel.ResidenceStatus =
                                              string.IsNullOrWhiteSpace(model.ResidenceStatus)
                                                  ? riskType.ItemTxt
                                                  : model.ResidenceStatus;

                                            break;
                                        //case "Second Nationality (if applicable)":
                                        //    imodel.Parnter = riskType.ItemTxt;
                                        //    break;
                                        case "product, service & activity":
                                            imodel.ProductName =
                                              string.IsNullOrWhiteSpace(model.ProductName)
                                                  ? riskType.ItemTxt
                                                  : model.ProductName;

                                            break;
                                        case "if more than one product(put the riskiest product)":
                                            imodel.HighestRiskProduct =
                                              string.IsNullOrWhiteSpace(model.HighestRiskProduct)
                                                  ? riskType.ItemTxt
                                                  : model.HighestRiskProduct;

                                            break;
                                        case "delivery channel":
                                            imodel.DeliveryChannelName =
                                              string.IsNullOrWhiteSpace(model.DeliveryChannelName)
                                                  ? riskType.ItemTxt
                                                  : model.DeliveryChannelName;

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
                                            imodel.ModeOfPayment =
                                              string.IsNullOrWhiteSpace(model.Modeofpayment)
                                                  ? riskType.ItemTxt
                                                  : model.Modeofpayment;

                                            break;
                                        case "dual use goods match":
                                            imodel.DualUseGoods = riskType.ItemTxt;
                                            break;
                                        case "more dual use goods match":
                                            imodel.MoreDualUseGoods = riskType.ItemTxt;
                                            break;
                                        default:
                                            // Optional: log unmatched value
                                            break;
                                    }
                                }
                            }
                        }

                    }



                    //var result = _mapper.Map<Menumodel>(_kycService.GetMenuRightsByClientId(clientId));

                    //if (result != null)
                    //{
                    Console.WriteLine("Generate risk");



                    //model.IsPeP = isPep;
                    var str1 = _kycService.GetRiskLovId(_mapper.Map<KycIndividualDTO>(imodel), corpModel, "I", culture, model.ClientId);
                    if (str1.Result == null)
                    {

                        //return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                        return;

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
                    var militarygoodslovId = GetSafely(spStr1, 37);
                    var MoremilitarygoodslovId = GetSafely(spStr1, 39);

                    var str = _kycService.GetRiskTypeId(_mapper.Map<KycIndividualDTO>(imodel), corpModel, "I", culture, model.ClientId);
                    if (str.Result == null)
                    {

                        //return Json(new { success = false, message = "Unable to calculate risk due to insufficient data." });
                        return;
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
                    var militarygoodsId = GetSafely(spStr, 37);
                    var MoremilitarygoodsId = GetSafely(spStr, 39);
                    RiskAPIRequestModel riskModel = new RiskAPIRequestModel();

                    riskModel.CustomerId = _ccDTO.CustomerId;
                    riskModel.CustomerName = _ccDTO.FirstName;
                    riskModel.CaseVersion = _ccDTO.Version;


                    riskModel.ClientId = _clientHandler.GetClientId();
                    riskModel.CreatedBy = _clientHandler.GetUserId();
                    if (_ccDTO.Nationality == "0")
                    {
                        riskModel.MainNationality = "";
                    }
                    else
                    {
                        riskModel.MainNationality = _ccDTO.Nationality;
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
                        riskType8.Id = Convert.ToString(IndmodeofpaymentlovId);
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
                    if (militarygoodsId != "0" && militarygoodsId != "")
                    {
                        var riskTypeMil = new RiskTypeListModel();
                        riskTypeMil.Id = Convert.ToString(militarygoodslovId);
                        var riskItemMil = new RiskItemListModel();
                        riskItemMil.Id = militarygoodsId.ToString();
                        var riskItemListMil = new List<RiskItemListModel>();
                        riskItemListMil.Add(riskItemMil);
                        riskTypeMil.RiskItemList = riskItemListMil;
                        riskTypeList.Add(riskTypeMil);
                    }
                    if (MoremilitarygoodsId != "0" && MoremilitarygoodsId != "")
                    {
                        var riskTypeMoreMil = new RiskTypeListModel();
                        riskTypeMoreMil.Id = Convert.ToString(MoremilitarygoodslovId);
                        var riskItemMoreMil = new RiskItemListModel();
                        riskItemMoreMil.Id = MoremilitarygoodsId.ToString();
                        var riskItemListMoreMil = new List<RiskItemListModel>();
                        riskItemListMoreMil.Add(riskItemMoreMil);
                        riskTypeMoreMil.RiskItemList = riskItemListMoreMil;
                        riskTypeList.Add(riskTypeMoreMil);
                    }


                    riskModel.RiskTypeList = riskTypeList;
                    riskModel.CaseVersion = _ccDTO.Version;
                    riskModel.SelectedVersions = model.SelectedVersions;
                    var riskResult = _riskAPIController.KycRiskAssessment(riskModel);
                    var xyz = riskResult;

                }
            }
        }
        [HttpPost]
        public async Task<IActionResult> StudioUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0) return BadRequest("No file uploaded");

                var clientId = _clientHandler.GetClientId();
                var branchId = _clientHandler.GetBranchId();

                DocumentsModel _documentsModel = _fileUploader.UploadFile(clientId, ItemType.caseDocument, branchId, file);

                return Ok(new
                {
                    success = true,
                    name = file.FileName,
                    fileName = file.FileName,
                    fullPath = _documentsModel.DocFullPath
                });
            }
            catch (Exception ex)
            {
                log.Error(ex, "Studio file upload failed");
                return StatusCode(500, "Internal server error during upload");
            }
        }

        private string GetSafely(string[] segments, int index)
        {
            if (segments == null || index < 0 || segments.Length <= index) return "0";
            return segments[index] ?? "0";
        }

        public IActionResult GetCaseVersions(string customerId, int version)
        {
            try
            {
                var currentVersion = version;

                //var customerCases = _customerCaseService
                //    .GetAllVersionCases(customerId)
                //    .Where(x => x.Version != currentVersion)   // Exclude current version
                //    .OrderByDescending(x => x.Version)
                //    .ToList();



                var customerCases = _customerCaseService
                    .GetAllVersionCases(customerId)
                    .OrderByDescending(x => x.Version)
                    .ToList();

                var result = customerCases.Select(x => new
                {
                    id = x.Id,
                    customerId = x.CustomerId,
                    version = x.Version,
                    status = x.Status,
                    createdOn = x.CreatedOn
                }).ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        public IActionResult GetRiskCaseVersions(string customerId, int version, string type)
        {
            try
            {
                var currentVersion = version;

                //var customerCases = _customerCaseService
                //    .GetAllVersionCases(customerId)
                //    .Where(x => x.Version != currentVersion)   // Exclude current version
                //    .OrderByDescending(x => x.Version)
                //    .ToList();
                var riskReports = _mapper.Map<List<RiskReportModel>>(_riskService.GetLastestRiskVersion(customerId, type));
                //model.RiskVersionData = _mapper.Map<List<RiskReportModel>>(riskReports);



                var result = riskReports
                    .Where(x => int.TryParse(x.AssessmentVersion, out int version) && version > 1)
                    .Select(x => new
                    {
                        id = x.Id,
                        customerId = x.CustomerCode,
                        version = x.AssessmentVersion
                    })
                    .ToList();

                return Json(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("/case/GetCustomerNameById")]
        public JsonResult GetCustomerNameById(string customerId)
        {
            var entity = _customerCaseService.GetCaseFullDetailsByCustId(customerId);
            if (entity != null)
            {
                var name = string.IsNullOrWhiteSpace(entity.CompanyName) ? (entity.FirstName + " " + entity.LastName).Trim() : entity.CompanyName;
                if (string.IsNullOrWhiteSpace(name)) name = entity.onb_name;
                return Json(new { success = true, name = name, type = entity.CustomerType });
            }
            return Json(new { success = false, message = "Customer not found" });
        }

        [HttpGet("/case/GetAllGroupEntities")]
        public JsonResult GetAllGroupEntities()
        {
            var clientId = _clientHandler.GetClientId();
            var userId = _clientHandler.GetUserId();
            var GroupId = _clientHandler.GetGroupId();
            var _UserGroupModel = _mapper.Map<UserGroupModel>(_UserGroupService.GetDetails(GroupId));
            
            // Get all cases to extract groups
            var allCases = _customerMasterService.GetAllGroupEntities(clientId);
            
            var groups = allCases
                .Where(c => !string.IsNullOrEmpty(c.GroupId) && !string.IsNullOrEmpty(c.GroupEntityof))
                .SelectMany(c => 
                {
                    var ids = c.GroupId.Split(',').Select(x => x.Trim()).ToArray();
                    var names = c.GroupEntityof.Split(',').Select(x => x.Trim()).ToArray();
                    var risks = (c.GroupRisk ?? "").Split(',').Select(x => x.Trim()).ToArray();
                    var list = new List<dynamic>();
                    for (int i = 0; i < ids.Length; i++)
                    {
                        if (i < names.Length)
                        {
                            list.Add(new { GroupId = ids[i], GroupEntityof = names[i], GroupRisk = i < risks.Length ? risks[i] : "Unclassified" });
                        }
                    }
                    return list;
                })
                .GroupBy(g => new { g.GroupId, g.GroupEntityof, g.GroupRisk })
                .Select(g => new
                {
                    groupId = g.Key.GroupId,
                    groupName = g.Key.GroupEntityof,
                    riskLevel = g.Key.GroupRisk ?? "Unclassified",
                    memberCount = g.Count()
                })
                .ToList();
                
            return Json(new { success = true, data = groups });
        }

        [HttpPost("/case/CreateGroupEntity")]
        public IActionResult CreateGroupEntity([FromBody] CreateGroupRequest model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.GroupName) || model.CustomerIds == null || !model.CustomerIds.Any())
            {
                return BadRequest("Invalid payload. Group name and at least one customer ID are required.");
            }

            string groupId = Guid.NewGuid().ToString();

            // Update each customer case to be part of the group
            foreach (var custId in model.CustomerIds)
            {
                var caseDetails = _customerCaseService.GetCaseFullDetailsByCustId(custId);
                if (caseDetails != null)
                {
                    try
                    {
                        caseDetails.GroupId = groupId;
                        caseDetails.GroupEntityof = model.GroupName;
                        caseDetails.GroupRisk = "Low Risk"; // Default risk, or calculate if needed
                        
                        _customerMasterRepository.AddGroupMember(groupId, custId, model.GroupName, caseDetails.GroupRisk);
                        _customerCaseService.Update(caseDetails);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex, "Error adding to group");
                    }
                }
            }

            return Ok(new { success = true, message = "Group created successfully.", groupId = groupId, groupName = model.GroupName });
        }

        [HttpGet("/case/GetGroupMembers")]
        public JsonResult GetGroupMembers(string groupId)
        {
            try
            {
                var members = _customerMasterRepository.GetGroupMembers(groupId);
                return Json(new { success = true, data = members });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, detail = ex.InnerException?.Message });
            }
        }

        [HttpPost("/case/AddGroupMember")]
        public JsonResult AddGroupMember([FromBody] GroupMemberRequest request)
        {
            try
            {
                var success = _customerMasterRepository.AddGroupMember(request.GroupId, request.CustomerId);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error adding member: " + ex.Message + "\n" + ex.StackTrace });
            }
        }

        [HttpPost("/case/RemoveGroupMember")]
        public JsonResult RemoveGroupMember([FromBody] GroupMemberRequest request)
        {
            try
            {
                var success = _customerMasterRepository.RemoveGroupMember(request.GroupId, request.CustomerId);
                return Json(new { success });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error removing member" });
            }
        }
    }

    public class BulkComparisonRequest
    {
        [JsonProperty("ids")]
        public List<string> Ids { get; set; }
    }

    public class CreateGroupRequest
    {
        [JsonProperty("groupName")]
        public string GroupName { get; set; }
        
        [JsonProperty("customerIds")]
        public List<string> CustomerIds { get; set; }
    }

    public class GroupMemberRequest
    {
        [JsonProperty("groupId")]
        public string GroupId { get; set; }

        [JsonProperty("customerId")]
        public string CustomerId { get; set; }
    }
}
