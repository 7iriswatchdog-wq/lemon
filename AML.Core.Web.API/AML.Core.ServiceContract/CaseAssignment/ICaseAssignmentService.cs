using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CaseAssignment;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.CaseAssignment
{
    public interface ICaseAssignmentService : IBaseService
    {
        ServiceResponse<int> Create(CaseAssignmentDTO _CaseAssignmentDTO);
        CaseAssignmentDTO GetDetails(int Id);
        ServiceResponse<int> Update(CaseAssignmentDTO _CaseAssignmentDTO);
        ServiceResponse<int> Delete(int Id);
        List<CaseAssignmentDTO> GetAll();
    }
}
