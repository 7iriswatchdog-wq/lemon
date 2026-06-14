using AML.Core.Common.StaticResource;
using AML.DTO.DTO.InternalWatchListExcel;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.InternalWatchList
{
    public interface IInternalWatchListService : IBaseService
    {
        ServiceResponse<List<InternalWatcListExcelDTO>> LoadInternalWatchExcelData(string fileName, int sheetNo = 1);

        ServiceResponse<int> InsertUploadLogs(SourceUploadLogsDTO Uploadlogs);
    }
}
