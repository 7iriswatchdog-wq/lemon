using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.SectoralTMS
{
    [Table("stm_transaction")]
    public class StmTransactionDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("tran_ref_no")]
        public string TranRefNo { get; set; }

        [Column("sector_id")]
        public int SectorId { get; set; }

        [Column("tran_date")]
        public DateTime TranDate { get; set; }

        [Column("tran_type")]
        public string TranType { get; set; }

        [Column("tran_mode")]
        public string TranMode { get; set; }

        [Column("delivery_channel")]
        public string DeliveryChannel { get; set; }

        [Column("product")]
        public string Product { get; set; }

        [Column("amount")]
        public decimal Amount { get; set; }

        [Column("currency")]
        public string Currency { get; set; }

        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; }

        [Column("customer_type")]
        public string CustomerType { get; set; }

        [Column("customer_nationality")]
        public string CustomerNationality { get; set; }

        [Column("remitter_id")]
        public string RemitterId { get; set; }

        [Column("remitter_name")]
        public string RemitterName { get; set; }

        [Column("remitter_country")]
        public string RemitterCountry { get; set; }

        [Column("beneficiary_id")]
        public string BeneficiaryId { get; set; }

        [Column("beneficiary_name")]
        public string BeneficiaryName { get; set; }

        [Column("beneficiary_country")]
        public string BeneficiaryCountry { get; set; }

        [Column("policy_no")]
        public string PolicyNo { get; set; }

        [Column("policy_inception_date")]
        public DateTime? PolicyInceptionDate { get; set; }

        [Column("property_ref")]
        public string PropertyRef { get; set; }

        [Column("property_value")]
        public decimal? PropertyValue { get; set; }

        [Column("property_purchase_date")]
        public DateTime? PropertyPurchaseDate { get; set; }

        [Column("purpose")]
        public string Purpose { get; set; }

        [Column("branch_code")]
        public string BranchCode { get; set; }

        [Column("is_high_risk_country")]
        public int IsHighRiskCountry { get; set; }

        [Column("is_high_risk_customer")]
        public int IsHighRiskCustomer { get; set; }

        [Column("has_pep")]
        public int HasPep { get; set; }

        [Column("is_multi_party")]
        public int IsMultiParty { get; set; }

        [Column("rule_hit_status")]
        public string RuleHitStatus { get; set; }

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

        [NotMapped]
        public List<StmTransactionPartyDTO> Parties { get; set; } = new List<StmTransactionPartyDTO>();

        [Column("sector_name")]
        public string SectorName { get; set; }
    }

    [Table("stm_transaction_party")]
    public class StmTransactionPartyDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("transaction_id")]
        public int TransactionId { get; set; }

        [Column("party_role")]
        public string PartyRole { get; set; }

        [Column("customer_id")]
        public string CustomerId { get; set; }

        [Column("customer_master_id")]
        public int? CustomerMasterId { get; set; }

        [Column("customer_name")]
        public string CustomerName { get; set; }

        [Column("nationality")]
        public string Nationality { get; set; }

        [Column("customer_type")]
        public string CustomerType { get; set; }

        [Column("id_type")]
        public string IdType { get; set; }

        [Column("id_number")]
        public string IdNumber { get; set; }

        [Column("mobile")]
        public string Mobile { get; set; }

        [Column("address")]
        public string Address { get; set; }

        [Column("relation_to_primary")]
        public string RelationToPrimary { get; set; }

        [Column("share_percentage")]
        public decimal? SharePercentage { get; set; }

        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }
    }
}
