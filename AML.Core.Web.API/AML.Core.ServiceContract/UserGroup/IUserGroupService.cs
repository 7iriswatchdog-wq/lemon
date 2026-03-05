using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.UserGroup;

namespace AML.Core.ServiceContract.UserGroup
{
    public interface IUserGroupService : IBaseService
    {
        ServiceResponse<int> Create(UserGroupDTO _userGroupDTO);
        UserGroupDTO GetDetails(int Id);
        ServiceResponse<int> Update(UserGroupDTO _userGroupDTO);
        ServiceResponse<int> Delete(int Id);
        List<UserGroupDTO> GetAll(int clientId);
    }
}
