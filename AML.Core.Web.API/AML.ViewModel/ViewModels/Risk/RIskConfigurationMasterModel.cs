using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Risk
{
    public class RIskConfigurationMasterModel
    {
        public string RiskCategory { get; set; }
        public string RiskCategoryID { get; set; }
        public SelectList RiskCategories { get; set; }


        public string RiskTypeCategoryID { get; set; }
        public string RiskTypeCategoryName { get; set; }
        public SelectList RiskTypeCategory { get; set; }

        public List<RiskTypeCategoryModel> AvailableRiskTypeCategory { get; set; }
        public List<RiskTypeCategoryModel> AddedRiskTypeCategory { get; set; }

        public string RiskTypeID { get; set; }
        public SelectList RiskTypes { get; set; }
        public string RiskTypeName { get; set; }

        public List<RiskTypeModel> AvailableRiskType { get; set; }
        public List<RiskTypeModel> AddedRiskType { get; set; }

        public List<RiskItemsModel> AvailableItems { get; set; }
        public List<RiskItemsModel> AddedItems { get; set; }
        public bool userAuthorised { get; set; }
        public int ClientId { get; set; }

        public int CreatedBy { get; set; }
    }
    public class RiskTypeCategoryModel

    {
        public int Id { get; set; }
        public string RiskTypeCategory { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }

    public class RiskTypeModel
    {
        public int Id { get; set; }
        public string RiskType { get; set; }

        public int RiskTypePercentage { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }


    public class RiskItemsModel
    {
        public int Id { get; set; }
        public string RiskItem { get; set; }
        public string RiskScore { get; set; }
        public string OverrideScore { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }
}
