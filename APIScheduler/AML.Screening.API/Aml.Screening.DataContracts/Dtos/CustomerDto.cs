using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Aml.Screening.DataContracts.Dtos
{
    public class CustomerDto
    {
        /// <summary>
        /// Gets or sets the caseid.
        /// </summary>
        /// <value>
        /// The caseid.
        /// </value>
        [Display(Name = "Case ID :")]
        [Required(ErrorMessage = "Case ID Required")]
        public string CASEID { get; set; }
        /// <summary>
        /// Gets or sets the customercode.
        /// </summary>
        /// <value>
        /// The customercode.
        /// </value>
        [Display(Name = "Customer Code :")]
        //[Required(ErrorMessage="Customer Code Required")]
        //[MaxLength(25, ErrorMessage = "Must be Maximum 25 Characters")]
        // [RegularExpression("^([a-zA-Z0-9]+)$", ErrorMessage = "Invalid Customer Code")]
        public string CUSTOMERCODE { get; set; }
        /// <summary>
        /// Gets or sets the customerfullname.
        /// </summary>
        /// <value>
        /// The customerfullname.
        /// </value>
        [Display(Name = "Customer FullName:")]
        [Required(ErrorMessage = "Customer FullName Required")]
        [MaxLength(250, ErrorMessage = "Must be Maximum 250 Characters")]
        [RegularExpression("^([a-zA-Z .-]+)$", ErrorMessage = "Invalid Name")]
        public string CUSTOMERFULLNAME { get; set; }
        /// <summary>
        /// Gets or sets the customeridtype.
        /// </summary>
        /// <value>
        /// The customeridtype.
        /// </value>
        [Display(Name = "Customer Id Type Code:")]
        //[MaxLength(2, ErrorMessage = "Id Type Code Must be Maximum 2 Characters")]
        //[MinLength(1, ErrorMessage = "Id Type Code Must be Minimum 1 Characters")]
        //[RegularExpression("^[0-9]+", ErrorMessage = "Id Type Code Must be Numeric String")]
        public string CUSTOMERIDTYPE { get; set; }
        /// <summary>
        /// Gets or sets the customertype.
        /// </summary>
        /// <value>
        /// The customertype.
        /// </value>
        [Display(Name = "Customer category:")]
        public string CUSTOMERCATEGORY { get; set; }
        /// <summary>
        /// Gets or sets the customeridnumber.
        /// </summary>
        /// <value>
        /// The customeridnumber.
        /// </value>        
        //[RegularExpression("^[a-zA-Z0-9]*$", ErrorMessage = "Only Alphabets and Numbers allowed.")]
        [Display(Name = "Customer Id Number:")]
        public string CUSTOMERIDNUMBER { get; set; }
        /// <summary>
        /// Gets or sets the customertype.
        /// </summary>
        /// <value>
        /// The customertype.
        /// </value>
        //[Display(Name = "Customer Type:")]
        //[Required(ErrorMessage = "Customer Type Required")]
        //[MaxLength(50, ErrorMessage = "Customer Type Must Not Exceed 50 Characters")]
        //[RegularExpression("^([a-zA-Z .-]+)$", ErrorMessage = "Only Alphabets AND Space are allowed.")]
        //public string CUSTOMERTYPE { get; set; }
        /// <summary>
        /// Gets or sets the customercountry.
        /// </summary>
        /// <value>
        /// The customercountry.
        /// </value>
        [Display(Name = "Customer Country:")]
        //  [MaxLength(2, ErrorMessage = "Customer Country Code Must be Maximum 2 Characters")]
        //   [MinLength(2, ErrorMessage = "Customer Country Code Must be Minimum 2 Characters")]
        [RegularExpression("^([a-zA-Z ]+)$", ErrorMessage = "Only space and alphabets are allowed.")]
        public string CUSTOMERCOUNTRY { get; set; }
        /// <summary>
        /// Gets or sets the customernationality.
        /// </summary>
        /// <value>
        /// The customernationality.
        /// </value>        
        [Display(Name = "Customer Nationality:")]
        //[Required(ErrorMessage = "Customer Nationality Required")]
        //  [MaxLength(2, ErrorMessage = "Customer Nationality Code Must be Maximum 2 Characters")]
        //   [MinLength(2, ErrorMessage = "Customer Nationality Code Must be Minimum 2 Characters")]
        [RegularExpression("[a-zA-Z ]*", ErrorMessage = "Only space and alphabets are allowed.")]
        public string CUSTOMERNATIONALITY { get; set; }
        /// <summary>
        /// Gets or sets the customerdob.
        /// </summary>
        /// <value>
        /// The customerdob.
        /// </value>        
        [Display(Name = "Customer DOB:")]
        [Required(ErrorMessage = "DOB Required")]
        // [RegularExpression("^[0-9]{4}-(((0[13578]|(10|12))-(0[1-9]|[1-2][0-9]|3[0-1]))|(02-(0[1-9]|[1-2][0-9]))|((0[469]|11)-(0[1-9]|[1-2][0-9]|30)))$", ErrorMessage = "Invalid Date.")]
        public string CUSTOMERDOB { get; set; }
        /// <summary>
        /// Gets or sets the customermobilenumber.
        /// </summary>
        /// <value>
        /// The customermobilenumber.
        /// </value>        
        [Display(Name = "Mobile Number:")]
        [MaxLength(20, ErrorMessage = "Mobile Number must be less than 20 characters")]
        [RegularExpression("[0-9]*", ErrorMessage = "Invalid Mobile Number, No special characters allowed.")]
        public string CUSTOMERMOBILENUMBER { get; set; }
        /// <summary>
        /// Gets or sets the createdon.
        /// </summary>
        /// <value>
        /// The createdon.
        /// </value>
        [Display(Name = "Created Date:")]
        [MaxLength(20, ErrorMessage = "Date must be in dd/MM/yyyy HH:mm:ss Format")]
        [RegularExpression(@"^(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d)) (?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Date must be in dd/MM/yyyy HH:mm:ss Format.")]
        public string CREATEDON { get; set; }
        /// <summary>
        /// Gets or sets the updatedon.
        /// </summary>
        /// <value>
        /// The updatedon.
        /// </value>
        [Display(Name = "Updated Date:")]
        [MaxLength(20, ErrorMessage = "Date must be in dd/MM/yyyy HH:mm:ss Format")]
        [RegularExpression(@"^(((0|1)[0-9]|2[0-9]|3[0-1])\/(0[1-9]|1[0-2])\/((19|20)\d\d)) (?:0?[0-9]|1[0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]$", ErrorMessage = "Date must be in dd/MM/yyyy HH:mm:ss Format.")]
        public string UPDATEDON { get; set; }

        [Display(Name = "Cutoff Threshold:")]
        [Range(0, 99, ErrorMessage = "Threshold can only be a number and in range 0 - 99")]
        public int? THRESHOLD { get; set; }
        public int CLIENTID { get; set; }

        public string WHITELISTINGDATE { get; set; }

        public string WHITELISTING { get; set; }
    }
}
