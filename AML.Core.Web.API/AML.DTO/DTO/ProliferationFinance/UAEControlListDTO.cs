using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.ProliferationFinance
{
    public class UAEControlListDTO
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("CasNumber")]
        public string CasNumber { get; set; }
        [Column("SynonymName")]
        public string SynonymName { get; set; }
        [Column("HsCode")]
        public string HsCode { get; set; }
        [Column("ChemicalName")]
        public string ChemicalName { get; set; }
        [Column("Eccn")]
        public string Eccn { get; set; }
        [Column("PhysicalState")]
        public string PhysicalState { get; set; }
        [Column("PermitRequired")]
        public string PermitRequired { get; set; }
        [Column("Description")]
        public string Description { get; set; }
        [Column("MatchScore")]
        public int MatchScore { get; set; }
    }
}
