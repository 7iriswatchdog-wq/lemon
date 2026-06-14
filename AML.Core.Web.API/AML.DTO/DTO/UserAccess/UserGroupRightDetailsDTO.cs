using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.UserAccess
{
    public class UserGroupRightDetailsDTO
    {
        [Column("user_group_id")]
        public int UserGroupId { get; set; }

        [Column("user_group_code")]
        public string UserGroupCode { get; set; }

        [Column("user_group_name")]
        public string UserGroupName { get; set; }

        [Column("module_id")]
        public int ModuleId { get; set; }

        [Column("module_code")]
        public string ModuleCode { get; set; }

        [Column("module_name")]
        public string ModuleName { get; set; }

        [Column("func_id")]
        public int FunctionalityId { get; set; }

        [Column("func_code")]
        public string FunctionalityCode { get; set; }

        [Column("func_name")]
        public string FunctionalityName { get; set; }
    }
    public class UserModulesDTO
    {
        [Column("module_id")]
        public int ModuleId { get; set; }
        [Column("func_id")]
        public int FuncId { get; set; }
    }
}
