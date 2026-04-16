using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.InternalWatchList;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.InternalWatchListExcel;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using ExcelDataReader;
using System.Text;

namespace AML.Core.Repository.InternalWatchList
{
    public class InternalWatchListRepository: BaseRepository, IInternalWatchListRepository
    {
        public InternalWatchListRepository(IConfiguration configuration, IHttpContextAccessor context) : base(configuration, context)
        { }
        public ServiceResponse<List<InternalWatcListExcelDTO>> LoadInternalWatchExcelData(string fileName, int sheetNo = 1)
        {
            ServiceResponse<List<InternalWatcListExcelDTO>> serviceResponse = new ServiceResponse<List<InternalWatcListExcelDTO>>();
            try
            {
                List<InternalWatcListExcelDTO> excelData = new List<InternalWatcListExcelDTO>();
                
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = System.IO.File.Open(fileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                        {
                            ConfigureDataTable = (_) => new ExcelDataTableConfiguration()
                            {
                                UseHeaderRow = true
                            }
                        });

                        var table = result.Tables[sheetNo - 1];
                        foreach (DataRow row in table.Rows)
                        {
                            if (row[0] != DBNull.Value)
                            {
                                excelData.Add(new InternalWatcListExcelDTO()
                                {
                                    Type = Convert.ToString(row[0]),
                                    FullName = Convert.ToString(row[1]),
                                    Nationality = Convert.ToString(row[2]),
                                    DOB = Convert.ToString(row[3]),
                                    Source = Convert.ToString(row[4]),
                                    CustomerID = Convert.ToString(row[5]),
                                    REMARKS = Convert.ToString(row[6])
                                });
                            }
                        }
                    }
                }
                serviceResponse.Message = "Excel Data Loaded.";
                serviceResponse.Status = StaticResource.SuccessStatusCode;
                serviceResponse.Result = excelData;
                return serviceResponse;
            }
            catch (Exception ex)
            {
                serviceResponse.Message = ex.Message;
                serviceResponse.Status = StaticResource.FailStatusCode;
            }
            return serviceResponse;
        }
        public ServiceResponse<int> InsertUploadLogs(SourceUploadLogsDTO Uploadlogs)
        {
            ServiceResponse<int> serviceResponse = new ServiceResponse<int>();
            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@p_source", Uploadlogs.Source);
                parameters.Add("@p_totalcount", Uploadlogs.TotalRecords);///this is the id of the customermaster table which will be appended with prefix in the insert statement
                var response = ExecuteScalar("ins_upload_logs", parameters, commandType: CommandType.StoredProcedure).ParseInt();
                serviceResponse.Result = response;
                serviceResponse.Message = "Customer case added successfully.";
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
