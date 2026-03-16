using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CustomerCase
{
    public class CorporateScreeningDTO
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseName { get; set; }
        public string Mobile { get; set; }
        public string AccomplishedCountry { get; set; }
        public string DOB { get; set; }
        public string Nationality { get; set; }
        public List<CustomeDetailsDTO> CustomerDetailList { get; set; }
        public int CreatedBy { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }
        public string CorporateType { get; set; }
        public string GroupEntityOf { get; set; }
        public bool IsPep { get; set; }
        public bool IsSan { get; set; }
        public bool IsRre { get; set; }
        public bool IsIns { get; set; }
        public bool IsDd { get; set; }
        public bool IsPoi { get; set; }
        public bool IsRel { get; set; }
        public string CustomerType { get; set; }

        public int ClientId { get; set; }

        public string CustomerCode { get; set; }

        public string MobileNo { get; set; }

        public DateTime AccomplishedDate { get; set; }

        public string CompanyName { get; set; }

        public string CompanyCode { get; set; }

        public string Tradelicense { get; set; }

        public string customerCodeprefix { get; set; }

        public string Type { get; set; }

        
        public string Modeofpayment { get; set; }
        
        public string DeliveryChannelName { get; set; }
       
        public string ProductName { get; set; }
        

        public string ResidenceStatus { get; set; }
        

        public string OccupatinTypeTxt { get; set; }
     
        public string EntityTypeTxt { get; set; }
        
        public string BusinessType { get; set; }

        public string CIFNumber { get; set; }

        public string ScreeningOptions { get; set; }

        public DateTime? IdExpiryDate { get; set; }

        public string Residence { get; set; }

        public string TradeLicenseAuthority { get; set; }

        public string TradeLicenseSector { get; set; }

        public string CounterParty { get; set; }

        public string ProductType { get; set; }

        public string ProductValue { get; set; }

    }

    public class CustomeDetailsDTO
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string MatchCategory { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerIdNumber { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }
        public string Mobile { get; set; }
        public string CreatedOn { get; set; }
        public string SharePercent { get; set; }
        public bool isDeleted { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseName { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }
        public string Designation { get; set; }
        public string CustomerType { get; set; }
        public string EmiratesIdNumber { get; set; }
        public string EmiratesIdExpiry { get; set; }
        public string CustomerIdExpiry { get; set; }

    }
}
