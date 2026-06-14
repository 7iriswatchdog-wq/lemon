using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.SectoralTMS
{
    [Table("stm_rule")]
    public class StmRuleDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("rule_code")]
        public string RuleCode { get; set; }

        [Column("rule_name")]
        public string RuleName { get; set; }

        [Column("rule_description")]
        public string RuleDescription { get; set; }

        [Column("sector_id")]
        public int SectorId { get; set; }

        [Column("risk_rating")]
        public string RiskRating { get; set; }

        [Column("rule_score")]
        public int RuleScore { get; set; }

        [Column("logical_operator")]
        public string LogicalOperator { get; set; }

        [Column("action_on_hit")]
        public string ActionOnHit { get; set; }

        [Column("is_active")]
        public int IsActive { get; set; }

        [Column("is_system_rule")]
        public int IsSystemRule { get; set; }

        [Column("client_id")]
        public int ClientId { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("updated_on")]
        public DateTime? UpdatedOn { get; set; }

        [Column("updated_by")]
        public int? UpdatedBy { get; set; }

        // Joined/Display fields
        [Column("sector_name")]
        public string SectorName { get; set; }

        [Column("sector_code")]
        public string SectorCode { get; set; }

        [NotMapped]
        public List<StmRuleConditionDTO> Conditions { get; set; } = new List<StmRuleConditionDTO>();
    }

    [Table("stm_rule_condition")]
    public class StmRuleConditionDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("rule_id")]
        public int RuleId { get; set; }

        [Column("sequence_no")]
        public int SequenceNo { get; set; }

        [Column("field_name")]
        public string FieldName { get; set; }

        [Column("field_aggregation")]
        public string FieldAggregation { get; set; }

        [Column("operator")]
        public string Operator { get; set; }

        [Column("compare_value")]
        public string CompareValue { get; set; }

        [Column("compare_field")]
        public string CompareField { get; set; }

        [Column("timeframe_value")]
        public int? TimeframeValue { get; set; }

        [Column("timeframe_unit")]
        public string TimeframeUnit { get; set; }

        [Column("conjunction")]
        public string Conjunction { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }
    }
}
