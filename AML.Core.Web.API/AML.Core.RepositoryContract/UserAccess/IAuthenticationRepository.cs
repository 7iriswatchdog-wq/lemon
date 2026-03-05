using AML.Core.Common.StaticResource;
using AML.DTO.DTO.User;

namespace AML.Core.RepositoryContract.UserAccess
{
    public interface IAuthenticationRepository : IBaseRepository
    {
        ServiceResponse<UserDTO> VerifyUser(string _userName, string _password, string _clientName);

        ServiceResponse<int> UpdateUserPassword(int _userId, string _password);
        ServiceResponse<UserDTO> VerifyUserByUserName(string userName, string clientName);
        ServiceResponse<UserDTO> VerifyOTP(string userName, int otp, int ClientId);
        ServiceResponse<UserDTO> SaveOTP(string userName, int otp, int clientId);
        ServiceResponse<int> UpdateLogin(int userid, string status);
    }
}
