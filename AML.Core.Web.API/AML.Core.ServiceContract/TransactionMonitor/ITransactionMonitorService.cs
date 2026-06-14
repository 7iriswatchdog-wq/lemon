using AML.Core.Common.StaticResource;
using AML.DTO.DTO.TransactionMonitor;
using AML.ViewModel.ViewModels.TransactionMonitor;
using System;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.TransactionMonitor
{
    public interface ITransactionMonitorService : IBaseService
    {
        List<TMSMasterDTO> GetAllTMSMasterCategories(string culture);
        List<TMSTypeCategoryDTO> GetRiskCategoryTypes(string riskCategoryID, string culture);
        ServiceResponse<int> InsertTMSMaster(TMSCaseModelDTO model);
        List<TMSCaseModelDTO> GetAll();
        TMSCaseModelDTO GetCaseDetailsById(int id);
        ServiceResponse<int> UpdateTmsCase(TMSCaseModelDTO model, string culture);
        ServiceResponse<List<TMSCaseModelDTO>> GetTMSBetweenDates(DateTime fromDate, DateTime toDate);
        ServiceResponse<List<TMSRulesDTO>> GetRules();
        List<TMSMasterDTO> GetRiskTypes(string riskCategoryType, string culture);
        List<TMSTypeCategoryDTO> GetRiskCategoryType(string riskCategoryID, string culture);
        List<TMSMasterDTO> GetRiskType(string riskTypeCategoryID, string culture);
        List<TMSMasterDTO> GetRiskItems(int riskTypeID, string culture);
        List<TMSRulesViewModelDTO> GetAllRules();
        ServiceResponse<int> InsertTransactionMonitor(TransactionMonitorAPIDTO model);

        ServiceResponse<int> InsertTMSRulesMaster(TMSRulesMasterDTO model);
        ServiceResponse<int> InsertTMSRulesDetails(TMSRulesDetailsDTO model);
        List<TMSRulesMasterDTO> GetAllTMSRules(int ClientId);

        ServiceResponse<int> UpdateTMSRulesMaster(TMSRulesMasterDTO model);
        ServiceResponse<int> UpdateTMSRulesDetails(TMSRulesDetailsDTO model);

        ServiceResponse<int> UpdateTMSRuleMasterStatus(int ruleid);
        ServiceResponse<int> UpdateTMSRuleDetailsStatus(int ruleid);

        List<TMSNewRulesNamesDTO> GetRuleNames();

        List<TMSRulesDetailsDTO> GetAllRuleparamters(int id);

        List<TMSFieldsmodelDTO> GetFieldsnames();

        List<TMSNewMasterDTO> GetAllTransactionsForTMS(TMSNewMasterDTO newTransaction = null);

        List<TMSNewMasterDTO> GetAllTransactionsBycustomerIdforTMS(TMSNewMasterDTO newTransaction = null);

        List<TMSNewMasterDTO> GetAllUnmonitoredTransactionsForTMS();

        List<TMSCustomerTypeModelDTO> GetCustomTypedrpdown();

        List<TMSTranTypeDTO> GetTranTypedrpdown();

        ServiceResponse<int> InsertTMSDraftLog(TMSDraftLogModelDTO model);

        ServiceResponse<int> GetTMSDraftLogRuleId();

        ServiceResponse<bool> InsertRuleViolatedTransactions(List<TMSNewMasterDTO> model, TMSRulesMasterDTO ruleHit);
        ServiceResponse<bool> SetMonitoredStatus(List<TMSNewMasterDTO> transactions);
		List<TMSCaseDTO> GetAllTMSCase();
		List<TMSCaseDTO> GetAllTMSCasefordashboard(int clientid);
        TMSCaseDTO GetTMSCaseDetailsById(string tranRefno);
        List<TMSCaseDTO> GetTMSCasehitDetailsById(int tranCaseId);
        List<TMSCaseDTO> GetAllLastTransactionByRemitterId(string RemitterId);
        List<TMSCaseDTO> GetAllLastTransactionByBeneficiaryId(string BeneficiaryId);
        List<TMSCaseDTO> GetAllExculsiveTransactionBetweenRemitterBeneficiary(string RemitterId,string BeneficiaryId);
        ServiceResponse<int> UpdateNewTmsCase(TMSCaseDTO model);
        List<TMSRulesMasterDTO> GetNewTMSRules(int clientid,int userid);
        TMSRulesMasterDTO GetNewTMSRules(int eid);
        ServiceResponse<int> Update(TMSRulesMasterDTO model);
       // List<TMSCaseDTO> GetCaseReportList(TMSCaseReportRequestDTO model);
        List<TMSCaseDTO> GetAllTMSCaseReport(TMSCaseReportRequestDTO model);

        List<TMSDraftLogModelDTO> GetAllTMSDraftlog(int clientid, int userid);


        //List<TMSDraftLogModelDTO> GetAllTMSDraftlogID(int id);

        List<DataListModel> GetAllTMSDraftlogID(int id);

        TMSDraftLogModelDTO GetAllTMSDraftlogResults(int id);

    }
  
}
