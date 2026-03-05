using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TMSMasterModel
    {
        public int Id { get; set; }
        public string LovRiskCategory { get; set; }
        public string LovTypeId { get; set; }
        public string LovTypeName { get; set; }
        public string LovRiskData { get; set; }
        public string LovRiskScore { get; set; }
        public string OverrideScore { get; set; }
        public string IsActive { get; set; }
        public List<TMSRulesViewModel> TMSRulesViewModel { get; set; }

    }
    public class TMSRulesViewModel
    {
        public int id { get; set; }
        public string lov_type_category { get; set; }
        public string lov_risk_category { get; set; }
        public string lov_type_name { get; set; }
        public string Over_ride_Score { get; set; } //frequency
        public string lov_risk_score { get; set; }//volume/count
        public string lov_risk_data { get; set; }//volume/amount
    }
}
