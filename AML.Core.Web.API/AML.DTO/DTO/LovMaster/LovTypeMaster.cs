using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.LovMaster
{
    public class LovTypeMasterDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lov_risk_catogory")]
        public string LovRiskCategory { get; set; }
        [Column("lov_risk_catogory_code")]
        public string LovRiskCategoryCode { get; set; }
        [Column("lov_type_id")]
        public string LovTypeId { get; set; }
        [Column("lov_type_name")]
        public string LovTypeName { get; set; }
        [Column("lov_type_category_id")]
        public string LovTypeCategoryId { get; set; }
       
        [Column("active_yn")]
        public bool IsActive { get; set; }
       

        [Column("Client_Id")]
        public int ClientId { get; set; }

        [Column("Created_By")]
        public int CreatedBy { get; set; }
        [Column("lov_country_duplicate")]
        public int lov_country_duplicate { get; set; }
        [Column("percentage")]
        public int Percentage { get; set; }

    }
}
