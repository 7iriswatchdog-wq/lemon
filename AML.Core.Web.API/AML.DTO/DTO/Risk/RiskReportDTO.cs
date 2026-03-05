using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Risk
{
    public class RiskReportDTO
    {
        [Column("s_no")]
        public int Id { get; set; }
        [Column("customercode")]
        public string CustomerCode { get; set; }
        [Column("customername")]
        public string CustomerName { get; set; }
        [Column("risk_type")]
        public string RiskType { get; set; }
        [Column("final_score")]
        public string FinalScore { get; set; }
        [Column("score_before_override")]
        public string ScoreBeforeOverride { get; set; }
        [Column("assessment_version")]
        public string AssessmentVersion { get; set; }
        [Column("created_by")]
        public string User { get; set; }
        [Column("remarks")]
        public string Remarks { get; set; }

        [Column("dateofassessment")]
        public DateTime DateofAssessment { get; set; }

    }

    public class RiskExcelReportDTO
    {
        [Column("customercode")]
        public string CustomerCode { get; set; }
        [Column("customername")]
        public string CustomerName { get; set; }
        [Column("dateofassessment")]
        public string DateOfAssessment { get; set; }
        [Column("risk_score_sum")]
        public string RiskScoreSum { get; set; }
        [Column("risk_type")]
        public string RiskType { get; set; }
        [Column("assessment_version")]
        public string RiskAssessmentVersion { get; set; }
        [Column("risk_score_count")]
        public string RiskScoreCount { get; set; }
        [Column("risk_score_before_override")]
        public string RiskScoreBeforeOverride { get; set; }
        [Column("final_risk_score")]
        public string FinalRiskScore { get; set; }
        [Column("Customer_nationality")]
        public string CustomerNationality { get; set; }
        [Column("lov_type_name")]
        public string LovTypeName { get; set; }
        [Column("lov_risk_data")]
        public string LovRiskData { get; set; }
        [Column("lov_risk_score")]
        public string LovRiskScore { get; set; }
        [Column("Over_ride_Score")]
        public string OverRideScore { get; set; }
        [Column("remarks")]
        public string Remarks { get; set; }
    }
}
