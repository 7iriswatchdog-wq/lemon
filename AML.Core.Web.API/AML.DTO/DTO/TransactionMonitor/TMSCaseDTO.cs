using AML.Core.Common.StaticResource;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace AML.DTO.DTO.TransactionMonitor
{
    public class TMSCaseDTO
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("TranRefno")]
        public string TranRefno { get; set; }
        [Column("TranDate")]
        public string TranDate { get; set; }
        [Column("TranType")]
        public string TranType { get; set; }
        [Column("RemitterId")]
        public string CustomerId { get; set; }
        [Column("RemitterName")]
        public string CustomerName { get; set; }
        [Column("RemitterNationCode")]
        public string RemitterNationality { get; set; }

        [Column("tms_rm_rulename")]
        public string RuleViolated { get; set; }
        [Column("monitoredon")]
        public DateTime? Created_on { get; set; }

        public string CreatedOn
        {
            get
            {
                return Created_on.ToString();



            }
            set
            {
                Created_on = value.ParseDB();
            }
        }

        [Column("BeneficiaryId")]
        public string BeneficiaryId { get; set; }


        [Column("BeneficiaryName")]
        public string BeneficiaryName { get; set; }

        [Column("BeneficiaryNationCode")]
        public string BeneficiaryNationCode { get; set; }



        [Column("BeneficiaryCountry")]
        public string BeneficiaryCountry { get; set; }
        [Column("BeneficiaryAccountNo")]
        public string BeneficiaryAccountNo { get; set; }
        [Column("Amount")]
        public string Amount { get; set; }
        [Column("status")]
        public int Status { get; set; }
        [Column("updatedby")]
        public int UpdatedBy { get; set; }
        [Column("updatedon")]
        public string updatedon { get; set; }
        [Column("comments")]
        public string comments { get; set; }
        [Column("StatusDescription")]
        public string StatusDescription { get; set; }
        [Column("TransactionScore")]
        public string TransactionScore { get; set; }
    }
}
