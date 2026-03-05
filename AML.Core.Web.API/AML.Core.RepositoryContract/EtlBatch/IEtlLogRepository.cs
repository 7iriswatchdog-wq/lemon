using AML.Core.Common.StaticResource;
using AML.DTO.DTO.EtlBatch;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AML.Core.RepositoryContract.EtlBatch
{
    public interface IEtlLogRepository: IBaseRepository
    {
        ServiceResponse<List<EtlBatchDTO>> GetAll(string datefrom, string dateTo);
        ServiceResponse<int> Create(EtlBatchDTO _etlBatchDTO);
        ServiceResponse<int> Update(EtlBatchDTO _etlBatchDTO);
    }
}
