using AML.Core.Common.StaticResource;
using AML.DTO.DTO.TransactionMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using System.Text;

namespace AML.DTO.DTO.CustomerCase
{
    public class CustomerMasterDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("cust_ref_id")]
        public string CustomerReferenceID { get; set; }
        [Column("cust_id")]
        public string CustomerId { get; set; }
        [Column("fname")]
        public string FirstName { get; set; }
        [Column("mname")]
        public string MiddleName { get; set; }
        [Column("lname")]
        public string LastName { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        [Column("dob")]
        public DateTime DOB { get; set; }
        [Column("cust_type")]
        public string CustomerType { get; set; }
        [Column("cust_id_type")]
        public string CustomerIdType { get; set; }
        [Column("cust_id_number")]
        public string CustomerIdNumber { get; set; }
        [Column("mobile")]
        public string Mobile { get; set; }
        public string GroupEntityof { get; set; }
        public string CustDOB
        {
            get
            {
                return DOB.ToUIDDateFormat();
            }
            set
            {
                DOB = value.ParseDB().GetValueOrDefault();
            }
        }
        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_user")]
        public string CreatedUser { get; set; }
        [Column("updated_by")]
        public int UpdatedBy { get; set; }
        [Column("created_on")]
        public DateTime? CreatedOnDB { get; set; }
        public string CreatedOn
        {
            get
            {
                return CreatedOnDB.ToUIDDateFormat();
            }
            set
            {
                CreatedOnDB = value.ParseDB();
            }
        }
        [Column("updated_on")]
        public DateTime? UpdatedOnDB { get; set; }
        public string UpdatedOn
        {
            get
            {
                return UpdatedOnDB.ToUIDDateFormat();
            }
            set
            {
                UpdatedOnDB = value.ParseDB();
            }
        }
        [Column("batch_id")]
        public int Batch { get; set; }
        [Column("is_deleted")]
        public int IsDeleted { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("customer_final_risk_score")]
        public string CustomerFinalRiskScore { get; set; }
        [Column("customer_screen_match_score")]
        public int CustomerScreenMatchScore { get; set; }

        [Column("record_count")]
        public int RecordCount { get; set; }

        [Column("whitelisted_for_screening")]
        public string WhiteListed { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("user_name")]
        public string UserId { get; set; }
        [Column("Client_Name")]
        public string CompanyName { get; set; }

        [Column("c6threshold")]
        public int C6Threshold { get; set; }
        [Column("threshold")]
        public int Threshold { get; set; }

        [Column("type")]
        public string Type { get; set; }
        [Column("mode_of_payment")]
        public string Modeofpayment { get; set; }
        [Column("delivery_channel")]
        public string DeliveryChannelName { get; set; }
        [Column("product_name")]
        public string ProductName { get; set; }
        [Column("residence_status")]

        public string ResidenceStatus { get; set; }
        [Column("profession")]

        public string OccupatinTypeTxt { get; set; }
        [Column("legal_status")]
        public string EntityTypeTxt { get; set; }
        [Column("bussiness_type")]
        public string BusinessType { get; set; }
        public string Gender { get; set; }


        [Column("address")]
        public string Address { get; set; }
        [Column("establishment_date")]
        public string EstablishmentDate { get; set; }


        [Column("cust_ref_id")]
        public string onb_cust_ref_id { get; set; }
        [Column("lname")]
        public string onb_name { get; set; }
        [Column("dob")]
        public string onb_dob { get; set; }
        [Column("nationality")]
        public string onb_nationality { get; set; }
        [Column("cust_type")]
        public string onb_cust_type { get; set; }

        [Column("cust_id_type")]
        public string onb_cust_id_type { get; set; }
        [Column("cust_id_number")]
        public string onb_cust_id { get; set; }
        [Column("mobile")]

        public string onb_mobile { get; set; }
        [Column("created_by")]
        public int onb_created_by { get; set; }
        [Column("created_on")]
        public DateTime onb_created_on { get; set; }
        [Column("updated_by")]
        public int onb_updated_by { get; set; }
        [Column("emirates_id")]
        public string onb_emirates_id { get; set; }
        [Column("emirates_id_expiry")]
        public DateTime onb_emirates_id_expiry { get; set; }
        [Column("passport_expiry")]
        public DateTime onb_passport_expiry { get; set; }
        [Column("profession")]
        public string onb_profession { get; set; }
        [Column("residence_status")]
        public string onb_residence_status { get; set; }
        [Column("insurance_product_type")]
        public string onb_insurance_product { get; set; }
        [Column("delivery_channel")]
        public string onb_delv_channel { get; set; }
        [Column("fund_source")]
        public string onb_src_funds { get; set; }
        [Column("paymeny_mode")]
        public string onb_mode_of_pymt { get; set; }
        [Column("department")]
        public string onb_dept { get; set; }
        [Column("policy_no")]
        public string onb_policy_no { get; set; }
        [Column("endt_no")]
        public string onb_endt_no { get; set; }
        [Column("document_no")]
        public string onb_doc_no { get; set; }
        [Column("party_type")]
        public string onb_party_type { get; set; }
        [Column("subclass")]
        public string onb_subclass { get; set; }
        [Column("Client_Id")]
        public string onb_app_name { get; set; }
        [Column("cust_ref_id")]
        public string onb_customerid { get; set; }
        [Column("is_screened")]
        public string onb_is_screened { get; set; }
        [Column("threshold")]
        public int onb_threshold { get; set; }
        [Column("dateofassessment")]
        public string DateofAssessment { get; set; }



        [Column("company_code")]
        public string CompanyCode { get; set; }
        [Column("FullName")]
        public string FullName { get; set; }

        [Column("tradelicense")]
        public string Tradelicense { get; set; }

        public string customerCodeprefix { get; set; }

        [Column("whitelisted_for_screening")]
        public string IsWhiteListed { get; set; }

        [Column("cif_number")]
        public string CIFNumber { get; set; }

        [Column("parent_id")]
        public string ParentID { get; set; }

        [Column("screeningoption")]
        public string ScreeningOptions { get; set; }

        [Column("id_issue_date")]
        public DateTime? IdIssueDate { get; set; }
        [Column("id_expiry_date")]
        public DateTime? IdExpiryDate { get; set; }
        [Column("residence")]
        public string Residence { get; set; }

        [Column("sowsofcountry")]
        public string SOWSOFCountry { get; set; }
        [Column("employer")]

        public string Employer { get; set; }

        [Column("employerindustry")]
        public string EmployerIndustry { get; set; }
        [Column("employersector")]
        public string EmployerSector { get; set; }

        [Column("goldenvisa")]

        public string GoldenVisa { get; set; }
        [Column("tradelicenseauthority")]
        public string TradeLicenseAuthority { get; set; }
        [Column("tradelicensesector")]
        public string TradeLicenseSector { get; set; }
        [Column("countryparty")]
        public string CounterParty { get; set; }
        [Column("productrefno")]
        public string ProductRefNo { get; set; }
        [Column("productvalue")]
        public string ProductValue { get; set; }

        [Column("share")]
        public int Share { get; set; }
        [Column("designation")]
        public string Designation { get; set; }

        [Column("counterypartyname")]
        public string CounterPartyName { get; set; }


        [Column("relationship")]
        public string Relationship { get; set; }

        [Column("flagtype")]
        public string FlagType { get; set; }
    }
    public class CreatedAndUpdatedByNames
    {
        [Column("CreatedBy")]
        public string CreatedBy { get; set; }
        [Column("UpdatedBy")]
        public string UpdatedBy { get; set; }
    }
    public class CustomerCaseDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("cust_master_id")]
        public int CustomerMasterId { get; set; }
        [Column("uid")]
        public string UID { get; set; }
        [Column("rollbackCount")]
        public int RollbackCount { get; set; }

        [Column("cust_id")]
        public string CustomerId { get; set; }
        [Column("fname")]
        public string FirstName { get; set; }
        [Column("mname")]
        public string MiddleName { get; set; }
        [Column("lname")]
        public string LastName { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        [Column("nationality_name")]
        public string NationalityName { get; set; }
        [Column("comments")]
        public string Comments { get; set; }
        [Column("dob")]
        public DateTime DOB { get; set; }
        [Column("customer_id_type")]
        public string CustomerIdType { get; set; }
        [Column("customer_id_number")]
        public string CustomerIdNumber { get; set; }
        [Column("mobile")]
        public string Mobile { get; set; }

        public string CustDOB
        {
            get
            {
                return DOB.ToUIDDateFormat();
            }
            set
            {
                DOB = value.ParseDB().GetValueOrDefault();
            }
        }
        [Column("is_delete")]
        public int IsDelete { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("CaseStatus")]
        public string CaseStatus { get; set; }
        [Column("batch_id")]
        public int Batch { get; set; }
        [Column("match_score")]
        public int MatchScore { get; set; }
        [Column("source")]
        public string Source { get; set; }
        [Column("cust_type")]
        public string CustomerType { get; set; }
        [Column("match_category")]
        public string MatchCategory { get; set; }
        [Column("match_type")]
        public string MatchType { get; set; }
        [Column("source_unique_id")]
        public string SourceUniqueId { get; set; }
        [Column("risk_score")]
        public int RiskScore { get; set; }
        [Column("owner")]
        public int Owner { get; set; }
        [Column("is_matched")]
        public int IsMatched { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_user")]
        public string CreatedUser { get; set; }
        [Column("updated_by")]
        public int UpdatedBy { get; set; }
        [Column("updated_user")]
        public string UpdatedUser { get; set; }
        [Column("UserEmail")]
        public string UserEmail { get; set; }

        [Column("created_on")]
        public string CreatedOn { get; set; }
        [Column("cif_number")]
        public string CIFNumber { get; set; }
        [Column("id_issue_date")]
        public DateTime? IdIssueDate { get; set; }
        [Column("id_expiry_date")]
        public DateTime? IdExpiryDate { get; set; }

        [Column("dateofwhitelisting")]
        public DateTime?  DateOfWhitelisting { get; set; }
        //[Column("created_on")]
        //public DateTime? CreatedOnDB { get; set; }
        //public string CreatedOn
        //{
        //    get
        //    {
        //        return CreatedOnDB.ToString();



        //    }
        //    set
        //    {
        //        CreatedOnDB = value.ParseDB();
        //    }
        //}
        [Column("updated_on")]
        public DateTime? UpdatedOnDB { get; set; }
        public string UpdatedOn
        {
            get
            {
                return UpdatedOnDB.ToUIDDateFormat();
            }
            set
            {
                UpdatedOnDB = value.ParseDB();
            }
        }

        [Column("whitelisted_for_screening")]
        public string IsWhiteListed { get; set; }
        public List<ApiResultModel> ApiResults { get; set; }
        public List<ApiResultModel> ApiResultsjson { get; set; }
        public List<ApiResultModel> ApiResultsjsonCorp { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("user_name")]
        public string UserId { get; set; }
        [Column("Client_Name")]
        public string CompanyName { get; set; }
        public int sendMail { get; set; }
        [Column("c6threshold")]
        public int C6Threshold { get; set; }
        [Column("threshold")]
        public int Threshold { get; set; }

        [Column("customer_type")]
        public string CorporateType { get; set; }
        [Column("group_entity_for")]

        public string GroupEntityof { get; set; }
        [Column("gender")]
        public string Gender { get; set; }
        [Column("birthyear")]
        public string BirthYear { get; set; }

        public bool IsPep { get; set; } = false;
        public bool IsSan { get; set; } = false;
        public bool IsRre { get; set; } = false;
        public bool IsIns { get; set; } = false;
        public bool IsDd { get; set; } = false;
        public bool IsPoi { get; set; } = false;
        public bool IsRel { get; set; } = false;
        [Column("customercode")]
        public string customercode { get; set; }
        [Column("individual_risk_score")] 
        public string Individual_final_risk_score { get; set; }

        [Column("corporate_risk_score")]
        public string corporate_final_risk_score { get; set; }
        [Column("individual_risk_sum")]
        public string Individual_final_risk_sum { get; set; }

        [Column("corporate_risk_sum")]
        public string Corporate_final_risk_sum { get; set; }

        [Column("individual_risk_override")]
        public string Individual_Risk_Override { get; set; }

        [Column("corporate_risk_override")]
        public string Corporate_Risk_Override { get; set; }
        [Column("no_match")]
        public int NoMatch { get; set; }
        [Column("true_dometic_pep")]
        public int TrueDomesticpep { get; set; }
        [Column("true_foreign_pep")]
        public int TrueForeignpep { get; set; }
        [Column("true_adversemedia")]
        public int TrueAdverseMedia { get; set; }
        [Column("partial_domestic_pep")]
        public int PartialDomesticpep { get; set; }
        [Column("partial_foreign_pep")]
        public int PartialForeignpep { get; set; }
        [Column("partial_adversemedia")]
        public int Partialadversemedia { get; set; }
        [Column("true_uae_un_sanction")]
        public int TrueUAEUNSanction { get; set; }
        [Column("true_other_sanction")]
        public int TrueOtherSanction { get; set; }
        [Column("address")]
        public string Address { get; set; }
        [Column("establishment_date")]
        public string EstablishmentDate { get; set; }

        [Column("type")]
        public string Type { get; set; }
        [Column("mode_of_payment")]
        public string Modeofpayment { get; set; }
        [Column("delivery_channel")]
        public string DeliveryChannelName { get; set; }
        [Column("product_name")]
        public string ProductName { get; set; }
        [Column("residence_status")]

        public string ResidenceStatus { get; set; }
        [Column("profession")]

        public string OccupatinTypeTxt { get; set; }
        [Column("legal_status")]
        public string EntityTypeTxt { get; set; }
        [Column("bussiness_type")]
        public string BusinessType { get; set; }

        [Column("parent_id")]
        public string ParentID { get; set; }

        [Column("case_change_status")]
        public string CaseChangeStatus { get; set; }
        [Column("screeningoption")]
        public string ScreeningOptions { get; set; }

        [Column("residence")]

        public string Residence { get; set; }
        [Column("sowsofcountry")]
        public string SOWSOFCountry { get; set; }

        [Column("employer")]

        public string Employer { get; set; }
        [Column("employerindustry")]
        public string EmployerIndustry { get; set; }
        [Column("employersector")]
        public string EmployerSector { get; set; }
        [Column("goldenvisa")]

        public string GoldenVisa { get; set; }
        [Column("customer_screen_match_score")]
        public int CustomerScreenMatchScore { get; set; }

        [Column("schedulertrackerId")]
        public string ScheduelerTrackerId { get; set; }

        [Column("schedulerprocessedon")]
        public string SchedulerProcessedOn { get; set; }

        
        public string onb_cust_ref_id { get; set; }

        public string onb_name { get; set; }
        public string onb_dob { get; set; }
        public string onb_nationality { get; set; }
        public string onb_cust_type { get; set; }
        public string onb_cust_id_type { get; set; }
        public string onb_cust_id { get; set; }
        public string onb_mobile { get; set; }
        public string  onb_created_by { get; set; }
        public DateTime onb_created_on { get; set; }
        public string onb_updated_by { get; set; }
        public string onb_emirates_id { get; set; }
        public DateTime onb_emirates_id_expiry { get; set; }
        public DateTime onb_passport_expiry { get; set; }
        public string onb_profession { get; set; }
        public string onb_residence_status { get; set; }
        public string onb_insurance_product { get; set; }
        public string onb_delv_channel { get; set; }
        public string onb_src_funds { get; set; }
        public string onb_mode_of_pymt { get; set; }
        public string onb_dept { get; set; }
        public string onb_policy_no { get; set; }
        public string onb_endt_no { get; set; }
        public string onb_doc_no { get; set; }
        public string onb_party_type { get; set; }
        public string onb_subclass { get; set; }
        public string onb_app_name { get; set; }
        public string onb_customerid { get; set; }
        public string onb_is_screened { get; set; }
        public int onb_threshold { get; set; }

        public string FinalRiskScore { get; set; }
        public string TotalScore { get; set; }
       

        [Column("company_code")]
        public string CompanyCode { get; set; }

        [Column("tradelicense")]
        public string Tradelicense { get; set; }

        [Column("customerCodeprefix")]
        public string customerCodeprefix { get; set; }

        [Column("tradelicenseauthority")]
        public string TradeLicenseAuthority { get; set; }
        [Column("tradelicensesector")]
        public string TradeLicenseSector { get; set; }
        [Column("countryparty")]
        public string CounterParty { get; set; }
        [Column("productrefno")]
        public string ProductRefNo { get; set; }
        [Column("productvalue")]
        public string ProductValue { get; set; }

        [Column("share")]
        public int Share { get; set; }
        [Column("designation")]
        public string Designation { get; set; }
        [Column("counterypartyname")]
        public string CounterPartyName { get; set; }

        [Column("relationship")]
        public string Relationship { get; set; }

        [Column("flagtype")]
        public string FlagType { get; set; }

    }

    
    public class ETLDataLoadReportDTO
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Customer { get; set; }
        public int MatchType { get; set; }
        public int ClientId { get; set; }
        public string Customers { get; set; }
    }
    public class ApiResultModel
    {
        public bool isAdverse { get; set; }
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchuid { get; set; }
        public string matchcategory { get; set; }
        public string matchtype { get; set; }
        public string nationality { get; set; }
        public string matchidnumber { get; set; }
        public string matchdob { get; set; }
        public List<string> alias { get; set; }

        public string matchresourcesid { get; set; }

        public string matchdatasets { get; set; }

        public string matchgender { get; set; }
    }
    public class CaseDetailRequestDTO
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Cust_type { get; set; }
        public string User { get; set; }

        public int UserID { get; set; }
        public string Status { get; set; }
    }
    public class MainDashboard
    {
        public CustomerMasterDTO customerMasterDTO { get; set; }
        public TMSCaseDTO tMSCaseDTO { get; set; }
    }

    public class PassportDetailsDTO
    {
        [Column("case_id")]
        public string CaseId { get; set; }
        [Column("passport_No")]
        public string PassportNo { get; set; }
        [Column("passport_issue_place")]
        public string PassportIssusePlace { get; set; }
        [Column("passport_issue_date")]
        public string PassportIssuesDate { get; set; }
        [Column("passport_expiry_date")]
        public string PassportExpiryDate { get; set; }
        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("client_id")]
        public int ClientId { get; set; }


    }
    public class ShareholderDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("fullname")]
        public string Name { get; set; }
        [Column("share")]
        public int Share { get; set; }
        [Column("designation")]
        public string Designation { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        //[Column("client_id")]
        //public string Residence { get; set; }
        [Column("tradelicense")]
        public string TradeLicence { get; set; }
        [Column("registrationdate")]
        public DateTime? RegistrationDate { get; set; }
        
        [Column("idtype")]
        public string IdType { get; set; }
        [Column("idnumber")]
        public string IdNumber { get; set; }
        [Column("issuedate")]
        public DateTime? IssueDate { get; set; }
        [Column("expirydate")]
        public DateTime? IdExpiry { get; set; }
        [Column("cif")]
        public string Cif { get; set; }
        [Column("type")]
        public string Type { get; set; }   // ? Important
        [Column("thershold")]
        public int Thershold { get; set; }
        [Column("client_id")]
        public int ClientId { get; set; }
        [Column("createdby")]
        public int UserId { get; set; }
        [Column("companycode")]
        public string CompanyCode { get; set; }

        [Column("companyname")]
        public string CompanyName { get; set; }

        [Column("document_full_path")]
        public string DocFullPath { get; set; }
        [Column("document_file_name")]
        public string DocumentFileName { get; set; }

        [Column("residence")]
        public string Residence { get; set; }
        [Column("employer")]
        public string Employer { get; set; }
        [Column("goldenvisa")]
        public string GoldenVisa { get; set; }

        [Column("cust_type")]
        public string CustType { get; set; }

        [Column("employerindustry")]
        public string EmployerIndustry { get; set; }
        [Column("employersector")]
        public string EmployerSector { get; set; }

        [Column("sowsofcountry")]
        public string SOWSOFCountry { get; set; }

        [Column("tradelicenseauthority")]
        public string TradeLicenseAuthority { get; set; }
        [Column("tradelicensesector")]
        public string TradeLicenseSector { get; set; }

        [Column("gender")]
        public string Gender { get; set; }

        [Column("relationship")]
        public string Relationship { get; set; }

        [Column("flagtype")]
        public string FlagType { get; set; }


    }

    
}
