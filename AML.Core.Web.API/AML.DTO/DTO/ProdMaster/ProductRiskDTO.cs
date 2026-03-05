using AML.DTO.DTO.Risk;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.ProdMaster
{
    public class ProductRiskDTO
    {
        [Column("id")]
        public int Id { get; set; }
        
        [Column("productcode")]
        public string ProductCode { get; set; }
        [Column("productname")]
        public string ProductName { get; set; }
        [Column("prod_final_risk_score")]

        public string FinalRiskScore { get; set; }
        [Column("prod_risk_score_sum")]
        public int ProdRiskScoreSum { get; set; }
        [Column("prod_risk_score_count")]
        public int ProdRiskScoreCount { get; set; }
        [Column("prod_risk_score_before_override")]
        public string RiskScoreBeforeOverride { get; set; }
        [Column("dateofassessment")]

        public DateTime DateofAssessment { get; set; }


        public List<ProdRiskTypeCategoryDTO> ProdRiskTypeCategoryDTO { get; set; }

        public List<ProdReportDataDTO> ProdReportDataDTO { get; set; }


        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("version")]
        public int version { get; set; }
    }
}
