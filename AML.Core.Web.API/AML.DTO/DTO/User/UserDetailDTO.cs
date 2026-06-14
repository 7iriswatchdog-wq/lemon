using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.User
{
    public class UserDetailDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("user_id")]
        public int UserId { get; set; }
        [Column("date_of_join")]
        public DateTime? DateOfJoin { get; set; }
        
        [Column("city")]
        public string City { get; set; }
        [Column("phone")]
        public string Phone { get; set; }
        [Column("fax")]
        public string Fax { get; set; }
        [Column("email")]
        public string Email { get; set; }
        [Column("address_1")]
        public string Address1 { get; set; }
        [Column("address_2")]
        public string Address2 { get; set; }
        [Column("identity_type")]
        public int IdentityType { get; set; }
        [Column("approval_limit")]
        public decimal ApprovalLimit { get; set; }
        [Column("identification_number")]
        public string IdentificationNumber { get; set; }
        [Column("id_num_date_of_issue")]
        public DateTime? IdNumDateOfIssue { get; set; }
        [Column("id_num_expry_date")]
        public DateTime? IdNumExpryDate { get; set; }
        [Column("visa_type")]
        public int VisaType { get; set; }
        [Column("current_add")]
        public string CurrentAdd { get; set; }
        [Column("visa_number")]
        public string VisaNumber { get; set; }
        [Column("visa_num_date_of_issue")]
        public DateTime? VisaNumDateOfIssue { get; set; }
        [Column("visa_num_expry_date")]
        public DateTime? VisaNumExpryDate { get; set; }

        [Column("cpr_number")]
        public string CprNumber { get; set; }
        [Column("cpr_num_date_of_issue")]
        public DateTime? CprNumDateOfIssue { get; set; }
        [Column("cpr_num_expry_date")]
        public DateTime? CprNumExpryDate { get; set; }
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

        [Column("Client_Id")]
        public int ClientId { get; set; }
    }
}
