using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Risk
{
    public class RiskCorpCustomerDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("customername")]
        public string LegalNameOfEntity { get; set; }
        [Column("customercode")]
        public string UniqueID { get; set; }
        [Column("dateofassessment")]
        public DateTime DateofAssessment { get; set; }
        public int CountryOfIncorporation { get; set; }
        [Column("Customer_nationality")]
        public string CountryOfIncorporationTxt { get; set; }


        [Column("final_risk_score")]
        public string RiskAssessmentRating { get; set; }
        [Column("risk_score_before_override")]
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        [Column("risk_sum")]
        public int RiskScoreSum { get; set; }
        [Column("risk_count")]
        public int RiskScoreCount { get; set; }

        public string IsWhiteListed { get; set; }

        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTO> ReportDataDTO { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("version")]
        public int version { get; set; }
        [Column("remarks")]
        public string Remarks { get; set; }

        [Column("riskoverride")]
        public string RiskOverRide { get; set; }
        [Column("type")]
        public string Type { get; set; }

    }
}
