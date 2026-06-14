using AML.Core.Common.StaticResource;
using AML.DTO.DTO.User;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
namespace AML.Core.ServiceContract.User
{
    public interface IUserService : IBaseService
    {
        /// <summary>
        /// IUserService is hiding(using keyword "new") the IUserService's Healthcheck to enable call to IUserService's HealthCheck.
        /// </summary>
        /// <returns>HealthCheck Information</returns>
        new Task<object> HealthCheckAsync();
        ServiceResponse<int> Create(UserDTO _userDTO);
        List<UserDTO> GetAll(int clientId);
        ServiceResponse<int> Update(UserDTO _userDTO);
        UserDTO GetDetails(int Id);
        ServiceResponse<int> BlockUser(UserDTO _userDTO);
        ServiceResponse<int> Delete(UserDTO _userDTO);

        bool ValidateApiAuthentication(string username, string password, string loginUserId);
        List<UserPasswordLogModelDTO> GetAllPasswordLogs(string startDate, string endDate,int ClientId);

        List<UserDTO> GetAuthorisedUser(string _controllerName, string _actionname, int clientId);
    }
}
