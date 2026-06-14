using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Screening.ServiceContracts.ServiceContracts
{
    public interface IBlackListServices
    {

        Task<IEnumerable<FuzzyFilterDto>> GetBlackListByFilter(CustomerDto dto);
        Task<BLACKLIST> GetBlackListDetailsByUId(IdDto dto);

        Task<List<CASELOGMATCH>> SearchBlackListOffline(SearchDto dto);

        //hided by sanjana
        //Task<List<BlackListDto>> SearchBlackListOffline(SearchDto dto);
        Task<ServiceResponse> AddBlackList(WatchList watchlist);
        Task<ServiceResponse> DeleteBlackList(WatchlistRequestDto watchlist);
        Task<IEnumerable<WSearchResult>> GetWatchlistByFilter(WSearchDto dto);

        Task<WSearchResult> GetWatchlistByUID(WatchlistRequestDto watchlist);

        Task<ServiceResponse> UpdateBlackList(WatchListModel watchlistmodeldto);

    }
}
