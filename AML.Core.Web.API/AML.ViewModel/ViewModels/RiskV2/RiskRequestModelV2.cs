using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RiskRequestModelV2: RiskReportRequestModelV2
    {
        public DateTime OnDate { get; set; }
    }

    public class RiskReportRequestModelV2
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string RiskType { get; set; }
        public bool WithDuplicate { get; set; }
        public int RiskLevel { get; set; }
        public string userID { get; set; }
        public SelectList Users { get; set; }
       
    }
}
