using AML.ViewModel.ViewModels.Report;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.Web.Controllers.Reports
{
    public class DatasetUpdateLogsModel
    {
        public int Id { get; set; }

        public string Datasets { get; set; }

        public string Delta { get; set; }
        
        public string Humiliated { get; set; }
        
        public string Action { get; set; }
        
        public string UpdatedDate { get; set; }
    }

    public class DatasetUpdateReportDownloadModel
    {
        public string UploadedBy { get; set; }
        public int TotalRows { get; set; }
        public List<DatasetUpdateLogsModel> Data { get; set; }

    }

    
}
