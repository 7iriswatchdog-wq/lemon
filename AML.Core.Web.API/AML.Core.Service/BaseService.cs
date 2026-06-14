using AML.Core.RepositoryContract;
using AML.Core.ServiceContract;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Configuration;
using AML.Core.DataContract.Authentication;
using System.Threading.Tasks;
using AML.Core.Common.StaticResource;

namespace AML.Core.Service
{
    public class BaseService : IBaseService
    {
        IBaseRepository _baseRepository;
        IBaseService _baseService;
        public IConfiguration Configuration;

        private LoginUser _loginUser;

        public BaseService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public BaseService(IBaseRepository baseRepository, IConfiguration configuration)
        {
            _baseRepository = baseRepository;
            Configuration = configuration;
        }

        public BaseService(IBaseRepository baseRepository, IBaseService baseService, IConfiguration configuration)
        {
            _baseRepository = baseRepository;
            _baseService = baseService;
            Configuration = configuration;
        }

        /// <summary>
        /// A pass through for the other services inheriting from Base Service to implement.
        /// </summary>
        public virtual Task<object> HealthCheckAsync()
        {
            // Not developed yet.
            throw new NotImplementedException();
        }

        /// <summary>
        /// Login user information
        /// </summary>
        public LoginUser LoginUser
        {
            get
            {
                if (_loginUser == null)
                {
                    throw new UnauthorizedAccessException();
                }
                else
                {
                    return _loginUser;
                }
            }

            set { _loginUser = value; }
        }
    }
}
