using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.UserGroup
{
    public class UserGroupModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Designation Code is required.")]
        public string Code { get; set; }
        [Required(ErrorMessage = "Designation Name is required.")]
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
