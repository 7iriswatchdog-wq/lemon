using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.IdentityType;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.DTO.DTO.IdentityType;

namespace AML.Core.Service.IdentityType
{
    public class IdentityTypeService : BaseService, IIdentityTypeService
    {
        IIdentityTypeRepository _identityTypeRepository;
        public IdentityTypeService(IIdentityTypeRepository identityTypeRepository, IConfiguration configuration, IHostingEnvironment environment) : base(identityTypeRepository, configuration)
        {
            _identityTypeRepository = identityTypeRepository;
        }

        public ServiceResponse<int> Create(IdentityTypeDTO _identityTypeDTO)
        {
            //Perform business requirements here
            _identityTypeDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _identityTypeRepository.Create(_identityTypeDTO);
        }

        public IdentityTypeDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _identityTypeRepository.GetDetails(Id).Result;
        }

        public List<IdentityTypeDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            var list = _identityTypeRepository.GetAll(clientId).Result;
            if (list != null)
            {
                list.RemoveAll(x => !string.Equals(x.Name, "Passport", StringComparison.OrdinalIgnoreCase) && 
                                    !string.Equals(x.Name, "Emirates ID", StringComparison.OrdinalIgnoreCase));
            }
            return list;
        }

        public ServiceResponse<int> Update(IdentityTypeDTO _identityTypeDTO)
        {
            //Perform business requirements here
            _identityTypeDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _identityTypeRepository.Update(_identityTypeDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _identityTypeRepository.Delete(Id);
        }
    }
}
