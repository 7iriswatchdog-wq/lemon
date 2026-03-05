using AML.Core.DataContract.Enum;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace AML.ViewModel.ViewModels.Report
{
    public class ReportLogSearchModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string MatchType { get; set; }
        public SelectList MatchTypeList { get; set; } 
        public string SourceType { get; set; }
        public SelectList SourceTypeList { get; set; }
        public bool IsPDF { get; set; }
        public string CaseStatus { get; set; }
        public SelectList CaseStatusList { get; set; }
        public string userID { get; set; }
        public SelectList Users { get; set; }
        public string SearchText { get; set; }
        public int ClientId { get; set; }
        public string CustomerType { get; set; }
        public SelectList CustomerCategories { get; set; }
        public string CustomerIdType { get; set; }
        public string UpdatedByUserID { get; set; }
        public DateTime TransactionStartDate { get; set; }
        public DateTime TransactionEndDate { get; set; }

        public string pep { get; set; }

        public string IdStatus { get; set; }

    }
    public class ReportInternalWatchListLogModel
    {
        public string uid { get; set; }
        public string category { get; set; }
        public string fullname { get; set; }
        public string nationality { get; set; }
        public string dob { get; set; }
        public string type { get; set; }
        public string createdon { get; set; }
    }
}
