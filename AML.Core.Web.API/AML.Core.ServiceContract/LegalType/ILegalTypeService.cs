using AML.Core.Common.StaticResource;
using AML.DTO.DTO.LegalType;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.LegalType
{
    public interface ILegalTypeService : IBaseService
    {
        ServiceResponse<int> Create(LegalTypeDTO _legalTypeDTO);
        LegalTypeDTO GetDetails(int Id);
        ServiceResponse<int> Update(LegalTypeDTO _legalTypeDTO);
        ServiceResponse<int> Delete(int Id);
        List<LegalTypeDTO> GetAll();
    }
}
