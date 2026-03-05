using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionScreening
{

    public class TransactionMatchDetails 
    {
        public string CASEID { get; set; }
        public string TRANREFNO { get; set; }
        public List<MatchDetailsModel> MATCHRECORDS { get; set; }
       
    }
    public class MatchDetailsModel
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
        }
    }

