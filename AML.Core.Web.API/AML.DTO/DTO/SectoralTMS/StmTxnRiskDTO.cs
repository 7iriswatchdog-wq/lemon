using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.SectoralTMS
{
    [Table("stm_txn_risk_factor")]
    public class StmTxnRiskFactorDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("factor_code")]
        public string FactorCode { get; set; }
        [Column("factor_name")]
        public string FactorName { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("sector_id")]
        public int SectorId { get; set; }
        [Column("field_name")]
        public string FieldName { get; set; }
        [Column("factor_type")]
        public string FactorType { get; set; }
        [Column("weight")]
        public int Weight { get; set; }
        [Column("is_active")]
        public int IsActive { get; set; }
        [Column("sequence_no")]
        public int SequenceNo { get; set; }
        [Column("client_id")]
        public int ClientId { get; set; }
        [Column("created_on")]
        public DateTime? CreatedOn { get; set; }
        [Column("created_by")]
        public int? CreatedBy { get; set; }

        // Display / join columns
        [Column("sector_code")]
        public string SectorCode { get; set; }
        [Column("sector_name")]
        public string SectorName { get; set; }

        [NotMapped]
        public List<StmTxnRiskBandDTO> Bands { get; set; } = new List<StmTxnRiskBandDTO>();
    }

    [Table("stm_txn_risk_band")]
    public class StmTxnRiskBandDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("factor_id")]
        public int FactorId { get; set; }
        [Column("band_label")]
        public string BandLabel { get; set; }
        [Column("numeric_min")]
        public decimal? NumericMin { get; set; }
        [Column("numeric_max")]
        public decimal? NumericMax { get; set; }
        [Column("match_value")]
        public string MatchValue { get; set; }
        [Column("match_in_list")]
        public string MatchInList { get; set; }
        [Column("score")]
        public int Score { get; set; }
        [Column("rating")]
        public string Rating { get; set; }
        [Column("sequence_no")]
        public int SequenceNo { get; set; }
    }

    [Table("stm_txn_risk_result")]
    public class StmTxnRiskResultDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("transaction_id")]
        public int TransactionId { get; set; }
        [Column("sector_id")]
        public int SectorId { get; set; }
        [Column("total_score")]
        public int TotalScore { get; set; }
        [Column("risk_rating")]
        public string RiskRating { get; set; }
        [Column("factor_count")]
        public int FactorCount { get; set; }
        [Column("factor_breakdown")]
        public string FactorBreakdown { get; set; }
        [Column("client_id")]
        public int ClientId { get; set; }
        [Column("computed_on")]
        public DateTime? ComputedOn { get; set; }
    }

    public class StmTxnRiskItemResult
    {
        public string FactorCode { get; set; }
        public string FactorName { get; set; }
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
        public string BandLabel { get; set; }
        public int Score { get; set; }
        public string Rating { get; set; }
        public int Weight { get; set; }
        public int WeightedScore { get; set; }
    }

    public class StmTxnRiskEvaluation
    {
        public int TransactionId { get; set; }
        public string TranRefNo { get; set; }
        public int TotalScore { get; set; }
        public int MaxPossibleScore { get; set; }
        public string RiskRating { get; set; }   // Low / Medium / High
        public int FactorCount { get; set; }
        public List<StmTxnRiskItemResult> Items { get; set; } = new List<StmTxnRiskItemResult>();
    }
}
