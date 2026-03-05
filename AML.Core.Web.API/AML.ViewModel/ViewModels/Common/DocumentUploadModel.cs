using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using AML.DTO.DTO.Risk;
using AML.ViewModel.ViewModels.CodesMaster;
using Microsoft.AspNetCore.Http;
namespace AML.ViewModel.ViewModels.Common
{
    public class DocumentUploadModel
    {
        public int itemId { get; set; }
        public int branchId { get; set; }
        [Required(ErrorMessage = "Source document required")]
        public IFormFile fileUpload { get; set; }
        public string typeId { get; set; }
        public int ClientId { get; set; }
        public int C6Threshold { get; set; }
        public int Threshold { get; set; }

        public string CompanyCode { get; set; }
        public List<RiskTypeCategoryDTO> RiskTypeCategoryDTO { get; set; }
        public List<CodesTableModel> CodesTables { get; set; }
        public List<string> CodeNames { get; set; }= new List<string>();

        public List<bool> IsChecked { get; set; } = new List<bool>();

        public bool IsPep { get; set; } = false;
        public bool IsSan { get; set; } = false;
		public bool IsRre { get; set; } = false;
		public bool IsIns { get; set; } = false;
		public bool IsDd { get; set; } = false;
		public bool IsPoi { get; set; } = false;
		public bool IsRel { get; set; } = false;

	}
}
