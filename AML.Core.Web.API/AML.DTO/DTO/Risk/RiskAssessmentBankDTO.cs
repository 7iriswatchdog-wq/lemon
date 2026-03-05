using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Risk
{
    public class RiskAssessmentBankDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("customername")]
        public string LegalNameOfTheEntity { get; set; }
        [Column("dateofassessment")]
        public DateTime EntityDate { get; set; }
        [Column("customercode")]
        public string UniqueIDNumber { get; set; }
        [Column("nationality")]
        public string CountryOfIncorporation { get; set; }
        [Column("nationality")]
        public string TxtCountryOfIncorporation { get; set; }


        [Column("final_risk_score")]
        public string RiskAssessmentRating { get; set; }
        [Column("risk_score_before_override")]
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        [Column("risk_sum")]
        public int RiskScoreSum { get; set; }
        [Column("risk_count")]
        public int RiskScoreCount { get; set; }
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
