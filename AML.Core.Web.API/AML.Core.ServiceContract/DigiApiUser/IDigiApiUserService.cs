using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Country;
using AML.DTO.DTO.DigiApiUser;

namespace AML.Core.ServiceContract.DigiApiUser
{
    public interface IDigiApiUserService : IBaseService
    {
        DigiApiUserDTO GetByUsername(string Username);
        ServiceResponse<int> Update(DigiApiUserDTO _DigiApiUserDTO);
    }
}
