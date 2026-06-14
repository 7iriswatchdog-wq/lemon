using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.DigiApiUser;
using AML.DTO.DTO.DigiApiUser;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AML.Core.Repository.DigiApiUser
{
    public class DigiApiUserRepository : BaseRepository, IDigiApiUserRepository
    {
        public DigiApiUserRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<DigiApiUserDTO> GetByUsername(string Username)
        {
            ServiceResponse<DigiApiUserDTO> serviceResponse = new ServiceResponse<DigiApiUserDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_username", Username);
                serviceResponse.Result = GetFirstOrDefault<DigiApiUserDTO>("get_digiapiuser_by_username", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Digi Api User details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Update(DigiApiUserDTO _DigiApiUserDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_companyid", _DigiApiUserDTO.CompanyId);
                parameters.Add("@p_apiname", _DigiApiUserDTO.ApiName);
                parameters.Add("@p_token", _DigiApiUserDTO.Token);
                parameters.Add("@p_username", _DigiApiUserDTO.Username);
                parameters.Add("@p_userid", _DigiApiUserDTO.UserId);
                var response = ExecuteScalar("mod_digiapiuser", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Digi Api User updated successfully.";
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
