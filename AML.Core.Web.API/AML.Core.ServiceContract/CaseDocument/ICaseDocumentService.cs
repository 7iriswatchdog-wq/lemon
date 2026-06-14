using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CaseDocument;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.CaseDocument
{

    public interface ICaseDocumentService : IBaseService
    {
        ServiceResponse<int> Create(CaseDocumentDTO _CaseDocumentDTO);
        CaseDocumentDTO GetDetails(int Id);
        ServiceResponse<int> Update(CaseDocumentDTO _CaseDocumentDTO);
        ServiceResponse<int> Delete(int Id);
        List<CaseDocumentDTO> GetCaseDocumentByCaseId(int caseId);
        List<CaseDocumentDTO> GetAll();
        List<DocumentCategoryDTO> GetAllCategories();
        List<DocumentTypeDTO> GetAllTypes();
        

    }
}
