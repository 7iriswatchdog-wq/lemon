using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Text;

namespace AML.DTO.DTO.CaseComment
{
    public class CaseCommentDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("case_id")]
        public string CaseId { get; set; }
        [Column("comment")]
        public string Comment { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("created_user")]
        public string CreatedUser { get; set; }
        [Column("duration_in_words")]
        public string Duration { get; set; }
        [Column("comment_type")]
        public string CommentType { get; set; }
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
