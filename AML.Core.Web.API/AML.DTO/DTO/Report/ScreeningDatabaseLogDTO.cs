using System.ComponentModel.DataAnnotations.Schema;

namespace AML.Web.Controllers.Reports
{
    public class ScreeningDatabaseLogDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("individual")]
        public int Individual { get; set; }

        [Column("corporate")]
        public int Corporate { get; set; }
        [Column("deleted")]
        public int Deleted { get; set; }
        [Column("updateddate")]
        public string UpdatedDate { get; set; }
    }
}