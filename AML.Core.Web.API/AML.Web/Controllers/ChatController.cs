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
        public async Task<IActionResult> GetStatus(string caseId)
        {
            try
            {
                CustomerCaseDTO caseDetails = null;

                // 1. Try to treat as numeric CaseId first
                if (int.TryParse(caseId, out int numericCaseId))
                {
                    caseDetails = _customerCaseService.GetCaseFullDetailsByCaseId(numericCaseId);
                }

                // 2. If not found or not numeric, treat as CustomerId (e.g. NAT32)
                if (caseDetails == null)
                {
                    caseDetails = _customerCaseService.GetCaseFullDetailsByCustId(caseId);
                }

                if (caseDetails == null)
                {
                    return NotFound(new { message = "Case not found" });
                }

                // Use the correct internal ID (CustId) for subsequent service calls
                string finalCustId = caseDetails.CustomerId;

                var shareholders = _customerCaseService.GetShareHoldersByCompanyCode(finalCustId);
                var riskAssessment = _riskService.GetLastestRiskVersion(finalCustId, caseDetails.CustomerType);

                string statusMessage = "";
                bool hasHits = false; // Simplified for simulation

                // Scenario 1: No shareholders and empty risk assessment
                if ((shareholders == null || !shareholders.Any()) && (riskAssessment == null || !riskAssessment.Any()))
                {
                    statusMessage = "Your risk assessment is incomplete. Please complete it to obtain a proper overall risk score and proceed further.";
                }
                // Scenario 2: Has shareholders (and potentially risk assessment)
                else if (shareholders != null && shareholders.Any())
                {
                    // Check for hits (simplified simulation logic)
                    hasHits = shareholders.Any(s => s.MatchScore > 0); 
                    if (hasHits)
                    {
                        statusMessage = "There are pending shareholder search hits that need to be updated. You should complete the review for both the shareholders and the risk assessment to proceed.";
                    }
                    else
                    {
                        statusMessage = $"Case #{finalCustId} is under review. Shareholders verified with no critical hits.";
                    }
                }
                else
                {
                    statusMessage = $"Case #{finalCustId} is currently in {caseDetails.CaseStatus} status. Risk Score: {caseDetails.RiskScore}.";
                }

                return Ok(new
                {
                    caseId = finalCustId,
                    customerName = $"{caseDetails.FirstName} {caseDetails.LastName}",
                    status = statusMessage,
                    customerType = caseDetails.CustomerType == "I" ? "Individual" : "Corporate",
                    createdOn = caseDetails.CreatedOn ?? "N/A"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving case status", error = ex.Message });
            }
        }
    }
}
