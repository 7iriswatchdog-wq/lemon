using AML.ViewModel.ViewModels.Risk;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.ViewModel.ViewModels.ProductMaster
{
    public class ProdRiskConfigurationModel
    {
        public string ProdRiskCategory { get; set; }
        public string ProdRiskCategoryID { get; set; }

        
        
        public List<ProdRiskTypeCategoryModel> AvailableProdRiskTypeCategory { get; set; }
        public List<ProdRiskTypeCategoryModel> AddedProdRiskTypeCategory { get; set; }

        public List<ProdRiskItemsModel> AvailableProdRiskItems { get; set; }
        public List<ProdRiskItemsModel> AddedProdRiskItems { get; set; }

        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }

        public int CreatedBy { get; set; }
    }

    public class ProdRiskTypeCategoryModel

    {
        public int Id { get; set; }
        public string ProdRiskTypeCategory { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }

    public class ProdRiskItemsModel
    {
        public int Id { get; set; }    
        public string ProdRiskCategoreyId { get; set; }

        public string ProdRiskCategorey { get; set; }
        public string ProdRiskItem { get; set; }
        public string ProdRiskScore { get; set; }
        public string OverrideScore { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }
}
