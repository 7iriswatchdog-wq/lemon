using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.ProliferationFinance
{
    public class ProliferationFinanceCaseDTO
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("CustomerType")]
        public string CustomerType { get; set; }
        [Column("CorporateId")]
        public string CorporateId { get; set; }
        [Column("CompanyName")]
        public string CompanyName { get; set; }
        [Column("HsCode")]
        public string HsCode { get; set; }
        [Column("CasNumber")]
        public string CasNumber { get; set; }
        [Column("Eccn")]
        public string Eccn { get; set; }
        [Column("ChemicalName")]
        public string ChemicalName { get; set; }
        [Column("SynonymName")]
        public string SynonymName { get; set; }
        [Column("CreatedOn")]
        public DateTime CreatedOn { get; set; }
        [Column("CreatedBy")]
        public int CreatedBy { get; set; }
        [Column("SourceList")]
        public string SourceList { get; set; }
        [Column("Status")]
        public string Status { get; set; }
        [Column("UpdatedOn")]
        public DateTime? UpdatedOn { get; set; }
        [Column("Score")]
        public string Score { get; set; }
        [Column("StatusReason")]
        public string StatusReason { get; set; }
        [Column("Type")]
        public string Type { get; set; } // Corporate/Individual
        [Column("MatchedChemicalName")]
        public string MatchedChemicalName { get; set; }
        [Column("SearchHitDetails")]
        public string SearchHitDetails { get; set; }
    }
}
