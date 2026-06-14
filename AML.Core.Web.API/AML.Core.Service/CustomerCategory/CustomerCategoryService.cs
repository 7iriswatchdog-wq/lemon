using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCategory;
using AML.Core.ServiceContract.CustomerCatogory;
using AML.DTO.DTO.CustomerCategory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AML.Core.Service.CustomerCategory
{
    public class CustomerCategoryService : BaseService, ICustomerCategoryService
    {
        ICustomerCategoryRepository _customerCategoryRepository;
        public CustomerCategoryService(ICustomerCategoryRepository customerCategoryRepository, IConfiguration configuration,
            IHostingEnvironment environment) : base(customerCategoryRepository, configuration)
        {
            _customerCategoryRepository = customerCategoryRepository;
        }
        public ServiceResponse<List<CustomerCategoryDTO>> GetAll()
        {
            return _customerCategoryRepository.GetAll();
        }

    }
}
