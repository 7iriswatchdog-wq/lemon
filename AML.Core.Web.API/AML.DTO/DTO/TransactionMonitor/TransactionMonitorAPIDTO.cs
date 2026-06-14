using System;
using AML.Core.Common.StaticResource;
//using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Dapper.Contrib.Extensions;
//using System.ComponentModel.DataAnnotations.Schema;

namespace AML.DTO.DTO.TransactionMonitor
{
    [Table("transactionmonitor")]
    public class TransactionMonitorAPIDTO
    {
        //[Column("id")]
        [ExplicitKey]
        public int id { get; set; }
        public string CustomerId { get; set; }
        [Required(ErrorMessage = "Transaction Reference No required")]

        public string TranRefno { get; set; }
        public DateTime TranDate { get; set; }
        public string TranType { get; set; }
        public string RemitterId { get; set; }
        public string RemitterName { get; set; }
        public string RemitterType { get; set; }
        public string RemitterNationCode { get; set; }
        public string RemitterCountryCode { get; set; }
        public string Currency { get; set; }
        public double Amount { get; set; }
        public string RemittancePurpose { get; set; }
        public string BeneficiaryId { get; set; }
        public string BeneficiaryName { get; set; }
        public string BeneficiaryType { get; set; }
        public string BeneficiaryNationCode { get; set; }
        public string BeneficiaryCountryCode { get; set; }
        public string BeneficiaryCurrencyCode { get; set; }
        public string BeneficiaryBankname { get; set; }
        public string BeneficiaryAccountNo { get; set; }
        public double FCYAmount { get; set; }
        public string IPAddress { get; set; }
        public string KioskID { get; set; }
        
        public string RemitterCountry { get; set; }
        public string BeneficiaryCountry { get; set; }
        public string IsHighRiskCountry { get; set; }
        public string IsHighRiskCustomer { get; set; }
        public string BeneficiaryBankCOI { get; set; }
        public double DebitAmount { get; set; }
        public double CreditAmount { get; set; }
        
        public string RemitterMobile { get; set; }
        public string RemitterEmail { get; set; }
        public string BeneficiaryMobile { get; set; }
        public string BeneficiaryEmail { get; set; }
        public DateTime BeneficiaryCreatedDate { get; set; }
        public int TransactionScore { get; set; }
        public string comments { get; set; }
        public int assignedto { get; set; }
        public int status { get; set; }
        public int batchno { get; set; }
        public int tmsstatus { get; set; }
        public int createdby { get; set; }
        public DateTime createdon { get; set; }
        public int updatedby { get; set; }
        public DateTime updatedon { get; set; }
        public int sendmail { get; set; }

        public int ClientId { get; set; }
    }
    
}


////public string CustomerSegment { get; set; }
//public double KioskLoadAmount { get; set; }
//public double IBTransferAmount { get; set; }
//public double P2PCreditAmount { get; set; }
//public double P2MDebitAmount { get; set; }
//public double P2MCreditAmount { get; set; }
//public double M2PDebitAmount { get; set; }
//public double M2PCreditAmount { get; set; }

