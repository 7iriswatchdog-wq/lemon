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

        [Column("delta")]
        public string Delta { get; set; }
        
        [Column("individualdelta")]
        public string Individual { get; set; }

        [Column("corporatedelta")]
        public string Corporate { get; set; }

        [Column("cumulative")]
        public string Cumulative { get; set; }



        [Column("createdon")]
        public DateTime UpdatedDate { get; set; }
    }
}
