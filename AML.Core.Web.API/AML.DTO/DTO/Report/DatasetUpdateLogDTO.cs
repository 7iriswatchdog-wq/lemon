using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.Web.Controllers.Reports
{
    public class DatasetUpdateLogDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("datasets")]
        public string Datasets { get; set; }

        [Column("detla")]
        public string Delta { get; set; }
        
        [Column("humiliated")]
        public string Humiliated { get; set; }
        
        [Column("action")]
        public string Action { get; set; }
        
        [Column("createdon")]
        public DateTime UpdatedDate { get; set; }
    }
}
