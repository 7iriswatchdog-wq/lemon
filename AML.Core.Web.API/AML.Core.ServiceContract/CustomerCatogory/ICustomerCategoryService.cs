using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CustomerCategory;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.CustomerCatogory
{
    public interface ICustomerCategoryService: IBaseService
    {
        ServiceResponse<List<CustomerCategoryDTO>> GetAll();
    }
}
