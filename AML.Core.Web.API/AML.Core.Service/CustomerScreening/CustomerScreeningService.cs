using AML.Core.Common.StaticResource;
using AML.Core.RepositoryContract.CustomerScreening;
using AML.Core.ServiceContract.CustomerScreening;
using AML.DTO.DTO.CustomerCase;
using AML.DTO.DTO.CustomerScreening;
using AML.DTO.DTO.Sanction;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.Core.Service.CustomerScreening
{

    public class CustomerScreeningService : BaseService, ICustomerScreeningService
    {
        ICustomerScreeningRepository _CustomerScreeningRepositoryRepository;
        public CustomerScreeningService(ICustomerScreeningRepository CustomerScreeningRepository, IConfiguration configuration,
            IHostingEnvironment environment) : base(CustomerScreeningRepository, configuration)
        {
            _CustomerScreeningRepositoryRepository = CustomerScreeningRepository;
        }

        //public CustomerScreeningRS ScreeningSearch(CustomerScreeningRQ _customerCaseDTO)
        //{
        //    throw new NotImplementedException();
        //}
        public int InsertSanctionScreeningLogs(SanctionScreeningLogDTO model)
        {
            return _CustomerScreeningRepositoryRepository.InsertSanctionScreeningLogs(model).Result;
        }

        public List<SanctionScreeningLogDTO> GetAllByDate(string startDate, string endDate,int clientId)
        {
            return _CustomerScreeningRepositoryRepository.GetAllByDate(startDate, endDate,clientId).Result;
        }
        public List<SanctionScreeningLogDTO> GetSanctionScreeningLogs(string Name, string Nationality, string DOB, string customerType, int ClientId)
        {
            return _CustomerScreeningRepositoryRepository.GetSanctionScreeningLogs(Name, Nationality,DOB,customerType,ClientId).Result;
        }

        public List<CustomerCaseDTO> GetCaseLogs(string Name, string Nationality, string DOB, string customerType, int ClientId)
        {
            return _CustomerScreeningRepositoryRepository.GetCaseLogs(Name, Nationality, DOB, customerType, ClientId).Result;
        }
    }
}
