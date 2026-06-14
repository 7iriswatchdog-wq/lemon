using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.RiskV2
{
    public class RIskConfigurationMasterDTOV2
    {
        public string RiskCategory { get; set; }
        public string RiskCategoryID { get; set; }


        public string RiskTypeCategoryID { get; set; }

        public string RiskTypeCategoryName { get; set; }

        public List<RiskTypeCategoryDTOV2> AvailableRiskTypeCategory { get; set; }
        public List<RiskTypeCategoryDTOV2> AddedRiskTypeCategory { get; set; }

        public string RiskTypeID { get; set; }

        public string RiskTypeName { get; set; }

        public List<RiskTypeDTOV2> AvailableRiskType { get; set; }
        public List<RiskTypeDTOV2> AddedRiskType { get; set; }



        public List<RiskItemsDTOV2> AvailableItems { get; set; }
        public List<RiskItemsDTOV2> AddedItems { get; set; }


        public List<RiskTypeCategoryDTOV2> RiskTypeCategories { get; set; }
        public List<ReportDataDTOV2> ReportDatas { get; set; }
        public int ClientId { get; set; }

        public int CreatedBy { get; set; }
    }

    public class RiskTypeDTOV2
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lov_type_name")]
        public string RiskType { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }

        public List<RiskItemsDTOV2> RiskItems { get; set; }
        public SelectList Items { get; set; }
        [Column("percentage")]
        public int RiskTypePercentage { get; set; }

        public int ItemScore { get; set; }
        public string ItemTxt { get; set; }
        public int SelectedItemId { get; set; }
        public int OverrideScore { get; set; }
        [Column("lov_country_duplicate")]
        public int lov_country_duplicate { get; set; }
        [Column("tooltip")]
        public string tooltip { get; set; }
    }


    public class RiskTypeCategoryDTOV2
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lov_type_category")]
        public string RiskCategory { get; set; }
        public string RiskCategoryCode { get; set; }
        public string RiskTypeCategory { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }

        public List<RiskTypeDTOV2> RiskTypes { get; set; }

    }

    public class RiskItemsDTOV2
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lov_risk_data")]
        public string RiskItem { get; set; }
        [Column("lov_risk_score")]
        public string RiskScore { get; set; }
        [Column("Over_ride_Score")]
        public string OverrideScore { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
        public string Score
        {
            get
            {
                return string.Format("{0}|{1}|{2}", RiskScore, OverrideScore,Id);
            }
        }
    }
    public class ReportDataDTOV2
    {
        [Column("lov_type_category_id")]
        public int lov_type_category_id { get; set; }
        [Column("lov_type_id")]
        public int lov_type_id { get; set; }
        [Column("lov_risk_data")]
        public string lov_risk_data { get; set; }
        [Column("lov_risk_score")]
        public int lov_risk_score { get; set; }
        [Column("Over_ride_Score")]
        public int Over_ride_Score { get; set; }
       
    }
}
