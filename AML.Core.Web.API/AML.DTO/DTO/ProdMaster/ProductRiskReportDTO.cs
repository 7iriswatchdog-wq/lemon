using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.ProdMaster
{
    public class ProductRiskReportDTO
    {
        [Column("s_no")]
        public int Id { get; set; }
        [Column("productcode")]
        public string ProductCode { get; set; }
        [Column("productname")]
        public string ProductName { get; set; }
        [Column("final_score")]
        public string FinalScore { get; set; }
        [Column("score_before_override")]
        public string ScoreBeforeOverride { get; set; }
        [Column("assessment_version")]
        public string AssessmentVersion { get; set; }
        [Column("created_by")]
        public string User { get; set; }

    }
}
