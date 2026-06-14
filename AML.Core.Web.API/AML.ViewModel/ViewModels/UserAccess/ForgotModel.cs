using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.UserAccess
{
    public class ForgotModel
    {
        [Required(ErrorMessage = "User Name is required.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Company Name is required.")]
        public string ClientName { get; set; }
    }


    public class ChangePasswordModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Old Password is required.")]
        public string OldPassword { get; set; }
        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
        public string RepeatPassword { get; set; }
        public string ReturnToken { get; set; }
        public string UserName { get; set; }
    }
    public class ResetPasswordModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "New Password is required.")]
        public string NewPassword { get; set; }
        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match.")]
        public string RepeatPassword { get; set; }
    }

     public class VerifyOTPModel
    {
        [Required(ErrorMessage = "OTP is required.")]
        public int Otp { get; set; }
        public string UserName { get; set; }
    }
}
