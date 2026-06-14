using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Country;

namespace AML.Core.RepositoryContract.Country
{
    public interface ICountryRepository : IBaseRepository
    {
        ServiceResponse<int> Create(CountryDTO _countryDTO);
        ServiceResponse<CountryDTO> GetDetails(int Id);

        ServiceResponse<CountryDTO> GetCountryNameByCode(string isoCode, int clientid);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<int> Update(CountryDTO _countryDTO);
        ServiceResponse<List<CountryDTO>> GetAll(int ClientId);
        ServiceResponse<List<CountryDTO>> GetListAll();
        ServiceResponse<List<CountryDTO>> GetListFATFAll();
        ServiceResponse<List<CountryDTO>> GetAllCountryRiskConfig(string culture);
        ServiceResponse<List<CountryDTO>> GetAllPartnerNationality(int ClientId);
    }
}
