using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.Corporate;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.ViewModel.ViewModels.CustomerCase
{
    public class CaseModel
    {
        public int Id { get; set; }
        //    [Required(ErrorMessage = "Customer Id required")]
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        //[Required(ErrorMessage = "Nationality required")]
        public string Nationality { get; set; }
		//[Required(ErrorMessage = "Date of Birth required")]

		//[Display(Name = "Date of Birth")]
		//[RegularExpression(@"\d{2}/\d{2}/\d{4}", ErrorMessage = "Please enter a valid date in the format dd/mm/yyyy.")]
		public string DOB { get; set; }
        //public int SourceId { get; set; }
        public int IsDelete { get; set; }
        public int CreatedBy { get; set; }
        public int Status { get; set; }
        public string CreatedUser { get; set; }
        public string UserEmail { get; set; }


        public int Owner { get; set; }
        public SelectList Nationalities { get; set; }
        public SelectList CustomerCategories { get; set; }
        public SelectList IdTypes { get; set; }
        public string NationalityName { get; set; }
        public DateTime CreatedOn { get; set; }
        //public string CreatedOnDB { get; set; }
        public int IsMatched { get; set; }
        public string Source { get; set; }
        public string UID { get; set; }
        //[Required(ErrorMessage = "Category required")]
        public string MatchCategory { get; set; }
        //[Required(ErrorMessage = "Id Type required")]
        public string CustomerIdType { get; set; }
        //[Required(ErrorMessage = "Id Number required")]
        public string CustomerIdNumber { get; set; }
        [MaxLength(20, ErrorMessage = "Mobile Number must be less than 20 characters")]
        [RegularExpression("[0-9]*", ErrorMessage = "Invalid Mobile Number, No special characters allowed.")]
        public string Mobile { get; set; }
        public int MatchScore { get; set; }
        public string MatchType { get; set; }
        public string SourceUniqueId { get; set; }
        public int RiskScore { get; set; }
        public string CustomerType { get; set; }
        public int UpdatedBy { get; set; }
        public string UpdatedUser { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedOnDB { get; set; }
        public string IsWhiteListed { get; set; }
        public List<AML.DTO.DTO.CustomerCase.ApiResultModel> ApiResultJson { get; set; }
        public int ClientId { get; set; }
        public string Url { get; set; }
        public string createdUserName { get; set; }
        public string Comments { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }
        public string BirthYear { get; set; }
        public string Gender { get; set; }
        public bool IsPep { get; set; } = false;
        public bool IsSan { get; set; } = false;
        public bool IsRre { get; set; } = false;
        public bool IsIns { get; set; } = false;
        public bool IsDd { get; set; } = false;
        public bool IsPoi { get; set; } = false;
        public bool IsRel { get; set; } = false;
        public string CompanyCode { get; set; }
        //public List<CodesTableModel> CodesTable { get; set; }

        public SelectList CodesTable { get; set; }
        public List<string> CodeNames { get; set; } = new List<string>();

        public List<bool> IsChecked { get; set; } = new List<bool>();

        public CaseDocumentModel CaseDocuments { get; set; }
        public IFormFile Document { get; set; }
        public SelectList CaseList { get; set; }
        public int CustomerMasterId { get; set; }

        public List<CaseDocumentModel> CaseDocumentsL { get; set; }

        public string Tradelicense { get; set; }

        public string customerCodeprefix { get; set; }

        
        public string Individual_final_risk_score { get; set; }

        public string corporate_final_risk_score { get; set; }

        
        public string Individual_final_risk_sum { get; set; }

        
        public string Corporate_final_risk_sum { get; set; }

        
        public string Individual_Risk_Override { get; set; }

        
        public string Corporate_Risk_Override { get; set; }

        public string CaseChangeStatus { get; set; }
        public int NoMatch { get; set; }
        
        public int TrueDomesticpep { get; set; }
        
        public int TrueForeignpep { get; set; }
        
        public int TrueAdverseMedia { get; set; }
       
        public int PartialDomesticpep { get; set; }
        
        public int PartialForeignpep { get; set; }
        
        public int Partialadversemedia { get; set; }
        public int TrueUAEUNSanction { get; set; }
        public int TrueOtherSanction { get; set; }

        public string Address { get; set; }

        public string EstablishmentDate { get; set; }

        public SelectList ProfessionalList { get; set; }

        public SelectList ResidentialStatusList { get; set; }
        public SelectList DeliveryChannelList { get; set; }
        public SelectList ProductTypeList { get; set; }

        public SelectList ModeofpaymentList { get; set; }

        public string Modeofpayment { get; set; }

        public string DeliveryChannelName { get; set; }
        public string ProductName { get; set; }

        public string ResidenceStatus { get; set; }

        public string OccupatinTypeTxt { get; set; }

        
        public string EntityTypeTxt { get; set; }
    
        public string BusinessType { get; set; }

        public string CIFNumber { get; set; }

        public string CaseStatus { get; set; }

        public string CaseRefId { get; set; }

        public bool IsCaseCreated { get; set; }

        public string TypeId { get; set; }

        public int itemId { get; set; }
        public int branchId { get; set; }
        [Required(ErrorMessage = "Source document required")]
        public IFormFile fileUpload { get; set; }

        public string Type { get; set; }

        public List<CustomerExcelData> ExcelUploadedData { get; set; } = new List<CustomerExcelData>();

        // Flag to show Excel upload modal
        public bool ShowExcelUploadModal { get; set; } = false;

        public string CreatedOnText { get; set; }

        public string ParentId { get; set; }

        public string Domesticpep { get; set; }

        public string ForeignPep { get; set; }

        public string RedFlags { get; set; }

        public string SanctionMatch { get; set; }

        public string UAEORUNSC { get; set; }

        public string HighestRiskProduct { get; set; }

        public string HighNetworkIndividual { get; set; }

        public string ScreeningOptions { get; set; }

        public DateTime? IdIssueDate { get; set; }

        public DateTime? IdExpiryDate { get; set; }

        public string UserGroupName { get; set; }

        public string Residence { get; set; }

        public string SOWSOFCountry { get; set; }

        public string Employer { get; set; }

        public string EmployerIndustry { get; set; }

        public string EmployerSector { get; set; }

        public string GoldenVisa { get; set; }

        public string CounterParty { get; set; }

        public string ProductRefNo { get; set; }

        public string ProductValue { get; set; }

        public string TradeLicenseAuthority { get; set; }

        public string TradeLicenseSector { get; set; }

        public int Share { get; set; }
        public string Designation { get; set; }

        public string CounterPartyName { get; set; }

        

        public string Relationship { get; set; }

        public string FlagType { get; set; }

        public List<ShareholderModel> Shareholders { get; set; } = new List<ShareholderModel>();

    }

    

    public class ScreenCaseModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Customer Id required")]
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "First Name required")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        public string LastName { get; set; }
        [Required(ErrorMessage = "Nationality required")]
        public string Nationality { get; set; }
		[Display(Name = "Date of Birth")]
		[RegularExpression(@"\d{2}/\d{2}/\d{4}", ErrorMessage = "Please enter a valid date in the format dd/mm/yyyy.")]
		public string DOB { get; set; }
        //public int SourceId { get; set; }
        public int IsDelete { get; set; }
        public int CreatedBy { get; set; }
        public int Status { get; set; }
        public string CreatedUser { get; set; }
        public int Owner { get; set; }
        public SelectList Nationalities { get; set; }
        public SelectList CustomerCategories { get; set; }
        public SelectList IdTypes { get; set; }
        public string NationalityName { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedOnDB { get; set; }
        public int IsMatched { get; set; }
        public string Source { get; set; }
        public string UID { get; set; }
        //[Required(ErrorMessage = "Category required")]
        public string MatchCategory { get; set; }

        public string CustomerIdType { get; set; }

        public string CustomerIdNumber { get; set; }
        [MaxLength(20, ErrorMessage = "Mobile Number must be less than 20 characters")]
        [RegularExpression("[0-9]*", ErrorMessage = "Invalid Mobile Number, No special characters allowed.")]
        public string Mobile { get; set; }
        public int MatchScore { get; set; }
        public string MatchType { get; set; }
        public string SourceUniqueId { get; set; }
        public int RiskScore { get; set; }
        [Required(ErrorMessage = "Customer Type  required")]
        public string CustomerType { get; set; }

        public string IsWhiteListed { get; set; }
        //public string ApiResultJson { get; set; }
        public List<AML.DTO.DTO.CustomerCase.ApiResultModel> ApiResultJson { get; set; }
    }
    public class CustomerExcelData
    {
        public string CustomerID { get; set; }
        public string Firstname { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
		[Display(Name = "Date of Birth")]
		[RegularExpression(@"\d{2}/\d{2}/\d{4}", ErrorMessage = "Please enter a valid date in the format dd/mm/yyyy.")]
		public string DOB { get; set; }
        public int Status { get; set; }
        public int Id { get; set; }

        public List<string> CodeNames { get; set; } = new List<string>();

        public List<bool> IsChecked { get; set; }

        public bool IsPep { get; set; }
        public bool IsSan { get; set; }
        public bool IsRre { get; set; }
        public bool IsIns { get; set; }
        public bool IsDd { get; set; }
        public bool IsPoi { get; set; }
        public bool IsRel { get; set; }

        public string Modeofpayment { get; set; }

        public string DeliveryChannelName { get; set; }
        public string ProductName { get; set; }

        public string ResidenceStatus { get; set; }

        public string OccupatinTypeTxt { get; set; }

        public string ScreeningStatus { get; set; }

    }
    public class CorporateExcelData
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

        public string ScreeningStatus { get; set; }

    }


    public class CustomerCaseApiModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Customer Id required")]
        public string CustomerId { get; set; }
        //[Required(ErrorMessage = "First Name required")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name required")]
        public string LastName { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerType { get; set; }
        public string MatchCategory { get; set; }
        public string CustomerIdNumber { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }
        public string Mobile { get; set; }
        public string CreatedOn { get; set; }
        public string CompanyCode { get; set; }
        public int ClientId { get; set; }
        [Required(ErrorMessage = "User Id required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Company Name required")]
        public string CompanyName { get; set; }
        public int Threshold { get; set; }
    }

    public class CustomerCaseWithShareholderApiModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Customer Id required")]
        public string CustomerId { get; set; }
        //[Required(ErrorMessage = "First Name required")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name required")]
        public string LastName { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerType { get; set; }
        public string MatchCategory { get; set; }
        public string CustomerIdNumber { get; set; }
        public string CompanyCode { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }
        public string Mobile { get; set; }
        public string CreatedOn { get; set; }
        public int ClientId { get; set; }
        [Required(ErrorMessage = "User Id required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Company Name required")]
        public string CompanyName { get; set; }
        public int Threshold { get; set; }
        [MaxLength(10, ErrorMessage = "Cannot screen with more than 10 shareholders")]
        public List<ShareholderDetails> Shareholders { get; set; }
    }


    public class ShareholderDetails
    {
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }


        //Newly Added Fields


      
        public string onb_cust_ref_id { get; set; }

        public string onb_name { get; set; }
        public string onb_dob { get; set; }
        public string onb_nationality { get; set; }
        public string onb_cust_type { get; set; }
        public string onb_cust_id_type { get; set; }
        public string onb_cust_id { get; set; }
        public string onb_mobile { get; set; }
        public string onb_created_by { get; set; }
        public string onb_created_on { get; set; }
        public string onb_updated_by { get; set; }
        public string onb_emirates_id { get; set; }
        public string onb_emirates_id_expiry { get; set; }
        public string onb_passport_expiry { get; set; }
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





    }

    public class ShareholderResults
    {
        public string isMatched { get; set; }
         public int matchScore { get; set; }
        public string customerId { get; set; }
        public string IsRiskScore { get; set; }
        public string IsRiskStatus { get; set; }

        
    }

    public class CustomerApiModel
    {
        /// <summary>
        /// Gets or sets the caseid.
        /// </summary>
        /// <value>
        /// The caseid.
        /// </value>
        [Display(Name = "Case ID :")]
        //[Required(ErrorMessage = "Case ID Required")]
        public string Id { get; set; }
        /// <summary>
        /// Gets or sets the customercode.
        /// </summary>
        /// <value>
        /// The customercode.
        /// </value>
        [Display(Name = "Customer Code :")]
        //[Required(ErrorMessage="Customer Code Required")]
        [MaxLength(25, ErrorMessage = "Must be Maximum 25 Characters")]
        [RegularExpression("^([a-zA-Z0-9]+)$", ErrorMessage = "Invalid Customer Code")]
        public string CustomerId { get; set; }
        /// <summary>
        /// Gets or sets the customerfullname.
        /// </summary>
        /// <value>
        /// The customerfullname.
        /// </value>
        [Display(Name = "Customer FullName:")]
        [Required(ErrorMessage = "Customer fist name Required")]
        [MaxLength(250, ErrorMessage = "Must be Maximum 250 Characters")]
        [RegularExpression("^([a-zA-Z .-]+)$", ErrorMessage = "Invalid Name")]
        public string FirstName { get; set; }
        /// <summary>
        /// Gets or sets the customerfullname.
        /// </summary>
        /// <value>
        /// The customerfullname.
        /// </value>
        [Display(Name = "Customer middle name:")]
        //[Required(ErrorMessage = "Customer Last name Required")]
        [MaxLength(250, ErrorMessage = "Must be Maximum 250 Characters")]
        [RegularExpression("^([a-zA-Z .-]+)$", ErrorMessage = "Invalid Name")]
        public string MiddleName { get; set; }
        /// <summary>
        /// Gets or sets the customerfullname.
        /// </summary>
        /// <value>
        /// The customerfullname.
        /// </value>
        [Display(Name = "Customer Last name:")]
        [Required(ErrorMessage = "Customer last name Required")]
        [MaxLength(250, ErrorMessage = "Must be Maximum 250 Characters")]
        [RegularExpression("^([a-zA-Z .-]+)$", ErrorMessage = "Invalid Name")]
        public string LastName { get; set; }
        /// <summary>
        /// Gets or sets the customeridtype.
        /// </summary>
        /// <value>
        /// The customeridtype.
        /// </value>
        [Display(Name = "Customer Id Type Code:")]
        [Required(ErrorMessage = "Customer Id Type Required")]
        //[MaxLength(2, ErrorMessage = "Id Type Code Must be Maximum 2 Characters")]
        //[MinLength(1, ErrorMessage = "Id Type Code Must be Minimum 1 Characters")]
        //[RegularExpression("^[0-9]+", ErrorMessage = "Id Type Code Must be Numeric String")]
        public string CustomerIdType { get; set; }
        /// <summary>
        /// Gets or sets the customertype.
        /// </summary>
        /// <value>
        /// The customertype.
        /// </value>
        [Display(Name = "Customer category:")]
        [Required(ErrorMessage = "Category Required")]
        public string MatchCategory { get; set; }
        /// <summary>
        /// Gets or sets the customeridnumber.
        /// </summary>
        /// <value>
        /// The customeridnumber.
        /// </value>        
        //[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
        [Display(Name = "Customer Id Number:")]
        [Required(ErrorMessage = "Customer Id number required")]
        [MaxLength(25, ErrorMessage = "Id Number Must be Maximum 35 Characters")]
        [RegularExpression("[^'\"]*", ErrorMessage = "Invalid Id Number")]
        public string CustomerIdNumber { get; set; }

        [Display(Name = "Customer Nationality:")]
        [Required(ErrorMessage = "Customer Nationality Required")]
        //  [MaxLength(2, ErrorMessage = "Customer Nationality Code Must be Maximum 2 Characters")]
        //   [MinLength(2, ErrorMessage = "Customer Nationality Code Must be Minimum 2 Characters")]
        [RegularExpression("[a-zA-Z]*", ErrorMessage = "Only Alphabets are allowed.")]
        public string Nationality { get; set; }
        /// <summary>
        /// Gets or sets the customerdob.
        /// </summary>
        /// <value>
        /// The customerdob.
        /// </value>        
        [Display(Name = "Customer DOB:")]
        [Required(ErrorMessage = "DOB Required")]
        [RegularExpression("^[0-9]{4}-(((0[13578]|(10|12))-(0[1-9]|[1-2][0-9]|3[0-1]))|(02-(0[1-9]|[1-2][0-9]))|((0[469]|11)-(0[1-9]|[1-2][0-9]|30)))$", ErrorMessage = "Invalid Date.")]
        public string DOB { get; set; }
        /// <summary>
        /// Gets or sets the customermobilenumber.
        /// </summary>
        /// <value>
        /// The customermobilenumber.
        /// </value>        
        [Display(Name = "Mobile Number:")]
        [MaxLength(20, ErrorMessage = "Mobile Number must be less than 20 characters")]
        [RegularExpression("[0-9]*", ErrorMessage = "Invalid Mobile Number, No special characters allowed.")]
        public string Mobile { get; set; }
        /// <summary>
        /// Gets or sets the createdon.
        /// </summary>
        /// <value>
        /// The createdon.
        /// </value>
        [Display(Name = "Created Date:")]
        [MaxLength(20, ErrorMessage = "Date must be in dd/MM/yyyy HH:mm:ss Format")]
        [RegularExpression(@"^(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d)) (?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Date must be in dd/MM/yyyy HH:mm:ss Format.")]
        public string CreatedOn { get; set; }
        public string CustomerType { get; set; }
    }


    public class APImodel{

        public string startDate { get; set; }
        public string endDate { get; set; }

        public string cust_type { get; set; }

        public int userId { get; set; }
    }
}
