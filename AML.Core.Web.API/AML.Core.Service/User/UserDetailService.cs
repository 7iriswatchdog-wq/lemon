using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.User;
using AML.Core.ServiceContract.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.User
{
    public class UserDetailService: BaseService
    {
        private readonly IUserDetailRepository _userDetailRepository;
        IHostingEnvironment _environment;
        public UserDetailService(IUserDetailRepository userDetailRepository, IConfiguration configuration, IHostingEnvironment environment) : base(userDetailRepository, configuration)
        {
            _userDetailRepository = userDetailRepository;
            _environment = environment;
        }

        public ServiceResponse<int> Create(object obj)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<int> Delete(object obj)
        {
            throw new NotImplementedException();
        }

        public List<object> GetAll(object obj)
        {
            throw new NotImplementedException();
        }

        public object GetDetails(object obj)
        {
            throw new NotImplementedException();
        }

        public ServiceResponse<int> Update(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
