using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CustomerCategory;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.CustomerCategory
{
    public interface ICustomerCategoryRepository: IBaseRepository
    {
        ServiceResponse<List<CustomerCategoryDTO>> GetAll();
    }
}
