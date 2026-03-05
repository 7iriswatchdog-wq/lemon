using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RiskReportModelV2
    {
        public int Id { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string RiskType { get; set; }
        public string FinalScore { get; set; }
        public string ScoreBeforeOverride { get; set; }
        public string AssessmentVersion { get; set; }
        public string User { get; set; }

        public string Remarks {get;set;}

    }
}
