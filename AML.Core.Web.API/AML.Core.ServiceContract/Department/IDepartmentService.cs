using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Department;
namespace AML.Core.ServiceContract.Department
{
    public interface IDepartmentService : IBaseService
    {
        ServiceResponse<int> Create(DepartmentDTO _departmentDTO);
        DepartmentDTO GetDetails(int Id);
        ServiceResponse<int> Update(DepartmentDTO _departmentDTO);
        ServiceResponse<int> Delete(int Id);
        List<DepartmentDTO> GetAll(int clientId);
    }
}
