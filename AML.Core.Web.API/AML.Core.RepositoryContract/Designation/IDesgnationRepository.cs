using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Designation;

namespace AML.Core.RepositoryContract.Designation
{
    public interface IDesignationRepository : IBaseRepository
    {
        ServiceResponse<int> Create(DesignationDTO _designationDTO);
        ServiceResponse<DesignationDTO> GetDetails(int Id);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<int> Update(DesignationDTO _designationDTO);
        ServiceResponse<List<DesignationDTO>> GetAll(int clientId);
    }
}
