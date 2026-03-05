using AML.Core.Common.StaticResource;
using AML.DTO.DTO.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.User
{
    public interface IUserDetailRepository: IBaseRepository
    {
        ServiceResponse<int> Create(UserDetailDTO _userDetailDTO);
        ServiceResponse<UserDetailDTO> GetDetails(int Id);
        ServiceResponse<int> Update(UserDetailDTO _userDetailDTO);
    }
}
