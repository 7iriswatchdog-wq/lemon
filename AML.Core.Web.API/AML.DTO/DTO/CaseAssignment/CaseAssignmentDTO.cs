using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CaseAssignment
{
    public class CaseAssignmentDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("case_id")]
        public int CaseId { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("comment")]
        public string Comment { get; set; }
        [Column("created_on")]
        public DateTime? CreatedOnDB { get; set; }
        public string CreatedOn
        {
            get
            {
                return CreatedOnDB.ToUIDDateFormat();
            }
            set
            {
                CreatedOnDB = value.ParseDB();
            }
        }
    }

}
