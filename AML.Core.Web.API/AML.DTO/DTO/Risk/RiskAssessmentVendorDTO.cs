using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Risk
{
    public class RiskAssessmentVendorDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("customername")]
        public string VendorName { get; set; }
        [Column("dateofassessment")]
        public DateTime RegisteredDate { get; set; }
        [Column("customercode")]
        public string RegistrationNumber { get; set; }
        [Column("nationality")]
        public string Country { get; set; }
        [Column("nationality")]
        public string CountryCode { get; set; }

        //KYC Doc
        
     
        [Column("final_risk_score")]
        public string RiskAssessmentRating { get; set; }
        [Column("risk_score_before_override")]
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        [Column("risk_sum")]
        public int RiskScoreSum { get; set; }
        [Column("risk_count")]
        public int RiskScoreCount { get; set; }
        public string TransactionMonitoringOverride { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTO> ReportDataDTO { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("version")]
        public int version { get; set; }
    }
}
