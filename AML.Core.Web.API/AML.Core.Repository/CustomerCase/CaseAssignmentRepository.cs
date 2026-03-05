using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.DTO.DTO.CaseAssignment;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.CustomerCase
{
    public class CaseAssignmentRepository : BaseRepository, ICaseAssignmentRepository
    {
        public CaseAssignmentRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<CaseAssignmentDTO>> GetAll()
        {
            ServiceResponse<List<CaseAssignmentDTO>> serviceResponse = new ServiceResponse<List<CaseAssignmentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CaseAssignmentDTO>("get_all_CaseAssignment", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Assignment details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<CaseAssignmentDTO> GetDetails(int Id)
        {
            ServiceResponse<CaseAssignmentDTO> serviceResponse = new ServiceResponse<CaseAssignmentDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CaseAssignmentDTO>("get_CaseAssignment_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Case Document details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(CaseAssignmentDTO _CaseAssignmentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", _CaseAssignmentDTO.CaseId);
                parameters.Add("@p_user_id", _CaseAssignmentDTO.UserId);
                parameters.Add("@p_comment", _CaseAssignmentDTO.Comment);
                parameters.Add("@p_created_on", _CaseAssignmentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseAssignmentDTO.CreatedBy);
                var response = ExecuteScalar("ins_case_assignment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Assignment added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CaseAssignmentDTO _CaseAssignmentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _CaseAssignmentDTO.Id);
                parameters.Add("@p_case_id", _CaseAssignmentDTO.CaseId);
                parameters.Add("@p_user_id", _CaseAssignmentDTO.UserId);
                parameters.Add("@p_created_on", _CaseAssignmentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseAssignmentDTO.CreatedBy);
                var response = ExecuteScalar("mod_CaseAssignment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Assignment updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Delete(int id)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", id);
                var response = ExecuteScalar("del_CaseAssignment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Document deleted successfully.";
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
