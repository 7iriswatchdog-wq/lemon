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
    public class ModuleRepository : BaseRepository, IModuleRepository
    {
        public ModuleRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }

        public ServiceResponse<List<ModuleDTO>> GetAll()
        {
            ServiceResponse<List<ModuleDTO>> serviceResponse = new ServiceResponse<List<ModuleDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<ModuleDTO>("get_all_module", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Module details fetched successfully.";
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
