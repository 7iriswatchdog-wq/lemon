using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CaseComment;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.CaseComment
{
    public interface ICaseCommentService : IBaseService
    {
        ServiceResponse<int> Create(CaseCommentDTO _CaseCommentDTO);
        CaseCommentDTO GetDetails(int Id);
        ServiceResponse<int> Update(CaseCommentDTO _CaseCommentDTO);
        ServiceResponse<int> Delete(int Id);
        List<CaseCommentDTO> GetAll();
        List<CaseCommentDTO> GetAllByCase(int CaseId);
    }
}
