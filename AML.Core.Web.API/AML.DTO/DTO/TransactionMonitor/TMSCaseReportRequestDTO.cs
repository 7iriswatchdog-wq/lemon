using System;
using System.Collections.Generic;
using System.Text;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSCaseReportRequestDTO
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string TxnStartDate { get; set; }
        public string TxnEndDate { get; set; }
        public string Status { get; set; }

    }
}
