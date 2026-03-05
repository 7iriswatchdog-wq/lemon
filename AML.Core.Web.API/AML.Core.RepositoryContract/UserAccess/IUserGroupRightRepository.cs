using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.UserAccess;

namespace AML.Core.RepositoryContract.UserAccess
{
    public interface IUserGroupRightRepository : IBaseRepository
    {
        // Application mapping table
        // No Update or Delete or getDetails of any single record is meaningless 
        ServiceResponse<int> Create(UserGroupRightDTO _userGroupRightDTO);

        ServiceResponse<int> DeleteByUserGroupId(int _userGroupID);

        //ServiceResponse<UserGroupRightDetailsDTO> GetDetailsByUserGroupId(int ugrId);
        ServiceResponse<List<UserGroupRightDetailsDTO>> GetDetailsByUserGroupId(int ugrId);

        ServiceResponse<UserGroupRightDetailsDTO> GetDetailsByModuleId(int modId);

        //ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID);
        ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID, string sessionId);
        ServiceResponse<UserGroupRightDTO> GetByUserGroupId(int ugrId);
        ServiceResponse<List<ClientMenuRightsModelDTO>> getClientMenuByClientId(int clientId);
    }
}
