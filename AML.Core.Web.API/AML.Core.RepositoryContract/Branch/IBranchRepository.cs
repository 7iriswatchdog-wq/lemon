using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Branch;

namespace AML.Core.RepositoryContract.Branch
{
    public interface IBranchRepository : IBaseRepository
    {
        ServiceResponse<int> Create(BranchDTO _branchDTO);
        ServiceResponse<BranchDTO> GetDetails(int Id);
        ServiceResponse<int> Update(BranchDTO _branchDTO);
        ServiceResponse<int> Delete(int Id);
        ServiceResponse<List<BranchDTO>>  GetAll(int clientId);
    }
}
