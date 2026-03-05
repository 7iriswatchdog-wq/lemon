using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserAccess;
using AML.Core.ServiceContract.UserAccess;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.UserAccess;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.UserAccess
{
    public class UserGroupRightService : BaseService, IUserGroupRightService
    {
        IUserGroupRightRepository _userGroupRightRepository;
        public UserGroupRightService(IUserGroupRightRepository userGroupRightRepository, IConfiguration configuration, IHostingEnvironment environment) : base(userGroupRightRepository, configuration)
        {
            _userGroupRightRepository = userGroupRightRepository;
        }

        public ServiceResponse<int> Create(UserGroupRightDTO _userGroupRightDTO)
        {
            _userGroupRightDTO.CreatedBy = 1;
            return _userGroupRightRepository.Create(_userGroupRightDTO);
        }

        public ServiceResponse<int> DeleteByUserGroupId(int _userGroupID)
        {
            return _userGroupRightRepository.DeleteByUserGroupId(_userGroupID);
        }

        public ServiceResponse<UserGroupRightDTO> GetByUserGroupId(int ugrId)
        {
            return _userGroupRightRepository.GetByUserGroupId(ugrId);
        }

        public ServiceResponse<UserGroupRightDetailsDTO> GetDetailsByModuleId(int modId)
        {
            return _userGroupRightRepository.GetDetailsByModuleId(modId);
        }

        public ServiceResponse<List<UserGroupRightDetailsDTO>> GetDetailsByUserGroupId(int ugrId)
        {
            return _userGroupRightRepository.GetDetailsByUserGroupId(ugrId);
        }
        public ServiceResponse<List<ClientMenuRightsModelDTO>> getClientMenuByClientId(int clientId)
        {
            return _userGroupRightRepository.getClientMenuByClientId(clientId);
        }
        //public ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID)
        public ServiceResponse<bool> CheckUserRightExixts(string _controllerName, string _actionname, int _userId, int _userGroupID, string sessionId)
        {
            //return _userGroupRightRepository.CheckUserRightExixts(_controllerName, _actionname, _userId, _userGroupID);
            return _userGroupRightRepository.CheckUserRightExixts(_controllerName, _actionname, _userId, _userGroupID, sessionId);
        }
    }
}
