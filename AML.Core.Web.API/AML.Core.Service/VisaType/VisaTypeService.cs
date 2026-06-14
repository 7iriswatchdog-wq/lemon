using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.IdentityType;
using AML.Core.Common.StaticResource;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.Core.ServiceContract.VisaType;
using AML.Core.RepositoryContract.VisaType;
using AML.DTO.DTO.VisaType;

namespace AML.Core.Service.VisaType
{
    public class VisaTypeService : BaseService, IVisaTypeService
    {
        IVisaTypeRepository _visaTypeRepository;
        public VisaTypeService(IVisaTypeRepository visaTypeRepository, IConfiguration configuration, IHostingEnvironment environment) : base(visaTypeRepository, configuration)
        {
            _visaTypeRepository = visaTypeRepository;
        }

        public ServiceResponse<int> Create(VisaTypeDTO _visaTypeDTO)
        {
            //Perform business requirements here
            _visaTypeDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _visaTypeRepository.Create(_visaTypeDTO);
        }

        public VisaTypeDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _visaTypeRepository.GetDetails(Id).Result;
        }

        public List<VisaTypeDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            return _visaTypeRepository.GetAll(clientId).Result;
        }

        public ServiceResponse<int> Update(VisaTypeDTO _visaTypeDTO)
        {
            //Perform business requirements here
            _visaTypeDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _visaTypeRepository.Update(_visaTypeDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _visaTypeRepository.Delete(Id);
        }
    }
}
