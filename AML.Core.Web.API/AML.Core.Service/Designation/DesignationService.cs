using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.Designation;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Designation;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.DTO.DTO.Designation;

namespace AML.Core.Service.Designation
{
    public class DesignationService : BaseService, IDesignationService
    {
        IDesignationRepository _designationRepository;
        public DesignationService(IDesignationRepository designationRepository, IConfiguration configuration, IHostingEnvironment environment) : base(designationRepository, configuration)
        {
            _designationRepository = designationRepository;
        }

        public ServiceResponse<int> Create(DesignationDTO _designationDTO)
        {
            //Perform business requirements here
            _designationDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _designationRepository.Create(_designationDTO);
        }

        public DesignationDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _designationRepository.GetDetails(Id).Result;
        }

        public List<DesignationDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            return _designationRepository.GetAll(clientId).Result;
        }

        public ServiceResponse<int> Update(DesignationDTO _designationDTO)
        {
            //Perform business requirements here
            _designationDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _designationRepository.Update(_designationDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _designationRepository.Delete(Id);
        }
    }
}
