using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Risk
{
    public class RiskDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("customercode")]
        public string CustomerCode { get; set; }
        [Column("customername")]
        public string CustomerName { get; set; }
        [Column("dateofassessment")]
        public DateTime DateofAssessment { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("final_risk_score")]
        public string FinalRiskScore { get; set; }
        [Column("risk_score_sum")]
        public float RiskScoreSum { get; set; }
        [Column("risk_score_count")]
        public int RiskScoreCount { get; set; }
        [Column("risk_score_before_override")]
        public string RiskScoreBeforeOverride { get; set; }

        [Column("Customer_nationality")]
        public string MainNationalityTxt { get; set; }
        public string IsWhiteListed { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<ReportDataDTO> ReportDataDTO { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("version")]
        public int version { get; set; }


        public string CustomerType { get; set; }
        [Column("remarks")] 
        public string Remarks { get; set; }
        [Column("riskoverride")]
        public string RiskOverRide { get; set; }
        [Column("type")]
        public string Type { get; set; }

        [Column("product_reference")]
        public string ProductReference { get; set; }
        [Column("product_value")]
        public decimal? ProductValue { get; set; }
        [Column("comments")]
        public string RiskComments { get; set; }

    }




    public class RiskBulkDTO
        {

        [Column("id")]
        public int Id { get; set; }

        [Column("customercode")]
        public string CustomerCode { get; set; }
        [Column("customername")]
        public string CustomerName { get; set; }
        [Column("Customer_nationality")]
        public string Nationality { get; set; }
        public string CustomerType { get; set; }
        public string Profession { get; set; }
        public string ResidenceType { get; set; }
        public string Product { get; set; }
        public string DeliveryChannel { get; set; }
        public string SourceOfFund { get; set; }
        public string ModeOfPayment { get; set; }
        public string Screened { get; set; }
        public string IsAdverse { get; set; }
        public string MoreProduct { get; set; }
        public string FATF { get; set; }
        public string Nat1 { get; set; }
        public string Nat2 { get; set; }
        public string Nat3 { get; set; }

        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("version")]
        public int version { get; set; }




       
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
        public string val { get; set; }
       
    }
}
