using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Risk
{
    public class RiskSummaryDTO
    {
        [Column("risk_type")]
        public string RiskType { get; set; }
        [Column("High_Risk_CountA")]
        public int HighRiskCount { get; set; }
        [Column("Medium_Risk_CountA")]
        public int MediumRiskCount { get; set; }
        [Column("Low_Risk_CountA")]
        public int LowRiskCount { get; set; }
    }
}
