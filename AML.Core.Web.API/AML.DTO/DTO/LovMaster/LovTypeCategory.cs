using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.LovMaster
{
    public class LovTypeCategoryDTO
    {
        [Column("id")]
        public int LovTypeCategoryId { get; set; }
        [Column("lov_risk_category")]
        public string LovRiskCategory { get; set; }
        [Column("lov_risk_category_code")]
        public string LovRiskCategoryCode { get; set; }
        [Column("lov_type_category")]
        public string LovCategoryType { get; set; }
        [Column("active_yn")]
        public bool IsActive { get; set; }

        [Column("Client_Id")]
        public int ClientId { get; set; }
        [Column("Created_By")]
        public int CreatedBy { get; set; }
    }
}
