using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Policy;

namespace AML.ViewModel.ViewModels.Kyc
{
    public class kycapimodel
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerType { get; set; }
        public string MatchCategory { get; set; }
        public string CustomerIdNumber { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }
        public string Mobile { get; set; }
        public string CreatedOn { get; set; }
        public int ClientId { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public int Threshold { get; set; }
        public string ResidenceStatus { get; set; }
        public string OccupatinTypeTxt { get; set; }
        public string EmployerName { get; set; }
        public string EmployerAddress { get; set; }
        public string ResidenceAddress { get; set; }
        public string Email { get; set; }
        public int PEPStatus { get; set; }

        public DateTime DateofIncorporation { get; set; }
        public string PlaceofIncorporation { get; set; }
        public SelectList CountryofIncorporationList { get; set; }
        public SelectList PartnerNationalityList { get; set; }
        public SelectList EntityType { get; set; }
        public List<LegalStatusModel> LegalStatus { get; set; }
        public string EntityTypeTxt { get; set; }
        public Address EntityAddress { get; set; }
        public string CorporateWebsite { get; set; }

        public string Telephone { get; set; }
        //[RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please a Enter Valid Email ID")]

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
        //public string DeliveryChannelName { get; set; }

        //public string Domesticpep { get; set; }

        //public string ForeignPep { get; set; }

        //public string RedFlags { get; set; }

        //public string SanctionMatch { get; set; }

        //public string UAEORUNSC { get; set; }

        //public string ModeOfPayment { get; set; }

        //public string LegalStatus { get; set; }

        //public string BusinessType { get; set; }

        //public string FATF { get; set; }

        //public string HighestRiskProduct { get; set; }

        //public string HighNetworkIndividual { get; set; }

        //public string Address { get; set; }

        //public string EstablishmentDate { get; set; }

        //public List<PassportDetails> PassportDetails { get; set; }









        public List<GroupEntity> GroupEntity { get; set; }
        public string ParentEntityName { get; set; }
        public string PlaceofIncorporationParent { get; set; }
        public List<PersonDetails> Partners { get; set; }
        public List<PersonDetails> SeniorManagements { get; set; }
        public List<PersonDetails> AuthorisedSignatories { get; set; }

        public PEPDetail PEPDetails { get; set; }
        public BankDetail BankDetails { get; set; }
        public string Remarks { get; set; }
        public int C6Threshold { get; set; }

        public int CreatedBy { get; set; }
    }

    //public class PassportDetails {

    //    public string CaseId { get; set; }

    //    public string PassportNo { get; set; }

    //    public string PassportIssusePlace { get; set; }

    //    public string PassportIssuesDate { get; set; }

    //    public string PassportExpiryDate { get; set; }

    //    public DateTime? CreatedOn { get; set; }

    //    public int CreatedBy { get; set; }

    //    public int ClientId { get; set; }


    //}


}
