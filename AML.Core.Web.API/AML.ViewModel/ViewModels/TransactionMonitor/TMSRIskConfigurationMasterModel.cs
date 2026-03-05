using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TMSRIskConfigurationMasterModel
    {
        public string RiskCategory { get; set; }
        public string RiskCategoryID { get; set; }
        public SelectList RiskCategories { get; set; }


        public string RiskTypeCategoryID { get; set; }
        public string RiskTypeCategoryName { get; set; }
        public SelectList RiskTypeCategory { get; set; }

        public List<TMSRiskTypeCategoryModel> AvailableRiskTypeCategory { get; set; }
        public List<TMSRiskTypeCategoryModel> AddedRiskTypeCategory { get; set; }

        public string RiskTypeID { get; set; }
        public SelectList RiskTypes { get; set; }
        public string RiskTypeName { get; set; }

        public List<TMSRiskTypeModel> AvailableRiskType { get; set; }
        public List<TMSRiskTypeModel> AddedRiskType { get; set; }

        public List<TMSRiskItemsModel> AvailableItems { get; set; }
        public List<TMSRiskItemsModel> AddedItems { get; set; }
        public bool userAuthorised { get; set; }
    }
    public class TMSRiskTypeCategoryModel

    {
        public int Id { get; set; }
        public string RiskTypeCategory { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }

    public class TMSRiskTypeModel
    {
        public int Id { get; set; }
        public string RiskType { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }


    public class TMSRiskItemsModel
    {
        public int Id { get; set; }
        public string RiskItem { get; set; }
        public string RiskScore { get; set; }
        public string OverrideScore { get; set; }
        public bool isActive { get; set; }
        public bool isDeletedInUI { get; set; }
    }
}

