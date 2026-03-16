using AML.ViewModel.ViewModels.CaseDocument;
using AML.ViewModel.ViewModels.CodesMaster;
using AML.ViewModel.ViewModels.CustomerCase;
using AML.ViewModel.ViewModels.Kyc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace AML.ViewModel.ViewModels.Corporate
{
    public class CorporateScreeningModel
    {
        public int Id { get; set; }
        //    [Required(ErrorMessage = "Customer Id required")]
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        // [Required(ErrorMessage = "Nationality required")]
        public string Nationality { get; set; }
        //[Required(ErrorMessage = "Date of Birth required")]
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
        // [Required(ErrorMessage = "Id Type required")]
        public string CustomerIdType { get; set; }
        //   [Required(ErrorMessage = "Id Number required")]
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
        //public bool IsPep { get; set; } = true;
        //public bool IsSan { get; set; } = true;
        //public bool IsRre { get; set; } = true;
        //public bool IsIns { get; set; } = true;
        //public bool IsDd { get; set; } = true;
        //public bool IsPoi { get; set; } = true;
        //public bool IsRel { get; set; } = true;

        //public List<CodesTableModel> CodesTable { get; set; }
        public SelectList CodesTable { get; set; }
        public List<string> CodeNames { get; set; } = new List<string>();
        public List<bool> IsChecked { get; set; } = new List<bool>();
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }

        public List<CorporateDetailsModel> CorporateDetailList { get; set; }

        public string Tradelicense { get; set; }

        public SelectList BusinessTypeList { get; set; }

        public SelectList ProductTypeList { get; set; }
        public List<ProductType> ProductType { get; set; }
        

        public SelectList DeliveryChannelList { get; set; }
        public List<DeliveryChannel> DeliveryChannel { get; set; }
        

        public SelectList EntityType { get; set; }

        public SelectList ModeofpaymentList { get; set; }

        public string TypeId { get; set; }

        public int itemId { get; set; }
        public int branchId { get; set; }
        [Required(ErrorMessage = "Source document required")]
        public IFormFile fileUpload { get; set; }

        public string CompanyCode { get; set; }

        public List<CorporateExcelData> ExcelUploadedData { get; set; } = new List<CorporateExcelData>();

        // Flag to show Excel upload modal
        public bool ShowExcelUploadModal { get; set; } = false;

        public string CaseRefId { get; set; }

        public bool IsCaseCreated { get; set; }

        public string CIFNumber { get; set; }

        


    }

    public class CustomeDetailsModel
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string MatchCategory { get; set; }
        //[Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        //[Required(ErrorMessage = "Last Name is required.")]
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
        public string BirthYear { get; set; }
        public string Gender { get; set; }
        public bool IsPep { get; set; } = true;
        public bool IsSan { get; set; } = true;
        public bool IsRre { get; set; } = true;
        public bool IsIns { get; set; } = true;
        public bool IsDd { get; set; } = true;
        public bool IsPoi { get; set; } = true;
        public bool IsRel { get; set; } = true;

        public List<bool> IsChecked { get; set; } = new List<bool>();

        public string EmiratesIdExpiry { get; set; }
        public string Designation { get; set; }
        public string EmiratesIdNumber { get; set; }
        public string CustomerIdExpiry { get; set; }
        public string CustomerType { get; set; }
        public string CorporateType { get; set; }

        public string Companycode { get; set; }
       public string IsPeP { get; set; }
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }
    }
    public class CorporateDetailsModel
    {
        // [Required(ErrorMessage = "Customer Code is require.")]
        public string CustomerCode { get; set; }
        [Required(ErrorMessage = "Company Name is require.")]
        public string FirstName { get; set; }
        public string LicenseNumber { get; set; }
        public string LicenseName { get; set; }
        public string MobileNo { get; set; }
        public SelectList Countries { get; set; }
        public string Nationality { get; set; }
        public string NationalityName { get; set; }
        public string AccomplishedCountry { get; set; }
        public DateTime AccomplishedDate { get; set; }
        public List<CustomeDetailsModel> CustomerDetailList { get; set; }
        public bool IsMatched { get; set; }
        public int MatchScore { get; set; }
        public int CreatedBy { get; set; }
        public string IsWhiteListed { get; set; }
        public bool isRemoved { get; set; }
        public string ApiResultJsonCorp { get; set; }
        public int ClientId { get; set; }
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

        public List<bool> IsChecked { get; set; } = new List<bool>();


        public string CompanyName { get; set; }
        public string CorporateType { get; set; }
        public string GroupEntityOf { get; set; }

        public string CustomerType { get; set; }
        public string LicenseIssueDate { get; set; }
        public string LicenseExpiryDate { get; set; }
        public string PlaceofIncorporation { get; set; }

        public string CompanyCode { get; set; }
        public string IsPeP { get; set; }
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }

        public string DOB { get; set; }

        public string Tradelicense { get; set; }

        public string customerCodeprefix { get; set; }

        public string Modeofpayment { get; set; }

        public string CIFNumber { get; set; }

        public string EntityTypeTxt { get; set; }

        public string BusinessType { get; set; }

        public string ProductName { get; set; }

        public string DeliveryChannelName { get; set; }

        public string Type { get; set; }

        public string Domesticpep { get; set; }

        public string ForeignPep { get; set; }

        public string RedFlags { get; set; }

        public string SanctionMatch { get; set; }

        public string UAEORUNSC { get; set; }

        public string HighestRiskProduct { get; set; }

        public string HighNetworkIndividual { get; set; }

        public string FATF { get; set; }

       
        public string ScreeningOptions { get; set; }

        public DateTime? IdExpiryDate { get; set; }

        public string Residence { get; set; }

        public string TradeLicenseAuthority { get; set; }

        public string TradeLicenseSector { get; set; }

        public string CounterParty { get; set; }

        public string ProductType { get; set; }

        public string ProductValue { get; set; }
    }

    public class ShareholderModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Share { get; set; }
        public string Designation { get; set; }
        public string Nationality { get; set; }
        public string Residence { get; set; }
        public string TradeLicence { get; set; }
        public DateTime? RegistrationDate { get; set; }
        
        public string IdType { get; set; }
        public string IdNumber { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? IdExpiry { get; set; }
        public string Cif { get; set; }
        public string Type { get; set; }   // ? Important

        public int ClientID { get; set; }

        public int UserId { get; set; }

        public string CompanyCode { get; set; }

        public string CompanyName { get; set; }

        public int Thershold { get; set; }

        public string DisplayId { get; set; }

        public string Employer { get; set; }

        public string GoldenVisa { get; set; }

        public string CustType { get; set; }

        public string Document { get; set; }

       
        public string DocumentFullPath { get; set; }

        public string EmployerIndustry { get; set; }

        public string EmployerSector { get; set; }

        public string SOWSOFCountry { get; set; }

      
        public string TradeLicenseAuthority { get; set; }
       
        public string TradeLicenseSector { get; set; }


    }

    public class ShareholderFormModel
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public int Thershold { get; set; }

        public List<CodesTableModel> CodesTables { get; set; }

        public List<ShareholderModel> Shareholders { get; set; } = new();

        public List<string> CodeNames { get; set; } = new List<string>();
    }
}
