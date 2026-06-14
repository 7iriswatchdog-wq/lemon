using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aml.Screening.ServiceContracts.ServiceContracts
{
    public interface ITransactionCaseLogServices
    {
        Task<ServiceResponse> Add(TRANSACTION_CASELOG caseLog);
        Task<List<TRANCASELOGMATCH>> GetTranCaseById(CaseRequestDto dto);
        Task<List<TRANSACTION_CASELOG>> GetByTranRefNo(TranCaseRequestDto dto);
    }
}

