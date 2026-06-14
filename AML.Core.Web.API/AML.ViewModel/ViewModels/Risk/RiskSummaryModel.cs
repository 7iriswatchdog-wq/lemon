using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RiskSummaryModel
    {
        public string RiskType { get; set; }
        public int HighRiskCount { get; set; }
        public int MediumRiskCount { get; set; }
        public int LowRiskCount { get; set; }
    }
}
