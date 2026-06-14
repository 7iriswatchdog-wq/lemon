using AML.Core.Common.StaticResource;
using AML.DTO.DTO.SectoralTMS;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.SectoralTMS
{
    public interface IStmService
    {
        // ----- Sector -----
        ServiceResponse<List<StmSectorDTO>> GetSectors(int clientId);

        // ----- Rules -----
        ServiceResponse<int> CreateRule(StmRuleDTO rule);
        ServiceResponse<int> UpdateRule(StmRuleDTO rule);
        ServiceResponse<int> ToggleRuleStatus(int ruleId, int isActive, int userId);
        ServiceResponse<List<StmRuleDTO>> GetRules(int clientId, string sectorCode = null);
        ServiceResponse<StmRuleDTO> GetRule(int ruleId);

        // ----- Transactions -----
        // Persists transaction + parties, then runs rules engine; returns case ID if hit (or 0)
        ServiceResponse<StmTransactionResultDTO> SubmitTransaction(StmTransactionDTO tran, int userId);
        ServiceResponse<List<StmTransactionDTO>> SearchTransactions(StmTransactionSearchDTO search);
        ServiceResponse<StmTransactionDTO> GetTransaction(int id);
        ServiceResponse<List<StmTransactionDTO>> GetCustomerTransactions(string customerId, int clientId);

        // ----- Customer -----
        ServiceResponse<List<dynamic>> SearchCustomers(string query, int clientId);
        ServiceResponse<dynamic> GetCustomerDetails(string customerId, int clientId);

        // ----- Cases -----
        ServiceResponse<List<StmCaseDTO>> GetOpenCases(StmCaseSearchDTO search);
        ServiceResponse<List<StmCaseDTO>> GetCompletedCases(StmCaseSearchDTO search);
        ServiceResponse<StmCaseDTO> GetCase(int caseId);
        ServiceResponse<int> ReviewCase(int caseId, string decision, string remarks, string status, int userId, string userName);
        ServiceResponse<int> AddCaseComment(int caseId, string comment, int userId, string userName);

        // ----- Rule engine (test mode) -----
        ServiceResponse<List<StmRuleEvaluationResultDTO>> TestRuleAgainstTransaction(StmRuleDTO rule, StmTransactionDTO tran);

        // ----- Transaction risk -----
        ServiceResponse<List<StmTxnRiskFactorDTO>> GetTxnRiskFactors(int sectorId, int clientId);
        ServiceResponse<List<StmTxnRiskFactorDTO>> GetAllTxnRiskFactors(int sectorId, int clientId);
        ServiceResponse<StmTxnRiskFactorDTO> GetTxnRiskFactor(int id);
        ServiceResponse<int> CreateTxnRiskFactor(StmTxnRiskFactorDTO factor);
        ServiceResponse<int> UpdateTxnRiskFactor(StmTxnRiskFactorDTO factor);
        ServiceResponse<int> DeleteTxnRiskFactor(int id);
        ServiceResponse<StmTxnRiskEvaluation> EvaluateTxnRisk(int transactionId);
        ServiceResponse<StmTxnRiskEvaluation> PreviewTxnRisk(StmTransactionDTO tran);
        ServiceResponse<StmTxnRiskResultDTO> GetSavedRiskResult(int transactionId);

        // ----- Module Permission (used by Reports / Admin UI) -----
        ServiceResponse<bool> IsStmModuleEnabled(int clientId);
        ServiceResponse<List<string>> GetAllowedSectorCodes(int clientId);
    }

    public class StmTransactionResultDTO
    {
        public int TransactionId { get; set; }
        public string TranRefNo { get; set; }
        public bool AnyRuleHit { get; set; }
        public int CaseId { get; set; }
        public string CaseRefNo { get; set; }
        public List<StmRuleEvaluationResultDTO> RuleResults { get; set; } = new List<StmRuleEvaluationResultDTO>();
    }

    public class StmRuleEvaluationResultDTO
    {
        public int RuleId { get; set; }
        public string RuleCode { get; set; }
        public string RuleName { get; set; }
        public string RiskRating { get; set; }
        public int Score { get; set; }
        public bool IsHit { get; set; }
        public string HitReason { get; set; }
        public List<string> ConditionResults { get; set; } = new List<string>();
    }
}
