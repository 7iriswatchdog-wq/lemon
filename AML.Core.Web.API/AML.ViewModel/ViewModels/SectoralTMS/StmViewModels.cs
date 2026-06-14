using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AML.ViewModel.ViewModels.SectoralTMS
{
    // ----- Sector -----
    public class StmSectorModel
    {
        public int Id { get; set; }
        public string SectorCode { get; set; }
        public string SectorName { get; set; }
        public string Description { get; set; }
        public int IsActive { get; set; }
    }

    // ----- Rule -----
    public class StmRuleModel
    {
        public int Id { get; set; }
        public string RuleCode { get; set; }
        public string RuleName { get; set; }
        public string RuleDescription { get; set; }
        public int SectorId { get; set; }
        public string SectorCode { get; set; }
        public string SectorName { get; set; }
        public string RiskRating { get; set; }
        public int RuleScore { get; set; }
        public string LogicalOperator { get; set; }
        public string ActionOnHit { get; set; }
        public int IsActive { get; set; }
        public int IsSystemRule { get; set; }
        public int ClientId { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? CreatedBy { get; set; }
        public List<StmRuleConditionModel> Conditions { get; set; } = new List<StmRuleConditionModel>();
        public SelectList SectorList { get; set; }
    }

    public class StmRuleConditionModel
    {
        public int Id { get; set; }
        public int RuleId { get; set; }
        public int SequenceNo { get; set; }
        public string FieldName { get; set; }
        public string FieldAggregation { get; set; }
        public string Operator { get; set; }
        public string CompareValue { get; set; }
        public string CompareField { get; set; }
        public int? TimeframeValue { get; set; }
        public string TimeframeUnit { get; set; }
        public string Conjunction { get; set; } = "AND";
        public string Description { get; set; }
    }

    // ----- Transaction -----
    public class StmTransactionModel
    {
        public int Id { get; set; }
        public string TranRefNo { get; set; }
        public int SectorId { get; set; }
        public string SectorCode { get; set; }
        public string SectorName { get; set; }
        public DateTime TranDate { get; set; } = DateTime.Now;
        public string TranType { get; set; }
        public string TranMode { get; set; }
        public string DeliveryChannel { get; set; }
        public string Product { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "AED";
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerType { get; set; }
        public string CustomerNationality { get; set; }
        public string RemitterId { get; set; }
        public string RemitterName { get; set; }
        public string RemitterCountry { get; set; }
        public string BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryCountry { get; set; }
        public string PolicyNo { get; set; }
        public DateTime? PolicyInceptionDate { get; set; }
        public string PropertyRef { get; set; }
        public decimal? PropertyValue { get; set; }
        public DateTime? PropertyPurchaseDate { get; set; }
        public string Purpose { get; set; }
        public string BranchCode { get; set; }
        public int IsHighRiskCountry { get; set; }
        public int IsHighRiskCustomer { get; set; }
        public int HasPep { get; set; }
        public int IsMultiParty { get; set; }
        public string RuleHitStatus { get; set; }
        public int ClientId { get; set; }
        public List<StmTransactionPartyModel> Parties { get; set; } = new List<StmTransactionPartyModel>();
        public SelectList SectorList { get; set; }
    }

    public class StmTransactionPartyModel
    {
        public int Id { get; set; }
        public int TransactionId { get; set; }
        public string PartyRole { get; set; }
        public string CustomerId { get; set; }
        public int? CustomerMasterId { get; set; }
        public string CustomerName { get; set; }
        public string Nationality { get; set; }
        public string CustomerType { get; set; }
        public string IdType { get; set; }
        public string IdNumber { get; set; }
        public string Mobile { get; set; }
        public string Address { get; set; }
        public string RelationToPrimary { get; set; }
        public decimal? SharePercentage { get; set; }
    }

    // ----- Case -----
    public class StmCaseModel
    {
        public int Id { get; set; }
        public string CaseRefNo { get; set; }
        public int TransactionId { get; set; }
        public int SectorId { get; set; }
        public string SectorCode { get; set; }
        public string SectorName { get; set; }
        public string RulesViolated { get; set; }
        public string RuleNames { get; set; }
        public int TotalRiskScore { get; set; }
        public string RiskRating { get; set; }
        public string Status { get; set; }
        public int? AssignedTo { get; set; }
        public int? ReviewedBy { get; set; }
        public DateTime? ReviewedOn { get; set; }
        public string ReviewDecision { get; set; }
        public string ReviewRemarks { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string TranRefNo { get; set; }
        public DateTime? TranDate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public List<StmCaseCommentModel> Comments { get; set; } = new List<StmCaseCommentModel>();
        public StmTransactionModel Transaction { get; set; }
    }

    public class StmCaseCommentModel
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public string CommentText { get; set; }
        public string ActionType { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedUser { get; set; }
    }

    public class StmReportFilterModel
    {
        public string SectorCode { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<StmTransactionModel> Transactions { get; set; } = new List<StmTransactionModel>();
        public List<StmCaseModel> Cases { get; set; } = new List<StmCaseModel>();
        public SelectList SectorList { get; set; }
    }

    public class StmRuleTestRequestModel
    {
        public StmRuleModel Rule { get; set; }
        public StmTransactionModel Transaction { get; set; }
    }
}
