using System.ComponentModel.DataAnnotations.Schema;

namespace AML.Web.Controllers.Reports
{
    public class DatasetUpdateLogDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("datasets")]
        public string Datasets { get; set; }

        [Column("delta")]
        public string Delta { get; set; }
        
        [Column("humiliated")]
        public string Humiliated { get; set; }
        
        [Column("action")]
        public string Action { get; set; }
        
        [Column("updateddate")]
        public string UpdatedDate { get; set; }
    }
}
