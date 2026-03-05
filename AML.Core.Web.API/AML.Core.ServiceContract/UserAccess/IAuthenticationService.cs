using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.User;
namespace AML.Core.ServiceContract.UserAccess
{
    public interface IAuthenticationService: IBaseService
    {
        ServiceResponse<UserDTO> VerifyUser(string _userName, string _password,string _clientName);
        ServiceResponse<int> UpdateUserPassword(int _userId, string _password);
        ServiceResponse<UserDTO> ForgotPassword(string _userName, string _clientName);
         ServiceResponse<UserDTO> VerifyOTP(string userName, int otp, int ClientId);
         ServiceResponse<UserDTO> SaveOTP(string userName, int otp, int ClientId);
        ServiceResponse<int> UpdateLogin(int userid, string status);
    }
}
