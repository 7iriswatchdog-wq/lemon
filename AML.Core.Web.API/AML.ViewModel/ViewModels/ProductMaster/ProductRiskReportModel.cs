using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.ProductMaster
{
    public class ProductRiskReportModel
    {
        public int Id { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string FinalScore { get; set; }
        public string ScoreBeforeOverride { get; set; }
        public string AssessmentVersion { get; set; }
        public string User { get; set; }
    }
}
