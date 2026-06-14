using AML.DTO.DTO.ProliferationFinance;
using System.Collections.Generic;
using static AML.DTO.DTO.ProliferationFinance.PFSearchResultsMongoDTO;

namespace AML.Core.RepositoryContract.ProliferationFinance
{
    public interface IProliferationFinanceMongoRepository
    {
        bool SaveSearchResults(PF_SEARCHRESULT result);
        PF_SEARCHRESULT GetSearchResultsByCaseId(int caseId);
        bool UpdateHitDecision(int caseId, string searchType, string decision, string remarks);
    }
}
