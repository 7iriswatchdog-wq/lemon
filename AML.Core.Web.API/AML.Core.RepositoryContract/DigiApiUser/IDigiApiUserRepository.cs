using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Designation;
using AML.DTO.DTO.DigiApiUser;

namespace AML.Core.RepositoryContract.DigiApiUser
{
    public interface IDigiApiUserRepository : IBaseRepository
    {
        ServiceResponse<DigiApiUserDTO> GetByUsername(string Username);
        ServiceResponse<int> Update(DigiApiUserDTO _DigiApiUserDTO);
    }
}
