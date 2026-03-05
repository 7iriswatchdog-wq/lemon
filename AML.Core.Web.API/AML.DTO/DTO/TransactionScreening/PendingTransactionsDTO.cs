using System;
using AML.Core.Common.StaticResource;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace AML.DTO.DTO.TransactionScreening
{
    public class PendingTransactionsDTO
    {
        [Column("tranrefno")]
        public string tranrefno { get; set; }
        [Column("createdon")]
        public DateTime createdon { get; set; }
        [Column("CaseCount")]
        public int CaseCount { get; set; }
        [Column("Pending")]
        public int Pending { get; set; }
        [Column("Approved")]
        public int Approved { get; set; }
        [Column("Rejected")]
        public int Rejected { get; set; }

        public DateTime? CreatedOnDB { get; set; }
        public string CreatedOn
        {
            get
            {
                return CreatedOnDB.ToUIDDateFormat();
            }
            set
            {
                CreatedOnDB = value.ParseDB();
            }
        }


    }
    public class PendingTransactionRequestDTO
    {
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        
        public int ClientId { get; set; }

    }
}
