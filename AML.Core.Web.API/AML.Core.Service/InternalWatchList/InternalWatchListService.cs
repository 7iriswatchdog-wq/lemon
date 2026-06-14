using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.EtlBatch;
using AML.Core.RepositoryContract.InternalWatchList;
using AML.Core.ServiceContract.InternalWatchList;
using AML.DTO.DTO.EtlBatch;
using AML.DTO.DTO.InternalWatchListExcel;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.InternalWatchList
{
    public class InternalWatchListService : BaseService, IInternalWatchListService
    {
        IInternalWatchListRepository _internalWatchListRepository;
        public InternalWatchListService(IInternalWatchListRepository internalWatchListRepository, IConfiguration configuration, IHostingEnvironment environment)
            : base(internalWatchListRepository, configuration)
        {
            _internalWatchListRepository = internalWatchListRepository;
        }

        public ServiceResponse<List<InternalWatcListExcelDTO>> LoadInternalWatchExcelData(string fileName, int sheetNo = 1)
        {

            return _internalWatchListRepository.LoadInternalWatchExcelData(fileName, sheetNo);
        }

        public ServiceResponse<int> InsertUploadLogs(SourceUploadLogsDTO Uploadlogs)
        {

            return _internalWatchListRepository.InsertUploadLogs(Uploadlogs);
        }
    }
}
