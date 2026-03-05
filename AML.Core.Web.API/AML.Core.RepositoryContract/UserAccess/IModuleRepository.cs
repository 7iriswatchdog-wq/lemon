using System;
using System.Collections.Generic;
using System.Text;
using AML.Core.Common.StaticResource;
using AML.DTO.DTO.UserAccess;

namespace AML.Core.RepositoryContract.UserAccess
{
    public interface IModuleRepository : IBaseRepository
    {
        // System Master Details Not permitted to perform any Insert, Update or Delete from Application
        ServiceResponse<List<ModuleDTO>> GetAll();
    }
}
