using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Aml.Screening.Services.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aml.Screening.Services.Services
{
    public class BlackListServices : IBlackListServices
    {
        /// <summary>
        /// The service
        /// </summary>
        private ScreeningServices _service;

        public BlackListServices(IConfiguration configuration)
        {
            _service = new ScreeningServices(configuration);
        }
        public async Task<IEnumerable<FuzzyFilterDto>> GetBlackListByFilter(CustomerDto dto)
        {
            try
            {
                var screendto = dto.ToOnlineCustomerScreenDto();
                var result = await _service.GetFuzzyMatches(screendto);
                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        //public async Task<IEnumerable<FuzzyFilterDto>> SearchBlackList(SearchDto dto)
        //{
        //    try
        //    {
        //        var screendto = dto.ToOnlineCustomerScreenDto();
        //        var result = await _service.GetFuzzyMatches(screendto);
        //        return result;
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}

        public Task<BLACKLIST> GetBlackListDetailsByUId(IdDto dto)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Searches the black list offline.
        /// </summary>
        /// <param name="dto">The dto.</param>
        /// <returns></returns>
        //public async Task<List<BlackListDto>> SearchBlackListOffline(SearchDto dto)
        //{
        //        return await _service.SearchBlacklist(dto);
        //}

        public async Task<List<CASELOGMATCH>> SearchBlackListOffline(SearchDto dto)
        {
            return await _service.SearchBlacklist(dto);
        }


        public async Task<ServiceResponse> AddBlackList(WatchList watchlist)
        {
            var dto = watchlist.ToBlackListDto();
            return await _service.AddBlackList(dto);
        }
        public async Task<ServiceResponse> DeleteBlackList(WatchlistRequestDto uid)
        {
            
            return await _service.DeleteBlackList(uid);
        }


        public async Task<IEnumerable<WSearchResult>> GetWatchlistByFilter(WSearchDto dto)
        {            
            return await _service.GetWatchlistByFilter(dto);
        }

        public async Task<WSearchResult> GetWatchlistByUID(WatchlistRequestDto dto)
        {
            return await _service.GetWatchlistByUID(dto);
        }

        public async Task<ServiceResponse> UpdateBlackList(WatchListModel watchlistmodeldto)
        {
            
            return await _service.UpdateBlackList(watchlistmodeldto);
        }
    }
}
