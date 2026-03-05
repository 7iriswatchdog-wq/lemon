using AML.ViewModel.ViewModels.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.InternalWathcList
{
    public class WatchListModel
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Name required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Nationality/Country of Incorporation is required")]
        public int NationalityId { get; set; }
        
        public string DOB { get; set; }
        public string Nationality { get; set; }
        [Required(ErrorMessage = "Source is required")]
        public string Source { get; set; }
        [Required(ErrorMessage = "Type is required")]
        public string Type { get; set; }

        public string category { get; set; }
        public string IdNumber { get; set; }
        public int CreatedBy { get; set; }
        public SelectList Nationalities { get; set; }
        public SelectList SourceList { get; set; }
        public DocumentUploadModel Document { get; set; } 
        public List<ExcelData> ExcelData { get; set; } = new List<ExcelData>();
        public int ClientId { get; set; }

        public string Remarks { get; set; }

        public string UID { get; set; }

      


    }

    
    public class ExcelData
    {
        public string CustomerId { get; set; }
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string DOB { get; set; }

        public string Remarks { get; set; }
    }

    public class WatchList
    {
        
        public string FULLNAME { get; set; }
        public string DOB { get; set; }
        public string NATIONALITY { get; set; }
        
        public string TYPE { get; set; }
        public string IDNUMBER { get; set; }
        public string UID { get; set; }
        public string CATEGORY { get; set; }
        public int CLIENTID { get; set; }

        public string REMARKS { get; set; }
    }


}
