using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.TransactionMonitor;
using AML.Core.ServiceContract.TransactionMonitor;
using AML.DTO.DTO.TransactionMonitor;
using AML.ViewModel.ViewModels.TransactionMonitor;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Service.TransactionMonitor
{
    public class TransactionMonitorService : BaseService, ITransactionMonitorService
    {
        ITransactionMonitorRepository _tmsmasterRepository;
        List<TMSNewMasterDTO> transactionsList;
        bool transactionRulesUpdated;
        List<TMSRulesMasterDTO> transactionRules;
        public TransactionMonitorService(ITransactionMonitorRepository tmsmasterRepository, IConfiguration configuration, IHostingEnvironment environment) : base(tmsmasterRepository, configuration)
        {
            _tmsmasterRepository = tmsmasterRepository;
            transactionsList = _tmsmasterRepository.GetAllTransactionsForTMS().Result;
            transactionRulesUpdated = false;
            transactionRules = null;
        }

       
        public List<TMSCaseModelDTO> GetAll()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAll().Result;
        }

        public ServiceResponse<int> UpdateTmsCase(TMSCaseModelDTO model, string culture)
        {
            return _tmsmasterRepository.UpdateTmsCase(model, culture);
        }
        public TMSCaseModelDTO GetCaseDetailsById(int id)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetCaseDetailsById(id).Result;
        }
        public ServiceResponse<List<TMSCaseModelDTO>> GetTMSBetweenDates(DateTime fromDate, DateTime toDate)
        {
            return _tmsmasterRepository.GetTMSBetweenDates(fromDate, toDate);
        }
        public ServiceResponse<int> InsertTMSMaster(TMSCaseModelDTO model)
        {
            return _tmsmasterRepository.InsertTMSMaster(model);
        }
        public ServiceResponse<List<TMSRulesDTO>> GetRules()
        {
            return _tmsmasterRepository.GetRules();
        }
        public List<TMSMasterDTO> GetAllTMSMasterCategories(string culture)
        {
            return _tmsmasterRepository.GetAllTMSMasterCategories(culture).Result;
        }
        public List<TMSTypeCategoryDTO> GetRiskCategoryTypes(string riskCategoryID, string culture)
        {
            return _tmsmasterRepository.GetRiskCategoryTypes(riskCategoryID, culture).Result;
        }
        public List<TMSMasterDTO> GetRiskTypes(string riskCategoryType, string culture)
        {
            return _tmsmasterRepository.GetRiskTypes(riskCategoryType, culture).Result;
        }
        public List<TMSTypeCategoryDTO> GetRiskCategoryType(string riskCategoryID, string culture)
        {
            return _tmsmasterRepository.GetRiskCategoryType(riskCategoryID, culture).Result;
        }
        public List<TMSMasterDTO> GetRiskType(string riskCategoryType, string culture)
        {
            return _tmsmasterRepository.GetRiskType(riskCategoryType, culture).Result;
        }
        public List<TMSMasterDTO> GetRiskItems(int riskTypeID, string culture)
        {
            return _tmsmasterRepository.GetRiskItems(riskTypeID, culture).Result;
        }
        public List<TMSRulesViewModelDTO> GetAllRules()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllRules().Result;
        }

        public ServiceResponse<int> InsertTransactionMonitor(TransactionMonitorAPIDTO model)
        {
            return _tmsmasterRepository.InsertTransactionMonitor(model);
        }

        public List<TMSNewRulesNamesDTO> GetRuleNames()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetRuleNames().Result;
        }

        public ServiceResponse<int> InsertTMSRulesMaster(TMSRulesMasterDTO model)
        {
            return _tmsmasterRepository.InsertTMSRulesMaster(model);
        }

        public ServiceResponse<int> UpdateTMSRulesDetails(TMSRulesDetailsDTO model)
        {
            return _tmsmasterRepository.UpdateTMSRulesDetails(model);
        }
        public ServiceResponse<int> InsertTMSDraftLog(TMSDraftLogModelDTO model)
        {
            return _tmsmasterRepository.InsertTMSDraftLog(model);
        }

        public ServiceResponse<int> GetTMSDraftLogRuleId()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetTMSDraftLogRuleId();
        }

        public ServiceResponse<int> UpdateTMSRulesMaster(TMSRulesMasterDTO model)
        {
            return _tmsmasterRepository.UpdateTMSRulesMaster(model);
        }

        public ServiceResponse<int> UpdateTMSRuleMasterStatus(int ruleid)
        {
            return _tmsmasterRepository.UpdateTMSRuleMasterStatus(ruleid);
        }
        public ServiceResponse<int> UpdateTMSRuleDetailsStatus(int ruleid)
        {
            return _tmsmasterRepository.UpdateTMSRuleDetailsStatus(ruleid);
        }


        public ServiceResponse<int> InsertTMSRulesDetails(TMSRulesDetailsDTO model)
        {
            return _tmsmasterRepository.InsertTMSRulesDetails(model);
        }
        
        public ServiceResponse<bool> InsertRuleViolatedTransactions(List<TMSNewMasterDTO> hits, TMSRulesMasterDTO ruleHit)
        {
            return _tmsmasterRepository.InsertRuleViolatedTransactions(hits, ruleHit);
        }
        
        public ServiceResponse<bool> SetMonitoredStatus(List<TMSNewMasterDTO> transactions)
        {
            return _tmsmasterRepository.SetMonitoredStatus(transactions);
        }

        public List<TMSRulesMasterDTO> GetAllTMSRules(int clientid)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllTMSRules(clientid).Result;
        }
        public List<TMSRulesDetailsDTO> GetAllRuleparamters(int id)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllRuleparamters(id).Result;
        }
        public List<TMSFieldsmodelDTO> GetFieldsnames()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetFieldsnames().Result;
        }


        public List<TMSCustomerTypeModelDTO> GetCustomTypedrpdown()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetCustomTypedrpdown().Result;
        }

        public List<TMSTranTypeDTO> GetTranTypedrpdown()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetTranTypedrpdown().Result;
        }


        public List<TMSNewMasterDTO> GetAllTransactionsForTMS(TMSNewMasterDTO newTransaction = null)
        {
            //Perform business requirements here
            //return _tmsmasterRepository.GetAllTransactionsForTMS().Result;


            //if (newTransaction != null) { transactionsList.Add(newTransaction);
            //    transactionsList = transactionsList.Where(transaction => transaction.createdon >= DateTime.Now.AddMonths(-6) && transaction.ClientId == newTransaction.ClientId).ToList();
            //}
            //else {
            //    transactionsList = transactionsList.Where(transaction => transaction.createdon >= DateTime.Now.AddMonths(-6)).ToList(); 
            //}

			if (newTransaction != null)
			{
				transactionsList.Add(newTransaction);
				transactionsList = transactionsList.Where(transaction =>  transaction.ClientId == newTransaction.ClientId).ToList();
			}
			else
			{
				transactionsList = transactionsList.Where(transaction => transaction.createdon >= DateTime.Now.AddMonths(-6)).ToList();
			}



			return transactionsList;
        }

        public List<TMSNewMasterDTO> GetAllTransactionsBycustomerIdforTMS(TMSNewMasterDTO newTransaction = null)
        {
            //Perform business requirements here
            //return _tmsmasterRepository.GetAllTransactionsForTMS().Result;


            //if (newTransaction != null) { transactionsList.Add(newTransaction);
            //    transactionsList = transactionsList.Where(transaction => transaction.createdon >= DateTime.Now.AddMonths(-6) && transaction.ClientId == newTransaction.ClientId).ToList();
            //}
            //else {
            //    transactionsList = transactionsList.Where(transaction => transaction.createdon >= DateTime.Now.AddMonths(-6)).ToList(); 
            //}

            if (newTransaction != null)
            {
                transactionsList.Add(newTransaction);
                transactionsList = transactionsList.Where(transaction => transaction.ClientId == newTransaction.ClientId && transaction.CustomerId == newTransaction.CustomerId).ToList();
            }
            else
            {
                transactionsList = transactionsList.Where(transaction => transaction.createdon >= DateTime.Now.AddMonths(-6)).ToList();
            }



            return transactionsList;
        }

        public List<TMSNewMasterDTO> GetAllUnmonitoredTransactionsForTMS()
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllUnmonitoredTransactionsForTMS().Result;
        }


		public List<TMSCaseDTO> GetAllTMSCase()
		{
			//Perform business requirements here
			return _tmsmasterRepository.GetAllTMSCase().Result;
		}
		public List<TMSCaseDTO> GetAllTMSCasefordashboard(int clientid)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllTMSCasefordashboard(clientid).Result;
        }

        public List<TMSCaseDTO> GetAllTMSCaseReport(TMSCaseReportRequestDTO TMSCaseReportRequestDTO) 
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllTMSCaseReport(TMSCaseReportRequestDTO).Result;
        }

       
        public TMSCaseDTO GetTMSCaseDetailsById(string tranRefno)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetTMSCaseDetailsById(tranRefno).Result;
        }

        public List<TMSCaseDTO> GetTMSCasehitDetailsById(int tranCaseId)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetTMSCasehitDetailsById(tranCaseId).Result;
        }

        public List<TMSCaseDTO> GetAllLastTransactionByRemitterId(string RemitterId)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllLastTransactionByRemitterId(RemitterId).Result;
        }
        public List<TMSCaseDTO> GetAllLastTransactionByBeneficiaryId(string BeneficiaryId)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllLastTransactionByBeneficiaryId(BeneficiaryId).Result;
        }
        public List<TMSCaseDTO> GetAllExculsiveTransactionBetweenRemitterBeneficiary(string RemitterId, string BeneficiaryId)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetAllExculsiveTransactionBetweenRemitterBeneficiary(RemitterId, BeneficiaryId).Result;
        }

        public ServiceResponse<int> UpdateNewTmsCase(TMSCaseDTO model)
        {
            return _tmsmasterRepository.UpdateNewTmsCase(model);
        }

        public List<TMSRulesMasterDTO> GetNewTMSRules(int clientid,int userid)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetNewTMSRules(clientid,userid).Result;
        }

        public TMSRulesMasterDTO GetNewTMSRules(int eid)
        {
            //Perform business requirements here
            return _tmsmasterRepository.GetNewTMSRules(eid).Result;
        }
        public ServiceResponse<int> Update(TMSRulesMasterDTO model)
        {
            return _tmsmasterRepository.Update(model);
        }

        public List<TMSDraftLogModelDTO> GetAllTMSDraftlog(int clientId,int userid)
        {
            return _tmsmasterRepository.GetAllTMSDraftlog(clientId,userid).Result;
        }

        public List<DataListModel> GetAllTMSDraftlogID(int id)
        {
            return _tmsmasterRepository.GetAllTMSDraftlogID(id).Result;
        }

        public TMSDraftLogModelDTO GetAllTMSDraftlogResults(int id)
        {
            return _tmsmasterRepository.GetAllTMSDraftlogResults(id).Result;
        }


    }
}
