using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using AML.Core.RepositoryContract.User;
using AML.DTO.DTO.User;
using AML.Core.Common.StaticResource;
using Dapper;
using System.Data;
using System.Linq;
using AML.DTO.DTO.Common;

namespace AML.Core.Repository.User
{
    public class UserRepository : BaseRepository, IUserRepository
    {
        public UserRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        {

        }
        public ServiceResponse<int> Create(UserDTO _userDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_fname", _userDTO.FName);
                parameters.Add("@p_lname", _userDTO.LName);
                parameters.Add("@p_code", _userDTO.EmpCode);
                parameters.Add("@p_username", _userDTO.UserName);
                parameters.Add("@p_password", _userDTO.Password);
                parameters.Add("@p_remarks", _userDTO.Remarks);
                parameters.Add("@p_designation_id", _userDTO.DesignationId);
                parameters.Add("@p_department_id", _userDTO.DepartmentId);
                parameters.Add("@p_user_group_id", _userDTO.UserGroupId);
                parameters.Add("@p_branch_id", _userDTO.BranchId);
                parameters.Add("@p_country_id", _userDTO.CountryId);
                parameters.Add("@p_identity_type_id", _userDTO.IdentityTypeId);
                parameters.Add("@p_is_active", 1);
                parameters.Add("@p_is_deleted", _userDTO.IsDeleted);
                parameters.Add("@p_is_blocked", _userDTO.IsBlocked);
                parameters.Add("@p_created_by", _userDTO.CreatedBy);
                parameters.Add("@p_clientId", _userDTO.ClientId);
                var response = ExecuteScalar("ins_user", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<UserDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<UserDTO>> serviceResponse = new ServiceResponse<List<UserDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@c_clientId", clientId);
                serviceResponse.Result = Get<UserDTO>("get_all_user", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Users details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Update(UserDTO _userDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _userDTO.Id);
                parameters.Add("@p_fname", _userDTO.FName);
                parameters.Add("@p_lname", _userDTO.LName);
                parameters.Add("@p_code", _userDTO.EmpCode);
                parameters.Add("@p_remarks", _userDTO.Remarks);
                parameters.Add("@p_designation_id", _userDTO.DesignationId);
                parameters.Add("@p_department_id", _userDTO.DepartmentId);
                parameters.Add("@p_user_group_id", _userDTO.UserGroupId);
                parameters.Add("@p_branch_id", _userDTO.BranchId);
                parameters.Add("@p_country_id", _userDTO.CountryId);
                parameters.Add("@p_identity_type_id", _userDTO.IdentityTypeId);
                parameters.Add("@p_updated_by", _userDTO.UpdatedBy);
                parameters.Add("@p_clientId", _userDTO.ClientId);
                var response = ExecuteScalar("mod_user", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Delete(UserDTO _userDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _userDTO.Id);
                parameters.Add("@p_is_deleted", _userDTO.IsDeleted);
                parameters.Add("@p_updated_by", _userDTO.UpdatedBy);
                var response = ExecuteScalar("del_user", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Deleted successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> BlockUser(UserDTO _userDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _userDTO.Id);
                parameters.Add("@p_is_blocked", _userDTO.IsBlocked);
                parameters.Add("@p_updated_by", _userDTO.UpdatedBy);
                var response = ExecuteScalar("mod_userblock", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Blocked successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<UserDTO> GetDetails(int Id)
        {
            ServiceResponse<UserDTO> serviceResponse = new ServiceResponse<UserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<UserDTO>("get_user_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "User details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<ApiUserDTO> GetApiUserDetails(string username, string password,string companyName)
        {
            ServiceResponse<ApiUserDTO> serviceResponse = new ServiceResponse<ApiUserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_username", username);
                parameters.Add("@p_password", password);
                parameters.Add("@p_companyName", companyName);
                serviceResponse.Result = GetFirstOrDefault<ApiUserDTO>("get_api_authentication", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "User details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<UserPasswordLogModelDTO>> GetAllPasswordLogs(string startDate, string endDate, int clientId)
        {
            ServiceResponse<List<UserPasswordLogModelDTO>> serviceResponse = new ServiceResponse<List<UserPasswordLogModelDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_datef", startDate, DbType.Date);
                parameters.Add("@p_datet", endDate, DbType.Date);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<UserPasswordLogModelDTO>("get_all_user_password_logs", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "User password change details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<List<UserDTO>> GetAuthorisedUser(string _controllerName, string _actionname, int clientId)
        {
            ServiceResponse<List<UserDTO>> serviceResponse = new ServiceResponse<List<UserDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("p_func_code", _actionname);
                parameters.Add("p_module_code", _controllerName);
                parameters.Add("p_clientId", clientId);
                serviceResponse.Result = Get<UserDTO>("get_all_authorised_user", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Users details fetched successfully.";
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
