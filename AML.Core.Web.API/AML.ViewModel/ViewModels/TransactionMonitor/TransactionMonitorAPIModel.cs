using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TransactionMonitorAPIModel
    {
        public int id { get; set; }
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
    }

    public class TMSInsertStatusModel
    {
        public string TranRefno { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
    }
}



//public string CustomerSegment { get; set; }
//public double KioskLoadAmount { get; set; }
//public double IBTransferAmount { get; set; }
//public double P2PCreditAmount { get; set; }
//public double P2MDebitAmount { get; set; }
//public double P2MCreditAmount { get; set; }
//public double M2PDebitAmount { get; set; }
//public double M2PCreditAmount { get; set; }




