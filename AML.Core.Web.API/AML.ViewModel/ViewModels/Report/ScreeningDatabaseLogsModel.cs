using AML.ViewModel.ViewModels.Report;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.Web.Controllers.Reports
{
    public class ScreeningDatabaseLogsModel
    {
        public int Id { get; set; }

        public int Individual { get; set; }

        
        public int Corporate { get; set; }
        
        public int Deleted { get; set; }
        
        public string UpdatedDate { get; set; }
    }

    public class ScreeningDatabaseReportDownloadModel
    {
        public string UploadedBy { get; set; }
        public int TotalRows { get; set; }
        public int Matched { get; set; }
        public int UnMatched { get; set; }
        public List<ScreeningDatabaseLogsModel> Data { get; set; }

    }

    
}