using Microsoft.VisualBasic;
using System;
using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.ApiAuthentication
{
    public class ApiAuthRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
        public string Secret { get; set; }
        [Required]
        public string CompanyName { get; set; }
    }

    public class ScreeinglogsModel
    {
        public int Id {get;set;}
        [Required]
        public int Individual { get; set; }
        [Required]
        public int Corporate { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdateOn { get; set; }

        public string is_delete_ind { get; set; }

        public string is_delete_corp { get; set; }

    }

}
