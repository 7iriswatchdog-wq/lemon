using AML.ViewModel.ViewModels.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.FreeSource
{
    public class FreeSourceModel
    {
        public string Source { get; set; }
        public int CreatedBy { get; set; }
        [Required(ErrorMessage ="Source Document required")]
        public DocumentUploadModel Document { get; set; }
    }
}
