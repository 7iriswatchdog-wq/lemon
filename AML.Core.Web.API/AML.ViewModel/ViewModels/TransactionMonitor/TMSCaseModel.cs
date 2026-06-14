using System;
using System.Collections.Generic;

namespace AML.ViewModel.ViewModels.TransactionMonitor
{
    public class TMSCaseModel
    {
        public int Id { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public int CustomerRiskScore { get; set; }
        public string RuleViolatedText { get; set; }
        public int EntityId { get; set; }
        public string EntityBranch { get; set; }
        public string UserId { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedOn { get; set; }
        public int Status { get; set; }
        public string Remarks { get; set; }
        public int UpdatedBy { get; set; }
        public string UpdatedOn { get; set; }
        public string UpdatedStatus { get; set; }
        public string UpdatedByUSer { get; set; }
        public List<TMSTransactionModel> TmsTransactionModel { get; set; }
        public List<TmsRuleViolatedModel> RuleViolated { get; set; }
    }
    public class TmsRuleViolatedModel
    {
        public int Id { get; set; }
        public string RuleName { get; set; }
    }
    public class TMSTransactionModel
    {
        public int Id { get; set; }
        public string TransactionDate { get; set; }
        public string TransactionNumber { get; set; }
        public string InternalRefNumber { get; set; }
        public string TransactionLocation { get; set; }
        public string Authorizer { get; set; }
        public string Currency { get; set; }
        public string Amount { get; set; }
        public string ProductCode { get; set; }
        public string Product { get; set; }
        public string Beneficiary { get; set; }
        public string GoodServices { get; set; }
        public string TransactionDescription { get; set; }
        public string User { get; set; }
    }
    public class TMSReportModel
    {
        public DateTime FromDt { get; set; }
        public DateTime ToDt { get; set; }
        public List<TMSCaseModel> TMSCaseModels { get; set; }
        public int TotalRows { get; set; }
    }
    public class TMSCaseExcelModel
    {
        public string CreatedOn { get; set; }
        public string UserId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string RuleViolatedText { get; set; }
        public string UpdatedStatus { get; set; }
    }
    
    public class Bank
    {
        public string Id { get; set; }
        public string Bankname { get; set; }
        public string CountryOfIncorporation { get; set; }
    }

    public class Remitter
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string Nationality { get; set; }
        public string Dob { get; set; }
        public Bank Bank { get; set; }
    }

    public class Beneficiary
    {
        public string CustomerId { get; set; }
        public string Name { get; set; }
        public string Nationality { get; set; }
        public string Dob { get; set; }
        public Bank Bank { get; set; }
    }

    public class Vendor
    {
        public string Name { get; set; }
        public string CountryOfIncorporation { get; set; }
    }

    public class TMSScreeningModel
    {
        public Remitter Remitter { get; set; }
        public Beneficiary Beneficiary { get; set; }
        public Vendor Vendor { get; set; }
    }
}
