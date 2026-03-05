using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCase;
using AML.Core.ServiceContract.CaseAssignment;
using AML.DTO.DTO.CaseAssignment;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.CaseAssignment
{

    public class CaseAssignmentService : BaseService, ICaseAssignmentService
    {
        ICaseAssignmentRepository _CaseAssignmentRepository;
        public CaseAssignmentService(ICaseAssignmentRepository CaseAssignmentRepository, IConfiguration configuration,
            IHostingEnvironment environment) : base(CaseAssignmentRepository, configuration)
        {
            _CaseAssignmentRepository = CaseAssignmentRepository;
        }

        public ServiceResponse<int> Create(CaseAssignmentDTO _CaseAssignmentDTO)
        {
            return _CaseAssignmentRepository.Create(_CaseAssignmentDTO);
        }

        public CaseAssignmentDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _CaseAssignmentRepository.GetDetails(Id).Result;
        }

        public List<CaseAssignmentDTO> GetAll()
        {
            //Perform business requirements here
            return _CaseAssignmentRepository.GetAll().Result;
        }

        public ServiceResponse<int> Update(CaseAssignmentDTO _CaseAssignmentDTO)
        {
            return _CaseAssignmentRepository.Update(_CaseAssignmentDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _CaseAssignmentRepository.Delete(Id);
        }
    }
}
