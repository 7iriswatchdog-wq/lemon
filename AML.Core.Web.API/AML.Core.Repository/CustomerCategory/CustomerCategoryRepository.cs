using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerCategory;
using AML.DTO.DTO.CustomerCategory;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.CustomerCategory
{
    public class CustomerCategoryRepository : BaseRepository, ICustomerCategoryRepository
    {
        public CustomerCategoryRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<List<CustomerCategoryDTO>> GetAll()
        {
            ServiceResponse<List<CustomerCategoryDTO>> serviceResponse = new ServiceResponse<List<CustomerCategoryDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<CustomerCategoryDTO>("get_all_customercatogory", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "Customery category fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
    }
}
