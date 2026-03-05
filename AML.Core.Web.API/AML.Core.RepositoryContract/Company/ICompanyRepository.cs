using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Company;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.Company
{
    public interface ICompanyRepository : IBaseRepository
    {
        ServiceResponse<int> Create(CompanyDTO _companyDTO);
        ServiceResponse<CompanyDTO> GetDetails(int Id);
        ServiceResponse<int> Update(CompanyDTO _companyDTO);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<CompanyDTO>> GetAll();
    }
}
