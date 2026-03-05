using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.EtlBatch;
using AML.DTO.DTO.EtlBatch;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace AML.Core.Repository.EtlBatch
{
    public class EtlLogRepository : BaseRepository, IEtlLogRepository
    {
        public EtlLogRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context) { }
        public ServiceResponse<List<EtlBatchDTO>> GetAll(string datefrom,string dateTo)
        {
            ServiceResponse<List<EtlBatchDTO>> serviceResponse = new ServiceResponse<List<EtlBatchDTO>>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_added_on_from", datefrom);
                parameters.Add("@p_added_on_to", dateTo);
                serviceResponse.Result = Get<EtlBatchDTO>("get_all_etllogs_by_date", parameters, commandType: CommandType.StoredProcedure).ToList();
                serviceResponse.Message = "ETL Logs fetched successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> Create(EtlBatchDTO _etlBatchDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_file_name", _etlBatchDTO.FileName);
                parameters.Add("@p_file_full_path", _etlBatchDTO.FileFullPath);
                parameters.Add("@p_total_rows", _etlBatchDTO.TotalRows);
                parameters.Add("@p_rows_recorded", _etlBatchDTO.RowsRecorded);
                parameters.Add("@p_added_by", _etlBatchDTO.AddedBy);
                parameters.Add("@p_type", _etlBatchDTO.Type);
                parameters.Add("@p_clientId", _etlBatchDTO.ClientId);
                var response = ExecuteScalar("ins_etlbatch", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "ETL Batch added successfully.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }

        public ServiceResponse<int> Update(EtlBatchDTO _etlBatchDTO)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_id", _etlBatchDTO.Id);
                parameters.Add("@p_rows_recorded", _etlBatchDTO.RowsRecorded);
                var response = ExecuteScalar("mod_etlbatch", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "ETL Batch Updated successfully.";
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
