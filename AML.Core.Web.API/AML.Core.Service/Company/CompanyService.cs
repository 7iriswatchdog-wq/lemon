using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Company;
using AML.Core.ServiceContract.Company;
using AML.DTO.DTO.Company;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.Company
{
    public class CompanyService : BaseService, ICompanyService
    {
        ICompanyRepository _CompanyRepository;
        public CompanyService(ICompanyRepository CompanyRepository, IConfiguration configuration, IHostingEnvironment environment) 
            : base(CompanyRepository, configuration)
        {
            _CompanyRepository = CompanyRepository;
        }

        public ServiceResponse<int> Create(CompanyDTO _CompanyDTO)
        {
            //Perform business requirements here
            _CompanyDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _CompanyRepository.Create(_CompanyDTO);
        }

        public CompanyDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _CompanyRepository.GetDetails(Id).Result;
        }

        public List<CompanyDTO> GetAll()
        {
            //Perform business requirements here
            return _CompanyRepository.GetAll().Result;
        }

        public ServiceResponse<int> Update(CompanyDTO _CompanyDTO)
        {
            //Perform business requirements here
            _CompanyDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _CompanyRepository.Update(_CompanyDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _CompanyRepository.Delete(Id);
        }
    }
}
