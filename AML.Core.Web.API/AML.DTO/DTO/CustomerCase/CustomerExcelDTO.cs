using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CustomerCase
{
    public class CustomerExcelDTO
    {
        public string CustomerID { get; set; }
        public string Firstname { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }

        public string CustomerIdType { get; set; }
        
        public string CustomerIdNumber { get; set; }

        public string IDexpiry { get; set; }

        public string Remarks { get; set; }
        public int Status { get; set; }
        public int Id { get; set; }

        public string Modeofpayment { get; set; }

        public string DeliveryChannelName { get; set; }
        public string ProductName { get; set; }

        public string ResidenceStatus { get; set; }

        public string OccupatinTypeTxt { get; set; }
    }
    public class CorporateExcelDTO
    {
        public string CustomerID { get; set; }
        public string EntityName { get; set; }
        public string CountryofIncorporation { get; set; }
        public string DateofIncorporation { get; set; }
        public int Status { get; set; }
        public int Id { get; set; }

        public string Modeofpayment { get; set; }

        public string DeliveryChannelName { get; set; }
        public string ProductName { get; set; }

        public string EntityTypeTxt { get; set; }

        public string BusinessType { get; set; }
    }
    public class IndividualExcel
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }
        public string DOB { get; set; }
        public string Nationality { get; set; }
        public string CustomerType { get; set; }
        public string Profession { get; set; }
        public string ResidenceStatus { get; set; }
        public string Product { get; set; }
        public string DeliveryChannel { get; set; }
        public string SourceOfFund { get; set; }
        public string ModeOfPayment { get; set; }
        public string Screened { get; set; }

        public string IsAdverse { get; set; }
        public string MoreProduct { get; set; }


        public string FinalRiskScore { get; set; }
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
        public string RiskScoreBeforeOverride { get; set; }
      
        public string RiskScreeningAndFindingOverride { get; set; }
       
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
        public string risk { get; set; }
    }
    public class CorporateExcel 
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string LegalNameOfEntity { get; set; }
        public string UniqueID { get; set; }
        public DateTime DateofAssessment { get; set; }
        public string CustomerType { get; set; }
        public int CountryOfIncorporation { get; set; }
        public string CountryOfIncorporationTxt { get; set; }
        public string Profession { get; set; }
        public string ResidenceStatus { get; set; }
        public string Product { get; set; }
        public string DeliveryChannel { get; set; }
        public string SourceOfFund { get; set; }
        public string ModeOfPayment { get; set; }
        public string Screened { get; set; }
        public string IsAdverse { get; set; }
        public string MoreProduct { get; set; }
        public string IsWhiteListed { get; set; }
        public string RiskAssessmentRating { get; set; }
        public string RiskAssessmentRatingWithoutOverride { get; set; }
        public int RiskScoreSum { get; set; }
        public int RiskScoreCount { get; set; }
     
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }
        public int CreatedBy { get; set; }
        public int version { get; set; }
        public string risk { get; set; }
        public string FATF { get; set; }
        public string Nat1 { get; set; }
        public string Nat2 { get; set; }
        public string Nat3 { get; set; }

    }
}
