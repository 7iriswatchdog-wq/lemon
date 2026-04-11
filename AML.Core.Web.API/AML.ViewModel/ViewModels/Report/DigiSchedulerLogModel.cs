using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Report
{
    public class DigiSchedulerLogModel
    {
        
        public string Id { get; set; }
      
        public string Source { get; set; }
      
        public string CreatedOn { get; set; }
   
        public int TotalHits { get; set; }
     
        public int TotalRecords { get; set; }
        public int ClientId { get; set; }

        public string SchedulerTrackerId { get; set; }
        public int CumulativeHits { get; set; }
    }
}
