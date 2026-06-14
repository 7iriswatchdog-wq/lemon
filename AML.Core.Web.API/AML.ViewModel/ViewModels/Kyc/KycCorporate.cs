using AML.ViewModel.ViewModels.CaseDocument;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.Kyc
{
    public class CorporateKycModel
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string FullName { get; set; }
        public DateTime DateofIncorporation { get; set; }

        public SelectList PlaceofIncorporationList { get; set; }
        public string PlaceofIncorporation { get; set; }
        public SelectList PlaceofIncorporationlist { get; set; }
        public SelectList CountryofIncorporationList { get; set; }
        public SelectList PartnerNationalityList { get; set; }
        public SelectList EntityType { get; set; }
        public List<LegalStatusModel> LegalStatus { get; set; }
        public string EntityTypeTxt { get; set; }
        public Address EntityAddress { get; set; }
        public string CorporateWebsite { get; set; }
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Please Enter a Valid Phone Number")]
        public string Telephone { get; set; }
        [Display(Name = "Email Address")]
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }
        public License CommercialLicense { get; set; }
        public string VATRegistrationNumber { get; set; }
        public List<Branch> Branches { get; set; }

        public string FundSourceTxt { get; set; }
        public string FundSourceOther { get; set; }
        public SelectList BusinessTypeList { get; set; }
        public List<BusinessNature> BusinessNature { get; set; }
        public string BusinessType { get; set; }
        public SelectList ProductTypeList { get; set; }
        public List<ProductType> ProductType { get; set; }
        public string ProductName { get; set; }

        public SelectList DeliveryChannelList { get; set; }
        public List<DeliveryChannel> DeliveryChannel { get; set; }
        public string DeliveryChannelName { get; set; }
        public List<GroupEntity> GroupEntity { get; set; }
        public string ParentEntityName { get; set; }
        public string PlaceofIncorporationParent { get; set; }
        public List<PersonDetails> Partners { get; set; }
        public List<PersonDetails> SeniorManagements { get; set; }
        public List<PersonDetails> AuthorisedSignatories { get; set; }
        public string PEPStatus { get; set; }

        public string Address {get;set;}
        public int PEPTypeStatus { get; set; }
        public string PEPName { get; set; }
        public string PEPTitle { get; set; }
        public int PEPType { get; set; }
        public PEPDetail PEPDetails { get; set; }
        public BankDetail BankDetails { get; set; }
        public string Remarks { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }
        public string IsPeP { get; set; }
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }

        public SelectList ModeofpaymentList { get; set; }

        public string Modeofpayment { get; set; }
    }
    public class DeliveryChannel
    {
        public int Id { get; set; }
        public string DeliveryChannelName { get; set; }
    }
    public class ProductType
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
    }
    public class LegalStatusModel
    {
        public int Id { get; set; }
        public string LegalStatus { get; set; }
    }
    public class BusinessNature
    {
        public int Id { get; set; }
        public string BusinessName { get; set; }
    }
    public class GroupEntity
    {
        public string cust_ref_id { get; set; }
        public string EntityName { get; set; }
        public SelectList PlaceofIncorporationList { get; set; }
        public string PlaceofIncorporation { get; set; }
        public bool IsRemoved { get; set; }
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }


    }
    public class BankDetail
    {
        public string BeneficiaryName { get; set; }
        public string BankName { get; set; }
        public string Currency { get; set; }
        public string IBANNumber { get; set; }
        public string AccountNumber { get; set; }
        public string SwiftCode { get; set; }
    }
    public class PEPDetail
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public int PEPTypeStatus { get; set; }

    }
    public class PersonDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public string Percentage { get; set; }
        public string Nationality { get; set; }
        public SelectList NationalityList { get; set; }
        public string EmiratesID { get; set; }
        public string EmiratesIDExpiry { get; set; }
        public string PassportNumber { get; set; }
        public string PassportExpiry { get; set; }
        public bool IsRemoved { get; set; }
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }

        public string cust_id_gen { get; set; }


    }

    public class Branch
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Address
    {
        public SelectList CountryList { get; set; }
        public string Country { get; set; }
        public string Emirate { get; set; }
        public string City { get; set; }
        public string POBox { get; set; }
    }
    public class License
    {
        public string LicenseNumber { get; set; }
        public string LicenseIssueDate { get; set; }
        public string LicenseIssuingAuthority { get; set; }
        public string LicenseExpiryDate { get; set; }
        public string PlaceofIssue { get; set; }
        public string BusinessActivity { get; set; }
        public SelectList LicenseType { get; set; }
        public string LicenseTypeTxt { get; set; }

    }

    public class Menumodel
    {
       
        public int Client_Id { get; set; }
       
        public int Menu_Id { get; set; }
       
        public int Created_By { get; set; }
       
        public int is_Active { get; set; }


        public string Menu_Name { get; set; }
    }


}
