using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.User;
using AML.Core.RepositoryContract.UserAccess;
using AML.Core.ServiceContract.Common;
using AML.Core.ServiceContract.UserAccess;
using AML.DTO.DTO.User;
using AML.ViewModel.ViewModels.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AML.Core.Service.UserAccess
{
    public class AuthenticationService : BaseService, IAuthenticationService
    {
        private readonly IAuthenticationRepository _authenticationRepository;
        private readonly IUserDetailRepository _userDetailRepository;
        private readonly ICommonService _commonService;
        public AuthenticationService(IAuthenticationRepository authenticationRepository, IUserDetailRepository userDetailRepository, ICommonService commonService,IConfiguration configuration, IHostingEnvironment environment) : base(authenticationRepository, configuration)
        {
            _authenticationRepository = authenticationRepository;
            _userDetailRepository = userDetailRepository;
            _commonService = commonService;
        }
        public ServiceResponse<int> UpdateUserPassword(int _userId, string _password)
        {
            return _authenticationRepository.UpdateUserPassword(_userId, _password);
        }

        public ServiceResponse<UserDTO> VerifyUser(string _userName, string _password, string _clientName)
        {
            return _authenticationRepository.VerifyUser(_userName, _password, _clientName);
        }
        string userEmail = null;
        public ServiceResponse<UserDTO> ForgotPassword(string _userName, string _clientName)
        {
            
            var result = _authenticationRepository.VerifyUserByUserName(_userName, _clientName);
            if(result.Result != null)
            {

                userEmail = result.Result.email;
            }
            return result;
        }

        public ServiceResponse<UserDTO> VerifyOTP(string _userName, int _otp, int ClientId)
        {
            return _authenticationRepository.VerifyOTP(_userName, _otp, ClientId);
        }
        public ServiceResponse<UserDTO> SaveOTP(string _userName, int _otp, int ClientId)
        {
            var result = _authenticationRepository.SaveOTP(_userName, _otp,ClientId);
            //var otp = _otp;
            //string Email = userEmail;
            //string Email = result.email;
            //HTML Template for Send email  
        //    var emailModel = new EmailModel
        //        (Email, // To
        //          "Reset Password Info", // Subject  
        //          "To reset your password, please use the following One Time Password(OTP): <br /><b>"+otp+"</b>", // Message  
        //           true // IsBodyHTML  
        //);
            //_commonService.SendOtpEmail(emailModel);
            return result;
        }
        public ServiceResponse<int> UpdateLogin(int userid, string status)
        {
            return _authenticationRepository.UpdateLogin(userid, status);
        }
    }
}
