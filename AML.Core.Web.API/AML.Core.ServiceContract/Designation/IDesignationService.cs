using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Designation;

namespace AML.Core.ServiceContract.Designation
{
    public interface IDesignationService : IBaseService
    {
        ServiceResponse<int> Create(DesignationDTO _designationDTO);
        DesignationDTO GetDetails(int Id);
        ServiceResponse<int> Update(DesignationDTO _designationDTO);
        ServiceResponse<int> Delete(int Id);
        List<DesignationDTO> GetAll(int clientId);
    }
}
