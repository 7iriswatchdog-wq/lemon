using AML.Core.Common.StaticResource;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.Company
{
    public class CompanyDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("license_number")]
        public string LicenseNumber { get; set; }
        [Column("legal_name")]
        public string LegalName { get; set; }
        [Column("legal_type")]
        public int LegalType { get; set; }
        [Column("legal_type_name")]
        public string LegalTypeName { get; set; }
        [Column("mobile")]
        public string Mobile { get; set; }
        [Column("created_by")]
        public int CreatedBy { get; set; }
        [Column("updated_by")]
        public int UpdatedBy { get; set; }
        [Column("created_on")]
        public DateTime? CreatedOnDB { get; set; }
        [Column("updated_on")]
        public DateTime? UpdatedOnDB { get; set; }
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }
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
