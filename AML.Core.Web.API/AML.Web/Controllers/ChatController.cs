using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.AI;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.ServiceContract.CaseComment;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.AI;
using AML.DTO.DTO.ProliferationFinance;
using AML.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AML.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatDataService _chatDataService;
        private readonly ICaseAssignmentService _caseAssignmentService;
        private readonly IUserService _userService;
        private readonly IAIService _aiService;
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ChatController> _logger;
        private readonly ICustomerCaseService _customerCaseService;
        private readonly ChatHistoryService _chatHistoryService;
        private readonly IUserGroupService _userGroupService;
        private readonly AML.Core.ServiceContract.Report.IReportService _reportService;
        private readonly ICaseCommentService _caseCommentService;

        // Rate-limit: max requests per user per window
        private const int RateLimitMaxRequests = 20;
        private const int RateLimitWindowSeconds = 60;

        public ChatController(
            ChatDataService chatDataService,
            ICaseAssignmentService caseAssignmentService,
            IUserService userService,
            IAIService aiService,
            IMemoryCache memoryCache,
            IConfiguration configuration,
            ILogger<ChatController> logger,
            ICustomerCaseService customerCaseService,
            ChatHistoryService chatHistoryService,
            IUserGroupService userGroupService,
            AML.Core.ServiceContract.Report.IReportService reportService,
            ICaseCommentService caseCommentService)
        {
            _chatDataService = chatDataService;
            _caseAssignmentService = caseAssignmentService;
            _userService = userService;
            _aiService = aiService;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;
            _customerCaseService = customerCaseService;
            _chatHistoryService = chatHistoryService;
            _userGroupService = userGroupService;
            _reportService = reportService;
            _caseCommentService = caseCommentService;
        }

        /// <summary>Returns true and writes a 429 SSE message when the user has exceeded the rate limit.</summary>
        private async Task<bool> IsRateLimitedAsync(System.IO.Stream responseStream = null)
        {
            string userId = HttpContext.Session.GetString("SessUserId") ?? "anon";
            string cacheKey = $"ratelimit_chat_{userId}";

            if (!_memoryCache.TryGetValue(cacheKey, out int count))
                count = 0;

            count++;
            _memoryCache.Set(cacheKey, count, TimeSpan.FromSeconds(RateLimitWindowSeconds));

            if (count > RateLimitMaxRequests)
            {
                if (responseStream != null)
                    await WriteStreamMatch(responseStream, $"⚠️ You have sent too many messages. Please wait a moment before trying again.");
                return true;
            }
            return false;
        }

        [HttpGet("status/{caseId}")]
        public async Task<IActionResult> GetStatus(string caseId, [FromQuery] string queryType = null)
        {
            try
            {
                // Handle predefined questions directly or when no caseId is provided
                if (queryType == "predefined" || string.IsNullOrWhiteSpace(caseId) || caseId == "undefined")
                {
                    // Check if the caseId is provided
                    if (string.IsNullOrWhiteSpace(caseId) || caseId == "undefined")
                    {
                        return Ok(new
                        {
                            caseId = "N/A",
                            customerName = "N/A",
                            status = "Please provide the Case ID to check the status.",
                            originalStatus = "Predefined response",
                            customerType = "N/A",
                            createdOn = "N/A",
                            queryType = queryType
                        });
                    }
                    else
                    {
                        return Ok(new
                        {
                            caseId = caseId,
                            customerName = "N/A",
                            status = "The case status is currently under review. Please check back later for updates.",
                            originalStatus = "Predefined response",
                            customerType = "N/A",
                            createdOn = "N/A",
                            queryType = queryType
                        });
                    }
                }

                // Input validation
                if (string.IsNullOrWhiteSpace(caseId) || caseId.Length > 50)
                {
                    return BadRequest(new { message = "Invalid case ID format.", errorCode = "INVALID_CASE_ID" });
                }

                CustomerCaseDTO caseDetails = _chatDataService.GetCaseDetails(caseId);

                if (caseDetails == null)
                {
                    return NotFound(new { message = $"I couldn't find a case matching ID '{caseId}' in your database. Please ensure the ID is correct and that the case has been properly registered.", errorCode = "CASE_NOT_FOUND" });
                }

                // Security check
                if (!IsAuthorizedForCase(caseDetails))
                {
                    return Unauthorized(new { message = $"You do not have authorization to view Case #{caseId}. Access is restricted to cases associated with your specific client account.", errorCode = "UNAUTHORIZED_ACCESS" });
                }

                var contextData = await GetContextDataAsync(caseDetails, queryType);
                string aiResponse = await _aiService.GetIntelligentReplyAsync(contextData.UserPrompt, contextData.Context);

                return Ok(new
                {
                    caseId = caseDetails.CustomerId,
                    customerName = $"{caseDetails.FirstName} {caseDetails.LastName}",
                    status = aiResponse,
                    originalStatus = contextData.ResponseMessage,
                    customerType = caseDetails.CustomerType == "I" ? "Individual" : "Corporate",
                    createdOn = caseDetails.CreatedOn ?? "N/A",
                    queryType = queryType
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GetStatus for case {CaseId}", caseId);
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving the case details. Please contact your system administrator.", errorCode = "CHAT_GET_STATUS_ERROR", error = ex.Message });
            }
        }

        [HttpGet("search-cases")]
        public IActionResult SearchCases([FromQuery] string query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    return Ok(new List<object>());
                }

                var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
                if (!int.TryParse(sessionClientIdStr, out int clientId))
                {
                    return Unauthorized(new { message = "Session expired." });
                }

                var userIdStr = HttpContext.Session.GetString("SessUserId");
                int userId = int.TryParse(userIdStr, out int uId) ? uId : 0;

                string usergroupName = "";
                var groupIdStr = HttpContext.Session.GetString("SessGroupId");
                if (int.TryParse(groupIdStr, out int groupId))
                {
                    var groupDetails = _userGroupService.GetDetails(groupId);
                    if (groupDetails != null)
                    {
                        usergroupName = groupDetails.Name;
                    }
                }

                var cases = _customerCaseService.GetAllBySearchValue(
                    userId,
                    "1900-01-01",
                    DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
                    null, // cust_type
                    query, // searchValue
                    null, // matchScore
                    0, // createdBy
                    10, // caseStatus
                    null, // riskLevel
                    usergroupName,
                    clientId,
                    "Yes" // includingDuplicate
                ) ?? new List<CustomerCaseDTO>();

                var groupMembersById = _customerCaseService.GetCasesByGroupId(query);
                if (groupMembersById != null && groupMembersById.Any())
                {
                    cases.AddRange(groupMembersById);
                }

                if (cases.Any())
                {
                    var groupIds = cases.Where(c => !string.IsNullOrEmpty(c.GroupId)).Select(c => c.GroupId).Distinct().ToList();
                    foreach (var gid in groupIds)
                    {
                        var members = _customerCaseService.GetCasesByGroupId(gid);
                        if (members != null)
                        {
                            cases.AddRange(members);
                        }
                    }
                    
                    cases = cases.GroupBy(c => c.Id).Select(g => g.First()).ToList();
                }

                if (!cases.Any())
                {
                    return Ok(new List<object>());
                }

                var result = cases.Select(c => new
                {
                    id = c.Id,
                    caseId = c.CustomerId,
                    customerName = $"{c.FirstName} {c.LastName}".Trim(),
                    status = c.CaseStatus ?? (c.Status switch
                    {
                        0 => "Pending",
                        2 => "Approved",
                        3 => "Rejected",
                        4 => "Pending with Senior Management",
                        5 => "Auto",
                        6 => "Pending with Daily Scheduler",
                        _ => "Unknown"
                    }),
                    customerType = c.CustomerType == "I" ? "Individual" : "Corporate"
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in SearchCases with query {Query}", query);
                return StatusCode(500, new { message = "An error occurred during search.", error = ex.Message });
            }
        }

        [HttpPost("stream/{caseId}")]
        public async Task StreamStatus(string caseId, [FromBody] List<ChatMessageDTO> history, [FromQuery] string queryType = null, [FromQuery] bool isNewChat = false, [FromQuery] string prompt = null)
        {
            Response.ContentType = "text/event-stream";
            var responseStream = Response.Body;

            try
            {
                // --- RATE LIMIT (#30) ---
                if (await IsRateLimitedAsync(responseStream)) return;

                // Handle case where no caseId is provided
                if (string.IsNullOrWhiteSpace(caseId) || caseId == "undefined")
                {
                    await WriteStreamMatch(responseStream, "Please provide the Case ID to check the status.");
                    return;
                }

                CustomerCaseDTO caseDetails = _chatDataService.GetCaseDetails(caseId);
                if (caseDetails == null)
                {
                    await WriteStreamMatch(responseStream, $"I'm sorry, I couldn't find a case with ID '{caseId}' in the system database. Please verify the ID and try again.");
                    return;
                }

                // --- SECURITY CHECK (#29) — deny by default if session is missing ---
                var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
                if (!int.TryParse(sessionClientIdStr, out int sessionClientId))
                {
                    await WriteStreamMatch(responseStream, "Your session has expired. Please log in again to access case details.");
                    return;
                }
                if (!IsAuthorizedForCase(caseDetails))
                {
                    await WriteStreamMatch(responseStream, "You do not have authorization to view Case #" + caseId + ". Access is restricted to cases associated with your account.");
                    return;
                }

                // Load existing chat history if not a new chat
                if (!isNewChat && (history == null || history.Count == 0))
                {
                    var savedHistory = _chatHistoryService.GetChatHistory("case", caseId);
                    if (savedHistory.Any())
                        history = savedHistory;
                }

                var contextData = await GetContextDataAsync(caseDetails, queryType);
                
                if (!isNewChat)
                    await _chatHistoryService.SaveChatHistoryAsync("case", history, caseId);
                else
                    _chatHistoryService.ClearChatHistory("case", caseId);
                
                var promptToUse = prompt ?? history?.LastOrDefault(m => m.Role == "user")?.Content ?? contextData.UserPrompt;
                await foreach (var part in _aiService.GetIntelligentReplyStreamAsync(promptToUse, contextData.Context, history, isNewChat))
                {
                    await WriteStreamMatch(responseStream, part);
                    await responseStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                // #25 — return a real error message, not a misleading auth message
                _logger.LogError(ex, "Error in StreamStatus for case {CaseId}", caseId);
                await WriteStreamMatch(responseStream, "An unexpected error occurred while processing your request. Please try again or contact your system administrator.");
            }
        }

        [HttpPost("stream/general")]
        public async Task StreamGeneral([FromBody] List<ChatMessageDTO> history, [FromQuery] string prompt, [FromQuery] string module = "General", [FromQuery] bool isNewChat = false)
        {
            Response.ContentType = "text/event-stream";
            var responseStream = Response.Body;

            try
            {
                // --- RATE LIMIT (#30) ---
                if (await IsRateLimitedAsync(responseStream)) return;

                // Load existing chat history if not a new chat
                if (!isNewChat && (history == null || history.Count == 0))
                {
                    var savedHistory = _chatHistoryService.GetChatHistory("general", module);
                    if (savedHistory.Any())
                        history = savedHistory;
                }

                string dynamicData = "";
                if (module == "Dashboard")
                    dynamicData = GetDashboardStats();

                string knowledgeBase = GetModuleKnowledge(module);
                string moduleContext = $"{module} context. {dynamicData}\n\n[SYSTEM KNOWLEDGE BASE]: {knowledgeBase}";

                if (!isNewChat)
                    await _chatHistoryService.SaveChatHistoryAsync("general", history, module);
                else
                    _chatHistoryService.ClearChatHistory("general", module);
                
                await foreach (var part in _aiService.GetGeneralReplyStreamAsync(prompt, moduleContext, history, isNewChat))
                {
                    await WriteStreamMatch(responseStream, part);
                    await responseStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in StreamGeneral for module {Module}", module);
                await WriteStreamMatch(responseStream, "An unexpected error occurred. Please try again or contact your system administrator.");
            }
        }

        private string GetDashboardStats()
        {
            var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
            if (!int.TryParse(sessionClientIdStr, out int sessionClientId))
                return "Unable to retrieve real-time dashboard statistics at this moment.";

            // #11 — cache duration from config (default 5 min)
            int cacheMins = int.TryParse(_configuration["AI:DashboardCacheMins"], out int cfgMins) ? cfgMins : 5;
            string cacheKey = $"DashboardStats_{sessionClientId}";

            if (!_memoryCache.TryGetValue(cacheKey, out string cachedStats))
            {
                int pending    = _reportService.GetCustomerCaseCount(0, sessionClientId);
                int approved   = _reportService.GetCustomerCaseCount(2, sessionClientId);
                int rejected   = _reportService.GetCustomerCaseCount(3, sessionClientId);
                int seniorMgmt = _reportService.GetCustomerCaseCount(4, sessionClientId);
                int auto       = _reportService.GetCustomerCaseCount(5, sessionClientId);
                int scheduler  = _reportService.GetCustomerCaseCount(6, sessionClientId);
                int total      = _reportService.GetCustomerCaseCount(10, sessionClientId);

                var indvidualcount = _reportService.GetRiskCount(1, sessionClientId);
                var indCountObj = indvidualcount?.FirstOrDefault();
                var corporatecount = _reportService.GetRiskCount(2, sessionClientId);
                var corpCountObj = corporatecount?.FirstOrDefault();
                int highRiskCount = (indCountObj?.high_risk_count ?? 0) + (corpCountObj?.high_risk_count ?? 0);

                var sanctionCases = _chatDataService.GetAllSanctionDashboard(sessionClientId, "");
                var topHighRiskList = string.Join(", ", sanctionCases
                    .Where(c => c.MatchScore > 80)
                    .OrderByDescending(c => c.MatchScore).Take(5)
                    .Select(c => $"#{c.CustomerId} {c.FirstName} {c.LastName} ({c.MatchScore}%)"));

                cachedStats = $"[LIVE DASHBOARD SUMMARY]: Pending: {pending}, Approved: {approved}, Rejected: {rejected}, " +
                              $"In Senior Management: {seniorMgmt}, Auto: {auto}, Daily Scheduler: {scheduler}. " +
                              $"High Risk Count: {highRiskCount}. TOP CASES: {topHighRiskList}. Total Cases: {total}.";

                _memoryCache.Set(cacheKey, cachedStats, TimeSpan.FromMinutes(cacheMins));
            }

            return cachedStats;
        }

        private string GetModuleKnowledge(string module)
        {
            return module switch
            {
                "Dashboard"                        => GetDashboardKnowledge(),
                "PF Creation"                      => GetPFKnowledge(),
                "Due Diligence" or "Case Creation" => GetDueDiligenceKnowledge(),
                "Reports"                          => GetReportsKnowledge(),
                "Admin"                            => GetAdminKnowledge(),
                "Process"                          => GetRiskProcessKnowledge(),
                // #5/#6 — previously missing modules
                "AmlTracker"                       => GetAmlTrackerKnowledge(),
                "SanctionScreening"                => GetSanctionScreeningKnowledge(),
                "ComplianceHub"                    => GetComplianceHubKnowledge(),
                "STM"                              => GetStmKnowledge(),
                _                                  => "General AML/KYC guidance for the Search system."
            };
        }

        private string GetDashboardKnowledge()
        {
            return @"[HANDBOOK: DASHBOARD]:
1. STATUS MONITORING: Track cases across 'Pending', 'Approved', 'Rejected', 'Senior Management', and 'Daily Scheduler'.
2. HIGH RISK ALERT: System classifies a match as High Risk if the Match Score exceeds 80.
3. SCHEDULER: The system's background engine automatically rescreens existing cases against updated sanction lists daily.";
        }

        private string GetAmlTrackerKnowledge()
        {
            return @"[HANDBOOK: AML TRACKER]:
1. PURPOSE: The AML Tracker is the central workbench for reviewing, assigning, and escalating active sanction cases.
2. TABS: Workspace (active queue), Customer 360 (full profile), Reports, Leaderboard, and SLA Config.
3. ASSIGNMENT: Cases can be assigned to analysts directly from the tracker grid.
4. SLA: Each case type has a configurable SLA deadline. Breaches are highlighted in the tracker.";
        }

        private string GetSanctionScreeningKnowledge()
        {
            return @"[HANDBOOK: INSTANT SANCTION SCREENING]:
1. PURPOSE: Run ad-hoc one-off sanction checks against UAE IEC, UN, and NAS lists without creating a full case.
2. RESULT: Each result shows the matched name, source list, and a match score. No case is stored.
3. LISTS SCREENED: UAE IEC, United Nations (UN), and UAE NAS.";
        }

        private string GetComplianceHubKnowledge()
        {
            return @"[HANDBOOK: COMPLIANCE HUB]:
1. PURPOSE: Central policy and regulatory reference library for the compliance team.
2. CONTENT: Houses UAE AML/CFT regulations, FATF guidance, and internal policy documents.
3. SEARCH: Full-text search across all uploaded policy documents.";
        }

        private string GetStmKnowledge()
        {
            return @"[HANDBOOK: SECTORAL TMS (STM)]:
1. PURPOSE: Transaction Monitoring System covering Insurance and Real Estate sectors.
2. MONITORING: Flags suspicious transaction patterns based on configurable rule thresholds.
3. RULES: Each rule has a risk weight. Transactions hitting multiple rules are escalated automatically.
4. REPORTING: Generates STR (Suspicious Transaction Reports) for regulatory submission.";
        }

        private string GetAdminKnowledge()
        {
            var clients = _customerCaseService.GetAllAdminClients();
            var activeCount = clients.Count(c => c.isActive == 1);
            var blockedCount = clients.Count(c => c.isActive == 0);
            var topClients = string.Join(", ", clients.Take(10).Select(c => $"{c.ClientName} ({c.Prefix})"));

            return $@"[LIVE ADMIN SQL REGISTRY]: 
- Total Clients: {clients.Count}
- Active: {activeCount}
- Blocked/Inactive: {blockedCount}
- Top 10 Clients (by ID): {topClients}

[HANDBOOK: ADMINISTRATION]:
1. USER GROUPS: Permissions are grouped into 'Admin', 'Compliance/Reviewer', and 'View-Only'.
2. PERMISSION MATRIX: Rights are defined as View, Add, Edit, or Delete per system menu.
3. CLIENT RIGHTS: Global system toggles for the Client ID that enable/disable specific modules like OCR or PF.";
        }

        private string GetPFKnowledge()
        {
            var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
            int sessionClientId = 0;
            int.TryParse(sessionClientIdStr, out sessionClientId);

            var cases = _chatDataService.GetAllPFCases(sessionClientId);
            var recentHits = cases.Where(c => c.CreatedOn >= DateTime.Now.AddDays(-7)).Count();
            
            return $@"[LIVE PROLIFERATION LEDGER]: 
- Total PF Cases: {cases.Count}
- Hits in the last 7 days: {recentHits}
[HANDBOOK: PROLIFERATION FINANCE]:
1. LEGAL BASIS: Screening is performed against 'UAE Cabinet Decision No. 156 of 2025' regarding dual-use items and chemical weapons.
2. DATABASE ACCESS: Real-time matched results are retrieved from the system's proliferation ledger.
3. SEARCH LAYERS: SQL HS/CAS Database & PDF Intelligent Keyword Search.";
        }

        private string GetDueDiligenceKnowledge()
        {
            var unscreenedCount = _customerCaseService.GetUnscreenedCustomers()?.Count ?? 0;
            return $@"[LIVE DUE DILIGENCE SQL QUEUE]: 
- Customers Pending Screening: {unscreenedCount}
[HANDBOOK: SCREENING & CREATION]:
1. MANDATORY FIELDS: Full Name, Nationality, Gender, Date of Birth, ID Number.
2. RISK CATEGORIES: PEP (Politically Exposed), SAN (Sanctions), UN, and UAE Local lists.";
        }

        private string GetReportsKnowledge()
        {
            // #12 — use the session's client ID so multi-tenant data is correctly scoped
            int.TryParse(HttpContext.Session.GetString("SessClientId"), out int sessionClientId);
            var completed = _customerCaseService.GetAllCompletedCases(0, null, null, null, null, 0, 0, null, sessionClientId, "0");
            return $@"[LIVE REPORTS ARCHIVE]:
- Total Completed Cases (Audit-Ready): {completed?.Count ?? 0}
[HANDBOOK: REPORTS & AUDIT]:
1. EVIDENCE: The 'Export' button generates a Case Process PDF (official audit trail).
2. AUDIT LOGS: Every action is logged in the system's ScreeningLogs table.";
        }

        private string GetRiskProcessKnowledge()
        {
            return @"[HANDBOOK: RISK & DECISION]:
1. RISK CALCULATION: Weighted sum of Country Risk, Occupation (PEPs), and Product/Service types.
2. ESCALATION: High-risk findings should be sent to 'Senior Management' for final sign-off.";
        }

        private async Task WriteStreamMatch(System.IO.Stream stream, string text)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new { t = text });
            var bytes = Encoding.UTF8.GetBytes($"data: {json}\n\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private CustomerCaseDTO GetCaseDetails(string caseId)
        {
            return _chatDataService.GetCaseDetails(caseId);
        }

        private (string Context, string UserPrompt, string ResponseMessage) GetContextData(CustomerCaseDTO caseDetails, string queryType)
        {
            string finalCustId = caseDetails.CustomerId;
            var shareholders = _chatDataService.GetShareholders(finalCustId);
            var riskAssessment = _chatDataService.GetRiskAssessment(finalCustId, caseDetails.CustomerType);

            string responseMessage = "";
            bool isHitQuery = queryType == "hit" || (caseDetails.CustomerId.Contains("hit", StringComparison.OrdinalIgnoreCase));

            if (isHitQuery)
            {
                if (shareholders != null && shareholders.Any())
                {
                    var hitCount = shareholders.Count(s => s.MatchScore > 0);
                    responseMessage = hitCount > 0 
                        ? $"Detected {hitCount} matches in shareholders for Case #{finalCustId}."
                        : $"No matches found in shareholders for Case #{finalCustId}.";
                }
                else
                {
                    responseMessage = $"Case #{finalCustId} has no shareholders.";
                }
            }
            else if (queryType == "risk")
            {
                var latestRisk = riskAssessment?.FirstOrDefault();
                string riskScore = latestRisk?.FinalScore ?? caseDetails.RiskScore.ToString();
                responseMessage = $"Risk assessment retrieved with score {riskScore}.";
            }
            else
            {
                string statusDescription = caseDetails.Status switch
                {
                    0 => "Pending",
                    2 => "Approved",
                    3 => "Rejected",
                    4 => "Pending with Senior Management",
                    5 => "Auto",
                    6 => "Pending with Daily Scheduler",
                    _ => caseDetails.CaseStatus ?? "Unknown"
                };
                var latestRisk = riskAssessment?.FirstOrDefault();
                string riskScore = latestRisk?.FinalScore ?? caseDetails.RiskScore.ToString();
                responseMessage = $"Case #{finalCustId} is currently {statusDescription} with risk score {riskScore}.";
            }

            var searchHits = _chatDataService.GetMongoSearchResults(caseDetails.Id);

            string context = $@"
Case Details:
- ID: {finalCustId}
- Name: {caseDetails.FirstName} {caseDetails.LastName}
- Current Status: {responseMessage}
- Risk Score: {caseDetails.RiskScore}
- Customer Type: {caseDetails.CustomerType}

Shareholders:
{string.Join("\n", shareholders?.Select(s => $"- {s.FirstName} {s.LastName}: Status: {s.CaseStatus}") ?? new[] { "None" })}

Latest Risk Assessment History:
{string.Join("\n", riskAssessment?.Select(r => $"- {r.AssessmentVersion}: {r.RiskType} ({r.FinalScore})") ?? new[] { "None" })}

Search Result Findings:
{string.Join("\n", searchHits.Select(h => $"- Match: {h.MatchedName}, Decision: {h.Decision}, Remark: {h.Remarks}") ?? new[] { "No proliferation finance matches found" })}
";

            string userPrompt = string.IsNullOrEmpty(queryType) ? "Summarize the status and pending actions." : $"Summarize the {queryType} details.";
            return (context, userPrompt, responseMessage);
        }

        private bool IsAuthorizedForCase(CustomerCaseDTO caseDetails)
        {
            // #29 — deny by default when session is missing or unparseable
            var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
            if (!int.TryParse(sessionClientIdStr, out int sessionClientId))
                return false;

            var recordClientId = caseDetails.ClientId;
            if (recordClientId == 0 && caseDetails.Id > 0)
            {
                recordClientId = _customerCaseService.GetDetails(caseDetails.Id)?.ClientId ?? 0;
            }

            return recordClientId == sessionClientId;
        }

        private async Task<(string Context, string UserPrompt, string ResponseMessage)> GetContextDataAsync(CustomerCaseDTO caseDetails, string queryType)
        {
            string finalCustId = caseDetails.CustomerId;
            
            // Use parallel calls for better performance
            var shareholdersTask = Task.Run(() => _chatDataService.GetShareholders(finalCustId));
            var riskAssessmentTask = Task.Run(() => _chatDataService.GetRiskAssessment(finalCustId, caseDetails.CustomerType));
            var searchHitsTask = Task.Run(() => _chatDataService.GetMongoSearchResults(caseDetails.Id));
            var commentsTask = Task.Run(() => _caseCommentService.GetAllByCase(caseDetails.Id));

            await Task.WhenAll(shareholdersTask, riskAssessmentTask, searchHitsTask, commentsTask);

            var shareholders = shareholdersTask.Result;
            var riskAssessment = riskAssessmentTask.Result;
            var searchHits = searchHitsTask.Result;
            var comments = commentsTask.Result;

            string finalCustType = !string.IsNullOrEmpty(caseDetails.CustomerType) 
                ? (caseDetails.CustomerType.Equals("I", StringComparison.OrdinalIgnoreCase) ? "Individual" : 
                   caseDetails.CustomerType.Equals("C", StringComparison.OrdinalIgnoreCase) ? "Corporate" : 
                   caseDetails.CustomerType) 
                : (!string.IsNullOrEmpty(caseDetails.MatchCategory) 
                    ? (caseDetails.MatchCategory.StartsWith("I", StringComparison.OrdinalIgnoreCase) ? "Individual" : "Corporate") 
                    : "Individual");

            var latestRiskFinal = riskAssessment?.FirstOrDefault();
            string riskScoreFinal = finalCustType == "Corporate" 
                ? caseDetails.corporate_final_risk_score 
                : caseDetails.Individual_final_risk_score;
            
            string riskOverride = finalCustType == "Corporate" 
                ? caseDetails.Corporate_Risk_Override 
                : caseDetails.Individual_Risk_Override;

            if (string.IsNullOrEmpty(riskScoreFinal) || riskScoreFinal == "0")
            {
                riskScoreFinal = "Unclassified";
            }
            else
            {
                if (riskScoreFinal.Contains("High", StringComparison.OrdinalIgnoreCase) && 
                    !string.IsNullOrEmpty(riskOverride) && 
                    riskOverride.Equals("override", StringComparison.OrdinalIgnoreCase))
                {
                    riskScoreFinal = "High(O)";
                }
                else
                {
                    riskScoreFinal = riskScoreFinal.Replace(" Risk", "", StringComparison.OrdinalIgnoreCase)
                                                   .Replace("High (O)", "High(O)", StringComparison.OrdinalIgnoreCase);
                }
            }

            string responseMessage = "";
            bool isHitQuery = queryType == "hit" || (caseDetails.CustomerId.Contains("hit", StringComparison.OrdinalIgnoreCase));

            if (isHitQuery)
            {
                if (shareholders != null && shareholders.Any())
                {
                    var hitCount = shareholders.Count(s => s.MatchScore > 0);
                    responseMessage = hitCount > 0 
                        ? $"Detected {hitCount} matches in shareholders for Case #{finalCustId}."
                        : $"No matches found in shareholders for Case #{finalCustId}.";
                }
                else
                {
                    responseMessage = $"Case #{finalCustId} has no shareholders.";
                }
            }
            else if (queryType == "risk")
            {
                responseMessage = $"Risk assessment retrieved with score {riskScoreFinal}.";
            }
            else
            {
                string statusDescription = caseDetails.Status switch
                {
                    0 => "Pending",
                    2 => "Approved",
                    3 => "Rejected",
                    4 => "Pending with Senior Management",
                    5 => "Auto",
                    6 => "Pending with Daily Scheduler",
                    _ => caseDetails.CaseStatus ?? "Unknown"
                };
                responseMessage = $"Case #{finalCustId} is currently {statusDescription} with risk score {riskScoreFinal}.";
            }

            string commentsStr = "None";
            if (comments != null && comments.Any())
            {
                commentsStr = string.Join("\n", comments.Select(c => $"- Date: {c.CreatedOnDB?.ToString("dd/MM/yyyy HH:mm") ?? "N/A"}, User: {c.CreatedUser ?? "System"}, Comment: {c.Comment}"));
            }

            string assignedUserName = "System";
            if (caseDetails.Owner > 0)
            {
                var ownerUser = _userService.GetDetails(caseDetails.Owner);
                if (ownerUser != null)
                {
                    assignedUserName = $"{ownerUser.FName} {ownerUser.LName}".Trim();
                }
            }
            else if (caseDetails.CreatedBy > 0)
            {
                var createdByUser = _userService.GetDetails(caseDetails.CreatedBy);
                if (createdByUser != null)
                {
                    assignedUserName = $"{createdByUser.FName} {createdByUser.LName}".Trim();
                }
            }
            else if (!string.IsNullOrEmpty(caseDetails.CreatedUser))
            {
                assignedUserName = caseDetails.CreatedUser;
            }
            else if (!string.IsNullOrEmpty(caseDetails.UserId))
            {
                assignedUserName = caseDetails.UserId;
            }

            string context = $@"
Case Details:
- ID: {finalCustId}
- PK_ID: {caseDetails.Id}
- Name: {caseDetails.FirstName} {caseDetails.LastName}
- Current Status: {responseMessage}
- Risk Score: {riskScoreFinal}
- Customer Type: {finalCustType}
- Created On: {caseDetails.CreatedOn ?? "2026-05-29 10:00:00"}
- Created By: {assignedUserName}
- Group ID: {caseDetails.GroupId ?? "None"}
- Group Entity Of: {caseDetails.GroupEntityof ?? "None"}

Shareholders:
{string.Join("\n", shareholders?.Select(s => $"- {s.FirstName} {s.LastName}: Status: {s.CaseStatus}, Match Score: {s.MatchScore}%") ?? new[] { "None" })}

Latest Risk Assessment History:
{string.Join("\n", riskAssessment?.Select(r => $"- {r.AssessmentVersion}: {r.RiskType} ({r.FinalScore})") ?? new[] { "None" })}

Search Result Findings:
{string.Join("\n", searchHits.Select(h => $"- Match: {h.MatchedName}, Decision: {h.Decision}, Remark: {h.Remarks}") ?? new[] { "No proliferation finance matches found" })}

Case Comments:
{commentsStr}
";

            string userPrompt = string.IsNullOrEmpty(queryType) ? "Summarize the status and pending actions." : $"Summarize the {queryType} details.";
            return (context, userPrompt, responseMessage);
        }
    }
}
