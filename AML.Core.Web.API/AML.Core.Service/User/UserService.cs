using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.DataContract;
using AML.Core.RepositoryContract.User;
using AML.Core.ServiceContract.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using AML.Core.ServiceContract;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.User;
using AML.ViewModel.ViewModels.User;
using AML.DTO.DTO.Common;

namespace AML.Core.Service.User
{
    public class UserService : BaseService, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserDetailRepository _userDetailRepository;
        IHostingEnvironment _environment;
        public UserService(IUserRepository userRepository, IUserDetailRepository userDetailRepository,IConfiguration configuration, IHostingEnvironment environment) : base(userRepository, configuration)
        {
            _userRepository = userRepository;
            _userDetailRepository = userDetailRepository;
            _environment = environment;
        }
        public ServiceResponse<int> Create(UserDTO _userDTO)
        {
            _userDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            var _Response = _userRepository.Create(_userDTO);
            if (_Response.Result > 0)
            {
                var _userDetail = _userDetailRepository.GetDetails(_Response.Result);
                _userDTO.UserDetail.UserId = _Response.Result;
                if (_userDetail.Result == null)
                {
                    return _userDetailRepository.Create(_userDTO.UserDetail);
                }
                else
                {
                    return _userDetailRepository.Update(_userDTO.UserDetail);
                }
            }
            return _Response;
        }
        
        public ServiceResponse<int> Delete(UserDTO _userDTO)
        {
            return _userRepository.Delete(_userDTO);
        }

        public List<UserDTO> GetAll(int clientId)
        {
            //Perform business requirements here
            return _userRepository.GetAll(clientId).Result;
        }

        public List<UserPasswordLogModelDTO> GetAllPasswordLogs(string startDate, string endDate,int clientId)
        {
            //Perform business requirements here
            return _userRepository.GetAllPasswordLogs(startDate, endDate, clientId).Result;
        }

        public UserDTO GetDetails(int Id)
        {
            try
            {
                var _UserDTO = _userRepository.GetDetails(Id).Result;
                if (_UserDTO.IsNotNullOrEmpty())
                {
                    _UserDTO.UserDetail = _userDetailRepository.GetDetails(Id).Result;
                    return _UserDTO;
                }
            }
            catch { }
            return new UserDTO();
        }

        public ServiceResponse<int> Update(UserDTO _userDTO)
        {
            //Perform business requirements here
            var _user = _userRepository.GetDetails(_userDTO.Id);
            if (_user.Result != null)
            {
                _userDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
                var _Response = _userRepository.Update(_userDTO);
                if (_Response.Result > 0)
                {
                    var _userDetails = _userDetailRepository.GetDetails(_userDTO.Id);
                    _userDTO.UserDetail.UserId = _userDTO.Id;
                    _userDTO.UserDetail.UpdatedBy = 1;
                    if (_userDetails.Result == null)
                    {
                        return _userDetailRepository.Create(_userDTO.UserDetail);
                    }
                    else
                    {
                        _userDTO.UserDetail.Id = _userDetails.Result.Id;
                        return _userDetailRepository.Update(_userDTO.UserDetail);
                    }
                }
                return _Response;
            }
            return null;
        }
        
        public ServiceResponse<int> BlockUser(UserDTO _userDTO)
        {
            return _userRepository.BlockUser(_userDTO);
        }

        public bool ValidateApiAuthentication(string username, string password, string loginUserId)
        {
            ApiUserDTO model = _userRepository.GetApiUserDetails(username, password,loginUserId).Result;

            if (model == null)
                return false;
            else
                return true;

        }
        public List<UserDTO> GetAuthorisedUser(string _controllerName, string _actionname, int clientId)
        {
            return _userRepository.GetAuthorisedUser(_controllerName, _actionname, clientId).Result;
        }
    }
}
