using System;
using System.Collections.Generic;
using System.Text;

namespace Aml.Screening.DataContracts.Dtos
{
    public class ServiceResponse
    {
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public string CaseId { get; set; }
    }

    public class ServiceResponse<T>
    {
        public string CaseId { get; set; }
        public string RiskScore { get; set; }
        public string RiskStatus { get; set; }
        public string ResponseCode { get; set; }
        public string ResponseMessage { get; set; }
        public T Data { get; set; }
    }
}
