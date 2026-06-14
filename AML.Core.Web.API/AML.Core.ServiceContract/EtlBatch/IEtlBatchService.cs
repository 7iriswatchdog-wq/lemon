using AML.Core.Common.StaticResource;
using AML.DTO.DTO.EtlBatch;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.EtlBatch
{
    public interface IEtlBatchService: IBaseService
    {
        ServiceResponse<List<EtlBatchDTO>> GetAll(string datefrom, string dateTo);
        ServiceResponse<int> Create(EtlBatchDTO _etlBatchDTO);
    }
}
