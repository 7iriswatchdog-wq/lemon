using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.Company
{

    public interface ICompanyService : IBaseService
    {
        ServiceResponse<int> Create(CompanyDTO _companyDTO);
        CompanyDTO GetDetails(int Id);
        ServiceResponse<int> Update(CompanyDTO _companyDTO);
        ServiceResponse<int> Delete(int Id);
        List<CompanyDTO> GetAll();
    }
}
