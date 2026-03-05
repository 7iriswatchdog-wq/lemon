using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.InternalWatchListExcel
{
    public class InternalWatcListExcelDTO
    {
        public string CustomerID { get; set; }
        public string FullName { get; set; }
        public string Nationality { get; set; }
        public string Source { get; set; }
        public string Type { get; set; }
        public string DOB { get; set; }

        public string REMARKS { get; set; }
    }


    public class SourceUploadLogsDTO
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Source { get; set; }
        public DateTime CreatedOn { get; set; }
        public int TotalRecords { get; set; }
    }
}
