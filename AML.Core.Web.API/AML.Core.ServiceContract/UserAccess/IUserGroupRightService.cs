using AML.Core.Common.StaticResource;
using AML.DTO.DTO.UserAccess;
using AML.DTO.DTO.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.UserAccess
{
    public interface IUserGroupRightService: IBaseService
    {
        ServiceResponse<int> Create(UserGroupRightDTO _userGroupRightDTO);
        ServiceResponse<int> DeleteByUserGroupId(int _userGroupID);
        ServiceResponse<UserGroupRightDTO> GetByUserGroupId(int ugrId);
        ServiceResponse<UserGroupRightDetailsDTO> GetDetailsByModuleId(int modId);
        //ServiceResponse<UserGroupRightDetailsDTO> GetDetailsByUserGroupId(int ugrId);
        ServiceResponse<List<UserGroupRightDetailsDTO>> GetDetailsByUserGroupId(int ugrId);
        //ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID);
        ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID, string sessionId);
        ServiceResponse<List<ClientMenuRightsModelDTO>> getClientMenuByClientId(int clientId);
    }
}
