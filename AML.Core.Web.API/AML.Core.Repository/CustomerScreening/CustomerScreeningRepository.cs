using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerScreening;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Sanction;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.CustomerScreening
{
    public class CustomerScreeningRepository : BaseRepository, ICustomerScreeningRepository
    {
        public CustomerScreeningRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        //public CustomerScreeningRS ScreeningSearch(CustomerScreeningRQ _customerCase)
        //{
            
        //}

        public ServiceResponse<int> InsertSanctionScreeningLogs(SanctionScreeningLogDTO model)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_cust_name", model.CustomerName);
                parameters.Add("@p_nationality", model.Nationality);
                parameters.Add("@p_dateofbirth", model.DOB);
                parameters.Add("@p_search_type", model.SearchType);
                parameters.Add("@p_match_name", model.MatchName);
                parameters.Add("@p_match_score", model.MatchScore);
                parameters.Add("@p_match_uid", model.MatchUID);
                parameters.Add("@p_match_category", model.MatchCategory);
                parameters.Add("@p_match_type", model.MatchType);
                parameters.Add("@p_match_nationality", model.MatchNationality);
                parameters.Add("@p_match_idnum", model.MatchIDNum);
                parameters.Add("@p_match_dob", model.MatchDOB);
                parameters.Add("@p_created_by", model.CreatedBy);
                parameters.Add("@p_clientId", model.ClientId);
                parameters.Add("@p_customerType", model.CustomerType);
                var response = ExecuteScalar("ins_sanction_screening_log", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Sanction Screening Log added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<SanctionScreeningLogDTO>> GetAllByDate(string startDate, string endDate,int clientId)
        {
            ServiceResponse<List<SanctionScreeningLogDTO>> serviceResponse = new ServiceResponse<List<SanctionScreeningLogDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_datef", startDate, DbType.Date);
                parameters.Add("@p_datet", endDate, DbType.Date);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<SanctionScreeningLogDTO>("get_sanction_screening_log_by_date", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Sanction Screening Logs fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<SanctionScreeningLogDTO>> GetSanctionScreeningLogs(string Name, string Nationality, string DOB, string customerType, int ClientId)
        {
            ServiceResponse<List<SanctionScreeningLogDTO>> serviceResponse = new ServiceResponse<List<SanctionScreeningLogDTO>>();
            try
            {
                
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_name", Name);
                parameters.Add("@p_nationality", Nationality);
                parameters.Add("@p_custType", customerType);
                parameters.Add("@p_dob", DOB);
                parameters.Add("@p_clientId", ClientId);
                serviceResponse.Result = Get<SanctionScreeningLogDTO>("get_sanction_screening_logs", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Sanction Screening Logs fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CustomerCaseDTO>> GetCaseLogs(string Name, string Nationality, string DOB, string customerType, int ClientId)
        {
            ServiceResponse<List<CustomerCaseDTO>> serviceResponse = new ServiceResponse<List<CustomerCaseDTO>>();
            try
            {
                var custType = customerType == "INDIVIDUAL" ? "I" : "C";
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_name", Name);
                parameters.Add("@p_nationality", Nationality);
                parameters.Add("@p_custType", custType);
                parameters.Add("@p_dob", DOB);
                parameters.Add("@p_clientId", ClientId);
                serviceResponse.Result = Get<CustomerCaseDTO>("get_customercase_logs", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Sanction Screening Logs fetched successfully.";
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
