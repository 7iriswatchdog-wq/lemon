using AML.ViewModel.ViewModels.CaseDocument;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.Kyc
{
    public class KycIndividualModel
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "Full Name is required")]
        public string FullName { get; set; }
        public DateTime DOB { get; set; }
        public SelectList CountryList { get; set; }
        public string Nationality { get; set; }
        public string ResidenceStatus { get; set; }
        public SelectList OccupationType { get; set; }
        public string OccupatinTypeTxt { get; set; }
        public string EmployerName { get; set; }
        public string EmployerAddress { get; set; }
        public string ResidenceAddress { get; set; }

        public string Address { get; set; }
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please a Enter Valid Email ID")]
        public string Email { get; set; }
        //[RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage = "Please Enter a Valid Phone Number")]
        public string Mobile { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerIdNumber { get; set; }
        public string PEPStatus { get; set; }
        public string GuardianName { get; set; }
        public string GuardianRelation { get; set; }
        public string Remarks { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }
        public int ClientId { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public int CreatedBy { get; set; }
        public string IsPep { get; set; }
        public SelectList CaseList { get; set; }
        public IFormFile Document { get; set; }
        public List<CaseDocumentModel> CaseDocumentsL { get; set; }
        public string PlaceOfBirth { get; set; }
        public string MaritalStatus { get; set; }
        
        public string IdExpdate { get; set; }
        
        public string BankAccountNo { get; set; }
        public string BankAccountName { get; set; }
        public string BankAccountBranch { get; set; }
        public string Sourceofincome { get; set; }
        public SelectList ProductTypeList { get; set; }
        public SelectList DeliveryChannelList { get; set; }

        public string DeliveryChannelName { get; set; }
        public string ProductName { get; set; }
        public SelectList ProfessionalList { get; set; }

        public SelectList ModeofpaymentList { get; set; }

        public string Modeofpayment { get; set; }


        public SelectList ResidentialStatusList { get; set; }



    }
}
