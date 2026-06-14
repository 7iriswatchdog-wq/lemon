using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CodesMaster;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.CodeMaster
{
    public interface ICodesMasterRepository : IBaseRepository
    {
        ServiceResponse<List<CodesTableDTO>> GetAllCodes(int CcClientId);
        ServiceResponse<CodesTableDTO> GetDetails(int Id, string Code, string Type);
        ServiceResponse<int> Create(CodesTableDTO _CodesTableDTO);
        ServiceResponse<int> Update(CodesTableDTO _CodesTableDTO);
        ServiceResponse<int> Delete(int Id, String code,string type);
    }
}
