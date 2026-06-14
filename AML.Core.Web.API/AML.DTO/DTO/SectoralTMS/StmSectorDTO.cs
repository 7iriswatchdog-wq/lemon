using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.SectoralTMS
{
    [Table("stm_sector")]
    public class StmSectorDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("sector_code")]
        public string SectorCode { get; set; }

        [Column("sector_name")]
        public string SectorName { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("is_active")]
        public int IsActive { get; set; }

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
    }
}
