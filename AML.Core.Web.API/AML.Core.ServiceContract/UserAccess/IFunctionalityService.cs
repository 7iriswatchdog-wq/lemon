using AML.Core.Common.StaticResource;
using AML.DTO.DTO.UserAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.UserAccess
{
    public interface IFunctionalityService: IBaseService
    {
        ServiceResponse<List<FunctionalityDTO>> GetAll(int clientId);
        ServiceResponse<FunctionalityDTO> GetDetailsByModuleId(int modId);
        ServiceResponse<FunctionalityDTO> GetDetails(int Id);
    }
}
