using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSRulesDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("rule_name")]
        public string RuleName { get; set; }
        public List<TransactionCategoryDTO> TransactionCategory { get; set; }
        public List<AllowedTransactionTypeDTO> AllowedTransactionTypes { get; set; }

        [Column("weight")]
        public int Weight { get; set; }
        [Column("max_allowed")]
        public int MaxAllowed { get; set; }
        [Column("check_for_company")]
        public bool CheckForCompany { get; set; }
        [Column("minimum_amount")]
        public int MinimumAmount { get; set; }
        [Column("days_company")]
        public int DaysCompany { get; set; }
        [Column("threshold_amount")]
        public int ThresholdAmount { get; set; }

        [Column("threshold")]
        public int Threshold { get; set; }
        [Column("scale")]
        public int Scale { get; set; }
        [Column("current_start_date")]
        public string CurrentStartDate { get; set; }
        [Column("current_end_date")]
        public string CurrentEndDate { get; set; }
        [Column("check_against_start_date")]
        public string CheckAgainstStartDate { get; set; }
        [Column("check_against_end_date")]
        public string CheckAgainstEndDate { get; set; }

    }
    public class TransactionCategoryDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("transaction_category")]
        public string TransactionCategory { get; set; }
        [Column("customer_count")]
        public int Customer { get; set; }
        [Column("start_date")]
        public string StartDate { get; set; }
        [Column("end_date")]
        public string EndDate { get; set; }
        [Column("days_count")]
        public int DaysCount { get; set; }
        [Column("transactions_count")]
        public int TransactionsCount { get; set; }
        [Column("max_allowed")]
        public int MaxAllowed { get; set; }

    }
    public class AllowedTransactionTypeDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("allowed_transaction_type")]
        public string AllowedTransactionType { get; set; }
    }

    public class TMSRulesMaster
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public int RuleScore { get; set; }
        public string DetailString { get; set; }
        public int IsActive { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    public class TMSRulesDetails
    {
        public string AutoId { get; set; }
        public int MasterId { get; set; }
        public string RuleType { get; set; }
        public string Source { get; set; }
        public string SourceAggregate { get; set; }
        public string Operation { get; set; }
        public string Target { get; set; }
        public string TargetAggregate { get; set; }
        public string Value { get; set; }
        public string TimeFrameSource { get; set; }
        public int TimeFrameValue { get; set; }
        public string TimeFrameType { get; set; }
        public string CompareTo { get; set; }
        public string CompareValue { get; set; }
        public string CompareField { get; set; }
        public string CompareFieldAggregate { get; set; }
        public int CompareTimeStartValue { get; set; }
        public string CompareTimeStartType { get; set; }
        public int CompareTimeEndValue { get; set; }
        public string CompareTimeEndType { get; set; }
        public string ForSame { get; set; }
        public int Status { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public int UpdatedBy { get; set; }
        public DateTime UpdatedOn { get; set; }
    }
}
