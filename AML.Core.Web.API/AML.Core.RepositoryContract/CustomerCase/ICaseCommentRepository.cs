using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CaseComment;
namespace AML.Core.RepositoryContract.CustomerCase
{
    public interface ICaseCommentRepository : IBaseRepository
    {
        ServiceResponse<int> Create(CaseCommentDTO _CaseCommentDTO);

        ServiceResponse<int> CreateProliferationCaseComments(CaseCommentDTO _CaseCommentDTO);
        ServiceResponse<CaseCommentDTO> GetDetails(int Id);
        ServiceResponse<int> Update(CaseCommentDTO _CaseCommentDTO);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<CaseCommentDTO>> GetAll();
        ServiceResponse<List<CaseCommentDTO>> GetAllByCase(int CaseId);

        ServiceResponse<List<CaseCommentDTO>> GetAllByCustomerId(string CustomerId);
        ServiceResponse<List<CaseCommentDTO>> GetAllProliferationByCase(int CaseId);
    }
}
