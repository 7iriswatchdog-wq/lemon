using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.Sanction
{
    public class SanctionScreeningModel
    {
        public int ID { get; set; }
        public string CustomerName { get; set; }
        public string Nationality { get; set; }
        public DateTime DOB { get; set; }
        public string SearchType { get; set; }
        public string MatchName { get; set; }
        public int MatchScore { get; set; }
        public string MatchUID { get; set; }
        public string MatchCategory { get; set; }
        public string MatchType { get; set; }
        public string MatchNationality { get; set; }
        public int MatchIDNum { get; set; }
        public DateTime MatchDOB { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
