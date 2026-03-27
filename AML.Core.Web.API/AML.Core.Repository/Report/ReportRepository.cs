using AML.Core.Common.StaticResource;
using AML.Core.DataContract.Enum;
using AML.Core.RepositoryContract.Report;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.Report;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.Report;
using AML.Web.Controllers.Reports;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Configuration;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AML.Core.Repository.Report
{
    public class ReportRepository: BaseRepository, IReportRepository
    {

        private readonly IHttpContextAccessor httpContextAccessor;
        public ReportRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<CaseReportListDTO>> GetCaseReportList(CaseReportRequestDTO requestModel)
            {
                ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
                try
                {
                //var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                //var UpdatedByUserId = String.IsNullOrEmpty(requestModel.UpdatedByUserId) ? "0" : requestModel.UpdatedByUserId;
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(requestModel.matchscore))
                {
                    var parts = requestModel.matchscore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                DynamicParameters parameters = new DynamicParameters();
                    parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                    parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                    parameters.Add("@c_status", Convert.ToInt32(requestModel.Status));
                    parameters.Add("@c_clientId", requestModel.ClientId);
                    parameters.Add("@cust_type", (requestModel.Cust_type));
                    parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                    parameters.Add("p_matchto", matchTo, DbType.Int32);
                    parameters.Add("p_createdBy", requestModel.createdBy);
                    parameters.Add("c_status", requestModel.caseStatus);
                    parameters.Add("p_riskLevel", requestModel.riskLevel);
                serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                    serviceResponse.Message = "CustomerCase details fetched successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseReportListDTO>> GetCaseReportListBySearch(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                //var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                //var UpdatedByUserId = String.IsNullOrEmpty(requestModel.UpdatedByUserId) ? "0" : requestModel.UpdatedByUserId;
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(requestModel.matchscore))
                {
                    var parts = requestModel.matchscore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@c_status", Convert.ToInt32(requestModel.Status));
                parameters.Add("@c_clientId", requestModel.ClientId);
                parameters.Add("@cust_type", (requestModel.Cust_type));
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy", requestModel.createdBy);
                parameters.Add("c_status", requestModel.caseStatus);
                parameters.Add("p_riskLevel", requestModel.riskLevel);
                parameters.Add("p_searchvalue", requestModel.SearchValue);
                serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_report_by_searchvalue", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CaseReportListDTO>> GetCaseManagementReportList(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(requestModel.matchscore))
                {
                    var parts = requestModel.matchscore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                var UpdatedByUserId = String.IsNullOrEmpty(requestModel.UpdatedByUserId) ? "0" : requestModel.UpdatedByUserId;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@p_userid", Convert.ToInt32(userID));
                parameters.Add("@cust_type", (requestModel.Cust_type));
                parameters.Add("p_usergroup", requestModel.usergroupName);
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy", requestModel.createdBy);
                parameters.Add("c_status", requestModel.caseStatus);
                parameters.Add("p_riskLevel", requestModel.riskLevel);
                if (requestModel.usergroupName == "Senior Management")
                {
                    serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_seniormanagement_excel_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                else
                {
                    serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_excel_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseReportListDTO>> GetCaseManagementSearchValueReportList(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                int? matchFrom = null;
                int? matchTo = null;

                if (!string.IsNullOrWhiteSpace(requestModel.matchscore))
                {
                    var parts = requestModel.matchscore.Split('-');

                    if (parts.Length == 2 &&
                        int.TryParse(parts[0], out int from) &&
                        int.TryParse(parts[1], out int to))
                    {
                        matchFrom = from;
                        matchTo = to;
                    }
                }
                var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                var UpdatedByUserId = String.IsNullOrEmpty(requestModel.UpdatedByUserId) ? "0" : requestModel.UpdatedByUserId;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@p_userid", Convert.ToInt32(userID));
                parameters.Add("@cust_type", (requestModel.Cust_type));
                parameters.Add("p_usergroup", requestModel.usergroupName);
                parameters.Add("p_matchfrom", matchFrom, DbType.Int32);
                parameters.Add("p_matchto", matchTo, DbType.Int32);
                parameters.Add("p_createdBy", requestModel.createdBy);
                parameters.Add("c_status", requestModel.caseStatus);
                parameters.Add("p_riskLevel", requestModel.riskLevel);
                parameters.Add("p_searchvalue", requestModel.SearchValue);
                if (requestModel.usergroupName == "Senior Management")
                {
                    serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_seniormanagement_search_value_excel_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                else
                {
                    serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_search_value_excel_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseReportListDTO>> GetCompletedCaseReportList(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                var UpdatedByUserId = String.IsNullOrEmpty(requestModel.UpdatedByUserId) ? "0" : requestModel.UpdatedByUserId;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@p_userid", Convert.ToInt32(userID));
                parameters.Add("@cust_type", (requestModel.Cust_type));
                serviceResponse.Result = Get<CaseReportListDTO>("get_all_customercase_completed_excel_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseReportListDTO>> GetCasePreviousWeekReportList(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                var UpdatedByUserId = String.IsNullOrEmpty(requestModel.UpdatedByUserId) ? "0" : requestModel.UpdatedByUserId;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@c_user", Convert.ToInt32(userID));
                parameters.Add("@c_status", Convert.ToInt32(requestModel.Status));
                parameters.Add("@c_clientId", requestModel.ClientId);
                parameters.Add("@cust_type", (requestModel.Cust_type));
                parameters.Add("@c_updated_by_id", Convert.ToInt32(UpdatedByUserId));
                serviceResponse.Result = Get<CaseReportListDTO>("get_all_previous_week_customercase_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<CaseReportListDTO>> GetLatestDate(int clientId)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
               
                DynamicParameters parameters = new DynamicParameters();
                
                parameters.Add("@c_clientId", clientId);
              
                serviceResponse.Result = Get<CaseReportListDTO>("get_latest_screening_date", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase last date fetched.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> GetCustomerCaseCount(int status,int clientId)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@c_status", status);
                parameters.Add("@c_clientid", clientId);
                var response = ExecuteScalar("get_customercase_count_by_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> GetCustomerTypeCount(string customertype, int clientId)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@c_customertype", customertype);
                parameters.Add("@c_clientid", clientId);
                var response = ExecuteScalar("get_customertype_count", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> GetapprovedCaseCount(int status, int clientId)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@c_status", status);
                parameters.Add("@c_clientid", clientId);
                var response = ExecuteScalar("get_approvedcase_count_by_status", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<RiskDashboardDTO>> GetRiskCount(int category,int clientId)
        {
            ServiceResponse<List<RiskDashboardDTO>> serviceResponse = new ServiceResponse<List<RiskDashboardDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();

                parameters.Add("@category_id", category);
                parameters.Add("@clientid", clientId);
                var response = Get<RiskDashboardDTO>("get_count_by_risk", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Result = response;
                serviceResponse.Message = "Risk Count Fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseReportListDTO>> GetCustomerReportList(ReportLogSearchModel requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@c_is_match", Convert.ToInt32(requestModel.MatchType));
                parameters.Add("c_clientId", requestModel.ClientId);
                serviceResponse.Result = Get<CaseReportListDTO>("get_customercase_by_match", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<UploadLogsListDTO>> GetUploadLogstList()
        {
            ServiceResponse<List<UploadLogsListDTO>> serviceResponse = new ServiceResponse<List<UploadLogsListDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<UploadLogsListDTO>("get_all_upload_logs", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<DigiSchedulerLogsDTO>> GetDigiSchedulerList(int clientId)
        {
            ServiceResponse<List<DigiSchedulerLogsDTO>> serviceResponse = new ServiceResponse<List<DigiSchedulerLogsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<DigiSchedulerLogsDTO>("get_all_digischeduler_logs", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Scheduler log details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseReportListDTO>> GetKycReportList(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<List<CaseReportListDTO>> serviceResponse = new ServiceResponse<List<CaseReportListDTO>>();
            try
            {
                var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@cust_type", (requestModel.Cust_type));
                parameters.Add("@c_user", Convert.ToInt32(userID));
                parameters.Add("@c_status", Convert.ToInt32(requestModel.Status));
                parameters.Add("@c_clientId", requestModel.ClientId);
                parameters.Add("@c_nomatch", requestModel.NoMatch);
                parameters.Add("@c_tdomesticpep", requestModel.TrueDomesticpep);
                parameters.Add("@c_tforeignpep", requestModel.TrueForeignpep);
                parameters.Add("@c_tadversemedia", requestModel.TrueForeignpep);
                parameters.Add("@c_pdomesticpep", requestModel.PartialDomesticpep);
                parameters.Add("@c_pforeignpep", requestModel.PartialForeignpep);
                parameters.Add("@c_padversemedia", requestModel.Partialadversemedia);
                parameters.Add("@c_expirystartdate", requestModel.expiryStartDate);
                parameters.Add("@c_expiryenddate", requestModel.expiryEndDate);
                parameters.Add("@c_pepdeclaration", requestModel.pepfrmclients);

                if (Convert.ToInt32(requestModel.idstatus) > 0)
                {
                    parameters.Add("@c_documentstatus", requestModel.idstatus);
                    serviceResponse.Result = Get<CaseReportListDTO>("get_all_kyc_report_by_expiry", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                else 
                {
                    serviceResponse.Result = Get<CaseReportListDTO>("get_all_kyc_report", parameters, commandType: CommandType.StoredProcedure).ToList();
                }
                serviceResponse.Message = "CustomerCase details fetched successfully.";
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

        public ServiceResponse<int> GetClientCountReportList(CaseReportRequestDTO requestModel)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                var userID = String.IsNullOrEmpty(requestModel.User) ? "0" : requestModel.User;
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(requestModel.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(requestModel.EndDate));
                parameters.Add("@c_clientId", requestModel.ClientId);
                serviceResponse.Result = ExecuteScalar("get_all_client_search_count", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
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
        public ServiceResponse<List<ScreeningDatabaseLogDTO>> GetScreeningDatabaseLogs(CaseReportRequestDTO model)
        {
            ServiceResponse<List<ScreeningDatabaseLogDTO>> serviceResponse = new ServiceResponse<List<ScreeningDatabaseLogDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_from", Convert.ToDateTime(model.StartDate));
                parameters.Add("@c_to", Convert.ToDateTime(model.EndDate));
                serviceResponse.Result = Get<ScreeningDatabaseLogDTO>("get_all_Screening_database_logs", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "CustomerCase details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
    }
}
