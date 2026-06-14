using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSMasterDTO
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

    }
    public class TMSTypeCategoryDTO
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

    }
    public class TMSTypeMasterDTO
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

    }
    public class TMSRulesViewModelDTO
    {
        [Column("id")]
        public int id { get; set; }
        [Column("lov_type_category")]
        public string lov_type_category { get; set; }
        [Column("lov_risk_category")]
        public string lov_risk_category { get; set; }
        [Column("lov_type_name")]
        public string lov_type_name { get; set; }
        [Column("Over_ride_Score")]
        public string Over_ride_Score { get; set; } //frequency
        [Column("lov_risk_score")]
        public string lov_risk_score { get; set; }//volume/count
        [Column("lov_risk_data")]
        public string lov_risk_data { get; set; }//volume/amount
    }

}
