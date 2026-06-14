using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.User
{
    public class UserDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("fname")]
        public string FName { get; set; }
        [Column("lname")]
        public string LName { get; set; }
        
        [Column("emp_code")]
        public string EmpCode { get; set; }
        [Column("user_name")]
        public string UserName { get; set; }
        [Column("password")]
        public string Password { get; set; }
        [Column("remarks")]
        public string Remarks { get; set; }
        [Column("designation_id")]
        public int DesignationId { get; set; }
        [Column("department_id")]
        public int DepartmentId { get; set; }
        [Column("user_group_id")]
        public int UserGroupId { get; set; }
        [Column("branch_id")]
        public int BranchId { get; set; }
        [Column("country_id")]
        public int CountryId { get; set; }
        [Column("identity_type_id")]
        public int IdentityTypeId { get; set; }
        [Column("is_active")]
        public int IsActive { get; set; }
        [Column("is_deleted")]
        public int IsDeleted { get; set; }
        [Column("is_blocked")]
        public int IsBlocked { get; set; }
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
        public UserDetailDTO UserDetail { get; set; }
        [Column("otp")]
        public int otp { get; set; }
        [Column("email")]
        public string email { get; set; }
        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("is_superAdmin")]
        public int isSuperAdmin { get; set; }
    }
    public class UserPasswordLogModelDTO
    {
        [Column("UserId")]
        public int UserId { get; set; }
        [Column("UserName")]
        public string UserName { get; set; }
        [Column("FullName")]
        public string FullName { get; set; }
        [Column("UpdatedBy")]
        public string ChangeBy { get; set; }
        [Column("UpdatedOn")]
        public string ChangedOn { get; set; }
    }
}
