using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RiskSummaryModelV2
    {
        public string RiskType { get; set; }
        public int HighRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int LowRiskCount { get; set; }
    }
}
