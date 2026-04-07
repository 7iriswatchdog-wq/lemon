using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Report
{
    public class DigiSchedulerLogsDTO
    {

        [Column("id")]
        public string Id { get; set; }
        [Column("source")]
        public string Source { get; set; }
        [Column("created_on")]
        public string CreatedOn { get; set; }
        [Column ("total_hits")]
        public int TotalHits { get; set; }
        [Column("total_records")]
        public int TotalRecords { get; set; }


        [Column("trackerId")]
        public string SchedulerTrackerId { get; set; }

    }
}
