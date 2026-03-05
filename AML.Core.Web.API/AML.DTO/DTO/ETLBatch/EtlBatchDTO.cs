using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.EtlBatch
{
    public class EtlBatchDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("file_name")]
        public string FileName { get; set; }
        [Column("file_full_path")]
        public string FileFullPath { get; set; }
        [Column("total_rows")]
        public int TotalRows { get; set; }
        [Column("rows_recorded")]
        public int RowsRecorded { get; set; }
        [Column("added_by")]
        public int AddedBy { get; set; }
        [Column("added_by_name")]
        public string AddedUser { get; set; }
        [Column("added_on")]
        public DateTime AddedOn { get; set; }
        [Column("type")]
        public int Type { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
    }
}
