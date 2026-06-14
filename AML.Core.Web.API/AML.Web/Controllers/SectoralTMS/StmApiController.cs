using AML.Core.ServiceContract.SectoralTMS;
using AML.DTO.DTO.SectoralTMS;
using AML.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AML.Web.Controllers.SectoralTMS
{
    /// <summary>
    /// REST API endpoints for Sectoral Transaction Monitoring System.
    /// All endpoints require a valid JWT (via [JwtAuthorize]).
    /// Base route: /api/stm
    /// </summary>
    [Route("api/stm")]
    [ApiController]
    public class StmApiController : ControllerBase
    {
        private readonly IStmService _stm;

        public StmApiController(IStmService stm)
        {
            _stm = stm;
        }

        // ----- Sectors -----
        [HttpGet("sectors")]
        [JwtAuthorize]
        public IActionResult GetSectors([FromQuery] int clientId)
            => Ok(_stm.GetSectors(clientId));

        // ----- Rules -----
        [HttpGet("rules")]
        [JwtAuthorize]
        public IActionResult GetRules([FromQuery] int clientId, [FromQuery] string sectorCode = null)
            => Ok(_stm.GetRules(clientId, sectorCode));

        [HttpGet("rules/{id:int}")]
        [JwtAuthorize]
        public IActionResult GetRule(int id) => Ok(_stm.GetRule(id));

        [HttpPost("rules")]
        [JwtAuthorize]
        public IActionResult CreateRule([FromBody] StmRuleDTO rule)
        {
            if (rule == null) return BadRequest(new { message = "Rule payload is required" });
            return Ok(_stm.CreateRule(rule));
        }

        [HttpPut("rules/{id:int}")]
        [JwtAuthorize]
        public IActionResult UpdateRule(int id, [FromBody] StmRuleDTO rule)
        {
            if (rule == null) return BadRequest(new { message = "Rule payload is required" });
            rule.Id = id;
            return Ok(_stm.UpdateRule(rule));
        }

        [HttpPatch("rules/{id:int}/status")]
        [JwtAuthorize]
        public IActionResult ToggleRule(int id, [FromBody] ToggleStatusDTO payload)
            => Ok(_stm.ToggleRuleStatus(id, payload.IsActive, payload.UserId));

        [HttpPost("rules/test")]
        [JwtAuthorize]
        public IActionResult TestRule([FromBody] StmRuleTestPayload payload)
        {
            if (payload?.Rule == null || payload?.Transaction == null)
                return BadRequest(new { message = "Rule and Transaction are required" });
            return Ok(_stm.TestRuleAgainstTransaction(payload.Rule, payload.Transaction));
        }

        // ----- Transactions -----
        [HttpPost("transactions")]
        [JwtAuthorize]
        public IActionResult SubmitTransaction([FromBody] StmTransactionDTO tran, [FromQuery] int userId)
        {
            if (tran == null) return BadRequest(new { message = "Transaction payload is required" });
            return Ok(_stm.SubmitTransaction(tran, userId));
        }

        [HttpPost("transactions/search")]
        [JwtAuthorize]
        public IActionResult SearchTransactions([FromBody] StmTransactionSearchDTO search)
            => Ok(_stm.SearchTransactions(search ?? new StmTransactionSearchDTO()));

        [HttpGet("transactions/{id:int}")]
        [JwtAuthorize]
        public IActionResult GetTransaction(int id) => Ok(_stm.GetTransaction(id));

        [HttpGet("transactions/customer/{customerId}")]
        [JwtAuthorize]
        public IActionResult GetCustomerTransactions(string customerId, [FromQuery] int clientId)
            => Ok(_stm.GetCustomerTransactions(customerId, clientId));

        // ----- Customers -----
        [HttpGet("customers/search")]
        [JwtAuthorize]
        public IActionResult SearchCustomers([FromQuery] string q, [FromQuery] int clientId)
            => Ok(_stm.SearchCustomers(q, clientId));

        [HttpGet("customers/{customerId}")]
        [JwtAuthorize]
        public IActionResult GetCustomer(string customerId, [FromQuery] int clientId)
            => Ok(_stm.GetCustomerDetails(customerId, clientId));

        // ----- Cases -----
        [HttpPost("cases/open")]
        [JwtAuthorize]
        public IActionResult GetOpenCases([FromBody] StmCaseSearchDTO search)
            => Ok(_stm.GetOpenCases(search ?? new StmCaseSearchDTO()));

        [HttpPost("cases/completed")]
        [JwtAuthorize]
        public IActionResult GetCompletedCases([FromBody] StmCaseSearchDTO search)
            => Ok(_stm.GetCompletedCases(search ?? new StmCaseSearchDTO()));

        [HttpGet("cases/{id:int}")]
        [JwtAuthorize]
        public IActionResult GetCase(int id) => Ok(_stm.GetCase(id));

        [HttpPost("cases/{id:int}/review")]
        [JwtAuthorize]
        public IActionResult ReviewCase(int id, [FromBody] StmReviewPayload payload)
            => Ok(_stm.ReviewCase(id, payload.Decision, payload.Remarks, payload.Status, payload.UserId, payload.UserName));

        [HttpPost("cases/{id:int}/comment")]
        [JwtAuthorize]
        public IActionResult AddComment(int id, [FromBody] StmCommentPayload payload)
            => Ok(_stm.AddCaseComment(id, payload.Comment, payload.UserId, payload.UserName));

        // ----- Transaction Risk -----
        [HttpGet("risk/factors")]
        [JwtAuthorize]
        public IActionResult GetRiskFactors([FromQuery] int sectorId, [FromQuery] int clientId, [FromQuery] bool includeInactive = false)
            => Ok(includeInactive
                ? _stm.GetAllTxnRiskFactors(sectorId, clientId)
                : _stm.GetTxnRiskFactors(sectorId, clientId));

        [HttpGet("risk/factors/{id:int}")]
        [JwtAuthorize]
        public IActionResult GetRiskFactor(int id) => Ok(_stm.GetTxnRiskFactor(id));

        [HttpPost("risk/factors")]
        [JwtAuthorize]
        public IActionResult CreateRiskFactor([FromBody] StmTxnRiskFactorDTO factor)
        {
            if (factor == null) return BadRequest(new { message = "Factor payload is required" });
            return Ok(_stm.CreateTxnRiskFactor(factor));
        }

        [HttpPut("risk/factors/{id:int}")]
        [JwtAuthorize]
        public IActionResult UpdateRiskFactor(int id, [FromBody] StmTxnRiskFactorDTO factor)
        {
            if (factor == null) return BadRequest(new { message = "Factor payload is required" });
            factor.Id = id;
            return Ok(_stm.UpdateTxnRiskFactor(factor));
        }

        [HttpDelete("risk/factors/{id:int}")]
        [JwtAuthorize]
        public IActionResult DeleteRiskFactor(int id) => Ok(_stm.DeleteTxnRiskFactor(id));

        [HttpPost("risk/preview")]
        [JwtAuthorize]
        public IActionResult PreviewRisk([FromBody] StmTransactionDTO tran)
            => Ok(_stm.PreviewTxnRisk(tran));

        [HttpPost("risk/evaluate/{transactionId:int}")]
        [JwtAuthorize]
        public IActionResult EvaluateRisk(int transactionId)
            => Ok(_stm.EvaluateTxnRisk(transactionId));

        [HttpGet("risk/result/{transactionId:int}")]
        [JwtAuthorize]
        public IActionResult GetRisk(int transactionId)
            => Ok(_stm.GetSavedRiskResult(transactionId));
    }

    public class ToggleStatusDTO
    {
        public int IsActive { get; set; }
        public int UserId { get; set; }
    }

    public class StmRuleTestPayload
    {
        public StmRuleDTO Rule { get; set; }
        public StmTransactionDTO Transaction { get; set; }
    }

    public class StmReviewPayload
    {
        public string Decision { get; set; }
        public string Status { get; set; }
        public string Remarks { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }

    public class StmCommentPayload
    {
        public string Comment { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
    }
}
