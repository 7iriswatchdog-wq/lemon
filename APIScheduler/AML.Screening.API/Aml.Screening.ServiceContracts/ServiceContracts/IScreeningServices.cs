using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Screening.ServiceContracts.ServiceContracts
{
    public interface IScreeningServices
    {
        Task<CASELOGMATCH> CheckId(CustomerScreenDto dto);
        Task<ScreeningResponse> CheckNameNationalityDob(CustomerScreenDto sReq);
        Task<ScreeningResponse> CheckNameNationality(CustomerScreenDto sReq);
        Task<ScreeningResponse> CheckNameDob(CustomerScreenDto sReq);
        Task<CASELOGMATCH> CheckExactName(CustomerScreenDto dto);
        Task<IEnumerable<CASELOGMATCH>> CheckFuzzyName(CustomerScreenDto sReq);
        Task<ScreeningResponse> CheckAliasNameNationality(CustomerScreenDto sReq);
        Task<IEnumerable<WSearchResult>> GetWatchlistByFilter(WSearchDto dto);
    }
}
