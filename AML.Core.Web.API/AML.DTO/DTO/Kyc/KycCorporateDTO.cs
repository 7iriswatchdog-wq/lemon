using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Kyc
{

    public class CorporateKycDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("customer_id")]
        public string CustomerId { get; set; }
        [Column("fname")]
        public string FullName { get; set; }
        [Column("dob")]
        public string DateofIncorporation { get; set; }
        [Column("nationality")]
        public string PlaceofIncorporation { get; set; }
        [Column("entity_type")]
        public string EntityTypeTxt { get; set; }
        public AddressDTO EntityAddress { get; set; }
        [Column("corporate_website")]
        public string CorporateWebsite { get; set; }
        [Column("mobile")]
        public string Telephone { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("threshold")]
        public int Threshold { get; set; }
        public LicenseDTO CommercialLicense { get; set; }
        [Column("vat_number")]
        public string VATRegistrationNumber { get; set; }
        public List<BranchKycDTO> Branches { get; set; }
        [Column("fund_source")]
        public string FundSourceTxt { get; set; }
        [Column("fund_source_other")]
        public string FundSourceOther { get; set; }
        [Column("business_type")]
        public string BusinessType { get; set; }
        //public List<BusinessNatureDTO> BusinessNature { get; set; }


        public List<ProductTypeDTO> ProductType { get; set; }
        [Column("product_type")]
        public string ProductName { get; set; }

        //public List<DeliveryChannelDTO> DeliveryChannel { get; set; }
        [Column("delivery_channel")]
        public string DeliveryChannelName { get; set; }
        //public List<LegalStatusDTO> LegalStatus { get; set; }
        //public List<GroupEntityDTO> GroupEntity { get; set; }
        //[Column("parent_entity_name")]
        //public string ParentEntityName { get; set; }
        //[Column("parent_entity_place_of_incorporation")]
        //public string PlaceofIncorporationParent { get; set; }
        public List<PersonDetailsDTO> Partners { get; set; }
        public List<PersonDetailsDTO> SeniorManagements { get; set; }
        public List<PersonDetailsDTO> AuthorisedSignatories { get; set; }
        [Column("pep_status")]
        public string PEPStatus { get; set; }
        [Column("address")]
        public string Address { get; set; }
        public PEPDetailDTO PEPDetails { get; set; }
        public BankDetailDTO BankDetails { get; set; }
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
        [Column("remark")]
        public string Remarks { get; set; }
        [Column("pep_name")]
        public string PEPName { get; set; }
        [Column("pep_title")]
        public string PEPTitle { get; set; }

        [Column("pep_type_status")]
        public int PEPType { get; set; }
        public string IsPeP { get; set; }

        [Column("mode_of_payment")]
        public string Modeofpayment { get; set; }


        public string Domesticpep { get; set; }

        public string ForeignPep { get; set; }

        public string RedFlags { get; set; }

        public string SanctionMatch { get; set; }

        public string UAEORUNSC { get; set; }

        public string ModeOfPayment { get; set; }

        public string DualUseGoods { get; set; }

        public string MoreDualUseGoods { get; set; }

        public string MilitaryGoods { get; set; }

        public string MoreMilitaryGoods { get; set; }




        public string FATF { get; set; }

        public string HighestRiskProduct { get; set; }

        public string HighNetworkIndividual { get; set; }


    }
    public class DeliveryChannelDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("delivery_channel")]
        public string DeliveryChannelName { get; set; }
    }
    public class ProductTypeDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("product")]
        public string ProductName { get; set; }
    }
    public class LegalStatusDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("legal_status")]
        public string LegalStatus { get; set; }
    }
    public class BusinessNatureDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("business_name")]
        public string BusinessName { get; set; }
    }
    public class GroupEntityDTO
    {
        [Column("cust_ref_id")]
        public string cust_ref_id { get; set; }

        [Column("fname")]
        public string EntityName { get; set; }
        [Column("nationality")]
        public string PlaceofIncorporation { get; set; }
        public bool IsRemoved { get; set; }

    }

    public class BankDetailDTO
    {
        [Column("beneficiary_name")]
        public string BeneficiaryName { get; set; }
        [Column("bank_name")]
        public string BankName { get; set; }
        [Column("currency")]
        public string Currency { get; set; }
        [Column("iban_number")]
        public string IBANNumber { get; set; }
        [Column("account_number")]
        public string AccountNumber { get; set; }
        [Column("swift_code")]
        public string SwiftCode { get; set; }
    }
    public class PEPDetailDTO
    {
        [Column("pep_name")]
        public string Name { get; set; }
        [Column("pep_title")]
        public string Title { get; set; }
        [Column("pep_type_status")]
        public int PEPTypeStatus { get; set; }

    }
    public class PersonDetailsDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lname")]
        public string Name { get; set; }
        [Column("designation")]
        public string Designation { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        [Column("emirates_id")]
        public string EmiratesID { get; set; }
        [Column("emirates_id_expiry")]
        public string EmiratesIDExpiry { get; set; }
        [Column("cust_id_number")]
        public string PassportNumber { get; set; }
        [Column("passport_expiry")]
        public string PassportExpiry { get; set; }
        public bool IsRemoved { get; set; }
        [Column("share_percentage")]
        public string Percentage { get; set; }

        [Column("cust_id_gen")]
        public string cust_id_gen { get; set; }

    }

    public class BranchKycDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class AddressDTO
    {
        [Column("country")]
        public string Country { get; set; }
        [Column("emirate")]
        public string Emirate { get; set; }
        [Column("city")]
        public string City { get; set; }
        [Column("po_box")]
        public string POBox { get; set; }
    }
    public class LicenseDTO
    {
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

    }
}


