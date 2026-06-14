using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.VisaType;

namespace AML.Core.RepositoryContract.VisaType
{
    public interface IVisaTypeRepository : IBaseRepository
    {
        ServiceResponse<int> Create(VisaTypeDTO _identityTypeDTO);
        ServiceResponse<VisaTypeDTO> GetDetails(int Id);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<int> Update(VisaTypeDTO _identityTypeDTO);
        ServiceResponse<List<VisaTypeDTO>> GetAll(int clientId);
    }
}
