using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TMSRulesModel
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
        public List<TransactionCategoryModel> TransactionCategory { get; set; }
        public List<AllowedTransactionTypeModel> AllowedTransactionTypes { get; set; }
        public int MaxAllowed { get; set; }
        public int Weight { get; set; }
        public bool CheckForCompany { get; set; }
        public int MinimumAmount { get; set; }
        public int DaysCompany { get; set; }
        public int ThresholdAmount { get; set; }
        public int Threshold { get; set; }
        public int Scale { get; set; }
        public string CurrentStartDate { get; set; }
        public string CurrentEndDate { get; set; }
        public string CheckAgainstStartDate { get; set; }
        public string CheckAgainstEndDate { get; set; }
    }
    public class TransactionCategoryModel
    {

        public int Id { get; set; }

        public string TransactionCategory { get; set; }

        public int Customer { get; set; }

        public string StartDate { get; set; }

        public string EndDate { get; set; }

        public int DaysCount { get; set; }

        public int TransactionsCount { get; set; }

        public int MaxAllowed { get; set; }

    }
    public class AllowedTransactionTypeModel
    {

        public int Id { get; set; }

        public string AllowedTransactionType { get; set; }
    }
}
