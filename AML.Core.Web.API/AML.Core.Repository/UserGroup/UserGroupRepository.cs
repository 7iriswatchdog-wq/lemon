using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserGroup;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.UserGroup;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.UserGroup
{
    public class UserGroupRepository : BaseRepository, IUserGroupRepository
    {
        public UserGroupRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context){ }

        public ServiceResponse<List<UserGroupDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<UserGroupDTO>> serviceResponse = new ServiceResponse<List<UserGroupDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<UserGroupDTO>("get_all_usergroup", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "User Group details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<UserGroupDTO> GetDetails(int Id)
        {
            ServiceResponse<UserGroupDTO> serviceResponse = new ServiceResponse<UserGroupDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<UserGroupDTO>("get_usergroup_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "User Group details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(UserGroupDTO _userGroupDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _userGroupDTO.Code);
                parameters.Add("@p_name", _userGroupDTO.Name);
                parameters.Add("@p_description", _userGroupDTO.Description);
                parameters.Add("@p_created_by", _userGroupDTO.CreatedBy);
                parameters.Add("@p_clientId", _userGroupDTO.ClientId);
                var response = ExecuteScalar("ins_usergroup", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Group added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(UserGroupDTO _userGroupDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _userGroupDTO.Id);
                parameters.Add("@p_code", _userGroupDTO.Code);
                parameters.Add("@p_name", _userGroupDTO.Name);
                parameters.Add("@p_description", _userGroupDTO.Description);
                parameters.Add("@p_updated_by", _userGroupDTO.UpdatedBy);
                parameters.Add("@p_is_active", _userGroupDTO.IsActive);
                parameters.Add("@p_clientId", _userGroupDTO.ClientId);
                var response = ExecuteScalar("mod_usergroup", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Group updated successfully.";
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
                var response = ExecuteScalar("del_usergroup", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Group deleted successfully.";
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
