using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionScreening
{
    public class TransactionCaseAssignmentModel
    {
        public int Id { get; set; }
        public int CaseId { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string TransferUser { get; set; }
        public string Remark { get; set; }
    }
}
