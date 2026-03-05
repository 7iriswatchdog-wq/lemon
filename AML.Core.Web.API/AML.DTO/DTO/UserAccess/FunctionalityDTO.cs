using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.UserAccess
{
    public class FunctionalityDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("module_id")]
        public int ModuleId { get; set; }
        [Column("module_code")]
        public string ModuleCode { get; set; }
        [Column("module_name")]
        public string ModuleName { get; set; }
        [Column("code")]
        public string Code { get; set; }
        [Column("name")]
        public string Name { get; set; }
    }
}
