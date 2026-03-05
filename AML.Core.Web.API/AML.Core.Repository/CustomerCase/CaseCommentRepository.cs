using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.DTO.DTO.CaseComment;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace AML.Core.Repository.CustomerCase
{
    public class CaseCommentRepository : BaseRepository, ICaseCommentRepository
    {
        public CaseCommentRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<CaseCommentDTO>> GetAll()
        {
            ServiceResponse<List<CaseCommentDTO>> serviceResponse = new ServiceResponse<List<CaseCommentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CaseCommentDTO>("get_all_CaseComment", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Comment details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<CaseCommentDTO>> GetAllByCase(int CaseId)
        {
            ServiceResponse<List<CaseCommentDTO>> serviceResponse = new ServiceResponse<List<CaseCommentDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", CaseId);
                serviceResponse.Result = Get<CaseCommentDTO>("get_case_comments_by_case_id", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Case Comment details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<CaseCommentDTO> GetDetails(int Id)
        {
            ServiceResponse<CaseCommentDTO> serviceResponse = new ServiceResponse<CaseCommentDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<CaseCommentDTO>("get_CaseComment_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Case Comment details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(CaseCommentDTO _CaseCommentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_case_id", _CaseCommentDTO.CaseId);
                parameters.Add("@p_comment", _CaseCommentDTO.Comment);
                parameters.Add("@p_created_on", _CaseCommentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseCommentDTO.CreatedBy);
                var response = ExecuteScalar("ins_case_comment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Comment added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(CaseCommentDTO _CaseCommentDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _CaseCommentDTO.Id);
                parameters.Add("@p_case_id", _CaseCommentDTO.CaseId);
                parameters.Add("@p_comment", _CaseCommentDTO.Comment);
                parameters.Add("@p_created_on", _CaseCommentDTO.CreatedOn);
                parameters.Add("@p_created_by", _CaseCommentDTO.CreatedBy);
                var response = ExecuteScalar("mod_CaseComment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Comment updated successfully.";
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
                var response = ExecuteScalar("del_CaseComment", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Case Comment deleted successfully.";
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
