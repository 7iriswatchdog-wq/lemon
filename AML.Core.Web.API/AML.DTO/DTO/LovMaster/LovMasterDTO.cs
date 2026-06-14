using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.LovMaster
{
    public class LovMasterDTO
    {

        [Column("id")]
        public int Id { get; set; }
        [Column("lov_risk_category")]
        public string LovRiskCategory { get; set; }
        [Column("lov_risk_category_code")]
        public string LovRiskCategoryCode { get; set; }
        [Column("lov_type_id")]
        public string LovTypeId { get; set; }
        [Column("lov_type_name")]
        public string LovTypeName { get; set; }
        [Column("lov_risk_data")]
        public string LovRiskData { get; set; }
        [Column("lov_risk_score")]
        public string LovRiskScore { get; set; }

        [Column("Over_ride_Score")]
        public string OverrideScore { get; set; }
        [Column("active_yn")]
        public bool IsActive { get; set; }

        public string Score
        {
            get
            {
                return string.Format("{0}|{1}", LovRiskScore, OverrideScore);
            }
        }
        [Column("lov_type_category")]
        public string LovTypeCategory { get; set; }
        [Column("CategoryId")]
        public int CategoryId { get; set; }
        [Column("TypeId")]
        public int TypeId { get; set; }
        [Column("RiskId")]
        public int RiskId { get; set; }

        [Column("Client_Id")]
        public int ClientId { get; set; }

        [Column("Created_By")]
        public int CreatedBy { get; set; }

        [Column("lov_type_category_id")]
        public string LovTypeCategoryId { get;set;}
        [Column("percentage")] 
        public int Percentage { get; set; }
    }
}
