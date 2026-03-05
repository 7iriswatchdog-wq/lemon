using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Country
{
    public class CountryDTO
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("Code")]
        public string Code { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Description")]

        public string Description { get; set; }
        [Column("Riskscore")]
        public decimal Riskscore { get; set; }
        [Column("RiskRating")]
        public int RiskRating { get; set; }
        [Column("risk")]
        public string Risk { get; set; }
        [Column("UNCODE")]
        public string UNCode { get; set; }
        [Column("FATFRISKRating")]
        public string FATFRiskRating { get; set; }
        [Column("fatfriskscore")]
        public string FATFRiskScore { get; set; }
        [Column("isocode3digit")]
        public string ISOCode3digit { get; set; }

        [Column("Is_active")]
        public bool IsActive { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
    }
}
