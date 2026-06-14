using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Aml.Screening.DataContracts.Dtos
{
    public class SearchDto
    {
        /// <summary>
        /// Gets or sets the customerfullname.
        /// </summary>
        /// <value>
        /// The customerfullname.
        /// </value>
        [Display(Name = "Customer FullName:")]
        [Required(ErrorMessage = "Customer FullName Required")]
        [MaxLength(250, ErrorMessage = "Must be Maximum 250 Characters")]
        public string CUSTOMERFULLNAME { get; set; }

        /// <summary>
        /// Gets or sets the SEARCHtype.
        /// </summary>
        /// <value>
        /// The customertype.
        /// </value>
        [Display(Name = "Search Type:")]
        [Required(ErrorMessage = "Search Type Required")]
        [MaxLength(1, ErrorMessage = "Search Type Must Not Exceed 1 Characters")]
        public string SEARCHTYPE { get; set; }

        /// <summary>
        /// Gets or sets the customernationality.
        /// </summary>
        /// <value>
        /// The customernationality.
        /// </value>        
        [Display(Name = "Customer Nationality:")]
        [RegularExpression("[a-zA-Z ]*", ErrorMessage = "Only space and alphabets are allowed.")]
        public string CUSTOMERNATIONALITY { get; set; }
        /// <summary>
        /// Gets or sets the customerdob.
        /// </summary>
        /// <value>
        /// The customerdob.
        /// </value>        
        [Display(Name = "Customer DOB:")]
        public string CUSTOMERDOB { get; set; }

        [Display(Name = "Cutoff Threshold:")]
        [DefaultValue(0)]
        [Range(0, 99, ErrorMessage = "Threshold can only be a number and in range 0 - 99")]
        public int? THRESHOLD { get; set; }
        public int CLIENTID { get; set; }

        public string CUSTOMERTYPE { get; set; }
    }
    public class SearchResponseDto
    {
        public string MyProperty { get; set; }
    }
}
