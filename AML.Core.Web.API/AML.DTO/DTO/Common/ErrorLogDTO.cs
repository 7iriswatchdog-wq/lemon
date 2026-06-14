using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Common
{
    public  class ErrorLogDTO
    {
        [Column("id")]
        public int id { get; set; }
        [Column("module")]
        public string module { get; set; }
        [Column("Description")]
        public string description { get; set; }
        [Column("comments")]
        public string comments { get; set; }
        [Column("created_by")]
        public int createdBy { get; set; }
        [Column("created_on")]
        public DateTime created_on { get; set; }

        [Column("status_code")]
        public int status_code { get; set; }
    }
}
