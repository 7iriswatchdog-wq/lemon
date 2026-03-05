using System.ComponentModel.DataAnnotations;
namespace AML.ViewModel.ViewModels.Department
{
    public class DepartmentModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Department Code is required.")]
        [RegularExpression(@"^[a-zA-Z0-9]*$", ErrorMessage = "Please Enter Valid Code.")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Department Name is required.")]
        public string Name { get; set; }
        public string Description { get; set; }
        public int IsActive { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string UpdatedOn { get; set; }
        public int ClientId { get; set; }
    }
}
