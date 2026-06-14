using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.Sanction
{
    public class WatchListDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("fname")]
        public string FirstName { get; set; }
        [Column("mname")]
        public string MiddleName { get; set; }
        [Column("lname")]
        public string LastName { get; set; }
        [Column("type")]
        public string Type { get; set; }
        [Column("re_active_date")]
        public string ReActiveDate { get; set; }
        [Column("nationality")]
        public string Nationality { get; set; }
        [Column("country_id")]
        public int CountryId { get; set; }
        [Column("dob")]
        public string DOB { get; set; }
        [Column("passport_no")]
        public string PassportNo { get; set; }
        [Column("is_blocked")]
        public int IsBlocked { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("source")]
        public string Source { get; set; }
        [Column("source_unique_id")]
        public string SourceUniqueId { get; set; }
        [Column("narration")]
        public string Narration { get; set; }
        [Column("remarks")]
        public string Remarks { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("updated_by")]
        public int UpdatedBy { get; set; }
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
        [Column("updated_on")]
        public DateTime? UpdatedOnDB { get; set; }
        public string UpdatedOn
        {
            get
            {
                return UpdatedOnDB.ToUIDDateFormat();
            }
            set
            {
                UpdatedOnDB = value.ParseDB();
            }
        }
    }
}
