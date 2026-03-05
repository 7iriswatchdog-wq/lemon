using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using AML.DTO.DTO.TransactionScreening;
using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class TranScreenCaseModel
    {
        public int id { get; set; }
        public string TranRefNo { get; set; }
        public string CustRefNo { get; set; }
        public string CustType { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public DateTime DOB { get; set; }
        public int IsMatched { get; set; }
        public int MatchScore { get; set; }
        public int Threshold { get; set; }
        public string Source { get; set; }
        public string SourceUniqueId { get; set; }
        public String Comments { get; set; }
        public int Assignedto { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
        public string IsWhiteListed { get; set; }
        public int sendMail { get; set; }

    }
    public class Bank
    {
        public string TranRefNo { get; set; }
        public string Bankname { get; set; }
        public string CountryOfIncorporation { get; set; }
    }

    public class Remiter
    {

        public string TranRefNo { get; set; }
        //[Required]
        public string CustomerId { get; set; }
        //[Required]
        public string Name { get; set; }
        public string Nationality { get; set; }
        public string Dob { get; set; }
        public Bank Bank { get; set; }
    }

    public class Beneficiary
    {
        public string TranRefNo { get; set; }
        public string CustomerId { get; set; }
        //[Required]
        public string Name { get; set; }
        public string Nationality { get; set; }
        public string Dob { get; set; }
        public Bank Bank { get; set; }
    }

    public class Vendor
    {
        public string TranRefNo { get; set; }
        public string Name { get; set; }
        public string CountryOfIncorporation { get; set; }
    }

    public class ScreeningModel
    {
        public string TranRefNo { get; set; }
        public Remiter Remitter { get; set; }
        public Beneficiary Beneficiary { get; set; }
        public Vendor Vendor { get; set; }
        public int ClientId { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public int CreatedBy { get; set; }
    }

    public class ApiRespModel
    {
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchuid { get; set; }
        public string matchcategory { get; set; }
        public string matchtype { get; set; }
        public string nationality { get; set; }
        public string matchidnumber { get; set; }
        public string matchdob { get; set; }
        public string remarks { get; set; }
    }

    public class IsWhiteListedCheckModel
    {
        public string tranrefno { get; set; }

        public string IsWhiteListed { get; set; }
    }




}
