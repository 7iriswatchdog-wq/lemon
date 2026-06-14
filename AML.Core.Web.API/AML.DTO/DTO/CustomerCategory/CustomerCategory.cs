using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.CustomerCategory
{
    public class CustomerCategoryDTO
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
        [Column("score")]
        public int Score { get; set; }
    }
}
