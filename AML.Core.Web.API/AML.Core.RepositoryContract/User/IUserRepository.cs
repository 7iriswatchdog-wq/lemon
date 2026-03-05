using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Common;
using AML.DTO.DTO.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.User
{
    public interface IUserRepository : IBaseRepository
    {
        ServiceResponse<int> Create(UserDTO _userDTO);
        ServiceResponse<List<UserDTO>> GetAll(int clientId);
        ServiceResponse<int> Update(UserDTO _userDTO);
        ServiceResponse<int> Delete(UserDTO _userDTO);
        ServiceResponse<int> BlockUser(UserDTO _userDTO);
        ServiceResponse<UserDTO> GetDetails(int Id);
        ServiceResponse<ApiUserDTO> GetApiUserDetails(string username, string password, string loginUserId);
        ServiceResponse<List<UserPasswordLogModelDTO>> GetAllPasswordLogs(string startDate, string endDate, int clientId);
        ServiceResponse<List<UserDTO>> GetAuthorisedUser(string _controllerName, string _actionname, int clientId);
    }
}
