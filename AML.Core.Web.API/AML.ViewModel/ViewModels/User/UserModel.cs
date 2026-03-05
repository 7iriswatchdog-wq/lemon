using AML.ViewModel.ViewModels.Branch;
using AML.ViewModel.ViewModels.Department;
using AML.ViewModel.ViewModels.Designation;
using AML.ViewModel.ViewModels.UserGroup;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.User
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "First Name is required.")]
        public string FName { get; set; }
        [Required(ErrorMessage = "Last Name is required.")]
        public string LName { get; set; }
        [Required(ErrorMessage = "Employee Code is required.")]
        public string EmpCode { get; set; }
        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }
        //[Required(ErrorMessage = "Password is required.")]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string RePassword { get; set; }
        public string Email { get; set; }
        public string Remarks { get; set; }
        [Required(ErrorMessage = "Designation is required.")]
        public string DesignationId { get; set; }
        [Required(ErrorMessage = "Department is required.")]
        public string DepartmentId { get; set; }
        [Required(ErrorMessage = "User group is required.")]
        public string UserGroupId { get; set; }
        [Required(ErrorMessage = "Default branch is required.")]
        public string BranchId { get; set; }
        public int CountryId { get; set; }
        public int IdentityTypeId { get; set; }
        public int IsActive { get; set; }
        public int IsDeleted { get; set; }
        public int IsBlocked { get; set; }
        public int CreatedBy { get; set; }
        public int UpdatedBy { get; set; }
        public string CreatedOn { get; set; }
        public string UpdatedOn { get; set; }
        public UserDetailModel UserDetail { get; set; }
        public SelectList Departments { get; set; }
        public SelectList Designations { get; set; }
        public SelectList UserGroup { get; set; }
        public SelectList Branches { get; set; }
        public SelectList Countries { get; set; }
        public SelectList VisaTypes { get; set; }
        public SelectList IdentityTypes { get; set; }
        public int OTP { get; set; }
        public int ClientId { get; set; }
        public SelectList Clients { get; set; }
        public int isSuperAdmin { get; set; }
        public string ClientName { get; set; }
        public string imgSrc { get; set; }
        public string UserContactDetails { get; set; }
        public string UserIdentityDetails { get; set; }
        public string UserVisaDetails { get; set; }
    }
    public class UserPasswordLogModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string ChangeBy { get; set; }
        public string ChangedOn { get; set; }
    }
}
