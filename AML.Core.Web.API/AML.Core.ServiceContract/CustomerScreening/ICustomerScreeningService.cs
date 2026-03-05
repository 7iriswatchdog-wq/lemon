using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Sanction;
using System.Collections.Generic;

namespace AML.Core.ServiceContract.CustomerScreening
{

    public interface ICustomerScreeningService : IBaseService
    {
        //ServiceResponse<int> Create(CustomerScreeningDTO _customerCaseDTO);
        //CustomerScreeningDTO GetDetails(int Id);
        //ServiceResponse<int> Update(CustomerScreeningDTO _customerCaseDTO);
        //ServiceResponse<int> Delete(int Id);
        //List<CustomerScreeningDTO> GetAll();

        // MongoDB search update return type
        //CustomerScreeningRS ScreeningSearch(CustomerScreeningRQ _customerCaseDTO);

        int InsertSanctionScreeningLogs(SanctionScreeningLogDTO model);
        List<SanctionScreeningLogDTO> GetAllByDate(string startDate, string endDate,int clientId);
    }
}
