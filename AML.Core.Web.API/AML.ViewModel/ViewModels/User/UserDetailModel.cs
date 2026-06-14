using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.User
{
    public class UserDetailModel
    {
        
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime? DateOfJoin { get; set; }
        public string City { get; set; }
        [RegularExpression(@"^\s*(?:\+?(\d{1,3}))?[-. (]*(\d{3})[-. )]*(\d{3})[-. ]*(\d{4})(?: *x(\d+))?\s*$", ErrorMessage ="Please Enter a Valid Phone Number")]
        public string Phone { get; set; }
        public string Fax { get; set; }
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage ="Please a Enter Valid Email ID")]
        [Required(ErrorMessage = "Email-ID is required.")]
        public string Email { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public int IdentityType { get; set; }
        public decimal? ApprovalLimit { get; set; } 
        public string IdentificationNumber { get; set; }
        public DateTime? IdNumDateOfIssue { get; set; }
        public DateTime? IdNumExpryDate { get; set; }
        public int VisaType { get; set; }
        public string CurrentAdd { get; set; }
        public string VisaNumber { get; set; }
        public DateTime? VisaNumDateOfIssue { get; set; }
        public DateTime? VisaNumExpryDate { get; set; }
        public string CprNumber { get; set; }
        public DateTime? CprNumDateOfIssue { get; set; }
        public DateTime? CprNumExpryDate { get; set; }
    }
}
