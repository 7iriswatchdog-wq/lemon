using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.IdentityType;

namespace AML.Core.ServiceContract.IdentityType
{
    public interface IIdentityTypeService : IBaseService
    {
        ServiceResponse<int> Create(IdentityTypeDTO _identityTypeDTO);
        IdentityTypeDTO GetDetails(int Id);
        ServiceResponse<int> Update(IdentityTypeDTO _identityTypeDTO);
        ServiceResponse<int> Delete(int Id);
        List<IdentityTypeDTO> GetAll(int clientId);
    }
}
