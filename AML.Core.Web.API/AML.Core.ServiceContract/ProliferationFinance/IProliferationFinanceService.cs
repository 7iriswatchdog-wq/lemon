using AML.Core.Common.StaticResource;
using AML.DTO.DTO.ProliferationFinance;
using AML.Core.ServiceContract.Common;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.ProliferationFinance
{
    public interface IProliferationFinanceService
    {
        ServiceResponse<List<UAEControlListDTO>> SearchChemicals(ProliferationFinanceCaseDTO searchCriteria);
        ServiceResponse<string> SearchNonChemical(string keyword);
        ServiceResponse<int> CreateCase(ProliferationFinanceCaseDTO caseDto);
        ServiceResponse<List<ProliferationFinanceCaseDTO>> GetAllCases();
        ProliferationFinanceCaseDTO GetCaseById(int id);
        UAEControlListDTO GetChemicalById(int id);
        ServiceResponse<bool> UpdateCaseStatus(int caseId, string status);
        ServiceResponse<bool> UpdateCaseRemarks(int caseId, string remarks);
        ServiceResponse<bool> UpdateSearchHits(int caseId, string hitDetails);
    }
}
