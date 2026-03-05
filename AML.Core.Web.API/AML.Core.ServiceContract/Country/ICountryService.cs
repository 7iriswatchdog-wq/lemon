using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Country;

namespace AML.Core.ServiceContract.Country
{
    public interface ICountryService : IBaseService
    {
        ServiceResponse<int> Create(CountryDTO _countryDTO);
        CountryDTO GetDetails(int Id);
        ServiceResponse<int> Update(CountryDTO _countryDTO);
        ServiceResponse<int> Delete(int Id);
        List<CountryDTO> GetAll(int ClientId);
        List<CountryDTO> GetListAll();
        List<CountryDTO> GetListFATFAll();
        List<CountryDTO> GetAllCountryRiskConfig(string culture);
        List<CountryDTO> GetAllPartnerNationality(int ClientId);
        

    }
}
