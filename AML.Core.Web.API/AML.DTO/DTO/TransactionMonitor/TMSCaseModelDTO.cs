using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSCaseModelDTO
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("CustomerId")]
        public string CustomerId { get; set; }
        [Column("CustomerName")]
        public string CustomerName { get; set; }
        [Column("CustomerRiskScore")]
        public int CustomerRiskScore { get; set; }

        [Column("EntityId")]
        public int EntityId { get; set; }
        [Column("EntityBranch")]
        public string EntityBranch { get; set; }
        [Column("UserId")]
        public string UserId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("created_on")]
        public string CreatedOn { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("remarks")]
        public string Remarks { get; set; }
        [Column("updated_by")]
        public int UpdatedBy { get; set; }
        [Column("updated_on")]
        public string UpdatedOn { get; set; }
        [Column("updated_status")]
        public string UpdatedStatus { get; set; }
        [Column("UpdatedByUser")]
        public string UpdatedByUSer { get; set; }
        [Column("batch_id")]
        public int BatchId { get; set; }
        public List<TmsTransactionDTO> TmsTransactionModel { get; set; }
        public List<TmsRuleViolatedDTO> RuleViolated { get; set; }
        [Column("RuleViolatedText")]
        public string RuleViolatedText { get; set; }
    }
    public class TmsRuleViolatedDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("rules_violated")]
        public string RuleName { get; set; }
        [Column("batch_id")]
        public int BatchId { get; set; }

    }
    public class TmsTransactionDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("CustomerId")]
        public string CustomerId { get; set; }
        [Column("transaction_date")]
        public string TransactionDate { get; set; }
        [Column("transaction_number")]
        public string TransactionNumber { get; set; }
        [Column("internal_ref_number")]
        public string InternalRefNumber { get; set; }
        [Column("transaction_location")]
        public string TransactionLocation { get; set; }
        [Column("authorizer")]
        public string Authorizer { get; set; }
        [Column("currency")]
        public string Currency { get; set; }
        [Column("amount")]
        public string Amount { get; set; }
        [Column("product_code")]
        public string ProductCode { get; set; }
        [Column("product")]
        public string Product { get; set; }
        [Column("beneficiary")]
        public string Beneficiary { get; set; }
        [Column("goods_services")]
        public string GoodServices { get; set; }
        [Column("transaction_description")]
        public string TransactionDescription { get; set; }
        [Column("user")]
        public string User { get; set; }
    }
}
