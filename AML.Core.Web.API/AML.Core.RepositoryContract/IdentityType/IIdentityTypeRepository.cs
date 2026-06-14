using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.IdentityType;

namespace AML.Core.RepositoryContract.IdentityType
{
    public interface IIdentityTypeRepository : IBaseRepository
    {
        ServiceResponse<int> Create(IdentityTypeDTO _identityTypeDTO);
        ServiceResponse<IdentityTypeDTO> GetDetails(int Id);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<int> Update(IdentityTypeDTO _identityTypeDTO);
        ServiceResponse<List<IdentityTypeDTO>> GetAll(int clientId);
    }
}
