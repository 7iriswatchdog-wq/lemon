using Aml.Screening.DataContracts.Dtos;
using Aml.Screening.MongoModal.Modal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Aml.Screening.ServiceContracts.ServiceContracts
{
    public interface ICustomerServices
    {
        Task<ServiceResponse<CASELOGMATCH>> ScreenCustomerAsync(CustomerDto customerDto);
    }
}
