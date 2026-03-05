
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.CustomerCase
{
    public class DataListModel
    {
       
        public string matchuid { get; set; }
        public string matchtype { get; set; }
        public string matchcategory { get; set; }
        public string matchname { get; set; }
        public string matchscore { get; set; }
        public string matchnationality { get; set; }
        public string matchidno { get; set; }
        public string matchdob { get; set; }
        public string remarks { get; set; }

        public string matchresourcesid { get; set; }

        public string matchdatasets { get; set; }

        public string matchgender { get; set; }

        public List<string> searchTypes { get; set; }

        public List<RiskAssessmentDto> riskAssessments { get; set; }
    }

    public class RiskAssessmentDto
    {
        public int RiskTypeId { get; set; }

        public string RiskTypeText { get; set; }
        public int SelectedItemId { get; set; }
        public string ItemText { get; set; }   // ? SELECTED TEXT
        public int ItemScore { get; set; }
    }
}
