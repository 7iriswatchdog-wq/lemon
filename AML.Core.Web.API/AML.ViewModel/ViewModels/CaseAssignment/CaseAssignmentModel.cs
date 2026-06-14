using System;
using System.Collections.Generic;
using System.Text;

namespace AML.ViewModel.ViewModels.CaseAssignment
{
    public class CaseAssignmentModel
    {

        public int Id { get; set; }
        public int CaseId { get; set; }
        public int UserId { get; set; }
        public int CreatedBy { get; set; }
        public string Comment { get; set; }
        public DateTime? CreatedOn{ get; set; }
        public string TransferUser { get; set; }
        public string Remark { get; set; }

        public string Email { get; set; }

        public string CustomerId { get; set; }
    }
}
