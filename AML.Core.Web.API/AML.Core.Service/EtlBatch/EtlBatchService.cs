using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.EtlBatch;
using AML.Core.ServiceContract.EtlBatch;
using AML.DTO.DTO.EtlBatch;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.EtlBatch
{
    public class EtlBatchService:  BaseService, IEtlBatchService
    {
        IEtlLogRepository _etlLogRepository;
        public EtlBatchService(IEtlLogRepository etlLogRepository, IConfiguration configuration, IHostingEnvironment environment) : base(etlLogRepository, configuration)
        {
            _etlLogRepository = etlLogRepository;
        }
        public ServiceResponse<List<EtlBatchDTO>> GetAll(string datefrom, string dateTo)
        {
            //Perform business requirements here
            return _etlLogRepository.GetAll(datefrom, dateTo);
        }
        public ServiceResponse<int> Create(EtlBatchDTO _etlBatchDTO)
        {
            //Perform business requirements here
            _etlBatchDTO.AddedBy = 1; // Needto change this with loggedin User Id
            return _etlLogRepository.Create(_etlBatchDTO);
        }
    }
}
