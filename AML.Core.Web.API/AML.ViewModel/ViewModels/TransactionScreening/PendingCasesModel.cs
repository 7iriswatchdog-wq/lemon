using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class PendingCasesModel
    {
        public int id { get; set; }
        
        public string tranrefno { get; set; }
       
        public string custrefno { get; set; }
        
        public string name { get; set; }
       
        public int matchscore { get; set; }

        public string CreatedOn { get; set; }
        public string CreatedOnDB { get; set; }


    }
    public class PendingCasesRequestModel
    {
        public string tranrefno { get; set; }

    }
}
