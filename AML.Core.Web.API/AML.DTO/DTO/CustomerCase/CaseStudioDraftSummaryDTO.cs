using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.CustomerCase
{
    public class CaseStudioDraftSummaryDTO
    {
        [Column("DraftId")]
        public string DraftId { get; set; }

        [Column("DraftName")]
        public string DraftName { get; set; }

        [Column("NodeCount")]
        public int NodeCount { get; set; }

        [Column("EdgeCount")]
        public int EdgeCount { get; set; }

        [Column("UpdatedOn")]
        public DateTime UpdatedOn { get; set; }
    }
}

