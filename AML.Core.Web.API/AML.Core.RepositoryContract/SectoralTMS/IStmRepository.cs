using AML.Core.Common.StaticResource;
using AML.DTO.DTO.SectoralTMS;
using System.Collections.Generic;

namespace AML.Core.RepositoryContract.SectoralTMS
{
    public interface IStmRepository
    {
        // ----- Sector -----
        ServiceResponse<List<StmSectorDTO>> GetSectors(int clientId);
        ServiceResponse<StmSectorDTO> GetSectorByCode(string sectorCode, int clientId);

        // ----- Rules -----
        ServiceResponse<int> InsertRule(StmRuleDTO rule);
        ServiceResponse<int> InsertRuleCondition(StmRuleConditionDTO condition);
        ServiceResponse<int> UpdateRule(StmRuleDTO rule);
        ServiceResponse<int> DeleteRuleConditions(int ruleId);
        ServiceResponse<int> ToggleRuleStatus(int ruleId, int isActive, int updatedBy);
        ServiceResponse<List<StmRuleDTO>> GetRules(int clientId, string sectorCode = null);
        ServiceResponse<StmRuleDTO> GetRuleById(int ruleId);
        ServiceResponse<List<StmRuleConditionDTO>> GetRuleConditions(int ruleId);

        // ----- Transactions -----
        ServiceResponse<int> InsertTransaction(StmTransactionDTO tran);
        ServiceResponse<int> InsertTransactionParty(StmTransactionPartyDTO party);
        ServiceResponse<int> UpdateTransactionRuleStatus(int transactionId, string status, int updatedBy);
        ServiceResponse<List<StmTransactionDTO>> SearchTransactions(StmTransactionSearchDTO search);
        ServiceResponse<StmTransactionDTO> GetTransactionById(int id);
        ServiceResponse<StmTransactionDTO> GetTransactionByRefNo(string refNo, int clientId);
        ServiceResponse<List<StmTransactionPartyDTO>> GetTransactionParties(int transactionId);
        ServiceResponse<List<StmTransactionDTO>> GetCustomerTransactions(string customerId, int clientId);

        // ----- Customer lookup -----
        ServiceResponse<List<dynamic>> SearchCustomers(string query, int clientId);
        ServiceResponse<dynamic> GetCustomerDetails(string customerId, int clientId);

        // ----- Cases -----
        ServiceResponse<int> InsertCase(StmCaseDTO @case);
        ServiceResponse<int> UpdateCase(StmCaseDTO @case);
        ServiceResponse<int> InsertCaseComment(StmCaseCommentDTO comment);
        ServiceResponse<List<StmCaseDTO>> SearchCases(StmCaseSearchDTO search);
        ServiceResponse<StmCaseDTO> GetCaseById(int id);
        ServiceResponse<List<StmCaseCommentDTO>> GetCaseComments(int caseId);
        ServiceResponse<int> InsertRuleExecLog(StmRuleExecLogDTO log);

        // ----- Module Permission helpers (consumed by the Case Report flow) -----
        // Returns true if Menu_Id = 12 (Transaction Monitoring) is enabled for the
        // given client in client_right_master. Wrapped in the repo so callers
        // don't need to bring in another repo just to do this lookup.
        ServiceResponse<bool> IsStmModuleEnabled(int clientId);
        // Sector codes (e.g. "INS", "RE") the client has access to. National
        // Compliance (is_master = 1) returns every active sector.
        ServiceResponse<List<string>> GetAllowedSectorCodes(int clientId);

        // ----- Transaction Risk -----
        ServiceResponse<List<StmTxnRiskFactorDTO>> GetActiveRiskFactors(int sectorId, int clientId);
        ServiceResponse<List<StmTxnRiskFactorDTO>> GetAllRiskFactors(int sectorId, int clientId);
        ServiceResponse<StmTxnRiskFactorDTO> GetRiskFactorById(int id);
        ServiceResponse<List<StmTxnRiskBandDTO>> GetBandsForFactor(int factorId);
        ServiceResponse<int> InsertRiskFactor(StmTxnRiskFactorDTO factor);
        ServiceResponse<int> UpdateRiskFactor(StmTxnRiskFactorDTO factor);
        ServiceResponse<int> DeleteRiskFactor(int factorId);
        ServiceResponse<int> InsertRiskBand(StmTxnRiskBandDTO band);
        ServiceResponse<int> DeleteBandsForFactor(int factorId);
        ServiceResponse<int> SaveRiskResult(StmTxnRiskResultDTO result);
        ServiceResponse<StmTxnRiskResultDTO> GetRiskResultForTransaction(int transactionId);
    }
}
