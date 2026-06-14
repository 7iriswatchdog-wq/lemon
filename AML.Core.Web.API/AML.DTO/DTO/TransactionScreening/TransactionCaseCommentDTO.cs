using AML.Core.Common.StaticResource;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionScreening
{
    public class TransactionCaseCommentDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("case_id")]
        public string CaseId { get; set; }

        [Column("tranrefno")]
        public string TranRefNo { get; set; }
        [Column("comment")]
        public string Comment { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("created_user")]
        public string CreatedUser { get; set; }
        [Column("duration_in_words")]
        public string Duration { get; set; }
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

