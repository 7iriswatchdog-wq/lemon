using System.ComponentModel.DataAnnotations;
namespace AML.ViewModel.ViewModels.Country
{
    public class CountryModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Country Code is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Please Enter Valid Code.")]
        public string Code { get; set; }

        public string Name { get; set; }


        public string Description { get; set; }

        public decimal Riskscore { get; set; }

        public int RiskRating { get; set; }

        public string Risk { get; set; }

        public string UNCode { get; set; }

        public string FATFRiskRating { get; set; }

        public string FATFRiskScore { get; set; }

        public string ISOCode3digit { get; set; }

        public bool IsActive { get; set; }
        public int ClientId { get; set; }
    }
}
