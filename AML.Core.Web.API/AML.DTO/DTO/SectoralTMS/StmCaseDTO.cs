using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.SectoralTMS
{
    [Table("stm_case")]
    public class StmCaseDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("case_ref_no")]
        public string CaseRefNo { get; set; }

        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("sector_id")]
        public int SectorId { get; set; }

        [Column("rules_violated")]
        public string RulesViolated { get; set; }

        [Column("rule_names")]
        public string RuleNames { get; set; }

        [Column("total_risk_score")]
        public int TotalRiskScore { get; set; }

        [Column("risk_rating")]
        public string RiskRating { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("assigned_to")]
        public int? AssignedTo { get; set; }

        [Column("reviewed_by")]
        public int? ReviewedBy { get; set; }

        [Column("reviewed_on")]
        public DateTime? ReviewedOn { get; set; }

        [Column("review_decision")]
        public string ReviewDecision { get; set; }

        [Column("review_remarks")]
        public string ReviewRemarks { get; set; }

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

        // Joined display fields
        [Column("tran_ref_no")]
        public string TranRefNo { get; set; }

        [Column("tran_date")]
        public DateTime? TranDate { get; set; }

        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; }

        [Column("amount")]
        public decimal? Amount { get; set; }

        [Column("currency")]
        public string Currency { get; set; }

        [Column("sector_name")]
        public string SectorName { get; set; }

        [Column("sector_code")]
        public string SectorCode { get; set; }

        [Column("reviewed_by_user")]
        public string ReviewedByUser { get; set; }

        [Column("assigned_to_user")]
        public string AssignedToUser { get; set; }

        [NotMapped]
        public List<StmCaseCommentDTO> Comments { get; set; } = new List<StmCaseCommentDTO>();

        [NotMapped]
        public StmTransactionDTO Transaction { get; set; }
    }

    [Table("stm_case_comment")]
    public class StmCaseCommentDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("case_id")]
        public int CaseId { get; set; }

        [Column("comment_text")]
        public string CommentText { get; set; }

        [Column("action_type")]
        public string ActionType { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }

        [Column("created_by")]
        public int? CreatedBy { get; set; }

        [Column("created_user")]
        public string CreatedUser { get; set; }
    }

    [Table("stm_rule_exec_log")]
    public class StmRuleExecLogDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("rule_id")]
        public int RuleId { get; set; }

        [Column("is_hit")]
        public int IsHit { get; set; }

        [Column("hit_details")]
        public string HitDetails { get; set; }

        [Column("executed_on")]
        public DateTime? ExecutedOn { get; set; }
    }

    // ----- Request / Search DTOs ------------------------------------------
    public class StmCaseSearchDTO
    {
        public string SectorCode { get; set; }
        public string Status { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string RiskRating { get; set; }
        public int ClientId { get; set; }
    }

    public class StmTransactionSearchDTO
    {
        public string SectorCode { get; set; }
        public string CustomerId { get; set; }
        public string TranRefNo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string TranType { get; set; }
        public string RuleHitStatus { get; set; }
        public int ClientId { get; set; }
    }
}
