using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Sanction
{
    public class SanctionScreeningLogDTO
    {
        [Column("id")]
        public int ID { get; set; }
        [Column("cust_name")]
        public string CustomerName { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        [Column("cust_type")]
        public string CustomerType { get; set; }
        [Column("dateofbirth")]
        public DateTime DOB { get; set; }
        [Column("search_type")]
        public string SearchType { get; set; }
        [Column("match_name")]
        public string MatchName { get; set; }
        [Column("match_score")]
        public int MatchScore { get; set; }
        [Column("match_uid")]
        public string MatchUID { get; set; }
        [Column("match_category")]
        public string MatchCategory { get; set; }
        [Column("match_type")]
        public string MatchType { get; set; }
        [Column("match_nationality")]
        public string MatchNationality { get; set; }
        [Column("match_idnum")]
        public string MatchIDNum { get; set; }
        [Column("match_dob")]
        public DateTime MatchDOB { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("created_on")]
        public DateTime CreatedOn { get; set; }
        [Column("op_rec_count")]
        public int RecordCount { get; set; }
        public int ClientId { get; set; }
        [Column("created_user")]
        public string CreatedUser { get; set; }
    }
}
