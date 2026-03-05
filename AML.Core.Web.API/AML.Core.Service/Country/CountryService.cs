using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.ServiceContract.Country;
using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Country;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AML.DTO.DTO.Country;
namespace AML.Core.Service.Country
{
    public class CountryService : BaseService, ICountryService
    {
        ICountryRepository _countryRepository;
        public CountryService(ICountryRepository countryRepository, IConfiguration configuration, IHostingEnvironment environment) : base(countryRepository, configuration)
        {
            _countryRepository = countryRepository;
        }

        public ServiceResponse<int> Create(CountryDTO _countryDTO)
        {
            //Perform business requirements here
            // _countryDTO.CreatedBy = 1; // Needto change this with loggedin User Id
            return _countryRepository.Create(_countryDTO);
        }

        public CountryDTO GetDetails(int Id)
        {
            //Perform business requirements here
            return _countryRepository.GetDetails(Id).Result;
        }

        public List<CountryDTO> GetAll(int ClientId)
        {
            //Perform business requirements here
            return _countryRepository.GetAll(ClientId).Result;
        }
        public List<CountryDTO> GetListAll()
        {
            //Perform business requirements here
            return _countryRepository.GetListAll().Result;
        }
        public List<CountryDTO> GetListFATFAll()
        {
            //Perform business requirements here
            return _countryRepository.GetListFATFAll().Result;
        }
        public ServiceResponse<int> Update(CountryDTO _countryDTO)
        {
            //Perform business requirements here
            // _countryDTO.UpdatedBy = 1; // Needto change this with loggedin User Id
            return _countryRepository.Update(_countryDTO);
        }
        public ServiceResponse<int> Delete(int Id)
        {
            return _countryRepository.Delete(Id);
        }

        public List<CountryDTO> GetAllCountryRiskConfig(string culture)
        {
            //Perform business requirements here
            return _countryRepository.GetAllCountryRiskConfig(culture).Result;
        }
        public List<CountryDTO> GetAllPartnerNationality(int ClientId)
        {
            //Perform business requirements here
            return _countryRepository.GetAllPartnerNationality(ClientId).Result;
        }
       
    }
}
