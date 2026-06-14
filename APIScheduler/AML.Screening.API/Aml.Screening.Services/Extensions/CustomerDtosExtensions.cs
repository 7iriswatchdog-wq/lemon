using Aml.Screening.DataContracts.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.Services.Extensions
{
    public static class CustomerDtosExtensions
    {
        public static CustomerScreenDto ToOnlineCustomerScreenDto(this CustomerDto modal)
        {
            try
            {
                CustomerScreenDto toScreen = new CustomerScreenDto
                {
                    CustomerCode = modal.CUSTOMERCODE ?? string.Empty,
                    CustomerType = modal.CUSTOMERCATEGORY ?? string.Empty,
                    CustomerFullName = modal.CUSTOMERFULLNAME.Trim() ?? string.Empty,
                    CustomerIdType = modal.CUSTOMERIDTYPE ?? string.Empty,
                    CustomerIdNumber = modal.CUSTOMERIDNUMBER.Trim() ?? string.Empty,
                    CustomerNationality = modal.CUSTOMERNATIONALITY ?? string.Empty,
                    CustomerDob = modal.CUSTOMERDOB ?? string.Empty,
                    AccountNumber = string.Empty,
                    CaseId = modal.CASEID,
                    Threshold = modal.THRESHOLD ?? 0,
                    ClientId = modal.CLIENTID,
                    WWhitelisting= modal.WHITELISTING,
                    WhitelistingDate = modal.WHITELISTINGDATE
                };
                return toScreen;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public static CustomerScreenDto ToOfflineCustomerScreenDto(this SearchDto modal)
        {
            try
            {
                CustomerScreenDto toScreen = new CustomerScreenDto
                {
                    CustomerCode = string.Empty,
                    CustomerType = modal.CUSTOMERTYPE,
                    CustomerFullName = modal.CUSTOMERFULLNAME.Trim() ?? string.Empty,
                    CustomerIdType = string.Empty,
                    CustomerIdNumber = string.Empty,
                    CustomerNationality = modal.CUSTOMERNATIONALITY,
                    CustomerDob = modal.CUSTOMERDOB ?? string.Empty,
                    AccountNumber = string.Empty,
                    Threshold = modal.THRESHOLD ?? 0,
                    ClientId = modal.CLIENTID
                };
                return toScreen;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }

    
}
