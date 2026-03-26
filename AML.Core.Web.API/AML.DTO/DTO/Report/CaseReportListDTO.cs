using AML.Core.Common.StaticResource;
using AML.DTO.DTO.Kyc;
using AML.DTO.DTO.Risk;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Report
{
    public class CaseReportListDTO
    {
        [Column("cust_id")]
        public string CustomerID { get; set; }
        [Column("cust_type")]
        public string CustomerType { get; set; }
        [Column("customer_id_type")]
        public string CustomerIdType { get; set; }
        [Column("customer_name")]
        public string CustomerName { get; set; }
        [Column("fname")]
        public string FirstName { get; set; }
        [Column("mname")]
        public string MiddleName { get; set; }
        [Column("lname")]
        public string LastName { get; set; }
        [Column("status")]
        public string Status { get; set; }
        [Column("CaseStatus")]
        public string CaseStatus { get; set; }
        [Column("created_by")]
        public string CreatedBy { get; set; }
        [Column("created_on")]
        public string CreatedOn { get; set; }
        [Column("match")]
        public string Match { get; set; }
        [Column("is_matched")]
        public string IsMatched { get; set; }
        [Column("nationality")]
        public string Nationality { get; set;}
        [Column("Dob")]
        public string dob { get; set; }

        [Column("company_code")]
        public string CompanyCode { get; set; }

        [Column("dob")]
        public string DOB { get; set; }
      
        [Column("residence_status")]
        public string ResidenceStatus { get; set; }
        [Column("occupation_type")]
        public string OccupatinTypeTxt { get; set; }
        [Column("employer_name")]
        public string EmployerName { get; set; }
        [Column("employer_address")]
        public string EmployerAddress { get; set; }
        [Column("residence_address")]
        public string ResidenceAddress { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("mobile")]
        public string Mobile { get; set; }
        
        [Column("cust_id_number")]
        public string CustomerIdNumber { get; set; }
        [Column("threshold")]
        public int Threshold { get; set; }

        [Column("pep_status")]
        public string PEPStatus { get; set; }
        [Column("guardian_name")]
        public string GuardianName { get; set; }
        [Column("guardian_relation")]
        public string GuardianRelation { get; set; }
        [Column("remark")]
        public string Remarks { get; set; }
        public string IsPep { get; set; }
        [Column("individual_risk_score")]
        public string Individual_final_risk_score { get; set; }

        [Column("corporate_risk_score")]
        public string corporate_final_risk_score { get; set; }

        [Column("placeofbirth")]
        public string PlaceOfBirth { get; set; }
        [Column("martialstatus")]
        public string MaritalStatus { get; set; }

        [Column("idexpirydate")]
        public string IdExpdate { get; set; }

        [Column("bankaccnumber")]
        public string BankAccountNo { get; set; }
        [Column("bankaccname")]
        public string BankAccountName { get; set; }
        [Column("bankbranch")]
        public string BankAccountBranch { get; set; }
        [Column("sourceofincome")]
        public string Sourceofincome { get; set; }
        [Column("delivery_channel")]
        public string DeliveryChannelName { get; set; }
        [Column("product_type")]
        public string ProductName { get; set; }

        [Column("mode_of_payment")]
        public string Modeofpayment { get; set; }

        [Column("entity_type")]
        public string EntityTypeTxt { get; set; }

        [Column("corporate_website")]
        public string CorporateWebsite { get; set; }

        [Column("mobile")]
        public string Telephone { get; set; }


        [Column("vat_number")]
        public string VATRegistrationNumber { get; set; }

        [Column("fund_source")]
        public string FundSourceTxt { get; set; }

        [Column("business_type")]
        public string BusinessType { get; set; }

        [Column("country")]
        public string Country { get; set; }

        [Column("emirate")]
        public string Emirate { get; set; }

        [Column("city")]
        public string City { get; set; }

        [Column("po_box")]
        public string POBox { get; set; }

        [Column("cust_id_number")]
        public string LicenseNumber { get; set; }
        [Column("license_issue_date")]
        public string LicenseIssueDate { get; set; }
        [Column("license_issuing_authority")]
        public string LicenseIssuingAuthority { get; set; }
        [Column("license_expiry_date")]
        public string LicenseExpiryDate { get; set; }
        [Column("place_of_issue")]
        public string PlaceofIssue { get; set; }
        [Column("business_activity")]
        public string BusinessActivity { get; set; }
        [Column("license_type")]
        public string LicenseTypeTxt { get; set; }

        [Column("whitelisted_for_screening")]
        public string IsWhiteListed { get; set; }

        [Column("match_score")]
        public int MatchScore { get; set; }
        [Column("individual_risk_sum")]
        public string Individual_final_risk_sum { get; set; }

        [Column("corporate_risk_sum")]
        public string Corporate_final_risk_sum { get; set; }
        [Column("case_change_status")]
        public string CaseChangeStatus { get; set; }
        [Column("created_user")]
        public string CreatedUser { get; set; }

        [Column("updated_on")]
        public string UpdatedOn { get; set; }

        [Column("type")]
        public string Type { get; set; }

    }
    public class RiskDashboardDTO
    {
        public int category_id { get; set; }
        [Column("total_count")]
        public int total_count { get; set; }
        [Column("high_risk")]
        public int high_risk_count { get; set; }
        [Column("medium_risk")]
        public int medium_risk_count { get; set; }
        [Column("low_risk")]
        public int low_risk_count { get; set; }
    }


    public class clientSearchDTO 
    {
        [Column("search_count")]
        public int count { get; set; }    
    }
}