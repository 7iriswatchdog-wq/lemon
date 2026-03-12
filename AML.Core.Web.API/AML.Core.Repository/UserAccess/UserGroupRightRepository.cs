using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserAccess;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.UserAccess;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using AML.DTO.DTO.Common;

namespace AML.Core.Repository.UserAccess
{
    public class UserGroupRightRepository : BaseRepository, IUserGroupRightRepository
    {
        public UserGroupRightRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<int> Create(UserGroupRightDTO _userGroupRightDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_group_id", _userGroupRightDTO.UserGroupId);
                parameters.Add("@p_module_id", _userGroupRightDTO.ModuleId);
                parameters.Add("@p_func_id", _userGroupRightDTO.FunctionalityId);
                parameters.Add("@p_created_by", _userGroupRightDTO.CreatedBy);
                var response = ExecuteScalar("ins_usergroupright", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> DeleteByUserGroupId(int _userGroupID)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_group_id", _userGroupID);
                var response = ExecuteScalar("del_usergroupright_by_user_group_id", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<UserGroupRightDTO> GetByUserGroupId(int ugrId)
        {
            ServiceResponse<UserGroupRightDTO> serviceResponse = new ServiceResponse<UserGroupRightDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_group_id", ugrId);
                serviceResponse.Result = GetFirstOrDefault<UserGroupRightDTO>("get_usergroupright_by_user_group_id", parameters, commandType: CommandType.StoredProcedure);
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

        public ServiceResponse<UserGroupRightDetailsDTO> GetDetailsByModuleId(int modId)
        {
            ServiceResponse<UserGroupRightDetailsDTO> serviceResponse = new ServiceResponse<UserGroupRightDetailsDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_module_id", modId);
                serviceResponse.Result = GetFirstOrDefault<UserGroupRightDetailsDTO>("get_usergroupright_by_module_id", parameters, commandType: CommandType.StoredProcedure);
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

        public ServiceResponse<List<UserGroupRightDetailsDTO>> GetDetailsByUserGroupId(int ugrId)
        {
            ServiceResponse<List<UserGroupRightDetailsDTO>> serviceResponse = new ServiceResponse<List<UserGroupRightDetailsDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_group_id", ugrId);
                serviceResponse.Result = Get<UserGroupRightDetailsDTO>("get_usergroupright_by_user_group_id", parameters, commandType: CommandType.StoredProcedure).ToList();
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
        public ServiceResponse<List<ClientMenuRightsModelDTO>> getClientMenuByClientId(int clientId)
        {
            ServiceResponse<List<ClientMenuRightsModelDTO>> serviceResponse = new ServiceResponse<List<ClientMenuRightsModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<ClientMenuRightsModelDTO>("get_client_rights_by_clientId", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Client Rights fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        //public ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID)
        public ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID, string sessionId)
        {
            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_group_id", _userGroupID);
                parameters.Add("@p_user_id", _userId);
                parameters.Add("@p_func_code", _actionname);
                parameters.Add("@p_module_code", _controllerName);
                parameters.Add("@p_session_id", sessionId);
                var response = GetFirstOrDefault<int>("get_usergroupaccessright", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                //serviceResponse.Result = (response >= 1) ? true : false;
                serviceResponse.Result = (response >= 1);
                serviceResponse.Message = "Added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<bool> CheckNameuserrightExists(string _controllerName, string _actionname, int _userId, int _userGroupID, string sessionId,string functionName)
        {
            ServiceResponse<bool> serviceResponse = new ServiceResponse<bool>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_group_id", _userGroupID);
                parameters.Add("@p_user_id", _userId);
                parameters.Add("@p_func_code", _actionname);
                parameters.Add("@p_module_code", _controllerName);
                parameters.Add("@p_session_id", sessionId);
                parameters.Add("@p_functionname", functionName);
                var response = GetFirstOrDefault<int>("get_name_usergroupaccessright", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                //serviceResponse.Result = (response >= 1) ? true : false;
                serviceResponse.Result = (response >= 1);
                serviceResponse.Message = "Added successfully.";
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
