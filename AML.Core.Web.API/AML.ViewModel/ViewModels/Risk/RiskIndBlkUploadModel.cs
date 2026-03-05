using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RiskIndBlkUploadModel
    {
        public IFormFile fileUpload { get; set; }
       
    }

    public class GetRisk
    {
        public string riskTypeName { get; set; }
        public string riskName { get; set; }
    }

    public class Root
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string RiskCategory { get; set; }
        public string MainNationality { get; set; }
        public List<GetRisk> Risks { get; set; }
    }
}
