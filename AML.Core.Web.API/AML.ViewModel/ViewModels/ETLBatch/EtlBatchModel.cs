using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.ViewModel.ViewModels.EtlBatch
{
    public class EtlBatchModel
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public string FileName { get; set; }
        public string FileFullPath { get; set; }
        public string TotalRows { get; set; }
        public int RowsRecorded { get; set; }
        public int AddedBy { get; set; }        
        public string AddedUser { get; set; }
        public DateTime AddedOn { get; set; }
    }
}
