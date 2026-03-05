using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.Sanction;
using AML.DTO.DTO.Sanction;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.Sanction
{
    public class WatchListRepository : BaseRepository, IWatchListRepository
    {
        public WatchListRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }

        public ServiceResponse<int> Create(WatchListDTO _watchListDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_re_active_date", _watchListDTO.ReActiveDate);
                parameters.Add("@p_type", _watchListDTO.Type);
                parameters.Add("@p_fname", _watchListDTO.FirstName);
                parameters.Add("@p_mname", _watchListDTO.MiddleName);
                parameters.Add("@p_lname", _watchListDTO.LastName);
                parameters.Add("@p_nationality", _watchListDTO.Nationality);
                parameters.Add("@p_country_id", _watchListDTO.CountryId);
                parameters.Add("@p_dob", _watchListDTO.DOB);
                parameters.Add("@p_passport_no", _watchListDTO.PassportNo);
                parameters.Add("@p_status", _watchListDTO.Status);
                parameters.Add("@p_source", _watchListDTO.Source);
                parameters.Add("@p_source_unique_id", _watchListDTO.SourceUniqueId);
                parameters.Add("@p_narration", _watchListDTO.Narration);
                parameters.Add("@p_remarks", _watchListDTO.Remarks);
                parameters.Add("@p_is_blocked", _watchListDTO.IsBlocked);
                parameters.Add("@p_created_by", _watchListDTO.CreatedBy);
                var response = ExecuteScalar("ins_watchlist", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Watch List added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<List<WatchListDTO>> GetAll()
        {
            ServiceResponse<List<WatchListDTO>> serviceResponse = new ServiceResponse<List<WatchListDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                serviceResponse.Result = Get<WatchListDTO>("get_all_watchlist", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "watchlist details fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
    }
}
