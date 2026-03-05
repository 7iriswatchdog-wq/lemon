using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.UserAccess
{
    public class UserGroupRightDTO
    {
        [Column("user_group_id")]
        public int UserGroupId { get; set; }

        [Column("module_id")]
        public int ModuleId { get; set; }

        [Column("func_id")]
        public int FunctionalityId { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

        [Column("created_on")]
        public DateTime CreatedOn { get; set; }
    }
}
