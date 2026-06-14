using System.ComponentModel.DataAnnotations;
namespace AML.ViewModel.ViewModels.Corporate
{
    public class CorporateModel
    {
        public int LicenseNumber { get; set; }
          public string LicenseName { get; set; }
        [Required(ErrorMessage = "License Name is required.")]
        public int MobileNo { get; set; }     
       
    }
}
