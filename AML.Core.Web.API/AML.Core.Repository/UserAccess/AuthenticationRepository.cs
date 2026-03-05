using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserAccess;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.User;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.UserAccess
{
    public class AuthenticationRepository : BaseRepository, IAuthenticationRepository
    {
        public AuthenticationRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<UserDTO> VerifyUser(string _userName, string _password, string _clientName) 
        {
            ServiceResponse<UserDTO> serviceResponse = new ServiceResponse<UserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_name", _userName);
                parameters.Add("@p_password", _password);
                parameters.Add("@p_clientName", _clientName);
                serviceResponse.Result = GetFirstOrDefault<UserDTO>("get_authenticated_user", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Authentication Match fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> UpdateUserPassword(int _userId, string _password)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_id", _userId);
                parameters.Add("@p_password", _password);
                var response = ExecuteScalar("mod_user_password", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User Password Updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<UserDTO> VerifyUserByUserName(string _userName,string _clientName)
        {
            ServiceResponse<UserDTO> serviceResponse = new ServiceResponse<UserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_name", _userName);
                parameters.Add("@p_clientName", _clientName);
                serviceResponse.Result = GetFirstOrDefault<UserDTO>("get_authenticated_user_by_username", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Authentication Match fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

          //vidya
        public ServiceResponse<UserDTO> SaveOTP(string _userName, int _oTP, int clientId)
        {
            ServiceResponse<UserDTO> serviceResponse = new ServiceResponse<UserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_name", _userName);
                parameters.Add("@p_otp", _oTP);
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = GetFirstOrDefault<UserDTO>("save_otp", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "OTP saved successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<UserDTO> VerifyOTP(string _userName, int _oTP, int _clientId)
        {
            ServiceResponse<UserDTO> serviceResponse = new ServiceResponse<UserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_name", _userName);
                parameters.Add("@p_otp", _oTP);
                parameters.Add("@p_clientId", _clientId);
                serviceResponse.Result = GetFirstOrDefault<UserDTO>("get_authenticated_user_by_otp", parameters, commandType: CommandType.StoredProcedure);
                if (serviceResponse.Result != null)
                {
                    serviceResponse.Message = "Authentication Match fetched successfully.";
                    serviceResponse.Status = StaticResource.SuccessStatusCode;
                }
                else
                {
                    serviceResponse.Message = "Invalid OTP.";
                    serviceResponse.Status = StaticResource.NotFoundStatusCode;
                }
               
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> UpdateLogin(int userid, string status)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_user_id", userid);
                parameters.Add("@p_status", status);
                var response = ExecuteScalar("update_login_session", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "User login session updated successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        //vidya
    }
}
