using System.Collections.Generic;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Branch;

namespace AML.Core.ServiceContract.Branch
{
    public interface IBranchService : IBaseService
    {
        ServiceResponse<int> Create(BranchDTO _branchDTO);
        BranchDTO GetDetails(int Id);
        ServiceResponse<int> Update(BranchDTO _branchDTO);
        ServiceResponse<int> Delete(int Id);
        List<BranchDTO> GetAll(int clientId);
    }
}
