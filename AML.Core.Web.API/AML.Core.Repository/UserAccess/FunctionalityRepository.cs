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

namespace AML.Core.Repository.UserAccess
{
    public class FunctionalityRepository : BaseRepository, IFunctionalityRepository
    {
        public FunctionalityRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<List<FunctionalityDTO>> GetAll(int clientId)
        {
            ServiceResponse<List<FunctionalityDTO>> serviceResponse = new ServiceResponse<List<FunctionalityDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_clientId", clientId);
                serviceResponse.Result = Get<FunctionalityDTO>("get_all_functionality", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Functionality details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<FunctionalityDTO> GetDetailsByModuleId(int modId)
        {
            ServiceResponse<FunctionalityDTO> serviceResponse = new ServiceResponse<FunctionalityDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", modId);
                serviceResponse.Result = GetFirstOrDefault<FunctionalityDTO>("get_functionality_by_module_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Functionality details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<FunctionalityDTO> GetDetails(int Id)
        {
            ServiceResponse<FunctionalityDTO> serviceResponse = new ServiceResponse<FunctionalityDTO>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", Id);
                serviceResponse.Result = GetFirstOrDefault<FunctionalityDTO>("get_functionality_by_id", parameters, commandType: CommandType.StoredProcedure);
                serviceResponse.Message = "Functionality details fetched successfully.";
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
