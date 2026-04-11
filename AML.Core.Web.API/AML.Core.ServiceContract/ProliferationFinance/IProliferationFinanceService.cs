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
        ServiceResponse<List<ProliferationFinanceCaseDTO>> GetAllCases(int clientId);
        ProliferationFinanceCaseDTO GetCaseById(int id);
        UAEControlListDTO GetChemicalById(int id);
        ServiceResponse<bool> UpdateCaseStatus(int caseId, string status);
        ServiceResponse<bool> UpdateCaseRemarks(int caseId, string remarks);
        ServiceResponse<bool> UpdateSearchHits(int caseId, string hitDetails);
        ServiceResponse<List<ProliferationFinanceCaseDTO>> GetCaseHistory(string corporateId, int clientId);

        ProliferationFinanceCaseDTO GetVersionAllCases(int caseId,string corporateId, int clientId);

        // MongoDB methods
        ServiceResponse<bool> SyncSearchResultsToMongo(int caseId, string productName, List<DTO.DTO.ProliferationFinance.UAEControlListDTO> chemicalHits, string pdfHits);
        ServiceResponse<bool> UpdateMongoHitDecision(int caseId, int hitIndex, string decision, string remarks);
        AML.DTO.DTO.ProliferationFinance.PFSearchResultsMongoDTO.PF_SEARCHRESULT GetMongoSearchResults(int caseId);
    }
}
