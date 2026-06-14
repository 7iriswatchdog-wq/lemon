using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.ProdMaster
{
    public class ProdtypecategoryDTO
    {
        [Column("id")]
        public int ProdTypeCategoryId { get; set; }
     
        [Column("prod_type_category")]
        public string ProdCategoryType { get; set; }
        [Column("active_yn")]
        public bool IsActive { get; set; }

        [Column("Client_Id")]
        public int ClientId { get; set; }
    }
}
