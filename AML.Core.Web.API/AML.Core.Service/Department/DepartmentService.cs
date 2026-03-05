using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.Department;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Department;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.DTO.DTO.Department;

namespace AML.Core.Service.Department
{
    public class DepartmentService : BaseService, IDepartmentService
    {
        IDepartmentRepository _departmentRepository;
        public DepartmentService(IDepartmentRepository departmentRepository, IConfiguration configuration, IHostingEnvironment environment) : base(departmentRepository, configuration)
        {
            _departmentRepository = departmentRepository;
        }

        public ServiceResponse<int> Create(DepartmentDTO _departmentDTO)
        {
            //Perform business requirements here
            _departmentDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _departmentRepository.Create(_departmentDTO);
        }

        public DepartmentDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _departmentRepository.GetDetails(Id).Result;
        }

        public List<DepartmentDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            return _departmentRepository.GetAll(clientId).Result;
        }

        public ServiceResponse<int> Update(DepartmentDTO _departmentDTO)
        {
            //Perform business requirements here
            _departmentDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _departmentRepository.Update(_departmentDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _departmentRepository.Delete(Id);
        }
    }
}
