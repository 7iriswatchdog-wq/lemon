using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Report
{
    public class UploadLogsListDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("source")]
        public string Source { get; set; }
        [Column("createdon")]
        public string CreatedOn { get; set; }
        [Column("totalrecords")]
        public string TotalRecords { get; set; }
    }
}
