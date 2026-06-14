using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.LegalType;
using AML.Core.ServiceContract.LegalType;
using AML.DTO.DTO.LegalType;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.LegalType
{
    public class LegalTypeService : BaseService, ILegalTypeService
    {
        ILegalTypeRepository _LegalTypeRepository;
        public LegalTypeService(ILegalTypeRepository LegalTypeRepository, IConfiguration configuration, IHostingEnvironment environment) 
            : base(LegalTypeRepository, configuration)
        {
            _LegalTypeRepository = LegalTypeRepository;
        }

        public ServiceResponse<int> Create(LegalTypeDTO _LegalTypeDTO)
        {
            //Perform business requirements here
            _LegalTypeDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _LegalTypeRepository.Create(_LegalTypeDTO);
        }

        public LegalTypeDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _LegalTypeRepository.GetDetails(Id).Result;
        }

        public List<LegalTypeDTO> GetAll()
        {
            //Perform business requirements here
            return _LegalTypeRepository.GetAll().Result;
        }

        public ServiceResponse<int> Update(LegalTypeDTO _LegalTypeDTO)
        {
            //Perform business requirements here
            _LegalTypeDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _LegalTypeRepository.Update(_LegalTypeDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _LegalTypeRepository.Delete(Id);
        }
    }
}
