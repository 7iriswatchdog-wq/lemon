using AML.Core.Common.StaticResource;
using AML.DTO.DTO.ProliferationFinance;
using AML.Core.RepositoryContract.Common;
using System.Collections.Generic;

namespace AML.Core.RepositoryContract.ProliferationFinance
{
    public interface IProliferationFinanceRepository : IBaseRepository
    {
        ServiceResponse<List<UAEControlListDTO>> SearchChemicals(ProliferationFinanceCaseDTO searchCriteria);
        ServiceResponse<int> CreateCase(ProliferationFinanceCaseDTO caseDto);
        ServiceResponse<List<ProliferationFinanceCaseDTO>> GetAllCases();
        ProliferationFinanceCaseDTO GetCaseById(int id);
        UAEControlListDTO GetChemicalById(int id);
        ServiceResponse<bool> UpdateCaseStatus(int caseId, string status);
        ServiceResponse<bool> UpdateCaseRemarks(int caseId, string remarks);
        ServiceResponse<bool> UpdateSearchHits(int caseId, string hitDetails);
    }
}
