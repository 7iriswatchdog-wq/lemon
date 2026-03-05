using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CodesMaster;
using System;
using System.Collections.Generic;
using System.Text;


namespace AML.Core.ServiceContract.CodeMaster
{
    public interface ICodeMasterService : IBaseService
    {
        List<CodesTableDTO> GetAllCodes(int CcClientId);
        CodesTableDTO GetDetails(int Id, string Code, string Type);
        ServiceResponse<int> Create(CodesTableDTO _CodesTableDTO);
        ServiceResponse<int> Update(CodesTableDTO _CodesTableDTO);
        ServiceResponse<int> Delete(int id, string code, string type);
    }
}
