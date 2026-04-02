using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Risk;
using AML.Core.ServiceContract.CaseAssignment;
using AML.Core.ServiceContract.User;
using AML.Core.ServiceContract.AI;
using AML.Core.ServiceContract.ProliferationFinance;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using AML.DTO.DTO.CaseAssignment;
using AML.DTO.DTO.AI;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AML.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ICustomerCaseService _customerCaseService;
        private readonly IRiskService _riskService;
        private readonly ICaseAssignmentService _caseAssignmentService;
        private readonly IUserService _userService;
        private readonly IAIService _aiService;
        private readonly IProliferationFinanceService _proliferationFinanceService;

        public ChatController(ICustomerCaseService customerCaseService, 
            IRiskService riskService, 
            ICaseAssignmentService caseAssignmentService,
            IUserService userService,
            IAIService aiService,
            IProliferationFinanceService proliferationFinanceService)
        {
            _customerCaseService = customerCaseService;
            _riskService = riskService;
            _caseAssignmentService = caseAssignmentService;
            _userService = userService;
            _aiService = aiService;
            _proliferationFinanceService = proliferationFinanceService;
        }

        [HttpGet("status/{caseId}")]
        public async Task<IActionResult> GetStatus(string caseId, [FromQuery] string queryType = null)
        {
            try
            {
                CustomerCaseDTO caseDetails = GetCaseDetails(caseId);

                if (caseDetails == null)
                {
                    return NotFound(new { message = $"I couldn't find a case matching ID '{caseId}' in your database. Please ensure the ID is correct and that the case has been properly registered." });
                }

                // --- SECURITY CHECK ---
                var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
                if (int.TryParse(sessionClientIdStr, out int sessionClientId))
                {
                    if (caseDetails.ClientId != sessionClientId)
                    {
                        return Unauthorized(new { message = $"You do not have authorization to view Case #{caseId}. Access is restricted to cases associated with your specific client account." });
                    }
                }
                // ----------------------

                var contextData = GetContextData(caseDetails, queryType);
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
                return StatusCode(500, new { message = "An unexpected error occurred while retrieving the case details. Please contact your system administrator.", error = ex.Message });
            }
        }

        [HttpPost("stream/{caseId}")]
        public async Task StreamStatus(string caseId, [FromBody] List<ChatMessageDTO> history, [FromQuery] string queryType = null, [FromQuery] bool isNewChat = false)
        {
            Response.ContentType = "text/event-stream";
            var responseStream = Response.Body;

            try
            {
                CustomerCaseDTO caseDetails = GetCaseDetails(caseId);
                if (caseDetails == null)
                {
                    await WriteStreamMatch(responseStream, $"I'm sorry, I couldn't find a case with ID '{caseId}' in the system database. Please verify the ID and try again.");
                    return;
                }

                // --- SECURITY CHECK ---
                var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
                if (int.TryParse(sessionClientIdStr, out int sessionClientId))
                {
                    if (caseDetails.ClientId != sessionClientId)
                    {
                        await WriteStreamMatch(responseStream, "It appears you do not have the necessary authorization to view the details for Case #" + caseId + ". Access is restricted to authorized users within your client group.");
                        return;
                    }
                }

                var contextData = GetContextData(caseDetails, queryType);
                
                await foreach (var part in _aiService.GetIntelligentReplyStreamAsync(contextData.UserPrompt, contextData.Context, history, isNewChat))
                {
                    await WriteStreamMatch(responseStream, part);
                    await responseStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                await WriteStreamMatch(responseStream, "It appears you do not have the necessary authorization to view the details for Case #" + caseId + ". Access is restricted to authorized users within your client group.");
            }
        }

        [HttpPost("stream/general")]
        public async Task StreamGeneral([FromBody] List<ChatMessageDTO> history, [FromQuery] string prompt, [FromQuery] string module = "General", [FromQuery] bool isNewChat = false)
        {
            Response.ContentType = "text/event-stream";
            var responseStream = Response.Body;

            try
            {
                string dynamicData = "";
                if (module == "Dashboard")
                {
                    dynamicData = GetDashboardStats();
                }

                string knowledgeBase = GetModuleKnowledge(module);
                string moduleContext = $"{module} context. {dynamicData}\n\n[SYSTEM KNOWLEDGE BASE]: {knowledgeBase}";

                await foreach (var part in _aiService.GetGeneralReplyStreamAsync(prompt, moduleContext, history, isNewChat))
                {
                    await WriteStreamMatch(responseStream, part);
                    await responseStream.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                await WriteStreamMatch(responseStream, $"Error: {ex.Message}");
            }
        }

        private string GetDashboardStats()
        {
             var sessionClientIdStr = HttpContext.Session.GetString("SessClientId");
             if (int.TryParse(sessionClientIdStr, out int sessionClientId))
             {
                 var allCases = _customerCaseService.GetAllSanctionDashboard(sessionClientId, "");
                 
                 int pending = allCases.Count(c => c.Status == 0);
                 int approved = allCases.Count(c => c.Status == 2);
                 int rejected = allCases.Count(c => c.Status == 3);
                 int seniorMgmt = allCases.Count(c => c.Status == 4);
                 int auto = allCases.Count(c => c.Status == 5);
                 int scheduler = allCases.Count(c => c.Status == 6);

                 var highRiskCount = allCases.Count(c => c.CustomerScreenMatchScore > 80);
                 var topHighRiskList = string.Join(", ", allCases.Where(c => c.CustomerScreenMatchScore > 80).OrderByDescending(c => c.CustomerScreenMatchScore).Take(5).Select(c => $"#{c.CustomerId} {c.FirstName} {c.LastName} ({c.CustomerScreenMatchScore}%)"));

                 return $"[LIVE DASHBOARD SUMMARY]: Pending: {pending}, Approved: {approved}, Rejected: {rejected}, In Senior Management: {seniorMgmt}, Auto: {auto}, Daily Scheduler: {scheduler}. High Risk Count: {highRiskCount}. TOP CASES: {topHighRiskList}. Total Cases: {allCases.Count}.";
             }
             return "Unable to retrieve real-time dashboard statistics at this moment.";
        }

        private string GetModuleKnowledge(string module)
        {
            return module switch
            {
                "Dashboard" => @"[HANDBOOK: DASHBOARD]: 
1. STATUS MONITORING: Track cases across 'Pending' (Status 0), 'Approved' (Status 2), 'Rejected' (Status 3), and 'Daily Scheduler' (Status 6).
2. HIGH RISK ALERT: System classifies a match as High Risk if the Score exceeds 80.
3. SCHEDULER: The system's background engine automatically rescreens existing cases against updated sanction lists daily.",
                
                "Due Diligence" | "Case Creation" => @"[HANDBOOK: SCREENING & CREATION]:
1. MANDATORY FIELDS: Full Name*, Nationality*, Gender*, Date of Birth*, ID Number*, and ID Type*. (* Indicates required for list screening).
2. INPUT METHODS: 
   - Manual: Core data entry for immediate screening.
   - OCR Upload: Extracts text from Passport or ID card images.
   - Bulk Upload: Uses a system Excel template for high-volume ingestion.
3. RISK CATEGORIES: Individuals are screened against PEP (Politically Exposed), SAN (Sanctions/OFAC/EU), UN (United Nations), and UAE Local lists. 
4. MATCH SCORE: A percentage (0-100%) indicating how closely the input name matches a listed sanction record.",

                "PF Creation" => @"[HANDBOOK: PROLIFERATION FINANCE]:
1. LEGAL BASIS: Screening is performed against 'UAE Cabinet Decision No. 156 of 2025' regarding dual-use items and chemical weapons.
2. SEARCH LAYERS:
   - Layer 1 (Database): Direct match against HS Codes, CAS Numbers, and Chemical Names.
   - Layer 2 (PDF Search): Intelligent keyword search within official legislation PDFs (Decision 156).
3. HIT REVIEW: All hits must be categorized as 'No Match', 'Potential Match', or 'Confirmed Hit'. Decision and Remarks are mandatory for each finding.",

                "Admin" => @"[HANDBOOK: ADMINISTRATION]:
1. USER GROUPS: Permissions are grouped into 'Admin', 'Compliance/Reviewer', and 'View-Only'.
2. PERMISSION MATRIX: Rights are defined as View, Add, Edit, or Delete per system menu.
3. CLIENT RIGHTS: Global system toggles for the Client ID that enable/disable specific modules like OCR or PF.",

                "Reports" => @"[HANDBOOK: REPORTS & AUDIT]:
1. COMPLETED CASES: Archive of all finalized screenings.
2. EVIDENCE: The 'Export' button generates a Case Process PDF, which is the official audit trail for compliance.
3. AUDIT LOGS: Every action (login, search, decision, or export) is logged in the system's Audit Trail (ScreeningLogs table).",

                "Process" => @"[HANDBOOK: RISK & DECISION]:
1. RISK CALCULATION: The final Risk Level (Low/Medium/High) is a weighted sum of categories: Country Risk (Sanctioned vs Non-Sanctioned), Occupation (PEPs), and Product/Service types.
2. OVERRIDE: Authorized users can manually adjust the Risk Level if they provide a justified business reason.
3. ESCALATION: High-risk or suspicious findings should be sent to 'Senior Management' (Status 4) for final sign-off.",

                _ => "General AML/KYC guidance for the Lemon WatchDog system."
            };
        }

        private async Task WriteStreamMatch(System.IO.Stream stream, string text)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(new { t = text });
            var bytes = Encoding.UTF8.GetBytes($"data: {json}\n\n");
            await stream.WriteAsync(bytes, 0, bytes.Length);
        }

        private CustomerCaseDTO GetCaseDetails(string caseId)
        {
            CustomerCaseDTO caseDetails = null;
            if (int.TryParse(caseId, out int numericCaseId))
            {
                caseDetails = _customerCaseService.GetCaseFullDetailsByCaseId(numericCaseId);
            }
            if (caseDetails == null)
            {
                caseDetails = _customerCaseService.GetCaseFullDetailsByCustId(caseId);
            }
            return caseDetails;
        }

        private (string Context, string UserPrompt, string ResponseMessage) GetContextData(CustomerCaseDTO caseDetails, string queryType)
        {
            string finalCustId = caseDetails.CustomerId;
            var shareholders = _customerCaseService.GetShareHoldersByCompanyCode(finalCustId);
            var riskAssessment = _riskService.GetLastestRiskVersion(finalCustId, caseDetails.CustomerType);

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

            var mongoData = _proliferationFinanceService.GetMongoSearchResults(caseDetails.Id);
            var searchHits = mongoData?.Hits ?? new List<AML.DTO.DTO.ProliferationFinance.PFSearchResultsMongoDTO.PF_Hit>();

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

MongoDB Search Result Findings:
{string.Join("\n", searchHits.Select(h => $"- Match: {h.MatchedName}, Decision: {h.Decision}, Remark: {h.Remarks}") ?? new[] { "No proliferation finance matches found in Mongo" })}
";

            string userPrompt = string.IsNullOrEmpty(queryType) ? "Summarize the status and pending actions." : $"Summarize the {queryType} details.";
            return (context, userPrompt, responseMessage);
        }
    }
}
