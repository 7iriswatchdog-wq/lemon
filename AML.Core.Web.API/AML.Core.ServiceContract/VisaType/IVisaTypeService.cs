using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.VisaType;

namespace AML.Core.ServiceContract.VisaType
{
    public interface IVisaTypeService : IBaseService
    {
        ServiceResponse<int> Create(VisaTypeDTO _visaTypeDTO);
        ServiceResponse<int> Delete(int Id);
        VisaTypeDTO GetDetails(int Id);
        ServiceResponse<int> Update(VisaTypeDTO _visaTypeDTO);
        List<VisaTypeDTO> GetAll(int clientId);
    }
}
