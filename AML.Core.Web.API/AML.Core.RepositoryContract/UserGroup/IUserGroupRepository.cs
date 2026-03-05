using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.UserGroup;

namespace AML.Core.RepositoryContract.UserGroup
{
    public interface IUserGroupRepository : IBaseRepository
    {
        ServiceResponse<int> Create(UserGroupDTO _userGroupDTO);

        ServiceResponse<UserGroupDTO> GetDetails(int Id);
        ServiceResponse<int> Delete(int Id);

        ServiceResponse<int> Update(UserGroupDTO _userGroupDTO);

        ServiceResponse<List<UserGroupDTO>> GetAll(int clientId);
    }
}
