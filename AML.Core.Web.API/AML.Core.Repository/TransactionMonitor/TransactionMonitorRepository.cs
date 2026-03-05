using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.TransactionMonitor;
using AML.DTO.DTO.TransactionMonitor;
using AML.ViewModel.ViewModels.TransactionMonitor;
using Dapper;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using MySql.Data.MySqlClient;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper.Contrib.Extensions;
using System.Threading.Tasks;

namespace AML.Core.Repository.TransactionMonitor
{
    public class TransactionMonitorRepository : BaseRepository, ITransactionMonitorRepository
    {
        public TransactionMonitorRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<List<TMSCaseModelDTO>> GetAll()
        {
            ServiceResponse<List<TMSCaseModelDTO>> serviceResponse = new ServiceResponse<List<TMSCaseModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSCaseModelDTO>("get_all_tms_case", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                ServiceResponse<List<TmsRuleViolatedDTO>> serviceResponse1 = new ServiceResponse<List<TmsRuleViolatedDTO>>();
                var tmsCases = serviceResponse.Result;
                for (int i = 0; i < tmsCases.Count; i++)
                {
                    parameters.Add("p_tms_case_batch_id", tmsCases[i].BatchId);
                    serviceResponse1.Result = Get<TmsRuleViolatedDTO>("get_tms_rules_violated_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "TMS rules violated list fetched successfully";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    tmsCases[i].RuleViolated = serviceResponse1.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<TMSCaseModelDTO> GetCaseDetailsById(int id)
        {
            ServiceResponse<TMSCaseModelDTO> serviceResponse = new ServiceResponse<TMSCaseModelDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                parameters.Add("@p_type", 1);
                serviceResponse.Result = GetFirstOrDefault<TMSCaseModelDTO>("get_all_tms_case_details_by_id", parameters, commandType: CommandType.StoredProcedure);
                TMSCaseModelDTO _tmsCase = new TMSCaseModelDTO();
                _tmsCase = serviceResponse.Result;
                serviceResponse.Message = "Customer profile fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

                ServiceResponse<List<TmsTransactionDTO>> serviceResponse1 = new ServiceResponse<List<TmsTransactionDTO>>();
                parameters.Add("@p_id", id);
                parameters.Add("@p_type", 2);
                serviceResponse1.Result = Get<TmsTransactionDTO>("get_all_tms_case_details_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                _tmsCase.TmsTransactionModel = serviceResponse1.Result;
                serviceResponse1.Message = "Customer transaction details fetched successfully.";
                serviceResponse1.Status = StaticResource.SuccessStatusCode;

                ServiceResponse<List<TmsRuleViolatedDTO>> serviceResponse2 = new ServiceResponse<List<TmsRuleViolatedDTO>>();

                parameters.Add("p_tms_case_batch_id", _tmsCase.BatchId);
                serviceResponse2.Result = Get<TmsRuleViolatedDTO>("get_tms_rules_violated_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse2.Message = "TMS rules violated list fetched successfully";
                serviceResponse2.Status = StaticResource.SuccessStatusCode;
                _tmsCase.RuleViolated = serviceResponse2.Result;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateTmsCase(TMSCaseModelDTO model, string culture)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_CaseId", model.Id);
                parameters.Add("@p_Status", model.Status);
                parameters.Add("@p_Remarks", model.Remarks);
                parameters.Add("p_culture", culture);
                parameters.Add("@p_UpdatedBy", model.UpdatedBy);
                parameters.Add("@p_UpdatedOn", DateTime.Now);
                parameters.Add("@p_UpdatedStatus", model.UpdatedStatus);
                var response = ExecuteScalar("mod_tms_case", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "status updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertTMSMaster(TMSCaseModelDTO tmsCase)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_customer_id", tmsCase.CustomerId);
                parameters.Add("p_customer_name", tmsCase.CustomerName);
                parameters.Add("p_customer_risk_score", tmsCase.CustomerRiskScore);
                parameters.Add("p_entity_id", tmsCase.EntityId);
                parameters.Add("p_entity_branch", tmsCase.EntityBranch);
                parameters.Add("p_user_id", tmsCase.UserId);
                var ruleData = "";
                var rule_index = 0;
                for (var j = 0; j < tmsCase.RuleViolated.Count; j++)
                {
                    ruleData +=
                        tmsCase.RuleViolated[j].RuleName + "Ø";
                    rule_index++;
                }
                var rowData = "";
                var index = 0;
                for (var i = 0; i < tmsCase.TmsTransactionModel.Count; i++)
                {
                    rowData +=
                           tmsCase.TmsTransactionModel[i].TransactionDate + "Ø" +
                           tmsCase.TmsTransactionModel[i].TransactionNumber + "Ø" +
                           tmsCase.TmsTransactionModel[i].InternalRefNumber + "Ø" +
                           tmsCase.TmsTransactionModel[i].TransactionLocation + "Ø" +
                           tmsCase.TmsTransactionModel[i].Authorizer + "Ø" +
                           tmsCase.TmsTransactionModel[i].Currency + "Ø" +
                           tmsCase.TmsTransactionModel[i].Amount + "Ø" +
                           tmsCase.TmsTransactionModel[i].ProductCode + "Ø" +
                           tmsCase.TmsTransactionModel[i].Product + "Ø" +
                           tmsCase.TmsTransactionModel[i].Beneficiary + "Ø" +
                           tmsCase.TmsTransactionModel[i].GoodServices + "Ø" +
                           tmsCase.TmsTransactionModel[i].TransactionDescription + "Ø" +
                           tmsCase.TmsTransactionModel[i].User + "¥";
                    index++;
                }
                parameters.Add("p_rule_data", ruleData);
                parameters.Add("p_rule_index", rule_index);

                parameters.Add("@p_tms_trans_data", rowData);
                parameters.Add("@p_index", index);
                serviceResponse.Result = ExecuteScalar("ins_tms_case", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSCaseModelDTO>> GetTMSBetweenDates(DateTime fromDate, DateTime toDate)
        {
            ServiceResponse<List<TMSCaseModelDTO>> serviceResponse = new ServiceResponse<List<TMSCaseModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_from_date", fromDate);
                parameters.Add("p_to_date", toDate);
                serviceResponse.Result = Get<TMSCaseModelDTO>("get_all_tms_cases_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "TMS cases fetched successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSRulesDTO>> GetRules()
        {
            ServiceResponse<List<TMSRulesDTO>> serviceResponse = new ServiceResponse<List<TMSRulesDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSRulesDTO>("get_all_tms_rules", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "TMS rules fetched succesfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                var tmsRules = serviceResponse.Result;
                for (var i = 0; i < tmsRules.Count; i++)
                {
                    ServiceResponse<List<TransactionCategoryDTO>> serviceResponse1 = new ServiceResponse<List<TransactionCategoryDTO>>();
                    parameters.Add("p_tms_rules_id", tmsRules[i].Id);
                    serviceResponse1.Result = Get<TransactionCategoryDTO>("get_tms_category_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "TMS Category fetched successfully";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    tmsRules[i].TransactionCategory = serviceResponse1.Result;
                    ServiceResponse<List<AllowedTransactionTypeDTO>> serviceResponse2 = new ServiceResponse<List<AllowedTransactionTypeDTO>>();
                    serviceResponse2.Result = Get<AllowedTransactionTypeDTO>("get_tms_allowed_transaction_type", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse2.Message = "TMS allowed transaction type fetched successfully";
                    serviceResponse2.Status = StaticResource.SuccessStatusCode;
                    tmsRules[i].AllowedTransactionTypes = serviceResponse2.Result;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSMasterDTO>> GetAllTMSMasterCategories(string culture)
        {
            ServiceResponse<List<TMSMasterDTO>> serviceResponse = new ServiceResponse<List<TMSMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_culture", culture);
                serviceResponse.Result = Get<TMSMasterDTO>("get_lovmaster_risk_category_all_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSTypeCategoryDTO>> GetRiskCategoryTypes(string riskCategoryID, string culture)
        {
            ServiceResponse<List<TMSTypeCategoryDTO>> serviceResponse = new ServiceResponse<List<TMSTypeCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_category", riskCategoryID);
                parameters.Add("p_culture", culture);
                serviceResponse.Result = Get<TMSTypeCategoryDTO>("get_lovtypeCategory_by_riskcategory_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }
        public ServiceResponse<List<TMSMasterDTO>> GetRiskTypes(string riskCategory, string culture)
        {
            ServiceResponse<List<TMSMasterDTO>> serviceResponse = new ServiceResponse<List<TMSMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_catogory", riskCategory);
                parameters.Add("p_culture", culture);
                serviceResponse.Result = Get<TMSMasterDTO>("get_lovtype_by_catogorytype_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSTypeCategoryDTO>> GetRiskCategoryType(string riskCategoryID, string culture)
        {
            ServiceResponse<List<TMSTypeCategoryDTO>> serviceResponse = new ServiceResponse<List<TMSTypeCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_risk_category_code", riskCategoryID);
                parameters.Add("p_culture", culture);
                serviceResponse.Result = Get<TMSTypeCategoryDTO>("get_lov_riskcategorytype_by_riskcategory_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;

        }
        public ServiceResponse<List<TMSMasterDTO>> GetRiskType(string riskCategory, string culture)
        {
            ServiceResponse<List<TMSMasterDTO>> serviceResponse = new ServiceResponse<List<TMSMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_catogory", riskCategory);
                parameters.Add("p_culture", culture);
                serviceResponse.Result = Get<TMSMasterDTO>("get_lov_risktype_by_categorytype_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk Types fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSMasterDTO>> GetRiskItems(int riskTypeID, string culture)
        {
            ServiceResponse<List<TMSMasterDTO>> serviceResponse = new ServiceResponse<List<TMSMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_lov_type_id", riskTypeID);
                parameters.Add("p_culture", culture);
                serviceResponse.Result = Get<TMSMasterDTO>("get_lov_riskitems_by_lovtype_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Risk items fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSRulesViewModelDTO>> GetAllRules()
        {
            ServiceResponse<List<TMSRulesViewModelDTO>> serviceResponse = new ServiceResponse<List<TMSRulesViewModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSRulesViewModelDTO>("get_transaction_monitor_rules", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSNewRulesNamesDTO>> GetRuleNames()
        {
            ServiceResponse<List<TMSNewRulesNamesDTO>> serviceResponse = new ServiceResponse<List<TMSNewRulesNamesDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSNewRulesNamesDTO>("get_all_rules_Names", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Rules details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSRulesDetailsDTO>> GetAllRuleparamters(int id)
        {
            ServiceResponse<List<TMSRulesDetailsDTO>> serviceResponse = new ServiceResponse<List<TMSRulesDetailsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = Get<TMSRulesDetailsDTO>("get_ruleparameters_byid", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Rule Details  fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> UpdateTMSRuleDetailsStatus(int ruleid)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@p_id", ruleid);
                serviceResponse.Result = ExecuteScalar("mod_tms_ruledetails_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Master Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> UpdateTMSRuleMasterStatus(int ruleid)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@p_id", ruleid);

                serviceResponse.Result = ExecuteScalar("mod_tms_rulemaster_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Master Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }



        public ServiceResponse<int> InsertTMSRulesMaster(TMSRulesMasterDTO tmsRulesMaster)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("p_tms_rm_rulename", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_rulescore", tmsRulesMaster.TMSRuleScore);
                parameters.Add("p_tms_rm_detail_string", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_isactive", tmsRulesMaster.TMSRuleIsActive);
                parameters.Add("p_tms_rm_createdby", tmsRulesMaster.TMSRuleCreatedBy);
                parameters.Add("p_tms_rm_descriptions", tmsRulesMaster.TMSRuleDescription);
                parameters.Add("p_tms_rm_outergrpoperator", tmsRulesMaster.TMSRuleOuterGrpoperator);
                parameters.Add("p_clientid", tmsRulesMaster.Client_Id);
                serviceResponse.Result = ExecuteScalar("ins_tms_rules_master", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Master Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> UpdateTMSRulesMaster(TMSRulesMasterDTO tmsRulesMaster)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("p_tms_rm_rulename", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_rulescore", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_detail_string", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_isactive", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_createdby", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_tms_rm_createdon", tmsRulesMaster.TMSRuleName);
                parameters.Add("p_clientid", tmsRulesMaster.Client_Id);


                serviceResponse.Result = ExecuteScalar("mod_tms_rules_master", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Master Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertTMSRulesDetails(TMSRulesDetailsDTO tmsRulesDetails)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("p_tms_rd_autoid", tmsRulesDetails.TMSRuleDetAutoID);
                parameters.Add("p_tms_rd_masterid", tmsRulesDetails.TMSRuleDetMasterID);
                parameters.Add("p_tms_rd_paramtype", tmsRulesDetails.TMSRuleDetParamType);
                parameters.Add("p_tms_rd_source", tmsRulesDetails.TMSRuleDetSource);
                parameters.Add("p_tms_rd_sourceaggregate", tmsRulesDetails.TMSRuleDetSourceAggr);
                parameters.Add("p_tms_rd_operator", tmsRulesDetails.TMSRuleDetOperator);
                parameters.Add("p_tms_rd_target", tmsRulesDetails.TMSRuleDetTarget);
                parameters.Add("p_tms_rd_targetaggregate", tmsRulesDetails.TMSRuleDetTargetAggr);
                parameters.Add("p_tms_rd_value", tmsRulesDetails.TMSRuleDetValue);
                parameters.Add("p_tms_rd_timeframesource", tmsRulesDetails.TMSRuleDetTimeFrameSource);
                parameters.Add("p_tms_rd_timeframevalue", tmsRulesDetails.TMSRuleDetTimeFrameVal);
                parameters.Add("p_tms_rd_timeframetype", tmsRulesDetails.TMSRuleDetTimeFrameType);
                parameters.Add("p_tms_rd_compareto", tmsRulesDetails.TMSRuleDetCompareTo);
                parameters.Add("p_tms_rd_comparevalue", tmsRulesDetails.TMSRuleDetCompareValue);
                parameters.Add("p_tms_rd_comparetimestartvalue", tmsRulesDetails.TMSRuleDetCompareTimeStart);
                parameters.Add("p_tms_rd_comparetimestarttype", tmsRulesDetails.TMSRuleDetCompareTimeStartType);
                parameters.Add("p_tms_rd_comparetimeendvalue", tmsRulesDetails.TMSRuleDetCompareTimeEnd);
                parameters.Add("p_tms_rd_comparetimeendtype", tmsRulesDetails.TMSRuleDetCompareTimeEndType);
                parameters.Add("p_tms_rd_forsame", tmsRulesDetails.TMSRuleDetForSame);
                parameters.Add("p_tms_rd_status", tmsRulesDetails.TMSRuleDetStatus);
                parameters.Add("p_tms_rd_createdby", tmsRulesDetails.TMSRuleDetCreatedBy);
                parameters.Add("p_tms_rd_descriptions", tmsRulesDetails.TMSRuleDetDescription);
                parameters.Add("p_tms_rd_operatorgrp", tmsRulesDetails.TMSOperatorGRP);
                parameters.Add("p_tms_rd_percentage", tmsRulesDetails.TMSRuleDetPercentValue);
                parameters.Add("p_tms_rd_if_source", tmsRulesDetails.TMSRuleDetSourceIfSource);
                parameters.Add("p_tms_rd_if_source_aggregate", tmsRulesDetails.TMSRuleDetSourceIfSourceAggr);
                parameters.Add("p_tms_rd_if_target", tmsRulesDetails.TMSRuleDetCompareFieldIfSource);
                parameters.Add("p_tms_rd_if_target_aggregate", tmsRulesDetails.TMSRuleDetCompareFieldIfSourceAggr);
                parameters.Add("p_tms_rd_if_source_value", tmsRulesDetails.TMSRuleDetSourceIfValue);
                parameters.Add("p_tms_rd_if_target_value", tmsRulesDetails.TMSRuleDetCompareFieldIfValue);
                parameters.Add("p_tms_rd_if_source_operator", tmsRulesDetails.TMSRuleDetSourceIfOperator);
                parameters.Add("p_tms_rd_if_target_operator", tmsRulesDetails.TMSRuleDetCompareFieldIfOperator);
                parameters.Add("@p_clientid", tmsRulesDetails.Client_Id);

                serviceResponse.Result = ExecuteScalar("ins_tms_rules_details", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {

                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        private static IEnumerable<int> RangeIterator(int start, int stop, int step)
        {
            int x = start;

            do
            {
                yield return x;
                x += step;
                if (step < 0 && x <= stop || 0 < step && stop <= x)
                    break;
            }
            while (true);
        }

        public ServiceResponse<bool> SetMonitoredStatus(List<TMSNewMasterDTO> transactions)
        {

            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = true;
                foreach (var i in RangeIterator(0, transactions.Count(), 1000))
                {
                    Console.WriteLine($"Update transactions {i} - {i + 1000}");

                    parameters.Add("p_ids", string.Join(',', transactions.Skip(i).Take(1000).Select(val => val.id)));
                    parameters.Add("p_status", 1);

                    serviceResponse.Result &= ExecuteScalar("mod_monitor_status", parameters, commandType: CommandType.StoredProcedure).ParseInt() == 1;
                }

                Console.WriteLine("Transaction monitor status updated successfully");

                serviceResponse.Message = "Transaction Rules Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public  ServiceResponse<int> InsertTransactionMonitor(TransactionMonitorAPIDTO tms)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();


            using (IDbConnection conn = GetConnections())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        tms.createdon = DateTime.Now;

                        serviceResponse.Result = conn.InsertAsync(entityToInsert: tms, transaction: trans).Result;
                       
                        if (serviceResponse.Result != 0)
                        {
                            serviceResponse.Message = "Transaction Details inserted successfully";
                            serviceResponse.Status = StaticResource.SuccessStatusCode;
                        }

                        else 
                        { 
                            serviceResponse.Message = "There was an error while insering the data. Please check the data provided";
                            serviceResponse.Status = StaticResource.FailStatusCode;
                        }
                        trans.Commit();

                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        serviceResponse.Message = ex.Message;
                        serviceResponse.Status = StaticResource.FailStatusCode;
                    }
                }

            }
                     
            return serviceResponse;
        }

        public void BulkInsertMySQL(DataSet table, string tableName, MySqlConnection connection, MySqlTransaction tran)
        {
            Console.WriteLine($"Update in table {tableName}");

            using MySqlCommand cmd = new MySqlCommand();

            cmd.Connection = connection;
            cmd.Transaction = tran;
            cmd.CommandText = $"SELECT * FROM " + tableName + " limit 0";

            using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
            {
                adapter.UpdateBatchSize = 10000;
                using MySqlCommandBuilder cb = new MySqlCommandBuilder(adapter);
                cb.SetAllValues = true;
                adapter.Update(table, tableName);
            };
        }

        public ServiceResponse<bool> InsertRuleViolatedTransactions(List<TMSNewMasterDTO> tmsTransactionsViolated, TMSRulesMasterDTO masterRule)
        {
            var WriteLog = true;

            if (WriteLog) Console.WriteLine("========================================");

            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();

            var dsMatchDetails = new DataSet();
            var dsCaseManagement = new DataSet();

            DataTable MatchDetailsTable = dsMatchDetails.Tables.Add("tms_match_details");
            DataTable CaseManagementTable = dsCaseManagement.Tables.Add("tms_casemgmnt_data");

            MatchDetailsTable.Columns.Add("tms_match_caseID", typeof(int));
            MatchDetailsTable.Columns.Add("tms_match_status", typeof(int));
            MatchDetailsTable.Columns.Add("tms_match_createdby", typeof(int));
            MatchDetailsTable.Columns.Add("tms_match_createdon", typeof(DateTime));
            MatchDetailsTable.Columns.Add("tms_match_hitTranRefno", typeof(string));
            MatchDetailsTable.Columns.Add("tms_match_monitorTranRefno", typeof(string));

            CaseManagementTable.Columns.Add("comments", typeof(string));
            CaseManagementTable.Columns.Add("monitoredby", typeof(int));
            CaseManagementTable.Columns.Add("violated_ruleId", typeof(int));
            CaseManagementTable.Columns.Add("violated_tranRefno", typeof(string));
            

            string CommandInsertCaseManagement = @"INSERT INTO tms_casemgmnt_data (
                                                        violated_ruleId, violated_tranRefno, monitoredby, monitoredon, comments
                                                   ) VALUES (
                                                        @violated_ruleId, @violated_tranRefno, @monitoredby, NOW(), @comments
                                                   );";

            using (var mySQLConnection = GetConnections())
            {
                mySQLConnection.Open();

                MySqlTransaction SQLTransaction = mySQLConnection.BeginTransaction();

                if (WriteLog) Console.WriteLine("Open SQL connetion and start SQLTransaction");

                try
                {
                    var mySqlDataAdapterCaseManagement = new MySqlDataAdapter { InsertCommand = new MySqlCommand(CommandInsertCaseManagement, mySQLConnection) };

                    mySqlDataAdapterCaseManagement.InsertCommand.Parameters.Add("@comments", MySqlDbType.VarChar, 100, "comments");
                    mySqlDataAdapterCaseManagement.InsertCommand.Parameters.Add("@monitoredby", MySqlDbType.Int32, 32, "monitoredby");
                    mySqlDataAdapterCaseManagement.InsertCommand.Parameters.Add("@violated_ruleId", MySqlDbType.Int32, 32, "violated_ruleId");
                    mySqlDataAdapterCaseManagement.InsertCommand.Parameters.Add("@violated_tranRefno", MySqlDbType.VarChar, 45, "violated_tranRefno");

                    if (WriteLog) Console.WriteLine("Created data adapters");

                    mySqlDataAdapterCaseManagement.InsertCommand.UpdatedRowSource = UpdateRowSource.None;

                    mySqlDataAdapterCaseManagement.UpdateBatchSize = 1;

                    foreach (TMSNewMasterDTO tmsTransactionViolated in tmsTransactionsViolated)
                    {
                        if (tmsTransactionViolated.status != 0 || tmsTransactionViolated.tmsstatus != 0) { continue; }

                        if (WriteLog) Console.WriteLine("========================================");
                        if (WriteLog) Console.WriteLine($"- Monitor transaction {tmsTransactionViolated.id}, status: {tmsTransactionViolated.status}, tmsStatus: {tmsTransactionViolated.tmsstatus}");

                        DataRow rowCaseManagement = dsCaseManagement.Tables["tms_casemgmnt_data"].NewRow();

                        rowCaseManagement["comments"] = tmsTransactionViolated.comments;
                        rowCaseManagement["monitoredby"] = tmsTransactionViolated.updatedby;
                        rowCaseManagement["violated_ruleId"] = masterRule.TMSRuleID;
                        rowCaseManagement["violated_tranRefno"] = tmsTransactionViolated.TranRefno;

                        dsCaseManagement.Tables["tms_casemgmnt_data"].Rows.Clear();
                        dsCaseManagement.Tables["tms_casemgmnt_data"].Rows.Add(rowCaseManagement);

                        try
                        {
                            mySqlDataAdapterCaseManagement.Update(dsCaseManagement, "tms_casemgmnt_data");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Transaction errored: {ex.Message}");
                            continue;
                        }

                        if (WriteLog) Console.WriteLine($"- Add {dsCaseManagement.Tables["tms_casemgmnt_data"].Rows.Count} transactions to case management table");

                        string new_query = "SELECT LAST_INSERT_ID();";
                        MySqlCommand cmd = new MySqlCommand(new_query, mySQLConnection);
                        int lastInsertedCaseId = Convert.ToInt32(cmd.ExecuteScalar());

                        if (WriteLog) Console.WriteLine($"- Got new id: {lastInsertedCaseId}");

                        var matchDetailRows = new List<DataRow>();

                        var transactionHitsForRule = new List<TMSNewMasterDTO>();

                        foreach (var param in masterRule.TMSRuleParameters)
                        {
                            if (WriteLog) Console.WriteLine($"-- Check against param {param.TMSRuleDetAutoID}");

                            var transactionHitsForParam = new List<TMSNewMasterDTO>();

                            string aggregatePropName = null;

                            if (param.TMSRuleDetSourceAggr != "" && param.TMSRuleDetTargetAggr == "") { aggregatePropName = param.TMSRuleDetSourceAggr; }
                            if (param.TMSRuleDetSourceAggr == "" && param.TMSRuleDetTargetAggr != "") { aggregatePropName = param.TMSRuleDetTargetAggr; }
                            if (param.TMSRuleDetForSame != "") { aggregatePropName = param.TMSRuleDetForSame; }

                            if (WriteLog) Console.WriteLine($"-- Aggregate prop name '{aggregatePropName}'");

                            var transactionsToInsert = new List<TMSNewMasterDTO>();

                            if (aggregatePropName.IsNotNullOrEmpty())
                            {
                                var groupedTransactions = tmsTransactionsViolated
                                    .GroupBy(val => val.GetType().GetProperty(aggregatePropName).GetValue(val, null))
                                    .Where(val => val.Any(val => val.RemitterId == tmsTransactionViolated.RemitterId))
                                    .SelectMany(gr => gr)
                                    .ToList();

                                if (WriteLog) Console.WriteLine($"-- prop name not empty, add {groupedTransactions.Count()} transactions to case details");

                                transactionsToInsert = groupedTransactions;
                            }
                            else
                            {
                                if (WriteLog) Console.WriteLine($"-- prop name empty, add violated transaction to case details");

                                transactionsToInsert = tmsTransactionsViolated;
                            }

                            foreach (var transactionToInsert in transactionsToInsert)
                            {
                                transactionHitsForParam.Add(transactionToInsert);
                            }

                            if (transactionHitsForRule.Count() == 0)
                            {
                                if (WriteLog) Console.WriteLine($"-- Insert {transactionHitsForParam.Count()} transactions from first param list.");

                                transactionHitsForRule.AddRange(transactionHitsForParam);
                                continue;
                            }

                            if (WriteLog) Console.WriteLine($"-- Insert {transactionHitsForParam.Count()} transactions from next param list.");

                            transactionHitsForRule = transactionHitsForRule.Intersect(transactionHitsForParam).ToList();
                        }

                        foreach (TMSNewMasterDTO transaction in transactionHitsForRule)
                        {
                            DataRow mathcDetailRow = dsMatchDetails.Tables["tms_match_details"].NewRow();

                            mathcDetailRow["tms_match_status"] = 0;
                            mathcDetailRow["tms_match_caseID"] = lastInsertedCaseId;
                            mathcDetailRow["tms_match_createdon"] = DateTime.Now;
                            mathcDetailRow["tms_match_createdby"] = transaction.createdby;
                            mathcDetailRow["tms_match_hitTranRefno"] = tmsTransactionViolated.TranRefno;
                            mathcDetailRow["tms_match_monitorTranRefno"] = transaction.TranRefno;

                            matchDetailRows.Add(mathcDetailRow);
                        }

                        var listWithMoreThanOneElementInGroup = matchDetailRows; // .GroupBy(val => val["tms_match_hitTranRefno"]).Where(row => row.Count() > 1).SelectMany(gr => gr).ToList();

                        if (WriteLog) Console.WriteLine($"- Got {(listWithMoreThanOneElementInGroup.Count() == 0 ? matchDetailRows : listWithMoreThanOneElementInGroup).Count()} total transactions after filtering param");

                        foreach (var row in listWithMoreThanOneElementInGroup.Count() == 0 ? matchDetailRows : listWithMoreThanOneElementInGroup)
                        {
                            dsMatchDetails.Tables["tms_match_details"].Rows.Add(row);
                        }

                        if (WriteLog) Console.WriteLine("- Finished monitoring transaction");

                        WriteLog = false;
                    }

                    Console.WriteLine("========================================");

                    BulkInsertMySQL(dsMatchDetails, "tms_match_details", mySQLConnection, SQLTransaction);

                    Console.WriteLine($"Added {dsMatchDetails.Tables["tms_match_details"].Rows.Count} transactions to match details");

                    SQLTransaction.Commit();
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);

                    SQLTransaction.Rollback();

                    serviceResponse.Result = false;

                    return serviceResponse;
                }

                serviceResponse.Result = true;
            }

            return serviceResponse;
        }

        public ServiceResponse<int> UpdateTMSRulesDetails(TMSRulesDetailsDTO tmsRulesDetails)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("p_tms_rd_autoid", tmsRulesDetails.TMSRuleDetAutoID);
                parameters.Add("p_tms_rd_masterid", tmsRulesDetails.TMSRuleDetMasterID);
                parameters.Add("p_tms_rd_paramtype", tmsRulesDetails.TMSRuleDetParamType);
                parameters.Add("p_tms_rd_source", tmsRulesDetails.TMSRuleDetSource);
                parameters.Add("p_tms_rd_sourceaggregate", tmsRulesDetails.TMSRuleDetSourceAggr);
                parameters.Add("p_tms_rd_operator", tmsRulesDetails.TMSRuleDetOperator);
                parameters.Add("p_tms_rd_target", tmsRulesDetails.TMSRuleDetTarget);
                parameters.Add("p_tms_rd_targetaggregate", tmsRulesDetails.TMSRuleDetTargetAggr);
                parameters.Add("p_tms_rd_value", tmsRulesDetails.TMSRuleDetValue);
                parameters.Add("p_tms_rd_timeframesource", tmsRulesDetails.TMSRuleDetTimeFrameSource);
                parameters.Add("p_tms_rd_timeframevalue", tmsRulesDetails.TMSRuleDetTimeFrameVal);
                parameters.Add("p_tms_rd_timeframetype", tmsRulesDetails.TMSRuleDetTimeFrameType);
                parameters.Add("p_tms_rd_compareto", tmsRulesDetails.TMSRuleDetCompareTo);
                parameters.Add("p_tms_rd_comparevalue", tmsRulesDetails.TMSRuleDetCompareValue);
                parameters.Add("p_tms_rd_comparefield", tmsRulesDetails.TMSRuleDetCompareField);
                parameters.Add("p_tms_rd_comparefieldaggregate", tmsRulesDetails.TMSRuleDetCompareFieldAggr);
                parameters.Add("p_tms_rd_comparetimestartvalue", tmsRulesDetails.TMSRuleDetCompareTimeStart);
                parameters.Add("p_tms_rd_comparetimestarttype", tmsRulesDetails.TMSRuleDetCompareTimeStartType);
                parameters.Add("p_tms_rd_comparetimeendvalue", tmsRulesDetails.TMSRuleDetCompareTimeEnd);
                parameters.Add("p_tms_rd_comparetimeendtype", tmsRulesDetails.TMSRuleDetCompareTimeEndType);
                parameters.Add("p_tms_rd_forsame", tmsRulesDetails.TMSRuleDetForSame);
                parameters.Add("p_tms_rd_status", tmsRulesDetails.TMSRuleDetStatus);
                parameters.Add("p_tms_rd_createdby", tmsRulesDetails.TMSRuleDetCreatedBy);
                parameters.Add("p_tms_rd_createdon", tmsRulesDetails.CreatedDB);

                serviceResponse.Result = ExecuteScalar("mod_tms_rules_details", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<List<TMSRulesMasterDTO>> GetAllTMSRules(int clientid)
        {
            ServiceResponse<List<TMSRulesMasterDTO>> serviceResponse = new ServiceResponse<List<TMSRulesMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_clientID", clientid);
                serviceResponse.Result = Get<TMSRulesMasterDTO>("get_all_tms_masterrules", parameters, commandType: CommandType.StoredProcedure).ToList();

                var tmsRules = serviceResponse.Result;

                for (var i = 0; i < tmsRules.Count; i++)
                {
                    ServiceResponse<List<TMSRulesDetailsDTO>> serviceResponse1 = new ServiceResponse<List<TMSRulesDetailsDTO>>();
                    parameters.Add("p_rule_id", tmsRules[i].TMSRuleID);
                    parameters.Add("p_clientID", clientid);
                    serviceResponse1.Result = Get<TMSRulesDetailsDTO>("get_all_tms_ruledetails", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse1.Message = "TMS Rules fetched successfully";
                    serviceResponse1.Status = StaticResource.SuccessStatusCode;
                    tmsRules[i].TMSRuleParameters = serviceResponse1.Result;
                }

                serviceResponse.Message = "Transaction Monitor Rules fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSNewMasterDTO>> GetAllTransactionsForTMS()
        {
            ServiceResponse<List<TMSNewMasterDTO>> serviceResponse = new ServiceResponse<List<TMSNewMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSNewMasterDTO>("get_all_transactions_for_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transactions fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSNewMasterDTO>> GetAllUnmonitoredTransactionsForTMS()
        {
            ServiceResponse<List<TMSNewMasterDTO>> serviceResponse = new ServiceResponse<List<TMSNewMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSNewMasterDTO>("get_all_unmonitored_transactions_for_tms", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transactions fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSFieldsmodelDTO>> GetFieldsnames()
        {
            ServiceResponse<List<TMSFieldsmodelDTO>> serviceResponse = new ServiceResponse<List<TMSFieldsmodelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSFieldsmodelDTO>("get_all_FieldNames", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transaction Monitor Rules fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSCustomerTypeModelDTO>> GetCustomTypedrpdown()
        {
            ServiceResponse<List<TMSCustomerTypeModelDTO>> serviceResponse = new ServiceResponse<List<TMSCustomerTypeModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSCustomerTypeModelDTO>("get_all_customtype", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transaction Monitor Rules fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSTranTypeDTO>> GetTranTypedrpdown()
        {
            ServiceResponse<List<TMSTranTypeDTO>> serviceResponse = new ServiceResponse<List<TMSTranTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<TMSTranTypeDTO>("get_all_trantype", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transaction Monitor Rules fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> InsertTMSDraftLog(TMSDraftLogModelDTO tmsDraftLog)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_tms_rule_id", tmsDraftLog.TMSRuleId);
                parameters.Add("p_tms_rule_name", tmsDraftLog.TMSRuleName);
                parameters.Add("p_tms_score", tmsDraftLog.TMSScore);
                parameters.Add("p_tms_rule_description", tmsDraftLog.TMSRuleDescription);
                parameters.Add("p_tms_rule_parameter", tmsDraftLog.TMSRuleparameter);
                parameters.Add("p_tms_ruleparameter_description", tmsDraftLog.TMSRuleParameterDescription);
                parameters.Add("p_tms_operator", tmsDraftLog.TMSRuleOperator);
                parameters.Add("p_tms_result", tmsDraftLog.TMSResult);
                parameters.Add("p_tms_createdBy", tmsDraftLog.TMSCreatedBy);
                parameters.Add("p_clientid", tmsDraftLog.client_id);
                serviceResponse.Result = ExecuteScalar("ins_tms_rule_draftlog", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Transaction Rules Master Details inserted successfully";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

   

        //public ServiceResponse<List<TMSCaseDTO>> GetAllTMSCase()
        //{
        //    ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
        //    try
        //    {
        //        DynamicParameters parameters = new DynamicParameters();

        //        serviceResponse.Result = Get<TMSCaseDTO>("get_all_new_tms_case", parameters, commandType: CommandType.StoredProcedure).ToList();
        //        serviceResponse.Message = "TMS Cases fetched successfully.";
        //        serviceResponse.Status = StaticResource.SuccessStatusCode;
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.Error.WriteLine(ex);
        //        serviceResponse.Message = ex.Message;
        //        serviceResponse.Status = StaticResource.FailStatusCode;
        //    }
        //    return serviceResponse;
        //}



        public ServiceResponse<List<TMSCaseDTO>> GetAllTMSCasefordashboard(int clientid)
        {
            ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientid",clientid);
                serviceResponse.Result = Get<TMSCaseDTO>("get_all_tmsfordashboard", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSCaseDTO>> GetAllTMSCaseReport(TMSCaseReportRequestDTO TMSCaseReportRequestDTO)
        {
            ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("c_from", Convert.ToDateTime(TMSCaseReportRequestDTO.StartDate));
                parameters.Add("c_to", Convert.ToDateTime(TMSCaseReportRequestDTO.EndDate));
                parameters.Add("c_txn_date_from", Convert.ToDateTime(TMSCaseReportRequestDTO.TxnStartDate));
                parameters.Add("c_txn_date_to", Convert.ToDateTime(TMSCaseReportRequestDTO.TxnEndDate));

                serviceResponse.Result = Get<TMSCaseDTO>("get_all_new_tms_case_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Transaction cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<TMSCaseDTO> GetTMSCaseDetailsById(string tranRefno)
        {
            ServiceResponse<TMSCaseDTO> serviceResponse = new ServiceResponse<TMSCaseDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranRefno", tranRefno);
                serviceResponse.Result = GetFirstOrDefault<TMSCaseDTO>("get_all_new_tms_case_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Transaction Case Details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<TMSCaseDTO>> GetTMSCasehitDetailsById(int tranCaseId)
        {
            ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranCaseId", tranCaseId);
                serviceResponse.Result = Get<TMSCaseDTO>("get_all_new_tms_casehits_by_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer profile fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<List<TMSCaseDTO>> GetAllLastTransactionByRemitterId(string RemitterId)
        {
            ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_RemitterId", RemitterId);
                serviceResponse.Result = Get<TMSCaseDTO>("get_all_last_transaction_by_remitterid", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer profile fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSCaseDTO>> GetAllLastTransactionByBeneficiaryId(string BeneficiaryId)
        {
            ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_BeneficiaryId", BeneficiaryId);
                serviceResponse.Result = Get<TMSCaseDTO>("get_all_last_transaction_by_beneficiaryId", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer profile fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<List<TMSCaseDTO>> GetAllExculsiveTransactionBetweenRemitterBeneficiary(string RemitterId, string BeneficiaryId)
        {
            ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_RemitterId", RemitterId);
                parameters.Add("@p_BeneficiaryId", BeneficiaryId);
                serviceResponse.Result = Get<TMSCaseDTO>("get_all_exclusive_transaction_by_remitterid_BeneficiaryId", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer profile fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }


        public ServiceResponse<int> UpdateNewTmsCase(TMSCaseDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_tranRefno", model.CustomerId);
                parameters.Add("@p_Status", model.Status);
                parameters.Add("@p_Comments", model.comments);
                parameters.Add("@p_UpdatedBy", model.UpdatedBy);

                var response = ExecuteScalar("mod_tms_newcase", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "status updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSRulesMasterDTO>> GetNewTMSRules(int clientid,int userid) 
        {
            ServiceResponse<List<TMSRulesMasterDTO>> serviceResponse = new ServiceResponse<List<TMSRulesMasterDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientID", clientid);
                parameters.Add("@p_createdBy",userid);
                serviceResponse.Result = Get<TMSRulesMasterDTO>("get_all_tms_masterrules", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<TMSRulesMasterDTO> GetNewTMSRules(int eid)
        {
            ServiceResponse<TMSRulesMasterDTO> serviceResponse = new ServiceResponse<TMSRulesMasterDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_eid", eid);
                serviceResponse.Result = GetFirstOrDefault<TMSRulesMasterDTO>("get_tms_rules_from_master_by_id", parameters, commandType: CommandType.StoredProcedure);

                serviceResponse.Message = "TMS Rules fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }



        public ServiceResponse<int> Update(TMSRulesMasterDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", model.TMSRuleID);
                parameters.Add("@p_Status", model.TMSRuleIsActive);
                var response = ExecuteScalar("mod_tms_rules_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "status updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> GetTMSDraftLogRuleId()
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = ExecuteScalar("get_tms_draftlog_ruleid", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "Rule ID  fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<TMSDraftLogModelDTO>> GetAllTMSDraftlog(int clientId, int userid)
        {
            ServiceResponse<List<TMSDraftLogModelDTO>> serviceResponse = new ServiceResponse<List<TMSDraftLogModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientID", clientId);
                parameters.Add("@p_createdBy", userid);
                serviceResponse.Result = Get<TMSDraftLogModelDTO>("get_all_tms_draftlog", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<DataListModel>> GetAllTMSDraftlogID(int id)
        {
            ServiceResponse<List<DataListModel>> serviceResponse = new ServiceResponse<List<DataListModel>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = Get<DataListModel>("get_all_tms_draftlog_ID", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<TMSDraftLogModelDTO> GetAllTMSDraftlogResults(int id)
        {
            ServiceResponse<TMSDraftLogModelDTO> serviceResponse = new ServiceResponse<TMSDraftLogModelDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                serviceResponse.Result = Get<TMSDraftLogModelDTO>("get_all_tms_draftresults", parameters, commandType: CommandType.StoredProcedure).FirstOrDefault();
                serviceResponse.Message = "Customer cases fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

		public ServiceResponse<List<TMSCaseDTO>> GetAllTMSCase()
		{
			ServiceResponse<List<TMSCaseDTO>> serviceResponse = new ServiceResponse<List<TMSCaseDTO>>();
			try
			{
				DynamicParameters parameters = new DynamicParameters();
				serviceResponse.Result = Get<TMSCaseDTO>("get_all_new_tms_case", parameters, commandType: CommandType.StoredProcedure).ToList();
				serviceResponse.Message = "Customer cases fetched successfully.";
				serviceResponse.Status = StaticResource.SuccessStatusCode;

			}
			catch (Exception ex)
			{
				Console.Error.WriteLine(ex);
				serviceResponse.Message = ex.Message;
				serviceResponse.Status = StaticResource.FailStatusCode;
			}
			return serviceResponse;
		}
	}
}
