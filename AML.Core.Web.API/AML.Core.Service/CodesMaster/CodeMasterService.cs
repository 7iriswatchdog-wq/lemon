using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CodeMaster;
using AML.Core.ServiceContract.CodeMaster;
using AML.DTO.DTO.Branch;
using AML.DTO.DTO.CodesMaster;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.CodesMaster
{
    public class CodeMasterService : BaseService, ICodeMasterService
    {
        ICodesMasterRepository _codeMasterRepository;
        public CodeMasterService(IConfiguration configuration, ICodesMasterRepository codeMasterRepository) : base(configuration)
        {
            _codeMasterRepository = codeMasterRepository;
        }

        public List<CodesTableDTO> GetAllCodes(int CcClientId)
        {
            return _codeMasterRepository.GetAllCodes(CcClientId).Result;
        }

        public CodesTableDTO GetDetails(int Id, string Code, string Type)
        {
            //Perform business requirements here
            return _codeMasterRepository.GetDetails(Id, Code, Type).Result;
        }
        public ServiceResponse<int> Create(CodesTableDTO _CodesTableDTO)
        {
            return _codeMasterRepository.Create(_CodesTableDTO);
        }
        public ServiceResponse<int> Update(CodesTableDTO _CodesTableDTO)
        {
            return _codeMasterRepository.Update(_CodesTableDTO);
        }

        public ServiceResponse<int> Delete(int Id, string code, string type)
        {
            return _codeMasterRepository.Delete(Id,code,type);
        }
    }
}
