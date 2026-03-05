using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Department;
namespace AML.Core.RepositoryContract.Department
{
    public interface IDepartmentRepository : IBaseRepository
    {
        ServiceResponse<int> Create(DepartmentDTO _departmentDTO);
        ServiceResponse<DepartmentDTO> GetDetails(int Id);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<int> Update(DepartmentDTO _departmentDTO);
        ServiceResponse<List<DepartmentDTO>> GetAll(int clientId);
    }
}
