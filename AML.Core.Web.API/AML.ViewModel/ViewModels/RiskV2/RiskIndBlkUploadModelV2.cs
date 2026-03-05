using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RiskIndBlkUploadModelV2
    {
        public IFormFile fileUpload { get; set; }
       
    }

    public class GetRiskV2
    {
        public string riskTypeName { get; set; }
        public string riskName { get; set; }
    }

    public class RootV2
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string RiskCategory { get; set; }
        public string MainNationality { get; set; }
        public List<GetRiskV2> Risks { get; set; }
    }
}
