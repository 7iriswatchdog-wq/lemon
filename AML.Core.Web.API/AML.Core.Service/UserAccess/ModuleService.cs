using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserAccess;
using AML.Core.ServiceContract;
using AML.Core.ServiceContract.UserAccess;
using AML.DTO.DTO.UserAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.UserAccess
{
    public class ModuleService :  BaseService, IModuleService
    {
        IModuleRepository _moduleRepository;
        public ModuleService(IModuleRepository moduleRepository, IConfiguration configuration, IHostingEnvironment environment) : base(moduleRepository, configuration)
        {
            _moduleRepository = moduleRepository;
        }

        public List<ModuleDTO> GetAll()
        {
            return _moduleRepository.GetAll().Result;
        }
    }
}
