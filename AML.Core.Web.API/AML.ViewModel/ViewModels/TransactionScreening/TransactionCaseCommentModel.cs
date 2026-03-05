using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class TransactionCaseCommentModel
    {
           public int Id { get; set; }
            public string TranRefNo { get; set; }
            public int CaseId { get; set; }
            public string Comment { get; set; }
            public int CreatedBy { get; set; }
            public DateTime? CreatedOn { get; set; }
            public string CreatedUser { get; set; }
            public string Duration { get; set; }
        }
        public class CaseCloseModel
        {
            public string TranRefNo { get; set; }
            public int CaseId { get; set; }
            public string Comment { get; set; }
            public int Action { get; set; }
        }

   
}
