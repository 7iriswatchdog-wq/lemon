using AML.DTO.DTO.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.ProdMaster
{
    public class ProdRiskConfigurationMasterDTO
    {
        public string ProdRiskCategory { get; set; }
        public string ProdRiskCategoryID { get; set; }
        public List<ProdRiskTypeCategoryDTO> AvailableProdRiskTypeCategory { get; set; }
        public List<ProdRiskTypeCategoryDTO> AddedProdRiskTypeCategory { get; set; }

        public List<ProdRiskItemsDTO> AvailableProdRiskItems { get; set; }
        public List<ProdRiskItemsDTO> AddedProdRiskItems { get; set; }

        public List<ProdRiskTypeCategoryDTO> RiskTypeCategories { get; set; }

        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }

        public int CreatedBy { get; set; }
    }

    


    public class ProdRiskTypeCategoryDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("prod_type_category")]
        public string ProdRiskTypeCategory { get; set; }
       
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }

        public List<ProdRiskItemsDTO> ProdRiskItems { get; set; }

        public SelectList ProdRiskItem { get; set; }

        public int prodItemScore { get; set; }
        public string prodItemTxt { get; set; }
        public int prodSelectedItemId { get; set; }


        public string OverrideScore { get; set; }


    }

    public class ProdRiskItemsDTO
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("prod_type_id")]
        public string ProdRiskCategoreyId { get; set; }

        [Column("prod_type_name")] 
        public string ProdRiskCategorey { get; set; }
        [Column("prod_risk_data")]
        public string ProdRiskItem { get; set; }
        [Column("prod_risk_score")]
        public string ProdRiskScore { get; set; }
        [Column("Over_ride_Score")]
        public string OverrideScore { get; set; }

        [Column("active_yn")] 
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }

        public int ClientId { get; set; }

        public string Score
        {
            get
            {
                return string.Format("{0}|{1}|{2}", ProdRiskScore, OverrideScore, Id);
            }
        }
      

    }

    public class  ProdReportDataDTO 
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("prod_type_id")]
        public string ProdRiskCategoreyId { get; set; }

        [Column("prod_type_name")]
        public string ProdRiskCategorey { get; set; }
        [Column("prod_risk_data")]
        public string ProdRiskItem { get; set; }
        [Column("prod_risk_score")]
        public int ProdRiskScore { get; set; }
        [Column("Over_ride_Score")]
        public string OverrideScore { get; set; }

        [Column("active_yn")]
        public bool isActive { get; set; }
    }




}
