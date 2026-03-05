using AML.Core.Common.StaticResource;
using AML.DTO.DTO.UserAccess;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.ServiceContract.UserAccess
{
    public interface IModuleService: IBaseService
    {
        List<ModuleDTO> GetAll();
    }
}
