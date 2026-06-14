
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

        public string Status { get; set; }

        public string FlagType { get; set; }
    }

    public class RiskAssessmentDto
    {
        public int RiskTypeId { get; set; }

        public string RiskTypeText { get; set; }
        public int SelectedItemId { get; set; }
        public string ItemText { get; set; }   // ? SELECTED TEXT
        public int ItemScore { get; set; }
    }

    public class SaveRemarkRequest
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string Type { get; set; }
        public List<DataListModel> Model { get; set; }
    }

    public class WebListModel
    {

        public string shortdescription { get; set; }
        public string url { get; set; }

        

        public int Matchscore { get; set; }
        public string MatchPresent { get; set; }
        public List<string> MatchedNameTerms{ get; set; }
        public List<string> CrimeKeyWordsFound{ get; set; }
        public string Title { get; set; }
        
        public string ENGINE { get; set; }

        public string Remarks { get; set; }

        public List<string> searchTypes { get; set; }

    }

    public class CaseLogResponseModel
    {
        public string CASEID { get; set; }
        public List<DataListModel> MatchRecords { get; set; }

        public List<WebListModel> WebRecords { get; set; }
    }
}
