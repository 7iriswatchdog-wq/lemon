using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AML.ViewModel.ViewModels.RiskAPI
{
    public class RiskAPIRequestModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string MainNationality { get; set; }
        public string RiskCategory { get; set; }
        public List<RiskTypeCategoryListModel> RiskTypeCategoryList { get; set; }
        public List<RiskTypeListModel> RiskTypeList { get; set; }
        public List<RiskTypeFinalListModel> RiskTypeFinalList { get; set; }
        public int ClientId { get; set; }
        [Required(ErrorMessage = "User Id required")]
        public string UserId { get; set; }
        [Required(ErrorMessage = "Company Name required")]
        public string CompanyName { get; set; }
        public int CreatedBy { get; set; }
        public int Version { get; set; }
        public int CaseVersion { get; set; }

        public string SelectedVersions { get; set; }
    }
    public class RiskTypeCategoryListModel
    {
        public string Id { get; set; }
        public List<RiskTypeListModel> RiskTypeList { get; set; }
    }
    public class RiskTypeListModel
    {
        public string Id { get; set; }
        public List<RiskItemListModel> RiskItemList { get; set; }

    }
    public class RiskTypeFinalListModel
    {
        public string Id { get; set; }
        public List<RiskTypeFinalListModel> RiskItemList { get; set; }

    }
    public class RiskConfigResultModel
    {
        public List<RiskTypeResultModel> RiskTypes { get; set; }
    }


    public class RiskTypeResultModel
    {
        public int Id { get; set; }
        public string RiskType { get; set; }
        public List<RiskItemResultModel> RiskItems { get; set; }
    }
    public class RiskItemResultModel
    {
        public int Id { get; set; }
        public string RiskItem { get; set; }
    }
    public class RiskConfigRequestModel
    {
        [Required(ErrorMessage = "Risk category is required")]
        public string RiskCategory { get; set; }
    }

    public class RiskItemListModel
    {
        public string Id { get; set; }
        public string RiskItem { get; set; }
    }
    public class RiskAPIResultModel
    {

        public UserResultModel User { get; set; }
        public string Status { get; set; }
    }
    public class UserResultModel
    {

        public string TotalParameter { get; set; }
        public string TotalScore { get; set; }
        public string FinalRiskScore { get; set; }
        public string RiskAsPerScore { get; set; }

    }
}
