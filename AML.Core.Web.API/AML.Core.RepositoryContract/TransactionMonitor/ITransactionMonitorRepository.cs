
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.TransactionMonitor;
using AML.ViewModel.ViewModels.TransactionMonitor;
using System;
using System.Collections.Generic;

namespace AML.Core.RepositoryContract.TransactionMonitor
{
    public interface ITransactionMonitorRepository : IBaseRepository
    {
        //ServiceResponse<List<TMSMasterDTO>> GetAllTMSMasterCategories(string culture);
        ServiceResponse<int> InsertTMSMaster(TMSCaseModelDTO model);
        //ServiceResponse<int> UpdateTMSMaster(TMSMasterDTO model, string culture);
        ServiceResponse<List<TMSCaseModelDTO>> GetAll();
        ServiceResponse<TMSCaseModelDTO> GetCaseDetailsById(int id);
        ServiceResponse<int> UpdateTmsCase(TMSCaseModelDTO model, string culture);
        ServiceResponse<List<TMSCaseModelDTO>> GetTMSBetweenDates(DateTime fromDate, DateTime toDate);
        ServiceResponse<List<TMSRulesDTO>> GetRules();
        ServiceResponse<List<TMSMasterDTO>> GetAllTMSMasterCategories(string culture);
        ServiceResponse<List<TMSTypeCategoryDTO>> GetRiskCategoryTypes(string riskCategoryID, string culture);
        ServiceResponse<List<TMSMasterDTO>> GetRiskTypes(string riskCategoryType, string culture);
        ServiceResponse<List<TMSTypeCategoryDTO>> GetRiskCategoryType(string riskCategoryID, string culture);
        ServiceResponse<List<TMSMasterDTO>> GetRiskType(string riskCategoryType, string culture);
        ServiceResponse<List<TMSMasterDTO>> GetRiskItems(int riskTypeID, string culture);
        ServiceResponse<List<TMSRulesViewModelDTO>> GetAllRules();
        ServiceResponse<int> InsertTMSRulesMaster(TMSRulesMasterDTO model);
        ServiceResponse<int> InsertTMSRulesDetails(TMSRulesDetailsDTO model);
        ServiceResponse<int> UpdateTMSRulesMaster(TMSRulesMasterDTO model);
        ServiceResponse<int> UpdateTMSRulesDetails(TMSRulesDetailsDTO model);

        ServiceResponse<int> UpdateTMSRuleMasterStatus(int ruleid);
        ServiceResponse<int> UpdateTMSRuleDetailsStatus(int ruleid);
        ServiceResponse<List<TMSRulesMasterDTO>> GetAllTMSRules(int clientid);

        ServiceResponse<List<TMSNewRulesNamesDTO>> GetRuleNames();

        ServiceResponse<List<TMSRulesDetailsDTO>> GetAllRuleparamters(int id);

        ServiceResponse<List<TMSFieldsmodelDTO>> GetFieldsnames();

        ServiceResponse<List<TMSCustomerTypeModelDTO>> GetCustomTypedrpdown();

        ServiceResponse<List<TMSTranTypeDTO>> GetTranTypedrpdown();
        ServiceResponse<List<TMSNewMasterDTO>> GetAllTransactionsForTMS();
        ServiceResponse<List<TMSNewMasterDTO>> GetAllUnmonitoredTransactionsForTMS();

        ServiceResponse<int> InsertTMSDraftLog(TMSDraftLogModelDTO model);

        ServiceResponse<int> GetTMSDraftLogRuleId();

        ServiceResponse<bool> InsertRuleViolatedTransactions(List<TMSNewMasterDTO> model, TMSRulesMasterDTO ruleHit);
        ServiceResponse<bool> SetMonitoredStatus(List<TMSNewMasterDTO> transactions);


        ServiceResponse<List<TMSCaseDTO>> GetAllTMSCase();

		ServiceResponse<List<TMSCaseDTO>> GetAllTMSCasefordashboard(int clientid);
		ServiceResponse<List<TMSCaseDTO>> GetAllTMSCaseReport(TMSCaseReportRequestDTO TMSCaseReportRequestDTO);

        ServiceResponse<TMSCaseDTO> GetTMSCaseDetailsById(string tranRefno);

        ServiceResponse<List<TMSCaseDTO>> GetTMSCasehitDetailsById(int tranCaseId);
        ServiceResponse<List<TMSCaseDTO>> GetAllLastTransactionByRemitterId(string RemitterId);
        ServiceResponse<List<TMSCaseDTO>> GetAllLastTransactionByBeneficiaryId(string BeneficiaryId);

        ServiceResponse<List<TMSCaseDTO>> GetAllExculsiveTransactionBetweenRemitterBeneficiary(string RemitterId, string BeneficiaryId);

        ServiceResponse<int> UpdateNewTmsCase(TMSCaseDTO model);
        ServiceResponse<List<TMSRulesMasterDTO>> GetNewTMSRules(int clientid,int userid);
        ServiceResponse<TMSRulesMasterDTO> GetNewTMSRules(int eid);

        ServiceResponse<int> Update(TMSRulesMasterDTO model);

        //List<TMSCaseDTO> GetAllTMSCaseReport(TMSCaseReportRequestDTO model);

        ServiceResponse<List<TMSDraftLogModelDTO>> GetAllTMSDraftlog(int clientId,int userid);

        ServiceResponse<List<DataListModel>> GetAllTMSDraftlogID(int id);

        ServiceResponse<TMSDraftLogModelDTO>  GetAllTMSDraftlogResults(int id);
        
        ServiceResponse<int> InsertTransactionMonitor(TransactionMonitorAPIDTO model);
      
    }
}
