using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CaseAssignment;
namespace AML.Core.RepositoryContract.CustomerCase
{
    public interface ICaseAssignmentRepository : IBaseRepository
    {
        ServiceResponse<int> Create(CaseAssignmentDTO _CaseAssignmentDTO);
        ServiceResponse<CaseAssignmentDTO> GetDetails(int Id);
        ServiceResponse<int> Update(CaseAssignmentDTO _CaseAssignmentDTO);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<CaseAssignmentDTO>> GetAll();
    }
}
