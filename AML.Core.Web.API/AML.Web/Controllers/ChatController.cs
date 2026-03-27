using AML.Core.Common.StaticResource;
using AML.Core.ServiceContract.CustomerCase;
using AML.Core.ServiceContract.Risk;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ICustomerCaseService _customerCaseService;
        private readonly IRiskService _riskService;

        public ChatController(ICustomerCaseService customerCaseService, IRiskService riskService)
        {
            _customerCaseService = customerCaseService;
            _riskService = riskService;
        }

        [HttpGet("status/{caseId}")]
        public async Task<IActionResult> GetStatus(string caseId, [FromQuery] string queryType = null)
        {
            try
            {
                CustomerCaseDTO caseDetails = null;

                // 1. Try numeric CaseId first
                if (int.TryParse(caseId, out int numericCaseId))
                {
                    caseDetails = _customerCaseService.GetCaseFullDetailsByCaseId(numericCaseId);
                }

                // 2. Fallback to CustId
                if (caseDetails == null)
                {
                    caseDetails = _customerCaseService.GetCaseFullDetailsByCustId(caseId);
                }

                if (caseDetails == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                string finalCustId = caseDetails.CustomerId;
                var shareholders = _customerCaseService.GetShareHoldersByCompanyCode(finalCustId);
                var riskAssessment = _riskService.GetLastestRiskVersion(finalCustId, caseDetails.CustomerType);

                string responseMessage = "";
                bool isStatusQuery = queryType == "status" || (string.IsNullOrEmpty(queryType) && !caseId.Contains("hit", StringComparison.OrdinalIgnoreCase));
                bool isHitQuery = queryType == "hit" || (caseId.Contains("hit", StringComparison.OrdinalIgnoreCase));

                if (isHitQuery)
                {
                    if (shareholders != null && shareholders.Any())
                    {
                        var hitCount = shareholders.Count(s => s.MatchScore > 0);
                        if (hitCount > 0)
                        {
                            responseMessage = $"I have detected {hitCount} potential match(es) in the shareholder search for Case #{finalCustId}. You should complete the review for these shareholders ensuring all hits are properly addressed to proceed.";
                        }
                        else
                        {
                            responseMessage = $"Good news! No potential search hits were found for the shareholders of Case #{finalCustId}. All shareholder screenings are clear.";
                        }
                    }
                    else
                    {
                        responseMessage = $"Case #{finalCustId} does not have any shareholders listed for potential hit analysis.";
                    }
                }
                else if (queryType == "risk")
                {
                    if (riskAssessment != null && riskAssessment.Any())
                    {
                        var latestRisk = riskAssessment.FirstOrDefault();
                        string riskDisplayScore = latestRisk?.FinalScore ?? caseDetails.RiskScore.ToString();
                        responseMessage = $"The latest risk assessment for Case #{finalCustId} has been successfully retrieved. It is currently being processed with a risk score of {riskDisplayScore}. Please ensure all risk parameters are reviewed.";
                    }
                    else
                    {
                        responseMessage = $"I couldn't find a completed risk assessment for Case #{finalCustId}. Please complete the risk assessment section to proceed with the case evaluation.";
                    }
                }
                else // Default to Status logic
                {
                    if ((shareholders == null || !shareholders.Any()) && (riskAssessment == null || !riskAssessment.Any()))
                    {
                        responseMessage = "You have an incomplete risk assessment for this case. Please complete the risk assessment and shareholder details to get a proper overall risk score and proceed further.";
                    }
                    else if (shareholders != null && shareholders.Any() && (riskAssessment == null || !riskAssessment.Any()))
                    {
                        responseMessage = "The case has shareholders listed, but the risk assessment is still pending. Please complete the risk assessment to finalize the case status.";
                    }
                    else
                    {
                        var latestRisk = riskAssessment?.FirstOrDefault();
                        string riskDisplayScore = latestRisk?.FinalScore ?? caseDetails.RiskScore.ToString();
                        responseMessage = $"Case #{finalCustId} is currently in {caseDetails.CaseStatus} status with an overall risk score of {riskDisplayScore}. All major assessments are currently under review.";
                    }
                }

                return Ok(new
                {
                    caseId = finalCustId,
                    customerName = $"{caseDetails.FirstName} {caseDetails.LastName}",
                    status = responseMessage,
                    customerType = caseDetails.CustomerType == "I" ? "Individual" : "Corporate",
                    createdOn = caseDetails.CreatedOn ?? "N/A",
                    queryType = queryType
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving case status", error = ex.Message });
            }
        }
    }
}
