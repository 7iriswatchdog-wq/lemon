using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.DigiApiUser;
using AML.Core.ServiceContract.DigiApiUser;
using AML.DTO.DTO.DigiApiUser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.DigiApiUser
{
    public class DigiApiUSerService : BaseService, IDigiApiUserService
    {
        IDigiApiUserRepository _digiApiUserRepository;
        public DigiApiUSerService(IDigiApiUserRepository digiApiUserRepository,IConfiguration configuration, IHostingEnvironment environment) : base(digiApiUserRepository, configuration)
        {
            _digiApiUserRepository = digiApiUserRepository;
        }
        public DigiApiUserDTO GetByUsername(string Username)
        {
            return _digiApiUserRepository.GetByUsername(Username).Result;
        }

        public ServiceResponse<int> Update(DigiApiUserDTO _DigiApiUserDTO)
        {
            return _digiApiUserRepository.Update(_DigiApiUserDTO);
        }
    }
}
