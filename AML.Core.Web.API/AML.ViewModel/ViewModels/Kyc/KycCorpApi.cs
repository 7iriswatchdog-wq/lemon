using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.Kyc
{
    public class KycCorpApi
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string CustomerIdType { get; set; }
        public string CustomerType { get; set; }
        public string Nationality { get; set; }
        public string DOB { get; set; }
        public string Mobile { get; set; }
        public string CreatedOn { get; set; }
        public string Email { get; set; }
        public string PlaceofIncorporation { get; set; }
        public string EntityTypeTxt { get; set; }
        public string FundSourceTxt { get; set; }
        public string BusinessType { get; set; }
        public string ProductName { get; set; }
        public string ParentEntityName { get; set; }
        public int PEPStatus { get; set; }
        public int ClientId { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public int Threshold { get; set; }
    }
}
