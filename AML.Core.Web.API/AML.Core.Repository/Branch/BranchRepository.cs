using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Branch;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.Branch;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.Branch
{
    public class BranchRepository : BaseRepository, IBranchRepository
    {
        public BranchRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<BranchDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<BranchDTO>> serviceResponse = new ServiceResponse<List<BranchDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<BranchDTO>("get_all_branch", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Branch details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<BranchDTO> GetDetails(int Id)
        {
            ServiceResponse<BranchDTO> serviceResponse = new ServiceResponse<BranchDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<BranchDTO>("get_branch_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Branch details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(BranchDTO _branchDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _branchDTO.Code);
                parameters.Add("@p_name", _branchDTO.Name);
                parameters.Add("@p_description", _branchDTO.Description);
                parameters.Add("@p_created_by", _branchDTO.CreatedBy);
                parameters.Add("@p_clientId", _branchDTO.ClientId);
                var response = ExecuteScalar("ins_branch", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Branch added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(BranchDTO _branchDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _branchDTO.Id);
                parameters.Add("@p_code", _branchDTO.Code);
                parameters.Add("@p_name", _branchDTO.Name);
                parameters.Add("@p_is_active", _branchDTO.IsActive);
                parameters.Add("@p_description", _branchDTO.Description);
                parameters.Add("@p_updated_by", _branchDTO.UpdatedBy);
                parameters.Add("@p_clientId", _branchDTO.ClientId);
                var response = ExecuteScalar("mod_branch", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Branch updated successfully.";
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
                var response = ExecuteScalar("del_branch", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Branch deleted successfully.";
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
