using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Common
{
    public class ApplicationSetupDTO
    {
        [Column("ID")]
        public int ID { get; set; }
        [Column("setup_group")]
        public string SetupGroup { get; set; }
        [Column("catogory_key")]
        public string CategoryKey { get; set; }
        [Column("setup_catogory")]
        public string SetupCategory { get; set; }
        [Column("catogory_value")]
        public string CategoryValue { get; set; }
        [Column("catogory_details")]
        public string CategoryDetail { get; set; }
    }
}
