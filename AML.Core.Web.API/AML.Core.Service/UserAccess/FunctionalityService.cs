using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserAccess;
using AML.Core.ServiceContract.UserAccess;
using AML.DTO.DTO.UserAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.UserAccess
{
    public class FunctionalityService : BaseService, IFunctionalityService
    {
        IFunctionalityRepository _functionalityRepository;
        public FunctionalityService(IFunctionalityRepository functionalityRepository, IConfiguration configuration, IHostingEnvironment environment) : base(functionalityRepository, configuration)
        {
            _functionalityRepository = functionalityRepository;
        }
        public ServiceResponse<List<FunctionalityDTO>> GetAll(int clientId)
        {
            return _functionalityRepository.GetAll(clientId);
        }

        public ServiceResponse<FunctionalityDTO> GetDetails(int Id)
        {
            return _functionalityRepository.GetDetails(Id);
        }

        public ServiceResponse<FunctionalityDTO> GetDetailsByModuleId(int modId)
        {
            return _functionalityRepository.GetDetailsByModuleId(modId);
        }
    }
}
