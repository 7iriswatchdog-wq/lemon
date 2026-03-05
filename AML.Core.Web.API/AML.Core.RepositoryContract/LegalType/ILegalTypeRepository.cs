using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Company;
using AML.DTO.DTO.LegalType;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.LegalType
{
    public interface ILegalTypeRepository : IBaseRepository
    {
        ServiceResponse<int> Create(LegalTypeDTO _legalTypeDTO);
        ServiceResponse<LegalTypeDTO> GetDetails(int Id);
        ServiceResponse<int> Update(LegalTypeDTO _legalTypeDTO);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<LegalTypeDTO>> GetAll();
    }
}
