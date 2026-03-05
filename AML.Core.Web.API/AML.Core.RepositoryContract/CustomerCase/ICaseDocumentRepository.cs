using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CaseDocument;
namespace AML.Core.RepositoryContract.CustomerCase
{
    public interface ICaseDocumentRepository : IBaseRepository
    {
        ServiceResponse<int> Create(CaseDocumentDTO _CaseDocumentDTO);
        ServiceResponse<CaseDocumentDTO> GetDetails(int Id);
        ServiceResponse<List<CaseDocumentDTO>> GetDetailsByCaseId(int caseId);
        ServiceResponse<int> Update(CaseDocumentDTO _CaseDocumentDTO);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<CaseDocumentDTO>> GetAll();
        ServiceResponse<List<DocumentCategoryDTO>> GetAllCategories();
        ServiceResponse<List<DocumentTypeDTO>> GetAllTypes();
        
    }
}
