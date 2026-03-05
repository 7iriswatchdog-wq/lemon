using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.Sanction
{
    public class SanctionWatchModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Re Active Date required")]
        public string ReActiveDate { get; set; }
        [Required(ErrorMessage = "First Name required")]
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last Name required")]
        public string LastName { get; set; }
        
        public string Nationality { get; set; }
        [Required(ErrorMessage = "Date of Birth required")]
        public string DOB { get; set; }
        public string PassportNo { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Nationality required")]
        public int CountryId { get; set; }
        public string NationalityName { get; set; }
        public string Narration { get; set; }
        public string Source { get; set; }
        public string Remarks { get; set; }
        public int IsBlocked { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public SelectList Nationalities { get; set; }
    }
}
