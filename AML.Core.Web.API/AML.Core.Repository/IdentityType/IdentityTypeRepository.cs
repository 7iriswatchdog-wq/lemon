using System;
using Microsoft.Extensions.Configuration;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.IdentityType;
using Dapper;
using Microsoft.AspNetCore.Http;
using AML.DTO.DTO.IdentityType;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace AML.Core.Repository.IdentityType
{
    public class IdentityTypeRepository : BaseRepository, IIdentityTypeRepository
    {
        public IdentityTypeRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<IdentityTypeDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<IdentityTypeDTO>> serviceResponse = new ServiceResponse<List<IdentityTypeDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<IdentityTypeDTO>("get_all_identitytype", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Identity Type details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<IdentityTypeDTO> GetDetails(int Id)
        {
            ServiceResponse<IdentityTypeDTO> serviceResponse = new ServiceResponse<IdentityTypeDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<IdentityTypeDTO>("get_identitytype_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Identity Type details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Create(IdentityTypeDTO _identityTypeDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_code", _identityTypeDTO.Code);
                parameters.Add("@p_name", _identityTypeDTO.Name);
                parameters.Add("@p_description", _identityTypeDTO.Description);
                parameters.Add("@p_created_by", _identityTypeDTO.CreatedBy);
                parameters.Add("@p_clientId", _identityTypeDTO.ClientId);
                var response = ExecuteScalar("ins_identitytype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Identity Type added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(IdentityTypeDTO _identityTypeDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _identityTypeDTO.Id);
                parameters.Add("@p_code", _identityTypeDTO.Code);
                parameters.Add("@p_name", _identityTypeDTO.Name);
                parameters.Add("@p_description", _identityTypeDTO.Description);
                parameters.Add("@p_updated_by", _identityTypeDTO.UpdatedBy);
                parameters.Add("@p_is_active", _identityTypeDTO.IsActive);
                var response = ExecuteScalar("mod_identitytype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Identity Type updated successfully.";
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
                var response = ExecuteScalar("del_identitytype", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Identity Type deleted successfully.";
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
