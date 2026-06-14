using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using Aml.Screening.ServiceContracts.ServiceContracts;
using Aml.Screening.Services.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Aml.Screening.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class BlackListController : ControllerBase
    {
        public readonly IBlackListServices _blackListServices;
        public readonly ICaseLogServices _caseLogServices;
        public readonly ITransactionCaseLogServices _trancaseLogServices;
        private IConfiguration _configuration;
        private ScreeningServices _service;

        public BlackListController(IBlackListServices blackListServices, ICaseLogServices caseLogServices, IConfiguration configuration, ITransactionCaseLogServices trancaseLogServices)
        {
            _blackListServices = blackListServices;
            _configuration = configuration;
            _caseLogServices = caseLogServices;
            _trancaseLogServices = trancaseLogServices;
            _service = new ScreeningServices(configuration);
        }
        [HttpPost]
        public async Task<ActionResult<IEnumerable<FuzzyFilterDto>>> ScreenCustomer([FromBody] CustomerDto customerDto)
        {
            try
            {
                var result = await _blackListServices.GetBlackListByFilter(customerDto);
                if (result != null)
                    return Ok(result);
                return NotFound();                
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<CASELOGMATCH>>> BlackListSearch([FromBody] SearchDto searchDto)
        {
            try
            {
                var result = await _blackListServices.SearchBlackListOffline(searchDto);
                if (result == null)
                    return BadRequest();
                else if (result.Count > 0)
                    return Ok(result);
                return NotFound();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //[HttpPost]
        //public async Task<ActionResult<IEnumerable<BlackListDto>>> BlackListSearch([FromBody] SearchDto searchDto)
        //{
        //    try
        //    {
        //        var result = await _blackListServices.SearchBlackListOffline(searchDto);
        //        if (result == null)                
        //            return BadRequest();                
        //        else if(result.Count > 0)
        //            return Ok(result);
        //        return NotFound();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        [HttpPost]
        public async Task<ActionResult<CASELOG>> GetByCaseId([FromBody] CaseRequestDto caseDto)
        {
            try
            {
                var result = await _caseLogServices.GetCaseById(caseDto);
                if (result == null)
                {
                    return NotFound();
                }
                else if ((result.MATCHRECORDS?.Any() ?? false) ||
                    (result.WEBRECORDS?.Any() ?? false))
                {
                    return Ok(result);
                }
                else 
                { 
                    return BadRequest("No records found.");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }



        [HttpPost]
        public async Task<ActionResult<List<TRANCASELOGMATCH>>> GetByTranCaseId([FromBody] CaseRequestDto caseDto)
        {
            try
            {
                var result = await _trancaseLogServices.GetTranCaseById(caseDto);
                if (result == null)
                    return NotFound();
                else if (result.Count > 0)
                    return Ok(result);
                else
                    return BadRequest();

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<List<TRANCASELOGMATCH>>> GetByTranRefNo([FromBody] TranCaseRequestDto caseDto)
        {
            try
            {
                var result = await _trancaseLogServices.GetByTranRefNo(caseDto);
                if (result == null)
                    return NotFound();
                else
                    return Ok(result);

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<IEnumerable<BlackListDto>>> AddBlackList([FromBody] WatchList watchListDto)
        {
            try
            {
                

                var result = await _blackListServices.AddBlackList(watchListDto);
                if (result.ResponseCode == "200")
                    return Ok(result);
                else
                    return BadRequest(result);

            }
            catch (Exception)
            {
                throw;
            }
        }
        [HttpPost]
        public async Task<ActionResult> DeleteBlackList([FromBody] WatchlistRequestDto uid)
        {
            try
            {
                var result = await _blackListServices.DeleteBlackList(uid);
                if (result.ResponseCode == "200")
                    return Ok(result);
                else
                    return BadRequest(result);

            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse>> GetWatchlist([FromBody] WSearchDto searchDto)
        {
            try
            {
                var result = await _blackListServices.GetWatchlistByFilter(searchDto);
                if (result == null)
                    return BadRequest();
                else if (result.Count() > 0)
                    return Ok(result);
                else
                    return NotFound();               

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse>> GetWatchlistByUID([FromBody] WatchlistRequestDto searchDto)
        {
            try
            {
                var result = await _blackListServices.GetWatchlistByUID(searchDto);
                if (result == null)
                    return BadRequest();
                else if (result != null)
                    return Ok(result);
                else
                    return NotFound();

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        [HttpPost]
        public async Task<ActionResult<ServiceResponse>> UpdateBlackList([FromBody] WatchListModel watchlistmodeldto)
        {
            try
            {
                var result = await _blackListServices.UpdateBlackList(watchlistmodeldto);
                if (result == null)
                    return BadRequest();
                else if (result != null)
                    return Ok(result);
                else
                    return NotFound();

            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        public async Task<ActionResult<IEnumerable<BlackListDto>>> BlackFuzzySearch([FromBody] CustomerScreenDto searchDto)
        {
            try
            {
                var fuzzyNameScreen = await _service.CheckFuzzyName(searchDto);
                if (fuzzyNameScreen == null)
                    return BadRequest();
                else if (fuzzyNameScreen != null )
                    return Ok(fuzzyNameScreen);
                return NotFound();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}



