using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Aml.Screening.ServiceContracts.ServiceContracts
{
    public interface ICaseLogServices
    {
        Task<ServiceResponse> Add(CASELOG caseLog);
        Task<CASELOG> GetCaseById(CaseRequestDto dto);
      
    }
}
