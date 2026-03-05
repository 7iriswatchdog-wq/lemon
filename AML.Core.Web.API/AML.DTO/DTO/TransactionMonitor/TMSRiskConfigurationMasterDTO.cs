using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSRIskConfigurationMasterDTO
    {
        public string RiskCategory { get; set; }
        public string RiskCategoryID { get; set; }


        public string RiskTypeCategoryID { get; set; }

        public string RiskTypeCategoryName { get; set; }

        public List<TMSRiskTypeCategoryDTO> AvailableRiskTypeCategory { get; set; }
        public List<TMSRiskTypeCategoryDTO> AddedRiskTypeCategory { get; set; }

        public string RiskTypeID { get; set; }

        public string RiskTypeName { get; set; }

        public List<TMSRiskTypeDTO> AvailableRiskType { get; set; }
        public List<TMSRiskTypeDTO> AddedRiskType { get; set; }



        public List<TMSRiskItemsDTO> AvailableItems { get; set; }
        public List<TMSRiskItemsDTO> AddedItems { get; set; }


        public List<TMSRiskTypeCategoryDTO> RiskTypeCategories { get; set; }

    }

    public class TMSRiskTypeDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lov_type_name")]
        public string RiskType { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }

        public List<TMSRiskItemsDTO> RiskItems { get; set; }
        public SelectList Items { get; set; }
        public int ItemScore { get; set; }
        public string ItemTxt { get; set; }
        public int SelectedItemId { get; set; }
        public int OverrideScore { get; set; }
    }


    public class TMSRiskTypeCategoryDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("lov_type_category")]
        public string RiskCategory { get; set; }
        public string RiskCategoryCode { get; set; }
        public string RiskTypeCategory { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }

        public List<TMSRiskTypeDTO> RiskTypes { get; set; }

    }

    public class TMSRiskItemsDTO
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
                return string.Format("{0}|{1}|{2}", RiskScore, OverrideScore, Id);
            }
        }
    }
}
