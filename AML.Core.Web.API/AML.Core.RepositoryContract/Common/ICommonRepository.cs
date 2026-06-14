using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.Common
{
    public interface ICommonRepository : IBaseRepository
    {
        string EncryptionString(string strValue);
        string DecryptionString(string strValue);
        List<ApplicationSetupDTO> GetConfigurationDetailsByGroup(string setupGroup,int clientId);
        ServiceResponse<string> createErrorlog(ErrorLogDTO errorlog);
    }
}
