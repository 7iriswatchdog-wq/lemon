using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class PendingTransactionsModel
    {
        public string tranrefno { get; set; }
        public string CreatedOnDB { get; set; }
        public string CreatedOn { get; set; }
        public int caseCount { get; set; }
        public int pending { get; set; }
        public int approved { get; set; }
        public int rejected { get; set; }

    }

}
