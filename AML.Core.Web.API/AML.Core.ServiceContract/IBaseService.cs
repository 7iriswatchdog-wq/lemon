using System;
using System.Threading.Tasks;
using AML.Core.Common.StaticResource;
using System.Collections.Generic;
namespace AML.Core.ServiceContract
{
    public interface IBaseService
    {
        // <summary>
        /// Declares HealthCheck to be implemeted by the deriving services .
        /// </summary>
        /// <returns>HealthCheck Information</returns>
        Task<object> HealthCheckAsync();

    }

    public interface IBREADOperationService    {
        ServiceResponse<int> Create(object obj);
        ServiceResponse<int> Update(object obj);

        ServiceResponse<int> Delete(object obj);

        object GetDetails(object obj);

        List<object> GetAll(object obj);
    }
}
