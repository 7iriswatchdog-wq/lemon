using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.UserGroup
{
    public class UserGroupDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("name")]
        public string Name { get; set; }
        [Column("description")]
        public string Description { get; set; }
        [Column("is_active")]
        public int IsActive { get; set; }
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
