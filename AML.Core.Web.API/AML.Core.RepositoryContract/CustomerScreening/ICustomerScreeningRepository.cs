using AML.Core.Common.StaticResource;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Sanction;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.RepositoryContract.CustomerScreening
{
    public interface ICustomerScreeningRepository : IBaseRepository
    {
        //CustomerScreeningRS ScreeningSearch(CustomerScreeningRQ _customerCase);
        ServiceResponse<int> InsertSanctionScreeningLogs(SanctionScreeningLogDTO model);
        ServiceResponse<List<SanctionScreeningLogDTO>> GetAllByDate(string startDate, string endDate,int clientId);

        ServiceResponse<List<SanctionScreeningLogDTO>> GetSanctionScreeningLogs(string Name, string Nationality, string DOB, string customerType, int ClientId);

        ServiceResponse<List<CustomerCaseDTO>> GetCaseLogs(string Name, string Nationality, string DOB, string customerType, int ClientId);

    }
    
}
