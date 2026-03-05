using AML.ViewModel.ViewModels.TransactionScreening;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class TransactionCaseProcessModel
    {
        public TranScreenCaseModel Case { get; set; }
        public List<TransactionCaseDocumentModel> CaseDocuments { get; set; }
        public List<TransactionCaseCommentModel> CaseComments { get; set; }
        public List<TransactionCaseAssignmentModel> CaseAssignments { get; set; }
        public SelectList DocumentTypes { get; set; }
        public SelectList DocumentCategories { get; set; }
        public SelectList Users { get; set; }
        public List<MatchDetailsModel> DataList { get; set; }
        public string Url { get; set; }
    }

    public class TransactionCaseProcessReportModel
    {
        public string RefNo { get; set; }
        public List<TranScreenCaseModel> Case { get; set; }
        public List<TransactionCaseDocumentModel> CaseDocuments { get; set; }
        public List<TransactionCaseCommentModel> CaseComments { get; set; }
        public List<TransactionCaseAssignmentModel> CaseAssignments { get; set; }
        public SelectList DocumentTypes { get; set; }
        public SelectList DocumentCategories { get; set; }
        public SelectList Users { get; set; }
        public List<MatchDetailsModel> DataList { get; set; }

        public List<TransactionMatchDetails> MatchDataList { get; set; }
        public string Url { get; set; }
    }
}
