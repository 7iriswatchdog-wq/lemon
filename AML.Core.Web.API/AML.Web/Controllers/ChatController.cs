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

        [HttpGet("stream/{caseId}")]
        public async Task StreamStatus(string caseId, [FromQuery] string queryType = null)
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
                
                await foreach (var part in _aiService.GetIntelligentReplyStreamAsync(contextData.UserPrompt, contextData.Context))
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

        [HttpGet("stream/general")]
        public async Task StreamGeneral([FromQuery] string prompt, [FromQuery] string module = "General")
        {
            Response.ContentType = "text/event-stream";
            var responseStream = Response.Body;

            try
            {
                await foreach (var part in _aiService.GetGeneralReplyStreamAsync(prompt, module))
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
