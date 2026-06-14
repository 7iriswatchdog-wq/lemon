using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.UserGroup;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.UserGroup;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.DTO.DTO.UserGroup;

namespace AML.Core.Service.UserGroup
{
    public class UserGroupService : BaseService, IUserGroupService
    {
        IUserGroupRepository _userGroupRepository;
        public UserGroupService(IUserGroupRepository userGroupRepository, IConfiguration configuration, IHostingEnvironment environment) : base(userGroupRepository, configuration)
        {
            _userGroupRepository = userGroupRepository;
        }

        public ServiceResponse<int> Create(UserGroupDTO _userGroupDTO)
        {
            //Perform business requirements here
            _userGroupDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _userGroupRepository.Create(_userGroupDTO);
        }

        public UserGroupDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _userGroupRepository.GetDetails(Id).Result;
        }

        public List<UserGroupDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            return _userGroupRepository.GetAll(clientId).Result;
        }

        public ServiceResponse<int> Update(UserGroupDTO _userGroupDTO)
        {
            //Perform business requirements here
            _userGroupDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _userGroupRepository.Update(_userGroupDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _userGroupRepository.Delete(Id);
        }
    }
}
