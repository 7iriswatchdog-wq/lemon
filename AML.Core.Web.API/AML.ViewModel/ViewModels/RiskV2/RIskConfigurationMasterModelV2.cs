using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.RiskV2
{
    public class RIskConfigurationMasterModelV2
    {
        public string RiskCategory { get; set; }
        public string RiskCategoryID { get; set; }
        public SelectList RiskCategories { get; set; }


        public string RiskTypeCategoryID { get; set; }
        public string RiskTypeCategoryName { get; set; }
        public SelectList RiskTypeCategory { get; set; }

        public List<RiskTypeCategoryModelV2> AvailableRiskTypeCategory { get; set; }
        public List<RiskTypeCategoryModelV2> AddedRiskTypeCategory { get; set; }

        public string RiskTypeID { get; set; }
        public SelectList RiskTypes { get; set; }
        public string RiskTypeName { get; set; }

        public List<RiskTypeModelV2> AvailableRiskType { get; set; }
        public List<RiskTypeModelV2> AddedRiskType { get; set; }

        public List<RiskItemsModelV2> AvailableItems { get; set; }
        public List<RiskItemsModelV2> AddedItems { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }

        public int CreatedBy { get; set; }
    }
    public class RiskTypeCategoryModelV2

    {
        public int Id { get; set; }
        public string RiskTypeCategory { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }

    public class RiskTypeModelV2
    {
        public int Id { get; set; }
        public string RiskType { get; set; }

        public int RiskTypePercentage { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }


    public class RiskItemsModelV2
    {
        public int Id { get; set; }
        public string RiskItem { get; set; }
        public string RiskScore { get; set; }
        public string OverrideScore { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }
}
